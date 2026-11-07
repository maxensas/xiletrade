using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Live;

namespace Xiletrade.Library.Services;

/// <summary>
/// Live searches of the trade site : the new listings are fetched and displayed in toasts.
/// </summary>
/// <remarks>
/// The fetches are limited for all the live searches together : at most one every <see cref="FetchInterval"/>,
/// none during a fetch, and none when a fetch limit of the api is used at <see cref="RateLimitReserve"/>
/// (the rest is kept for the price checks). A live result which can not be fetched is skipped, not queued :
/// its token expires quickly and the toasts stay fresh.
/// The connections and requests are made by the net service (session cookie, rate limits). Each change of
/// <see cref="IsRunning"/> or <see cref="IsPaused"/> is sent as a <see cref="LiveSearchStateMessage"/> (weak messenger)
/// to the view models which display it.
/// The steps are logged in debug only (no code in release).
/// </remarks>
public sealed class LiveSearchService : IDisposable
{
    /// <summary>Live searches at the same time : a new one stops the oldest.</summary>
    internal const int MaxSessions = 1;

    private const int MaxToasts = 5;
    private const int MaxPinned = 2;
    private const int MaxResultsPerMinute = 60;
    private const double RateLimitReserve = 0.8;
    // rate limit state of a response trusted during this time
    private const int RateLimitMaxAgeMs = 60_000;

    private static readonly TimeSpan FetchInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ToastDuration = TimeSpan.FromSeconds(30);

    private readonly ILogger<LiveSearchService> _logger;
    private readonly DataManagerService _dm;
    private readonly INetService _net;
    private readonly ITokenService _token;
    private readonly INotificationService _notif;

    private readonly Lock _gate = new();
    private readonly List<LiveSearchSession> _sessions = [];
    // ids of the toasts of the live searches displayed, oldest first (removed when they are closed or expire)
    private readonly Queue<string> _toasts = new();
    // ids of the pinned toasts, out of the toasts above
    private readonly HashSet<string> _pinned = new(StringComparer.Ordinal);
    private readonly LivePinStateViewModel _pins = new();

    // given to the toasts of the live results : created once
    private readonly Func<string, string, Task<bool>> _travelToHideout;
    private readonly Action<string> _dismissToast;
    private readonly Func<LiveItemViewModel, bool> _tryPin;

    // timestamp (Stopwatch) from which a fetch may start, 1 : fetch running
    private long _nextFetch;
    private int _fetching;

    // the live results are skipped, the connection is kept
    private volatile bool _isPaused;

    /// <summary>A live search is listening.</summary>
    internal bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _sessions.Count > 0;
            }
        }
    }

    /// <summary>The live results are skipped, the connection is kept : resumed at once.</summary>
    internal bool IsPaused => _isPaused;

    public LiveSearchService(ILogger<LiveSearchService> logger, DataManagerService dm, INetService net,
        ITokenService token, INotificationService notif, PoeApiService poeApi)
    {
        _logger = logger;
        _dm = dm;
        _net = net;
        _token = token;
        _notif = notif;
        _travelToHideout = poeApi.TravelToHideoutAsync;
        _dismissToast = DismissToast;
        _tryPin = TryPin;

        // toasts closed or expired : out of the toasts, a pinned one frees its place
        _notif.NotificationRemoved += OnNotificationRemoved;
    }

    /// <summary>
    /// Starts a live search : the query is searched to get its id, then its new listings are listened to.
    /// </summary>
    /// <param name="query">Json of the search (same as the custom search).</param>
    /// <returns>False when it could not start (the reason is displayed).</returns>
    internal async Task<bool> StartAsync(byte[] query, string league)
    {
        // the session is sent by the net service : checked here to tell the user before searching
        if (!_token.TryGetToken(out var sessionId, useCustom: true) || !RegexUtil.MD5().IsMatch(sessionId))
        {
#if DEBUG
            _logger.LogDebug("Live search : no valid POESESSID");
#endif
            Notify(Resources.Resources.Main298_LiveNoSession, NotificationStatus.Error);
            return false;
        }

        string searchId;
        try
        {
            var result = await _net.SendHTTPBytes(query, Strings.Api.Trade + league, Client.Trade).ConfigureAwait(false);
            searchId = result.Length > 0 ? _dm.Json.Deserialize<ResultData>(result)?.Id : null;
        }
        catch (Exception ex) // network, timeout, json, or any other error of the net service : never thrown to the command
        {
#if DEBUG
            _logger.LogDebug("Live search : search failed, {Exception} {Message}", ex.GetType().Name, ex.Message);
#endif
            Notify(Resources.Resources.Main299_LiveNotStarted + "\n" + ex.Message, NotificationStatus.Error);
            return false;
        }
        if (string.IsNullOrEmpty(searchId))
        {
#if DEBUG
            _logger.LogDebug("Live search : search without id");
#endif
            Notify(Resources.Resources.Main299_LiveNotStarted, NotificationStatus.Error);
            return false;
        }
#if DEBUG
        _logger.LogDebug("Live search : search id {SearchId}, league {League}", searchId, league);
#endif

        var session = new LiveSearchSession(_net, Strings.Api.Live + league + "/" + searchId, Strings.Api.Fetch,
            Strings.Api.GetSearchPage(league, searchId) + "/live", searchId,
            MaxResultsPerMinute, OnResult, OnAuthenticated, _logger);
        lock (_gate)
        {
            // one live search at a time for now : the oldest is stopped
            while (_sessions.Count >= MaxSessions)
            {
                _sessions[0].Stop();
                _sessions.RemoveAt(0);
            }
            _sessions.Add(session);
            _isPaused = false; // a new session is not paused
        }
        SendState();
        _ = RunAsync(session);
        return true;
    }

    /// <summary>The criteria of the form are empty : nothing to listen to.</summary>
    internal void NotifyNoCriteria() => Notify(Resources.Resources.Main309_LiveNoCriteria, NotificationStatus.Warning);

    // the live search really started : the server accepted the session of the user
    private void OnAuthenticated(LiveSearchSession session)
        => Notify(Resources.Resources.Main300_LiveStarted, NotificationStatus.Success);

    /// <summary>Stops all the live searches.</summary>
    internal void StopAll()
    {
        lock (_gate)
        {
            foreach (var session in _sessions)
            {
                session.Stop();
            }
            _sessions.Clear();
            _isPaused = false;
        }
        SendState();
    }

    /// <summary>Pauses the live searches (results skipped) or resumes them.</summary>
    internal void SetPaused(bool paused)
    {
        lock (_gate)
        {
            _isPaused = paused;
            foreach (var session in _sessions)
            {
                session.IsPaused = paused;
            }
        }
        SendState();
    }

    private void SendState() => WeakReferenceMessenger.Default.Send(new LiveSearchStateMessage(IsRunning, IsPaused));

    private async Task RunAsync(LiveSearchSession session)
    {
        var end = LiveSearchEnd.ConnectionLost;
        try
        {
            end = await session.RunAsync().ConfigureAwait(false);
        }
#if DEBUG
        catch (Exception ex)
        {
            _logger.LogDebug("Live search : error {Exception} {Message}", ex.GetType().Name, ex.Message);
        }
#else
        catch (Exception)
        {
            // ended as a lost connection
        }
#endif
        finally
        {
            // ended by itself (not stopped or replaced) : the state has changed
            bool removed;
            lock (_gate)
            {
                removed = _sessions.Remove(session);
                if (removed && _sessions.Count is 0)
                {
                    _isPaused = false;
                }
            }
            if (removed)
            {
                SendState();
            }
            session.Dispose();
        }
#if DEBUG
        _logger.LogDebug("Live search : ended {End}, {Count} live result(s)", end, session.ResultCount);
#endif

        var message = end switch
        {
            LiveSearchEnd.TooBroad => string.Format(Resources.Resources.Main301_LiveTooBroad, MaxResultsPerMinute),
            LiveSearchEnd.Refused => Resources.Resources.Main298_LiveNoSession,
            LiveSearchEnd.ConnectionLost => Resources.Resources.Main302_LiveConnectionLost,
            _ => null // stopped by the user : nothing to say
        };
        if (message is not null)
        {
            Notify(message, end is LiveSearchEnd.TooBroad ? NotificationStatus.Warning : NotificationStatus.Error);
        }
    }

    // called by the receive loop of a session : fetched now or skipped
    private void OnResult(LiveSearchSession session, string token)
    {
        var now = Stopwatch.GetTimestamp();
        var skip = IsPaused ? "paused"
            : now < Volatile.Read(ref _nextFetch) ? "interval"
            : IsOverBudget(_net.GetRateLimit(Strings.Net.TradeFetchRequestLimit)) ? "rate limit"
            : Interlocked.CompareExchange(ref _fetching, 1, 0) is not 0 ? "fetch running"
            : null;
        if (skip is not null)
        {
#if DEBUG
            _logger.LogDebug("Live search : result {Count} skipped, {Reason}", session.ResultCount, skip);
#endif
            return; // the next results will be fresher
        }
#if DEBUG
        _logger.LogDebug("Live search : result {Count} fetched", session.ResultCount);
#endif
        Volatile.Write(ref _nextFetch, now + (long)(FetchInterval.TotalSeconds * Stopwatch.Frequency));
        _ = FetchAsync(session, token);
    }

    // penalty running (restriction, retry after), or a limit used at the reserve
    private static bool IsOverBudget(RateLimitInfo limit)
    {
        if (limit is null)
        {
            return false;
        }
        var age = Environment.TickCount64 - limit.ReceivedAt;
        if (age < Math.Max(limit.Restriction, limit.RetryAfter) * 1000L)
        {
            return true;
        }
        return age < RateLimitMaxAgeMs && limit.Usage >= RateLimitReserve;
    }

    private async Task FetchAsync(LiveSearchSession session, string token)
    {
        try
        {
            // like the trade site : request of its script, from the page of the live search
            var body = await _net.SendHTTPBytes(ReadOnlyMemory<byte>.Empty, session.FetchUrl + token + "?query=" + session.SearchId,
                Client.Trade, isXml: true, session.Referer).ConfigureAwait(false);
            if (body.Length is 0)
            {
                return;
            }
            var data = _dm.Json.Deserialize<FetchData>(body, replace: true);
            var sound = true; // one sound for the items of a fetch
#if DEBUG
            var shown = 0;
#endif
            foreach (var info in data?.Result ?? [])
            {
                if (info?.Item is null || info.Listing is null)
                {
                    continue; // no longer listed
                }
                Show(new LiveItemViewModel(_dm, info, session.Referer, _travelToHideout, _dismissToast, _tryPin, _pins), sound);
                sound = false;
#if DEBUG
                shown++;
#endif
            }
#if DEBUG
            _logger.LogDebug("Live search : fetch {Results} result(s), {Toasts} toast(s)", data?.Result?.Length ?? 0, shown);
#endif
        }
        catch (Exception ex)
        {
            // rate limit (429) included : its state, kept by the net service, is read by the next result
#if DEBUG
            _logger.LogDebug("Live search : fetch failed, {Exception} {Message}", ex.GetType().Name, ex.Message);
#endif
            if (!session.FetchErrorShown)
            {
                session.FetchErrorShown = true;
                Notify(Resources.Resources.Error008_XFetch + " : " + ex.Message, NotificationStatus.Warning);
            }
        }
        finally
        {
            Volatile.Write(ref _fetching, 0);
        }
    }

    // at most MaxToasts toasts of the live searches : the oldest disappear at once (no fading), before the new one is displayed
    private void Show(LiveItemViewModel item, bool sound)
    {
        List<string> dismiss = null;
        lock (_gate)
        {
            RemoveToast(item.Id); // same listing displayed again : replaced, not counted twice
            while (_toasts.Count >= MaxToasts)
            {
                (dismiss ??= []).Add(_toasts.Dequeue());
            }
        }
        if (dismiss is not null)
        {
            foreach (var id in dismiss)
            {
                _notif.Dismiss(id, fade: false);
            }
        }

        _notif.Show(new ToastNotification
        {
            Id = item.Id,
            Title = item.Title,
            Message = item.PriceText,
            Status = NotificationStatus.Information,
            Duration = ToastDuration,
            Content = item,
            PlaySound = sound,
            Placement = NotificationPlacement.LowerQuarterRight
        });

        // after it is displayed : the removal of the toast replaced (event) does not remove it
        lock (_gate)
        {
            _toasts.Enqueue(item.Id);
        }
    }

    private void DismissToast(string id) => _notif.Dismiss(id);

    // pinned : out of the MaxToasts toasts, displayed again at the top of the screen until it is closed (MaxPinned at most)
    private bool TryPin(LiveItemViewModel item)
    {
        lock (_gate)
        {
            if (_pinned.Count >= MaxPinned)
                return false;

            RemoveToast(item.Id);
        }

        _notif.Show(new ToastNotification
        {
            Id = item.Id,
            Title = item.Title,
            Message = item.PriceText,
            Status = NotificationStatus.Information,
            Duration = null,
            Content = item,
            IsPinned = true,
            Placement = NotificationPlacement.TopRight
        });

        // after the toast is displayed again : its removal (replaced by the pinned one) is not a closing
        bool full;
        lock (_gate)
        {
            _pinned.Add(item.Id);
            full = _pinned.Count >= MaxPinned;
        }
        _pins.IsFull = full;
        return true;
    }

    // toast closed, expired or replaced : out of the toasts, a pinned one frees its place
    private void OnNotificationRemoved(object sender, string id)
    {
        bool full;
        lock (_gate)
        {
            if (!_pinned.Remove(id))
            {
                RemoveToast(id);
                return;
            }
            full = _pinned.Count >= MaxPinned;
        }
        _pins.IsFull = full;
    }

    // the order of the others is kept (under the gate)
    private void RemoveToast(string removed)
    {
        var count = _toasts.Count;
        for (int i = 0; i < count; i++)
        {
            var id = _toasts.Dequeue();
            if (id != removed)
            {
                _toasts.Enqueue(id);
            }
        }
    }


    private void Notify(string message, NotificationStatus status)
        => _notif.Show(new ToastNotification(Resources.Resources.Main296_LiveSearch, message, status));

    public void Dispose()
    {
        _notif.NotificationRemoved -= OnNotificationRemoved;
        StopAll();
    }
}

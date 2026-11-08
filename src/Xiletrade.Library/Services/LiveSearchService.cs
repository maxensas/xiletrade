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
/// none during a fetch, and none when a window of the fetch limits of the api would go over <see cref="MaxFetchUsage"/>
/// of its maximum (the rest is kept for the price checks, see <see cref="PoeApiService"/>). A live result which can not be fetched is skipped, not queued :
/// its token expires quickly and the toasts stay fresh.
/// Up to <see cref="MaxSessions"/> live searches together, each one numbered (its toasts show its number) and paused
/// or stopped alone, or all together (overlay). They share the limits above : more live results, not more requests.
/// The connections and requests are made by the net service (session cookie, rate limits). Each change of the
/// live searches is sent as a <see cref="LiveSearchStateMessage"/> (weak messenger) to the view models which display it.
/// The steps are logged in debug only (no code in release).
/// </remarks>
public sealed class LiveSearchService : IDisposable
{
    /// <summary>Live searches at the same time.</summary>
    internal const int MaxSessions = 4;

    private const int MaxToasts = 6;
    private const int MaxPinned = 3;
    private const int MaxResultsPerMinute = 60;
    /// <summary>Part of each window of the fetch limits the live searches can use.</summary>
    internal const double MaxFetchUsage = 0.6;

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
    private readonly LiveToastStateViewModel _toastState = new();

    // given to the toasts of the live results : created once
    private readonly Func<string, string, Task<bool>> _travelToHideout;
    private readonly Action<string> _dismissToast;
    private readonly Func<LiveItemViewModel, bool> _tryPin;

    // timestamp (Stopwatch) from which a fetch may start, 1 : fetch running
    private long _nextFetch;
    private int _fetching;

    // all the live searches paused (overlay) : set again on each one, each one can be paused or resumed alone
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

    /// <summary>All the live searches paused (overlay) : their results are skipped, the connections are kept.</summary>
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
    /// <param name="label">Name of the preset or search text, null : "Live" and its number.</param>
    /// <returns>False when it could not start (the reason is displayed).</returns>
    internal async Task<bool> StartAsync(byte[] query, string league, string label)
    {
        if (!CanStart(query))
        {
            return false;
        }

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
        LiveSearchSession session;
        lock (_gate)
        {
            // checked again : another form may have started one meanwhile
            if (_sessions.Count >= MaxSessions || IsStarted(query))
            {
                return false;
            }
            var number = GetFreeNumber();
            session = new LiveSearchSession(_net, Strings.Api.Live + league + "/" + searchId, Strings.Api.Fetch,
                Strings.Api.GetSearchPage(league, searchId) + "/live", searchId,
                MaxResultsPerMinute, OnResult, OnAuthenticated, _logger)
            {
                Number = number,
                Label = string.IsNullOrWhiteSpace(label) ? null : label,
                Query = query
            };
            _sessions.Add(session);
        }
#if DEBUG
        _logger.LogDebug("Live search {Number} : search id {SearchId}, league {League}", session.Number, searchId, league);
#endif
        SendState();
        _ = RunAsync(session);
        return true;
    }

    // a free place and not the same search as a running one (told to the user)
    private bool CanStart(byte[] query)
    {
        lock (_gate)
        {
            if (_sessions.Count >= MaxSessions)
            {
                return false;
            }
            if (!IsStarted(query))
            {
                return true;
            }
        }
        Notify(Resources.Resources.Main312_LiveDuplicate, NotificationStatus.Warning);
        return false;
    }

    // same json : same search (under the gate)
    private bool IsStarted(byte[] query)
    {
        foreach (var session in _sessions)
        {
            if (query.AsSpan().SequenceEqual(session.Query))
            {
                return true;
            }
        }
        return false;
    }

    // smallest number not used by a running live search (under the gate)
    private int GetFreeNumber()
    {
        for (int number = 1; number <= MaxSessions; number++)
        {
            var used = false;
            foreach (var session in _sessions)
            {
                used |= session.Number == number;
            }
            if (!used)
            {
                return number;
            }
        }
        return MaxSessions;
    }

    /// <summary>The criteria of the form are empty : nothing to listen to.</summary>
    internal void NotifyNoCriteria() => Notify(Resources.Resources.Main309_LiveNoCriteria, NotificationStatus.Warning);

    // the live search really started : the server accepted the session of the user
    private void OnAuthenticated(LiveSearchSession session)
        => Notify(session, Resources.Resources.Main300_LiveStarted, NotificationStatus.Success);

    /// <summary>Live searches listening, oldest first.</summary>
    internal LiveSearchInfo[] GetSessions()
    {
        lock (_gate)
        {
            var sessions = new LiveSearchInfo[_sessions.Count];
            for (int i = 0; i < sessions.Length; i++)
            {
                var session = _sessions[i];
                sessions[i] = new(session.Number,
                    session.Label ?? string.Concat(Resources.Resources.Main297_Live, " ", session.Number.ToString()), session.IsPaused);
            }
            return sessions;
        }
    }

    /// <summary>Stops a live search.</summary>
    internal void Stop(int number)
    {
        bool removed = false;
        lock (_gate)
        {
            for (int i = 0; i < _sessions.Count; i++)
            {
                if (_sessions[i].Number == number)
                {
                    _sessions[i].Stop();
                    _sessions.RemoveAt(i);
                    removed = true;
                    break;
                }
            }
            if (_sessions.Count is 0)
            {
                _isPaused = false;
            }
        }
        if (removed)
        {
            SendState();
        }
    }

    /// <summary>Pauses a live search (results skipped) or resumes it.</summary>
    internal void SetPaused(int number, bool paused)
    {
        lock (_gate)
        {
            foreach (var session in _sessions)
            {
                if (session.Number == number)
                {
                    session.IsPaused = paused;
                }
            }
        }
        SendState();
    }

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

    /// <summary>Pauses all the live searches (results skipped) or resumes them all.</summary>
    internal void SetPausedAll(bool paused)
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

    // the toasts show the number of their live search while several are running
    private void SendState()
    {
        var sessions = GetSessions();
        _toastState.HasSeveralLives = sessions.Length > 1;
        WeakReferenceMessenger.Default.Send(new LiveSearchStateMessage(sessions, IsPaused));
    }

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
            _logger.LogDebug("Live search {Number} : error {Exception} {Message}", session.Number, ex.GetType().Name, ex.Message);
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
        _logger.LogDebug("Live search {Number} : ended {End}, {Count} live result(s)", session.Number, end, session.ResultCount);
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
            Notify(session, message, end is LiveSearchEnd.TooBroad ? NotificationStatus.Warning : NotificationStatus.Error);
        }
    }

    // called by the receive loop of a session : fetched now or skipped
    private void OnResult(LiveSearchSession session, string token)
    {
        var now = Stopwatch.GetTimestamp();
        var skip = session.IsPaused ? "paused"
            : now < Volatile.Read(ref _nextFetch) ? "interval"
            : _net.GetFetchWait(1, MaxFetchUsage) > TimeSpan.Zero ? "rate limit"
            : Interlocked.CompareExchange(ref _fetching, 1, 0) is not 0 ? "fetch running"
            : null;
        if (skip is not null)
        {
#if DEBUG
            _logger.LogDebug("Live search {Number} : result {Count} skipped, {Reason}", session.Number, session.ResultCount, skip);
#endif
            return; // the next results will be fresher
        }
#if DEBUG
        _logger.LogDebug("Live search {Number} : result {Count} fetched", session.Number, session.ResultCount);
#endif
        Volatile.Write(ref _nextFetch, now + (long)(FetchInterval.TotalSeconds * Stopwatch.Frequency));
        _ = FetchAsync(session, token);
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
                Show(new LiveItemViewModel(_dm, info, session.Number, session.Referer, _travelToHideout, _dismissToast, _tryPin, _toastState), sound);
                sound = false;
#if DEBUG
                shown++;
#endif
            }
#if DEBUG
            _logger.LogDebug("Live search {Number} : fetch {Results} result(s), {Toasts} toast(s)", session.Number, data?.Result?.Length ?? 0, shown);
#endif
        }
        catch (Exception ex)
        {
            // rate limit (429) included : its state, kept by the net service, is read by the next result
#if DEBUG
            _logger.LogDebug("Live search {Number} : fetch failed, {Exception} {Message}", session.Number, ex.GetType().Name, ex.Message);
#endif
            if (!session.FetchErrorShown)
            {
                session.FetchErrorShown = true;
                Notify(session, Resources.Resources.Error008_XFetch + " : " + ex.Message, NotificationStatus.Warning);
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
        _toastState.IsPinFull = full;
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
        _toastState.IsPinFull = full;
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

    // title : the live search, its number and its label (several live searches)
    private void Notify(LiveSearchSession session, string message, NotificationStatus status)
    {
        var title = string.Concat(Resources.Resources.Main296_LiveSearch, " ", session.Number.ToString());
        if (session.Label is not null)
        {
            title = string.Concat(title, " : ", session.Label);
        }
        _notif.Show(new ToastNotification(title, message, status));
    }

    public void Dispose()
    {
        _notif.NotificationRemoved -= OnNotificationRemoved;
        StopAll();
    }
}

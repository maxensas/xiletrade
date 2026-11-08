using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Application.LiveSearch;

/// <summary>
/// One live search of the trade site : listens to its websocket (opened by the net service) and gives its results.
/// </summary>
/// <remarks>
/// Messages of the server : {"auth":true} once connected, then {"result":"&lt;token&gt;","count":n} for each new listing.
/// The token (JWT, valid about 10 seconds) is given to the owner, which fetches it or skips it.
/// The connection is restored a few times when it is lost (the owner is told of the first authentication only),
/// a search with too many results is paused, not stopped : the user refines it or resumes it (while paused, its results
/// are skipped and not counted).
/// Stopped by the closing handshake : the waiting read ends with the answer of the server, without exception (cancelled
/// only when the server does not answer).
/// The steps are logged in debug only (no code in release).
/// </remarks>
internal sealed class LiveSearchSession : IDisposable
{
    private const int BufferSize = 4096;
    private static readonly TimeSpan _closeTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan[] _reconnectDelays = [TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30)];

    private readonly INetService _net;
    private readonly string _liveUrl;
    private readonly int _maxResultsPerMinute;
    private readonly Action<LiveSearchSession, string> _onResult;
    private readonly Action<LiveSearchSession> _onAuthenticated;
    private readonly Action<LiveSearchSession> _onTooBroad;
    private readonly ILogger _logger;
    // connection and delays of the reconnection
    private readonly CancellationTokenSource _stop = new();
    // reads : cancelled only when the server does not answer the closing
    private readonly CancellationTokenSource _abort = new();

    // listened socket, null between the connections
    private volatile WebSocket _socket;
    // 1 : the closing is sent (once)
    private int _closing;

    // timestamps (Stopwatch) of the live results of the last minute
    private readonly Queue<long> _lastMinute = new();

    // authenticated once : the reconnections are not told to the owner
    private bool _started;
    private volatile bool _isPaused;

    /// <summary>Id of the search (query), sent with the fetch of the results.</summary>
    internal string SearchId { get; }

    /// <summary>Url of the fetch when the session started (game and server of that moment), the token is added.</summary>
    internal string FetchUrl { get; }

    /// <summary>Page of the live search on the trade site : referer of its fetches and travels.</summary>
    internal string Referer { get; }

    /// <summary>Live results received.</summary>
    internal int ResultCount { get; private set; }

    /// <summary>A fetch error was already displayed to the user.</summary>
    internal bool FetchErrorShown { get; set; }

    /// <summary>Number of the live search, from 1 to the maximum : displayed on its toasts.</summary>
    internal int Number { get; init; }

    /// <summary>Name of the preset or search text when it started, null : none ("Live" and its number displayed).</summary>
    internal string Label { get; init; }

    /// <summary>Json of the search : the same search is not started twice.</summary>
    internal byte[] Query { get; init; }

    /// <summary>Criteria of the custom search when it started : given back to the form.</summary>
    internal LiveSearchCriteria Criteria { get; init; }

    /// <summary>Paused by the user or for too many results : its results are skipped, they do not count as too many results.</summary>
    internal bool IsPaused
    {
        get => _isPaused;
        set => _isPaused = value;
    }

    /// <param name="liveUrl">Websocket of the live search, league and search id included.</param>
    /// <param name="onTooBroad">Paused for too many results in one minute.</param>
    internal LiveSearchSession(INetService net, string liveUrl, string fetchUrl, string referer, string searchId,
        int maxResultsPerMinute, Action<LiveSearchSession, string> onResult, Action<LiveSearchSession> onAuthenticated,
        Action<LiveSearchSession> onTooBroad, ILogger logger)
    {
        _onTooBroad = onTooBroad;
        _net = net;
        _liveUrl = liveUrl;
        FetchUrl = fetchUrl;
        Referer = referer;
        SearchId = searchId;
        _maxResultsPerMinute = maxResultsPerMinute;
        _onResult = onResult;
        _onAuthenticated = onAuthenticated;
        _logger = logger;
    }

    /// <summary>Listens until stopped, refused or lost.</summary>
    internal async Task<LiveSearchEnd> RunAsync()
    {
        var attempt = 0;
        while (!_stop.IsCancellationRequested)
        {
            var (end, authenticated) = await ListenAsync(_stop.Token).ConfigureAwait(false);
            if (end is not LiveSearchEnd.ConnectionLost || _stop.IsCancellationRequested)
            {
                return _stop.IsCancellationRequested ? LiveSearchEnd.Stopped : end;
            }
            attempt = authenticated ? 0 : attempt; // a connection which worked starts the attempts again
            if (attempt >= _reconnectDelays.Length)
            {
                return LiveSearchEnd.ConnectionLost;
            }
            try
            {
                await Task.Delay(_reconnectDelays[attempt++], _stop.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return LiveSearchEnd.Stopped;
            }
        }
        return LiveSearchEnd.Stopped;
    }

    /// <summary>Stops the session : closes its socket, the listening ends with the answer of the server.</summary>
    internal void Stop()
    {
        if (_stop.IsCancellationRequested)
        {
            return;
        }
        _stop.Cancel();
        if (_socket is { } socket)
        {
            _ = CloseOutputAsync(socket);
        }
    }

    // closing handshake sent once, the reads are cancelled if the server does not answer
    private async Task CloseOutputAsync(WebSocket socket)
    {
        if (Interlocked.Exchange(ref _closing, 1) is 1)
        {
            return;
        }
        try
        {
            _abort.CancelAfter(_closeTimeout);
            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, _abort.Token).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // already closed or aborted : the read ends anyway
        }
    }

    // one connection : ended by the server, the network, the user or a limit
    private async Task<(LiveSearchEnd End, bool Authenticated)> ListenAsync(CancellationToken cancellation)
    {
        var authenticated = false;
        WebSocket socket;
#if DEBUG
        _logger.LogDebug("Live search {Number} : connecting to {Url}", Number, _liveUrl);
#endif
        try
        {
            socket = await _net.ConnectTradeSocketAsync(_liveUrl, cancellation).ConfigureAwait(false);
#if DEBUG
            _logger.LogDebug("Live search {Number} : connected", Number);
#endif
        }
        catch (HttpRequestException ex)
        {
#if DEBUG
            _logger.LogDebug("Live search {Number} : connection refused, {Message}", Number, ex.Message);
#endif
            return (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound
                ? LiveSearchEnd.Refused : LiveSearchEnd.ConnectionLost, false);
        }
        catch (OperationCanceledException)
        {
            return (LiveSearchEnd.Stopped, false);
        }

        using (socket)
        {
            _socket = socket;
            if (_stop.IsCancellationRequested)
            {
                _ = CloseOutputAsync(socket); // stopped while connecting
            }
            var buffer = new ArrayBufferWriter<byte>(BufferSize);
            try
            {
                while (socket.State is WebSocketState.Open)
                {
                    buffer.ResetWrittenCount();
                    ValueWebSocketReceiveResult received;
                    do
                    {
                        received = await socket.ReceiveAsync(buffer.GetMemory(BufferSize), _abort.Token).ConfigureAwait(false);
                        buffer.Advance(received.Count);
                    }
                    while (!received.EndOfMessage);

                    if (received.MessageType is WebSocketMessageType.Close)
                    {
                        if (cancellation.IsCancellationRequested)
                        {
#if DEBUG
                            _logger.LogDebug("Live search {Number} : closed", Number);
#endif
                            return (LiveSearchEnd.Stopped, authenticated);
                        }
#if DEBUG
                        _logger.LogDebug("Live search {Number} : closed by the server, {Status} {Description}", Number, socket.CloseStatus, socket.CloseStatusDescription);
#endif
                        // policy violation : refused, not restored
                        return (socket.CloseStatus is WebSocketCloseStatus.PolicyViolation ? LiveSearchEnd.Refused : LiveSearchEnd.ConnectionLost, authenticated);
                    }
                    if (received.MessageType is not WebSocketMessageType.Text
                        || !TryReadMessage(buffer.WrittenSpan, out var auth, out var token))
                    {
#if DEBUG
                        _logger.LogDebug("Live search {Number} : message ignored, {Type} {Length} bytes", Number, received.MessageType, buffer.WrittenCount);
#endif
                        continue;
                    }
                    if (auth is { } isAuthenticated)
                    {
#if DEBUG
                        _logger.LogDebug("Live search {Number} : auth {Auth}", Number, isAuthenticated);
#endif
                        if (!isAuthenticated)
                        {
                            return (LiveSearchEnd.Refused, false);
                        }
                        authenticated = true;
                        if (!_started)
                        {
                            _started = true;
                            _onAuthenticated(this);
                        }
                    }
                    if (token is not null)
                    {
                        ResultCount++;
                        if (_isPaused)
                        {
                            _onResult(this, token); // skipped (paused), not counted
                            continue;
                        }
                        if (IsTooBroad())
                        {
                            // paused, not stopped : the count starts again when it is resumed
                            _isPaused = true;
                            _lastMinute.Clear();
#if DEBUG
                            _logger.LogDebug("Live search {Number} : paused, more than {Max} results in one minute", Number, _maxResultsPerMinute);
#endif
                            _onTooBroad(this);
                            continue;
                        }
                        _onResult(this, token);
                    }
                }
#if DEBUG
                _logger.LogDebug("Live search {Number} : socket state {State}", Number, socket.State);
#endif
                return (LiveSearchEnd.ConnectionLost, authenticated);
            }
            catch (OperationCanceledException)
            {
                // no answer of the server to the closing
                return (LiveSearchEnd.Stopped, authenticated);
            }
#if DEBUG
            catch (WebSocketException ex)
            {
                _logger.LogDebug("Live search {Number} : connection lost, {Error} {Message}", Number, ex.WebSocketErrorCode, ex.Message);
                return (LiveSearchEnd.ConnectionLost, authenticated);
            }
#else
            catch (WebSocketException)
            {
                return (LiveSearchEnd.ConnectionLost, authenticated);
            }
#endif
            finally
            {
                _socket = null;
            }
        }
    }

    // {"auth":true} or {"result":"<token>","count":n} : read without document
    internal static bool TryReadMessage(ReadOnlySpan<byte> json, out bool? auth, out string token)
    {
        auth = null;
        token = null;
        try
        {
            var reader = new Utf8JsonReader(json);
            if (!reader.Read() || reader.TokenType is not JsonTokenType.StartObject)
            {
                return false;
            }
            while (reader.Read() && reader.TokenType is JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals("auth"u8))
                {
                    reader.Read();
                    auth = reader.TokenType is JsonTokenType.True;
                }
                else if (reader.ValueTextEquals("result"u8))
                {
                    reader.Read();
                    token = reader.TokenType is JsonTokenType.String ? reader.GetString() : null;
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }
            }
            return auth is not null || token is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // more live results in one minute than allowed : the search is too broad
    internal bool IsTooBroad()
    {
        if (_maxResultsPerMinute <= 0)
        {
            return false;
        }
        var now = Stopwatch.GetTimestamp();
        _lastMinute.Enqueue(now);
        while (_lastMinute.Count > 0 && Stopwatch.GetElapsedTime(_lastMinute.Peek(), now) > TimeSpan.FromMinutes(1))
        {
            _lastMinute.Dequeue();
        }
        return _lastMinute.Count > _maxResultsPerMinute;
    }

    public void Dispose()
    {
        _stop.Dispose();
        _abort.Dispose();
    }
}

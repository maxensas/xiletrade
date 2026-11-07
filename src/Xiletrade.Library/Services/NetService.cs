using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

public class NetService : INetService
{
    private readonly ITokenService _token;
    private readonly DataManagerService _dm;

    private const string USERAGENT = "User-Agent";

    // cookies of the trade client (session of the user), shared with the websockets of the trade site
    private CookieContainer _tradeCookies;
    private const int MAX_CONCURRENT_REQUEST = 5;

    private readonly HttpClient _default = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(200)
    })
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly HttpClient _update = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(50)
    })
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    private readonly HttpClient _poePrice = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(50)
    })
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly HttpClient _ninja = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        AutomaticDecompression = DecompressionMethods.All
    })
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly HttpClient _gitHub = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(50)
    })
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private HttpClient Trade { get; set; }

    private readonly SemaphoreSlim _throttle = new(MAX_CONCURRENT_REQUEST);

    private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimits = new(StringComparer.Ordinal);

    public NetService(ILogger<NetService> logger, ITokenService token, 
        DataManagerService dm)
    {
        _token = token;
        _dm = dm;

        _default.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        _update.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        // same as the trade client: no connection kept idle, so no background read to abort when it is closed
        _update.DefaultRequestHeaders.ConnectionClose = true;
        _poePrice.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        _ninja.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        _gitHub.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);

        InitTradeClient(_dm.Config.Options.TimeoutTradeApi);

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void InitTradeClient(int timeout)
    {
        if (timeout >= 5 && timeout <= 120)
        {
            var handler = new SocketsHttpHandler { PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(200) };
            _tradeCookies = TryAddCookie(handler) ? handler.CookieContainer : null;
            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            client.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
            client.DefaultRequestHeaders.ConnectionClose = true;

            var old = Trade;
            Trade = client;
            if (old is not null)
            {
                _ = DisposeLaterAsync(old);
            }
        }
    }

    // the requests sent with the previous client end first : disposed once its timeout is over
    private static async Task DisposeLaterAsync(HttpClient client)
    {
        await Task.Delay(client.Timeout + TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        client.Dispose();
    }

    public RateLimitInfo GetRateLimit(string policy)
        => policy is not null && _rateLimits.TryGetValue(policy, out var info) ? info : null;

    private void UpdateRateLimit(HttpResponseMessage response)
    {
        var info = RateLimitInfo.Read(response.Headers);
        if (info is not null && RateLimitInfo.IsTradePolicy(info.Policy))
        {
            _rateLimits[info.Policy] = info;
        }
    }

    private bool TryAddCookie(SocketsHttpHandler handler)
    {
        if (_token.TryGetToken(out var token, useCustom: true) && RegexUtil.MD5().IsMatch(token))
        {
            var cookie = new CookieContainer();
            cookie.Add(new Cookie("POESESSID", token, "/", GetSessionDomain()));
            handler.CookieContainer = cookie;

            return true;
        }
        return false;
    }

    // site of the session : every server of pathofexile.com (www, fr, ru, br...) whatever the language of the trade api,
    // or the site of the gateway (kakao, taiwan, tencent), the trade client is created again when the gateway changes
    internal static string GetSessionDomain()
    {
        const string poeDomain = ".pathofexile.com";
        var host = new Uri(Strings.Api.Trade).Host;
        return host.EndsWith(poeDomain, StringComparison.OrdinalIgnoreCase) ? poeDomain : host;
    }

    /// <summary>
    /// Send Json request using GET method, the body is returned as UTF-8 bytes.
    /// </summary>
    public virtual Task<byte[]> SendHTTPBytes(string urlString, Client idClient)
        => SendHTTPBytes(ReadOnlyMemory<byte>.Empty, urlString, idClient);

    /// <summary>
    /// Send Json request using POST method (GET when <paramref name="json"/> is empty), the body is returned as UTF-8 bytes.
    /// </summary>
    /// <param name="json">Json body, in UTF-8.</param>
    /// <param name="urlString"></param>
    /// <param name="idClient"></param>
    /// <param name="isXml"></param>
    /// <param name="referer">Page of the trade site the request comes from, some tokens are checked with it (hideout).</param>
    /// <returns></returns>
    public virtual async Task<byte[]> SendHTTPBytes(ReadOnlyMemory<byte> json, string urlString, Client idClient, bool isXml = false, string referer = null)
    {
        var result = Array.Empty<byte>();
        var client = GetClient(idClient);
        var isTrade = client == Trade;

        await _throttle.WaitAsync();
        try
        {
            using var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(urlString),
                Method = !json.IsEmpty ? HttpMethod.Post : HttpMethod.Get,
            };
            request.Headers.ProxyAuthorization = null;
            //request.Headers.UserAgent.Add(new ProductInfoHeaderValue(Strings.Net.UserAgent));
            if (!json.IsEmpty)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new ReadOnlyMemoryContent(json);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
            }
            if (isXml)
            {
                // request of a script of the trade site : json expected
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                if (json.IsEmpty)
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
            }
            if (referer is not null)
            {
                request.Headers.Referrer = new Uri(referer);
            }
            if (idClient is Client.Xiletrade && _token.TryGetToken(out var token))
            {
                request.Headers.Authorization = new("Bearer", token);
            }

            using var response = await client.SendAsync(request).ConfigureAwait(false);
            if (isTrade)
            {
                UpdateRateLimit(response);
            }
            if (response.Content is not null)
            {
                result = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            response.EnsureSuccessStatusCode(); // throw HttpRequestException if response failed.
        }
        catch (HttpRequestException ex)
        {
            // json error of the api, not a page of the server (503 : maintenance, overloaded)
            if (isTrade && result.Length > 0 && result[0] is (byte)'{')
            {
                var message = _dm.Json.Deserialize<NetResponse>(result)?.Error?.Message;
                if (!string.IsNullOrEmpty(message))
                {
                    throw new HttpRequestException(message, ex, ex.StatusCode);
                }
            }
            throw;
        }
        catch (TaskCanceledException ex)
        {
            throw ex.InnerException is TimeoutException ? new TimeoutException("The request was canceled due to the configured timeout.", ex)
                : new TaskCanceledException("A task was canceled.", ex);
        }
        catch (Exception ex) // not done : ArgumentNullException / InvalidOperationException / AggregateException
        {
            throw ex.InnerException is ThreadAbortException || ex.Message.ToLowerInvariant().Contain("thread") ?
                 new Exception("Abort called before the end, Application thread error", ex)
                 : ex.InnerException is TimeoutException ? new TimeoutException("The request was canceled due to the configured timeout (inner).", ex)
                 //: ex.InnerException is TaskCanceledException ? new TaskCanceledException("A task was canceled (inner).", ex)
                 : new Exception("Unidentified exception.", ex);
        }
        finally
        {
            _throttle.Release();
        }
        return result;
    }

    public async Task<WebSocket> ConnectTradeSocketAsync(string urlString, CancellationToken cancellation)
    {
        var uri = new Uri(urlString);
        var socket = new ClientWebSocket();
        var options = socket.Options;
        options.Cookies = _tradeCookies ?? new CookieContainer(); // no session : refused by the server
        options.CollectHttpResponseDetails = true; // status of a refused connection
        options.SetRequestHeader("Origin", "https://" + uri.Host);
        options.SetRequestHeader(USERAGENT, Strings.Net.UserAgent);
        options.DangerousDeflateOptions = new WebSocketDeflateOptions();
        options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        try
        {
            await socket.ConnectAsync(uri, cancellation).ConfigureAwait(false);
            return socket;
        }
        catch (WebSocketException ex)
        {
            var status = socket.HttpStatusCode;
            socket.Dispose();
            throw new HttpRequestException($"{(int)status} {status}, {ex.Message}", ex, status);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    public HttpClient GetClient(Client idClient)
    {
        return idClient switch
        {
            Client.Trade => Trade,
            Client.Xiletrade => Trade,
            Client.Update => _update,
            Client.PoePrice => _poePrice,
            Client.Ninja => _ninja,
            Client.GitHub => _gitHub,
            _ => _default,
        };
    }

    public async Task<T> GetFromJsonAsync<T>(string urlString, Client idClient) where T : class, new()
    {
        var result = await SendHTTPBytes(urlString, idClient);
        if (result.Length > 0)
        {
            return _dm.Json.Deserialize<T>(result);
        }
        return null;
    }
}

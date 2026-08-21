using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>Service used to handle http requests and responses for Xiletrade.</summary>
internal class NetService
{
    private static IServiceProvider _serviceProvider;

    private const string USERAGENT = "User-Agent";
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
        PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(200)
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

    internal NetService(IServiceProvider service)
    {
        _serviceProvider = service;

        _default.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        _update.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        _poePrice.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        _ninja.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        _gitHub.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);

        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        InitTradeClient(dm.Config.Options.TimeoutTradeApi);
    }

    internal void InitTradeClient(int timeout)
    {
        if (timeout >= 5 && timeout <= 120)
        {
            var handler = new SocketsHttpHandler { PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(200) };
            TryAddCookie(handler);
            Trade = new(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            Trade.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        }
    }

    private static bool TryAddCookie(SocketsHttpHandler handler)
    {
        if (_serviceProvider.GetRequiredService<ITokenService>()
            .TryGetToken(out var token, useCustom: true) && RegexUtil.MD5().IsMatch(token))
        {
            var cookie = new CookieContainer();
            cookie.Add(new Uri("https://www.pathofexile.com"), new Cookie("POESESSID", token));
            handler.CookieContainer = cookie;

            return true;
        }
        return false;
    }

    /// <summary>
    /// Send Json request using GET method.
    /// </summary>
    /// <param name="urlString"></param>
    /// <param name="idClient"></param>
    /// <returns></returns>
    internal virtual Task<string> SendHTTP(string urlString, Client idClient)
        => SendHTTP(null, urlString, idClient);

    /// <summary>
    /// Send Json request using POST method.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="urlString"></param>
    /// <param name="idClient"></param>
    /// <returns></returns>
    internal virtual async Task<string> SendHTTP(string entity, string urlString, Client idClient, bool isXml = false)
    {
        var result = string.Empty;
        var client = GetClient(idClient);

        await _throttle.WaitAsync();
        try
        {
            using var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(urlString),
                Method = entity is not null ? HttpMethod.Post : HttpMethod.Get,
            };
            request.Headers.ProxyAuthorization = null;
            //request.Headers.UserAgent.Add(new ProductInfoHeaderValue(Strings.Net.UserAgent));
            if (entity is not null)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(entity, Encoding.UTF8, "application/json");
            }
            if (isXml)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            }
            if (idClient is Client.Xiletrade
                && _serviceProvider.GetRequiredService<ITokenService>().TryGetToken(out var token))
            {
                request.Headers.Authorization = new("Bearer", token);
            }

            using var response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.Content is not null)
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            if (client == Trade)
            {
                HandleTradeRateLimit(response);
            }
            response.EnsureSuccessStatusCode(); // throw HttpRequestException if response failed.
        }
        catch (HttpRequestException ex)
        {
            if (client == Trade && !string.IsNullOrEmpty(result))
            {
                var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                var message = dm.Json.Deserialize<NetResponse>(result)?.Error?.Message;
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

    internal HttpClient GetClient(Client idClient)
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

    internal async Task<T> GetFromJsonAsync<T>(string urlString, Client idClient) where T : class, new()
    {
        var result = await SendHTTP(urlString, idClient);
        if (!string.IsNullOrEmpty(result))
        {
            return _serviceProvider.GetRequiredService<DataManagerService>()
                .Json.Deserialize<T>(result);
        }
        return null;
    }

    private static void HandleTradeRateLimit(HttpResponseMessage response)
    {
        if (response is null)
        {
            return;
        }

        foreach (var header in response.Headers)
        {
            if (header.Key.AsSpan().SequenceEqual(Strings.Net.XrateLimitPolicy))
            {
                var timeout = GetResponseTimeouts(response, header.Value.First());
                _serviceProvider?.GetRequiredService<PoeApiService>()?.UpdateCooldown(timeout);
                break;
            }
        }
    }

    private static int[] GetResponseTimeouts(HttpResponseMessage response, ReadOnlySpan<char> policy) // return 'Retry After' in seconds or 0
    {
        int retrySeconds = 0, cdSearch = -1, cdFetch = -1, cdBulk = -1;
        var headers = response.Headers;

        bool isTradeSearch = policy.SequenceEqual(Strings.Net.TradeSearchRequestLimit);
        bool isTradeFetch = policy.SequenceEqual(Strings.Net.TradeFetchRequestLimit);
        bool isTradeBulk = policy.SequenceEqual(Strings.Net.TradeExchangeRequestLimit);

        if (!isTradeSearch && !isTradeFetch && !isTradeBulk)
            return [retrySeconds, cdSearch, cdFetch, cdBulk];

        string rule = null;
        string state = null;
        var filled = false;

        foreach ((var ruleStr, var stateStr) in Strings.Net.XRateRules)
        {
            foreach (var header in headers)
            {
                if (rule is null && header.Key.AsSpan().SequenceEqual(ruleStr))
                {
                    rule = header.Value.First();
                    continue;
                }
                if (state is null && header.Key.AsSpan().SequenceEqual(stateStr))
                {
                    state = header.Value.First();
                    continue;
                }

                if (rule is not null && state is not null)
                {
                    filled = true;
                    break;
                }
            }
            if (filled)
                break;
        }

        if (!filled)
            return [retrySeconds, cdSearch, cdFetch, cdBulk];

        if (TryGetCooldown(rule, state, out int cooldown))
        {
            if (isTradeSearch) cdSearch = cooldown;
            if (isTradeFetch) cdFetch = cooldown;
            if (isTradeBulk) cdBulk = cooldown;
        }

        string retryAfters = null;
        foreach (var header in headers)
        {
            if (retryAfters is null && header.Key.AsSpan().SequenceEqual(Strings.Net.RetryAfter))
            {
                retryAfters = header.Value.First();
                break;
            }
        }
        if (int.TryParse(retryAfters, NumberStyles.Any, CultureInfo.InvariantCulture, out int retry))
        {
            retrySeconds = retry; // Time to wait (in seconds) until the rate limit expires.
        }

        return [retrySeconds, cdSearch, cdFetch, cdBulk];
    }

    private static bool TryGetCooldown(ReadOnlySpan<char> rule, ReadOnlySpan<char> state, out int cooldown)
    {
        cooldown = 0;

        if (!VerifyRates(rule, state))
            return false;

        int ruleStart = 0;
        int stateStart = 0;

        for (int i = 0; i <= rule.Length; i++)
        {
            if (i < rule.Length && rule[i] is not ',')
                continue;

            var rulePart = rule[ruleStart..i];

            int stateRelativeEnd = state[stateStart..].IndexOf(',');
            int stateEnd = stateRelativeEnd >= 0 ? stateStart + stateRelativeEnd : state.Length;

            var statePart = state[stateStart..stateEnd];

            int ruleColon = rulePart.IndexOf(':');
            int stateColon = statePart.IndexOf(':');

            if (ruleColon < 0 || stateColon < 0)
                return false;

            _ = int.TryParse(rulePart[..ruleColon], NumberStyles.Any, CultureInfo.InvariantCulture, out int rLimit);

            _ = int.TryParse(statePart[..stateColon], NumberStyles.Any, CultureInfo.InvariantCulture, out int rState);

            if (rLimit > 0 && rState >= rLimit)
            {
                var ruleAfterColon = rulePart[(ruleColon + 1)..];

                int secondColon = ruleAfterColon.IndexOf(':');

                var ruleCooldown = secondColon >= 0 ? ruleAfterColon[..secondColon] : ruleAfterColon;

                _ = int.TryParse(ruleCooldown, NumberStyles.Any, CultureInfo.InvariantCulture, out int cdLimit);

                if (cdLimit > cooldown)
                    cooldown = cdLimit;
            }

            if (i == rule.Length)
                break;

            ruleStart = i + 1;
            stateStart = stateEnd + 1;
        }

        return true;
    }

    private static bool VerifyRates(ReadOnlySpan<char> rule, ReadOnlySpan<char> state)
    {
        int ruleCount = 1;
        int stateCount = 1;

        for (int i = 0; i < rule.Length; i++)
        {
            if (rule[i] is ',')
                ruleCount++;
        }

        for (int i = 0; i < state.Length; i++)
        {
            if (state[i] is ',')
                stateCount++;
        }

        return ruleCount == stateCount;
    }
}

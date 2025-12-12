using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>Service used to handle http requests and responses for Xiletrade.</summary>
public sealed class NetService
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

    public NetService(IServiceProvider service)
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
    internal Task<string> SendHTTP(string urlString, Client idClient)
        => SendHTTP(null, urlString, idClient);

    /// <summary>
    /// Send Json request using POST method.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="urlString"></param>
    /// <param name="idClient"></param>
    /// <returns></returns>
    internal async Task<string> SendHTTP(string entity, string urlString, Client idClient)
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
            
            if (idClient is Client.Xiletrade
                && _serviceProvider.GetRequiredService<ITokenService>().TryGetToken(out var token))
            {
                request.Headers.Authorization = new("Bearer", token);
            }

            using var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode(); // if Http response failed : throw HttpRequestException
            if (response.Content is not null)
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (client == Trade)
                {
                    HandleTradeRateLimit(response);
                }
            }
        }
        catch (HttpRequestException)
        {
            /*
            throw new HttpRequestException("The request encountered an exception.", ex, ex.StatusCode);
            */
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
        string sResult = await SendHTTP(urlString, idClient);
        if (sResult?.Length > 0)
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            return dm.Json.Deserialize<T>(sResult);
        }
        return null;
    }

    private static void HandleTradeRateLimit(HttpResponseMessage response)
    {
        //if error : {"error":{"code":3,"message":"Rate limit exceeded"}}
        if (response.Headers.Contains(Strings.Net.XrateLimitPolicy))
        {
            var service = _serviceProvider?.GetRequiredService<PoeApiService>();
            var timeout = GetResponseTimeouts(response);
            service?.UpdateCooldown(timeout);
        }
    }

    private static int[] GetResponseTimeouts(HttpResponseMessage response) // return 'Retry After' in seconds or 0
    {
        int retrySeconds = 0, cdSearch = -1, cdFetch = -1, cdBulk = -1;
        var headers = response.Headers;
        if (headers.TryGetValues(Strings.Net.XrateLimitPolicy, out IEnumerable<string> values))
        {
            string policy = values.First();
            bool isTradeSearch = policy is Strings.Net.TradeSearchRequestLimit;
            bool isTradeFetch = policy is Strings.Net.TradeFetchRequestLimit;
            bool isTradeBulk = policy is Strings.Net.TradeExchangeRequestLimit;
            if (isTradeSearch || isTradeFetch || isTradeBulk)
            {
                string rule = string.Empty;
                foreach (string ruleK in Strings.Net.RateRules)
                {
                    string searchtRule = Strings.Net.XrateLimit + ruleK;
                    var seekRule = headers.Where(x => x.Key.Contain(searchtRule));
                    if (seekRule.Any())
                    {
                        rule = searchtRule;
                        break;
                    }
                }
                if (rule.Length > 0 
                    && headers.TryGetValues(rule + Strings.Net.State, out IEnumerable<string> state))
                {
                    if (headers.TryGetValues(rule, out IEnumerable<string> rateLim))
                    {
                        string[] rateLimit = rateLim.First().Split(',');
                        string[] rateLimitState = state.First().Split(',');
                        if (rateLimit.Length == rateLimitState.Length)
                        {
                            int cooldown = 0;
                            for (int i = 0; i < rateLimit.Length; i++)
                            {
                                var rateLimitPart = rateLimit[i].Split(':');
                                var rateLimitStatePart = rateLimitState[i].Split(':');

                                _ = int.TryParse(rateLimitPart[0], NumberStyles.Any, CultureInfo.InvariantCulture, out int rLimit);
                                _ = int.TryParse(rateLimitStatePart[0], NumberStyles.Any, CultureInfo.InvariantCulture, out int rState);
                                if (rLimit > 0 && rState >= rLimit) // put (rState+1) if timeouts detected
                                {
                                    _ = int.TryParse(rateLimitPart[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int cdLimit);
                                    if (cdLimit > cooldown) cooldown = cdLimit;
                                }
                            }
                            // Think : multiple couldowns can still be applied. 
                            if (isTradeSearch) cdSearch = cooldown;
                            if (isTradeFetch) cdFetch = cooldown;
                            if (isTradeBulk) cdBulk = cooldown;
                        }
                    }

                    if (headers.TryGetValues(Strings.Net.RetryAfter, out IEnumerable<string> retry)
                        && int.TryParse(retry.First(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                    {
                        retrySeconds = result; // Time to wait (in seconds) until the rate limit expires.
                    }
                }
            }
        }
        return [retrySeconds, cdSearch, cdFetch, cdBulk];
    }
}

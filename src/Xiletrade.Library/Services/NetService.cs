using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Services;

/// <summary>Service used to handle http requests and responses for Xiletrade.</summary>
public sealed class NetService
{
    private const string USERAGENT = "User-Agent";
    private static IServiceProvider Service { get; set; }

    private static HttpClient Default { get; set; } = new();
    private static HttpClient Trade { get; set; } //= new();
    private static HttpClient Update { get; set; } = new();
    private static HttpClient PoePrice { get; set; } = new();
    private static HttpClient Ninja { get; set; } = new();

    public NetService(IServiceProvider service)
    {
        Service = service;

        Default.Timeout = TimeSpan.FromSeconds(10);
        Default.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        /*
        Trade.Timeout = TimeSpan.FromSeconds(5);
        Trade.DefaultRequestHeaders.Add("User-Agent", Strings.Net.UserAgent);
        */
        Update.Timeout = TimeSpan.FromSeconds(1000);
        Update.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);

        PoePrice.Timeout = TimeSpan.FromSeconds(10);
        PoePrice.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);

        Ninja.Timeout = TimeSpan.FromSeconds(10);
        Ninja.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);

        InitTradeClient(DataManager.Config.Options.TimeoutTradeApi);
    }

    internal void InitTradeClient(int timeout)
    {
        if (timeout >= 5 && timeout <= 120)
        {
            Trade = new()
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            Trade.DefaultRequestHeaders.Add(USERAGENT, Strings.Net.UserAgent);
        }
    }

    internal async Task<string> SendHTTP(string entity, string urlString, Client idClient)
    {
        string result = string.Empty;
        HttpClient client = GetClient(idClient);

        try
        {
            HttpRequestMessage request = new();
            request.RequestUri = new Uri(urlString);
            request.Headers.ProxyAuthorization = null;
            //request.Headers.UserAgent.Add(new ProductInfoHeaderValue(Strings.Net.UserAgent));

            request.Method = entity is not null ? HttpMethod.Post : HttpMethod.Get;
            if (entity is not null)
            {
                StringContent content = new(entity, Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = content;
            }

            HttpResponseMessage response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode(); // if Http response failed : throw HttpRequestException

            if (response.Content is not null)
            {
                result = await response.Content.ReadAsStringAsync();
                //if error : {"error":{"code":3,"message":"Rate limit exceeded"}}
                if (response.Headers.Contains(Strings.Net.XrateLimitPolicy))
                {
                    var vm = Service?.GetRequiredService<MainViewModel>();
                    vm?.Logic.Task.Price.CoolDown.Update(GetResponseTimeouts(response));
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
            throw ex.InnerException is ThreadAbortException || ex.Message.ToLowerInvariant().Contains("thread", StringComparison.Ordinal) ?
                 new Exception("Abort called before the end, Application thread error", ex)
                 : ex.InnerException is TimeoutException ? new TimeoutException("The request was canceled due to the configured timeout (inner).", ex)
                 //: ex.InnerException is TaskCanceledException ? new TaskCanceledException("A task was canceled (inner).", ex)
                 : new Exception("Unidentified exception.", ex);
        }
        return result;
    }

    private static HttpClient GetClient(Client idClient)
    {
        return idClient switch
        {
            Client.Trade => Trade,
            Client.Update => Update,
            Client.PoePrice => PoePrice,
            Client.Ninja => Ninja,
            _ => Default,
        };
    }

    private static string GetResponseTimeouts(HttpResponseMessage response) // return 'Retry After' in seconds or 0
    {
        int retrySeconds = 0, cdSearch = -1, cdFetch = -1, cdBulk = -1;

        if (response.Headers.TryGetValues(Strings.Net.XrateLimitPolicy, out IEnumerable<string> values))
        {
            string policy = values.First();
            bool isTradeSearch = policy is Strings.Net.TradeSearchRequestLimit;
            bool isTradeFetch = policy is Strings.Net.TradeFetchRequestLimit;
            bool isTradeBulk = policy is Strings.Net.TradeExchangeRequestLimit;
            if (isTradeSearch || isTradeFetch || isTradeBulk)
            {
                string[] rulesKind = ["Ip", "Account", "Client"];
                string rule = string.Empty;
                foreach (string ruleK in rulesKind)
                {
                    string searchtRule = Strings.Net.XrateLimit + ruleK;
                    var seekRule = response.Headers.Where(x => x.Key.Contains(searchtRule, StringComparison.Ordinal));
                    if (seekRule.Any())
                    {
                        rule = searchtRule;
                        break;
                    }
                }
                if (rule.Length > 0)
                {
                    if (response.Headers.TryGetValues(rule + Strings.Net.State, out IEnumerable<string> state))
                    {
                        if (response.Headers.TryGetValues(rule, out IEnumerable<string> rateLim))
                        {
                            string[] rateLimit = rateLim.First().Split(',');
                            string[] rateLimitState = state.First().Split(',');
                            if (rateLimit.Length == rateLimitState.Length)
                            {
                                int cooldown = 0;
                                for (int i = 0; i < rateLimit.Length; i++)
                                {
                                    _ = int.TryParse(rateLimit[i].Split(':')[0], NumberStyles.Any, CultureInfo.InvariantCulture, out int rLimit);
                                    _ = int.TryParse(rateLimitState[i].Split(':')[0], NumberStyles.Any, CultureInfo.InvariantCulture, out int rState);
                                    if (rLimit > 0 && rState >= rLimit) // put (rState+1) if timeouts detected
                                    {
                                        _ = int.TryParse(rateLimit[i].Split(':')[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int cdLimit);
                                        if (cdLimit > cooldown) cooldown = cdLimit;
                                    }
                                }
                                // Think : multiple couldowns can still be applied. 
                                if (isTradeSearch) cdSearch = cooldown;
                                if (isTradeFetch) cdFetch = cooldown;
                                if (isTradeBulk) cdBulk = cooldown;
                            }
                        }

                        if (response.Headers.TryGetValues(Strings.Net.RetryAfter, out IEnumerable<string> retry))
                        {
                            _ = int.TryParse(retry.First(), NumberStyles.Any, CultureInfo.InvariantCulture, out retrySeconds); // Time to wait (in seconds) until the rate limit expires.
                        }
                    }
                }
            }
        }
        StringBuilder sbReturn = new();
        sbReturn.Append(retrySeconds).Append(':').Append(cdSearch).Append(':').Append(cdFetch).Append(':').Append(cdBulk);
        return sbReturn.ToString();
    }
}

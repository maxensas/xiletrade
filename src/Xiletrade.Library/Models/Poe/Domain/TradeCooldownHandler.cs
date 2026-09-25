using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Timers;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Currently disabled
/// </summary>
public sealed class TradeCooldownHandler
{
    private readonly UIService _ui;

    private readonly Timer _cooldownTimer = new(1000);
    private readonly Action _cooldownAction;

    private int _timerValue = 0;
    private int _search = 0;
    private int _bulk = 0;
    private int _fetch = 0;

    internal bool IsEnabled => _cooldownTimer.Enabled;

    public TradeCooldownHandler(UIService ui)
    {
        _ui = ui;

        _cooldownAction = new(() =>
        {
            DecrementCooldown();

            if (_timerValue > 0)
            {
                //_vm.Form.RateText = Resources.Resources.Main184_rateLimit + " " + TimerValue + "s";
                _timerValue--;
                return;
            }
            //_vm.Form.RateText = string.Empty;
            //_vm.Form.Freeze = false;
            _cooldownTimer.Stop();
        });

        _cooldownTimer.Elapsed += Cooldown_Tick;
    }

    private void UpdateCooldown(int[] timeouts)
    {
        if (timeouts.Length is not 4)
        {
            return;
        }
        var searchCd = timeouts[1];
        if (searchCd >= 0) _search = searchCd;
        var fetchCd = timeouts[2];
        if (fetchCd >= 0) _fetch = fetchCd;
        var bulkCd = timeouts[3];
        if (bulkCd >= 0) _bulk = bulkCd;
    }

    internal void ApplyCooldown()
    {
        int cooldown = GetMaxCooldown();
        if (cooldown > _timerValue)
        {
            if (_cooldownTimer.Enabled)
            {
                _cooldownTimer.Stop();
            }

            _timerValue = cooldown;
            _cooldownTimer.Start();
            //_vm.Form.Freeze = true;

            //System.Threading.Thread.Sleep(1000 * (cooldown + 1));
        }
    }

    private void Cooldown_Tick(object sender, EventArgs e)
    {
        _ui.DelegateActionToUiThread(_cooldownAction);
    }

    private int GetMaxCooldown()
    {
        int[] allCooldowns = [_fetch, _bulk, _search];
        return allCooldowns.Max();
    }

    private void DecrementCooldown()
    {
        if (_fetch > 0) --_fetch;
        if (_bulk > 0) --_bulk;
        if (_search > 0) --_search;
        System.Diagnostics.Debug.WriteLine("COOLDOWN - Timer:" + _timerValue
            + " Fetch:" + _fetch + " Bulk:" + _bulk + " Search:" + _search);
    }

    private static int StringToInt(string timeout)
    {
        bool ok = int.TryParse(timeout, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out int cd);
        return ok ? cd : -1;
    }

    // Handling HttpResponse

    internal void HandleTradeRateLimit(HttpResponseMessage response)
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
                UpdateCooldown(timeout);
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

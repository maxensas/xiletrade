using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Headers;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Rate limit state of an api policy, read from the headers of a response.
/// </summary>
/// <remarks>
/// <c>X-Rate-Limit-Rules</c> lists the rules (ip, account, client...). For each rule,
/// <c>X-Rate-Limit-{rule}</c> gives <c>limit:period:penalty</c> and <c>X-Rate-Limit-{rule}-State</c> gives
/// <c>hits:period:restriction</c>. A header can hold several comma-separated items.
/// </remarks>
public sealed class RateLimitInfo
{
    private const string RulesHeader = "X-Rate-Limit-Rules";
    private const string RulePrefix = "X-Rate-Limit-";
    private const string StateSuffix = "-State";

    /// <summary>Name of the policy (trade-fetch-request-limit...).</summary>
    public string Policy { get; }

    /// <summary>Requests still allowed in the most constrained item.</summary>
    public int Remaining { get; }

    /// <summary>Active restriction, in seconds (0 when none).</summary>
    public int Restriction { get; }

    /// <summary>Seconds to wait before the next request, from the <c>Retry-After</c> header (0 when absent).</summary>
    public int RetryAfter { get; }

    /// <summary>Moment the response was received (<see cref="Environment.TickCount64"/>).</summary>
    public long ReceivedAt { get; }

    /// <summary>
    /// Requests that can be sent right now.
    /// </summary>
    public int Spare => Restriction > 0 ? 0 : Remaining;

    internal RateLimitInfo(string policy, int remaining, int restriction, int retryAfter = 0)
    {
        RetryAfter = retryAfter;
        Policy = policy;
        Remaining = remaining;
        Restriction = restriction;
        ReceivedAt = Environment.TickCount64;
    }

    /// <summary>True if the response was received less than <paramref name="maxAgeMs"/> milliseconds ago.</summary>
    public bool IsFresh(int maxAgeMs) => Environment.TickCount64 - ReceivedAt <= maxAgeMs;

    /// <summary>True for the trade policies which are tracked (search, fetch, exchange).</summary>
    internal static bool IsTradePolicy(string policy) => ResolvePolicy(policy) is not null;

    /// <summary>
    /// Reads the rate limit headers of a response.
    /// </summary>
    /// <returns>Null when the response has no usable rate limit headers (or is not a tracked trade policy).</returns>
    internal static RateLimitInfo Read(HttpResponseHeaders headers)
    {
        string policyValue = null, rules = null, retry = null;
        foreach (var header in headers.NonValidated)
        {
            var key = header.Key.AsSpan();
            if (key.SequenceEqual(Strings.Net.XrateLimitPolicy))
            {
                policyValue = header.Value.ToString();
            }
            else if (key.SequenceEqual(RulesHeader))
            {
                rules = header.Value.ToString();
            }
            else if (key.SequenceEqual(Strings.Net.RetryAfter))
            {
                retry = header.Value.ToString();
            }
        }

        var policy = ResolvePolicy(policyValue);
        if (policy is null || string.IsNullOrEmpty(rules))
        {
            return null;
        }

        int remaining = int.MaxValue, restriction = 0;
        var remainingRules = rules.AsSpan();
        while (NextItem(ref remainingRules, out var rule))
        {
            if (rule.IsEmpty || !TryGetRuleHeaders(headers, rule, out var limit, out var state))
            {
                continue;
            }
            if (!TryReadRule(limit, state, out int ruleRemaining, out int ruleRestriction))
            {
                return null;
            }
            remaining = Math.Min(remaining, ruleRemaining);
            restriction = Math.Max(restriction, ruleRestriction);
        }

        int retryAfter = 0;
        if (!string.IsNullOrEmpty(retry))
        {
            _ = int.TryParse(retry, NumberStyles.Integer, CultureInfo.InvariantCulture, out retryAfter);
        }
        return remaining is int.MaxValue ? null : new RateLimitInfo(policy, Math.Max(remaining, 0), restriction, Math.Max(retryAfter, 0));
    }

    // finds the "X-Rate-Limit-{rule}" and "X-Rate-Limit-{rule}-State" headers without building their names
    private static bool TryGetRuleHeaders(HttpResponseHeaders headers, ReadOnlySpan<char> rule, out string limit, out string state)
    {
        limit = null;
        state = null;
        foreach (var header in headers.NonValidated)
        {
            var key = header.Key.AsSpan();
            if (!key.StartsWith(RulePrefix))
            {
                continue;
            }
            var name = key[RulePrefix.Length..];
            if (name.SequenceEqual(rule))
            {
                limit = header.Value.ToString();
            }
            else if (name.EndsWith(StateSuffix)
                && name[..^StateSuffix.Length].SequenceEqual(rule))
            {
                state = header.Value.ToString();
            }
        }
        return !string.IsNullOrEmpty(limit) && !string.IsNullOrEmpty(state);
    }

    // returns the constant matching the policy (no allocation), or null if the policy is not tracked
    private static string ResolvePolicy(ReadOnlySpan<char> policy)
    {
        if (policy.SequenceEqual(Strings.Net.TradeSearchRequestLimit)) return Strings.Net.TradeSearchRequestLimit;
        if (policy.SequenceEqual(Strings.Net.TradeFetchRequestLimit)) return Strings.Net.TradeFetchRequestLimit;
        if (policy.SequenceEqual(Strings.Net.TradeExchangeRequestLimit)) return Strings.Net.TradeExchangeRequestLimit;
        return null;
    }

    /// <summary>
    /// Builds the state of a policy from the values of its rules.
    /// </summary>
    /// <param name="policy">Name of the policy.</param>
    /// <param name="rules">For each rule : the value of its limit header and of its state header.</param>
    /// <param name="retryAfter">Value of the <c>Retry-After</c> header, in seconds.</param>
    /// <returns>Null when a value can not be read (the state is then unknown).</returns>
    internal static RateLimitInfo Create(string policy, IEnumerable<(string Limit, string State)> rules, int retryAfter = 0)
    {
        int remaining = int.MaxValue, restriction = 0;
        foreach (var (limit, state) in rules)
        {
            if (!TryReadRule(limit, state, out int ruleRemaining, out int ruleRestriction))
            {
                return null;
            }
            remaining = Math.Min(remaining, ruleRemaining);
            restriction = Math.Max(restriction, ruleRestriction);
        }
        return remaining is int.MaxValue ? null : new RateLimitInfo(policy, Math.Max(remaining, 0), restriction, retryAfter);
    }

    // limit : "max:period:penalty[,...]" / state : "hits:period:restriction[,...]" (same number of items)
    // gives the most constrained item of the rule
    private static bool TryReadRule(ReadOnlySpan<char> limit, ReadOnlySpan<char> state, out int remaining, out int restriction)
    {
        remaining = int.MaxValue;
        restriction = 0;
        if (limit.IsEmpty || state.IsEmpty || limit.Count(',') != state.Count(','))
        {
            return false;
        }

        while (NextItem(ref limit, out var limitItem) && NextItem(ref state, out var stateItem))
        {
            if (!TryParseItem(limitItem, out int max, out _)
                || !TryParseItem(stateItem, out int hits, out int active))
            {
                return false;
            }
            remaining = Math.Min(remaining, max - hits);
            restriction = Math.Max(restriction, active);
        }
        return true;
    }

    // takes the next comma-separated item (trimmed) out of the text
    private static bool NextItem(ref ReadOnlySpan<char> text, out ReadOnlySpan<char> item)
    {
        if (text.IsEmpty)
        {
            item = default;
            return false;
        }
        int comma = text.IndexOf(',');
        if (comma < 0)
        {
            item = text.Trim();
            text = default;
        }
        else
        {
            item = text[..comma].Trim();
            text = text[(comma + 1)..];
        }
        return true;
    }

    // "a:b:c" => a and c (limit and penalty, or hits and active restriction)
    private static bool TryParseItem(ReadOnlySpan<char> text, out int first, out int third)
    {
        first = 0;
        third = 0;
        int c1 = text.IndexOf(':');
        if (c1 < 0)
        {
            return false;
        }
        var rest = text[(c1 + 1)..];
        int c2 = rest.IndexOf(':');
        if (c2 < 0)
        {
            return false;
        }
        var last = rest[(c2 + 1)..];
        return last.IndexOf(':') < 0
            && int.TryParse(text[..c1], NumberStyles.Integer, CultureInfo.InvariantCulture, out first)
            && int.TryParse(last, NumberStyles.Integer, CultureInfo.InvariantCulture, out third);
    }
}

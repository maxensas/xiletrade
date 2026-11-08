using System;
using System.Collections.Generic;
using System.Threading;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Requests of a policy of the trade api, compared to its limits : how long before more requests can be sent
/// while each window of each rule (ip, account...) stays under a part of its maximum.
/// </summary>
/// <remarks>
/// The state of the server is only known with a response : the requests of Xiletrade are counted when they are sent
/// (the ones waiting for their response included) and leave each window at the end of its period.
/// A response also gives, for each window, the hits the server counts in excess (requests of other clients on the same
/// ip or account, of a previous launch...) : their moments are unknown, they are taken as spread over the window and
/// leave it evenly during its period. They only count in their own window.
/// A restriction or a Retry-After blocks until its end. Thread safe.
/// </remarks>
internal sealed class RateLimitTracker
{
    // windows unknown (no response yet) : longest period of the trade api, the moments are kept during it
    private const int DefaultLongestPeriodMs = 10_800_000;

    private readonly Lock _gate = new();
    private readonly Func<long> _clock;

    // moments of the requests sent (ms), oldest first
    private readonly List<long> _sent = [];
    private RateLimitRule[] _rules = [];
    // hits counted in excess by the server in each window of the rules, when the last response was received
    private int[] _others = [];
    private long _othersAt;
    private long _longestPeriodMs = DefaultLongestPeriodMs;
    private long _blockedUntil;

    internal RateLimitTracker() : this(static () => Environment.TickCount64)
    {
    }

    /// <param name="clock">Current moment in milliseconds.</param>
    internal RateLimitTracker(Func<long> clock)
    {
        _clock = clock;
    }

    /// <summary>A request of the policy is sent.</summary>
    internal void OnSent()
    {
        lock (_gate)
        {
            var now = _clock();
            Trim(now);
            _sent.Add(now);
        }
    }

    /// <summary>State of the policy given by a response : windows, hits counted by the server, restriction.</summary>
    internal void OnResponse(RateLimitInfo info)
    {
        lock (_gate)
        {
            var now = _clock();
            var penalty = Math.Max(info.Restriction, info.RetryAfter);
            _blockedUntil = penalty > 0 ? now + penalty * 1000L : 0;
            if (info.Rules.Length is 0)
            {
                return;
            }

            _rules = info.Rules;
            _others = new int[_rules.Length];
            _othersAt = now;
            long longest = 0;
            for (int i = 0; i < _rules.Length; i++)
            {
                longest = Math.Max(longest, _rules[i].Period * 1000L);
            }
            _longestPeriodMs = longest;
            Trim(now);
            for (int i = 0; i < _rules.Length; i++)
            {
                var rule = _rules[i];
                _others[i] = Math.Max(0, rule.Hits - CountSince(now - rule.Period * 1000L));
            }
        }
    }

    /// <summary>Time to wait before <paramref name="count"/> requests can be sent.</summary>
    /// <param name="maxUsage">Part of the maximum of each window which can be used, from 0 to 1.</param>
    /// <returns>Zero : they can be sent now (windows unknown included).</returns>
    internal TimeSpan GetWait(int count, double maxUsage)
    {
        lock (_gate)
        {
            var now = _clock();
            if (now < _blockedUntil)
            {
                return TimeSpan.FromMilliseconds(_blockedUntil - now);
            }
            Trim(now);

            long wait = 0;
            for (int i = 0; i < _rules.Length; i++)
            {
                wait = Math.Max(wait, GetWait(i, now, count, maxUsage));
            }
            return TimeSpan.FromMilliseconds(wait);
        }
    }

    // first moment from which the window of a rule has room for the requests (under the gate)
    private long GetWait(int index, long now, int count, double maxUsage)
    {
        var rule = _rules[index];
        var periodMs = rule.Period * 1000L;
        var free = (int)(rule.Max * maxUsage + 1e-9) - count; // 10 * 0.6 is 6, not 5
        if (free < 0)
        {
            return periodMs; // more requests than the window allows
        }

        // requests of Xiletrade : the oldest ones have to leave the window
        var first = IndexSince(now - periodMs);
        var over = _sent.Count - first - free;
        var at = over > 0 ? _sent[first + over - 1] + periodMs : now;

        // hits of the others : leaving evenly until the end of the period which followed the response
        var others = _others[index];
        if (others > 0)
        {
            var mine = _sent.Count - IndexSince(at - periodMs);
            var room = free - mine;
            var end = _othersAt + periodMs;
            var leftAt = room <= 0 ? end : end - (long)((double)room / others * periodMs);
            at = Math.Max(at, Math.Min(leftAt, end));
        }
        return Math.Max(0, at - now);
    }

    // the moments out of all the windows are removed (under the gate)
    private void Trim(long now)
    {
        var index = IndexSince(now - _longestPeriodMs);
        if (index > 0)
        {
            _sent.RemoveRange(0, index);
        }
    }

    // first moment after the start of a window (under the gate)
    private int IndexSince(long start)
    {
        var index = _sent.Count;
        while (index > 0 && _sent[index - 1] > start)
        {
            index--;
        }
        return index;
    }

    private int CountSince(long start) => _sent.Count - IndexSince(start);
}

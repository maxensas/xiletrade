using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

internal static class FilterResultExtensions
{
    internal static FilterResultEntrie FindEntry(this FilterResult filterResult,
        ReadOnlySpan<char> id, ReadOnlySpan<char> type)
    {
        var entries = filterResult.Entries;
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.ID.AsSpan().SequenceEqual(id) &&
                entry.Type.AsSpan().SequenceEqual(type))
            {
                return entry;
            }
        }

        return null;
    }

    internal static FilterResultEntrie FindEntry(this FilterResult filterResult,
        ReadOnlySpan<char> id)
    {
        var entries = filterResult.Entries;
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.ID.AsSpan().SequenceEqual(id))
            {
                return entry;
            }
        }

        return null;
    }

    internal static FilterResultEntrie FindModEntry(this FilterResult filterResult
        , ReadOnlySpan<char> mod, bool sequenceEquality = true)
    {
        var entries = filterResult.Entries;
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (sequenceEquality ? entry.Text.AsSpan().SequenceEqual(mod)
                : entry.Text.AsSpan().Contain(mod))
            {
                return entry;
            }
        }

        return null;
    }

    internal static List<FilterResultEntrie> MatchEntries(this FilterResult filterResult, Regex regex)
    {
        var result = new List<FilterResultEntrie>();

        foreach (var entry in filterResult.Entries)
        {
            if (regex.IsMatch(entry.Text))
            {
                result.Add(entry);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all entries whose text begins with one of the prefixes followed by a line break.
    /// </summary>
    internal static List<FilterResultEntrie> WhereStartsWith(this FilterResult filterResult,
        string[] prefixes)
    {
        var list = new List<FilterResultEntrie>();

        foreach (var entry in filterResult.Entries)
        {
            foreach (var prefix in prefixes)
            {
                if (entry.Text.StartWith(prefix + Strings.LF))
                {
                    list.Add(entry);
                    break;
                }
            }
        }

        return list;
    }
}



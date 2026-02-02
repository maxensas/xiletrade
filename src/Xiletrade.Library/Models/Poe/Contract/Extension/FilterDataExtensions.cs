using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

// without factoring loops
internal static class FilterDataExtensions
{
    internal static IEnumerable<FilterResultEntrie> EnumerateEntries(this FilterData filter)
    {
        var results = filter.Result;

        for (int i = 0, n = results.Length; i < n; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0, m = entries.Length; j < m; j++)
            {
                yield return entries[j];
            }
        }
    }

    internal static FilterResultEntrie GetFilterDataEntry(this FilterData filter, ReadOnlySpan<char> id, bool sequenceEquality = true, bool checkText = false)
    {
        var results = filter.Result;

        for (int i = 0; i < results.Length; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0; j < entries.Length; j++)
            {
                var entry = entries[j];
                if (sequenceEquality ? entry.ID.AsSpan().SequenceEqual(id)
                    : entry.ID.AsSpan().Contain(id))
                {
                    if (checkText && string.IsNullOrWhiteSpace(entry.Text))
                    {
                        continue;
                    }
                    return entry;
                }
            }
        }

        return null;
    }

    internal static bool ContainModifier(this FilterData filter, ReadOnlySpan<char> modText, bool sequenceEquality = true)
    {
        var results = filter.Result;

        for (int i = 0; i < results.Length; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0; j < entries.Length; j++)
            {
                if (sequenceEquality ? entries[j].Text.AsSpan().SequenceEqual(modText)
                    : entries[j].Text.AsSpan().Contain(modText))
                {
                    return true;
                }
            }
        }
        return false;
    }

    internal static List<FilterResultEntrie> GetEntryStartsWith(this FilterData filterData, ReadOnlySpan<char> idPrefix)
    {
        var resultList = new List<FilterResultEntrie>();

        var results = filterData.Result;
        for (int i = 0; i < results.Length; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0; j < entries.Length; j++)
            {
                var entry = entries[j];
                if (entry.ID.AsSpan().StartWith(idPrefix))
                {
                    resultList.Add(entry);
                }
            }
        }

        return resultList;
    }

    internal static FilterResult GetFilterResultWithLabel(this FilterData filterData, ReadOnlySpan<char> labelName)
    {
        var results = filterData.Result;
        for (int i = 0; i < results.Length; i++)
        {
            var result = results[i];
            if (result.Label.AsSpan().SequenceEqual(labelName))
            {
                return result;
            }
        }

        return null;
    }

    internal static FilterResultEntrie FindPseudoEntryContainingMod(this FilterData filter, ReadOnlySpan<char> mod)
    {
        var entries = filter.Result[0].Entries; // pseudo filters

        for (int i = 0, n = entries.Length; i < n; i++)
        {
            var entry = entries[i];
            if (entry.Text.AsSpan().Contain(mod))
                return entry;
        }

        return null;
    }

    /// <summary>
    /// Replace Filter Text containing [..|..] strings.
    /// </summary>
    /// <param name="filter"></param>
    /// /// <param name="gameVersion"></param>
    /// <returns></returns>
    internal static FilterData ArrangeFilter(this FilterData filter, int gameVersion)
    {
        if (gameVersion is 0)
        {
            return filter;
        }

        var results = filter.Result;

        for (int i = 0; i < results.Length; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0; j < entries.Length; j++)
            {
                var entry = entries[j];
                entry.Text = entry.Text.ParseBracketMod();
            }
        }

        return filter;
    }
}

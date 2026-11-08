using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

// without factoring loops
internal static class FilterDataExtensions
{
    // exact texts of the entries, indexed once per filter data (entries are not modified once loaded, see ArrangeFilter)
    private static readonly ConditionalWeakTable<FilterData, EntryTextIndex> _textIndexes = new();

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

    internal static IEnumerable<FilterResultEntrie> EnumerateEntries(this FilterData filter, string searchField)
    {
        var results = filter.Result;

        for (int i = 0, n = results.Length; i < n; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0, m = entries.Length; j < m; j++)
            {
                if (entries[j].Text != null && entries[j].Text.Contain(searchField))
                    yield return entries[j];
            }
        }
    }

    internal static IEnumerable<string> EnumerateTextEntries(this FilterData filter)
    {
        var results = filter.Result;

        for (int i = 0, n = results.Length; i < n; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0, m = entries.Length; j < m; j++)
            {
                yield return entries[j].Text;
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
        if (sequenceEquality)
        {
            // hash lookup instead of a scan of all the entries
            return _textIndexes.GetValue(filter, static f => new EntryTextIndex(f)).Contains(modText);
        }

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
    /// Ordinal set of the entry texts : same result as SequenceEqual on each entry text (a null text is compared as empty).
    /// </summary>
    private sealed class EntryTextIndex
    {
        private readonly HashSet<string> _texts = new(StringComparer.Ordinal);
        private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _lookup;
        private readonly bool _useLookup;

        internal EntryTextIndex(FilterData filter)
        {
            foreach (var result in filter.Result ?? [])
            {
                foreach (var entry in result.Entries ?? [])
                {
                    _texts.Add(entry.Text ?? string.Empty);
                }
            }
            _useLookup = _texts.TryGetAlternateLookup(out _lookup);
        }

        // span lookup without allocation (supported by StringComparer.Ordinal), string lookup otherwise
        internal bool Contains(ReadOnlySpan<char> text) 
            => _useLookup ? _lookup.Contains(text) : _texts.Contains(text.ToString());
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

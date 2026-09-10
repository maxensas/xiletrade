using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

internal static class FilterResultExtensions
{
    internal static FilterResultEntrie FindEntryByIdAndType(this FilterResult filter,
        ReadOnlySpan<char> id, ReadOnlySpan<char> type)
    {
        var entries = filter.Entries;
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

    internal static FilterResultEntrie FindEntryById(this FilterResult filter,
        ReadOnlySpan<char> id, bool sequenceEquality = true)
    {
        var entries = filter.Entries;
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (sequenceEquality ? entry.ID.AsSpan().SequenceEqual(id)
                : entry.ID.AsSpan().Contain(id))
            {
                return entry;
            }
        }

        return null;
    }

    internal static FilterResultEntrie FindEntryByType(this FilterResult filter
        , ReadOnlySpan<char> mod, bool sequenceEquality = true)
    {
        var entries = filter.Entries;
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

    internal static List<FilterResultEntrie> GetMatchEntriesList(this FilterResult filter, Regex regex)
    {
        var result = new List<FilterResultEntrie>();

        foreach (var entry in filter.Entries)
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
    internal static List<FilterResultEntrie> GetWhereStartsWithList(this FilterResult filter, string[] prefixes)
    {
        var list = new List<FilterResultEntrie>();

        foreach (var entry in filter.Entries)
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

    internal static List<FilterResultEntrie> FindEntries(this FilterResult filter,
        ItemModifier mod, ItemData item, Regex inputRegex)
    {
        // a) simple match with the regex
        var entries = filter.GetMatchEntriesList(inputRegex);
        if (entries.Count > 0)
            return entries;

        // b) multi-line mod (first line)
        var inputSplit = mod.Parsed.Split("\\n");
        if (inputSplit.Length >= 2)
        {
            var inputRgx = new Regex("^" + inputSplit[0] + "$", RegexOptions.IgnoreCase);
            entries = filter.GetMatchEntriesList(inputRgx);
            if (entries.Count > 0)
                return entries;
        }

        // c) ConfluxEntry
        if ((item.Flag.Rarity.Rare || item.Flag.Rarity.Magic) &&
            filter.Label == Resources.Resources.General015_Explicit)
        {
            var confluxEntrie = filter.GetConfluxEntrie(mod);
            if (confluxEntrie is not null)
                return new List<FilterResultEntrie> { confluxEntrie };
        }

        // d) Full Multi-line
        if (item.Flag.Rarity.Unique || item.Flag.Rarity.Magic || item.Flag.Map.IsMap)
        {
            entries = filter.GetMultiLineEntrieList(mod);
        }

        return entries ?? new List<FilterResultEntrie>();
    }

    // private
    private static FilterResultEntrie GetConfluxEntrie(this FilterResult filter, ItemModifier mod)
    {
        var ConfluxEntrie = filter.FindEntryById(Strings.Stat.Conflux);
        if (ConfluxEntrie is not null)
        {
            foreach (var opt in ConfluxEntrie.Option.Options)
            {
                if (ConfluxEntrie.Text.Replace("#", opt.Text) == mod.Parsed)
                {
                    return ConfluxEntrie;
                }
            }
        }
        return null;
    }

    private static List<FilterResultEntrie> GetMultiLineEntrieList(this FilterResult filter, ItemModifier mod)
    {
        string modReg = RegexUtil.DecimalPattern().Replace(mod.Parsed, "#");
        var entries = filter.GetWhereStartsWithList([mod.Parsed, modReg]);
        if (entries.Count > 1 && mod.NextModInfo.ModKind.Length > 0)
        {
            var filtered = new List<FilterResultEntrie>();
            foreach (var entry in entries)
            {
                if (entry.Text.Contains(mod.NextModInfo.ModKind))
                {
                    filtered.Add(entry);
                }
            }
            if (filtered.Count > 0)
            {
                entries = filtered;
            }
        }
        return entries;
    }
}



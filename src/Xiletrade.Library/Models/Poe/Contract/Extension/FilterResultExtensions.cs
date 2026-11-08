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

    /// <summary>
    /// Returns all entries matching the modifier (see <see cref="IsModMatch"/>), null if none : no allocation without result.
    /// </summary>
    internal static List<FilterResultEntrie> GetModMatchEntries(this FilterResult filter, ReadOnlySpan<char> mod)
    {
        List<FilterResultEntrie> result = null;
        var entries = filter.Entries;
        for (int i = 0; i < entries.Length; i++)
        {
            if (IsModMatch(mod, entries[i].Text))
            {
                (result ??= new()).Add(entries[i]);
            }
        }
        return result;
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

    /// <returns>Entries found, null or empty if none.</returns>
    internal static List<FilterResultEntrie> FindEntries(this FilterResult filter,
        ItemModifier mod, ItemData item)
    {
        // a) simple match
        var entries = filter.GetModMatchEntries(mod.Parsed);
        if (entries is not null)
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

        return entries;
    }

    // private
    /// <summary>
    /// Same result as the regex : "^" + Escape(mod with numbers replaced by '#') + "$" (ignore case),
    /// each '#' being replaced by "[+-]?([0-9]+\.[0-9]+|[0-9]+|\#)", without any allocation.
    /// </summary>
    /// <param name="mod">Parsed modifier : its numbers and '#' match a signed number or '#' in text.</param>
    /// <param name="text">Filter entry text.</param>
    internal static bool IsModMatch(ReadOnlySpan<char> mod, ReadOnlySpan<char> text)
    {
        while (true)
        {
            int tokenLength = GetNumberTokenLength(mod);
            if (tokenLength > 0)
            {
                mod = mod[tokenLength..];
                // [+-]? : with the sign, then without (regex backtracking)
                if (text.Length > 0 && text[0] is '+' or '-' && IsNumberMatch(mod, text[1..]))
                {
                    return true;
                }
                return IsNumberMatch(mod, text);
            }
            if (mod.IsEmpty)
            {
                return text.IsEmpty || text is ['\n']; // '$' also matches before a final line feed
            }
            if (text.IsEmpty || !EqualsIgnoreCase(mod[0], text[0]))
            {
                return false;
            }
            mod = mod[1..];
            text = text[1..];
        }
    }

    // ([0-9]+\.[0-9]+|[0-9]+|\#) then the rest of the modifier
    private static bool IsNumberMatch(ReadOnlySpan<char> mod, ReadOnlySpan<char> text)
    {
        if (text.Length > 0 && text[0] is '#')
        {
            return IsModMatch(mod, text[1..]);
        }
        int intLength = CountDigits(text);
        if (intLength is 0)
        {
            return false;
        }
        if (intLength < text.Length && text[intLength] is '.')
        {
            for (int length = CountDigits(text[(intLength + 1)..]); length > 0; length--)
            {
                if (IsModMatch(mod, text[(intLength + 1 + length)..]))
                {
                    return true;
                }
            }
        }
        for (int length = intLength; length > 0; length--)
        {
            if (IsModMatch(mod, text[length..]))
            {
                return true;
            }
        }
        return false;
    }

    // number at the start of the modifier (same as RegexUtil.DecimalPattern) or '#', 0 if none
    private static int GetNumberTokenLength(ReadOnlySpan<char> mod)
    {
        if (mod.IsEmpty)
        {
            return 0;
        }
        if (mod[0] is '#')
        {
            return 1;
        }
        int sign = mod[0] is '+' or '-' ? 1 : 0;
        int intLength = CountDigits(mod[sign..]);
        if (intLength is 0)
        {
            return 0;
        }
        int length = sign + intLength;
        if (length < mod.Length && mod[length] is '.')
        {
            int decLength = CountDigits(mod[(length + 1)..]);
            if (decLength > 0)
            {
                length += 1 + decLength;
            }
        }
        return length;
    }

    private static int CountDigits(ReadOnlySpan<char> text)
    {
        int count = text.IndexOfAnyExceptInRange('0', '9');
        return count < 0 ? text.Length : count;
    }

    private static bool EqualsIgnoreCase(char a, char b)
        => a == b || char.ToLowerInvariant(a) == char.ToLowerInvariant(b);

    private static FilterResultEntrie GetConfluxEntrie(this FilterResult filter, ItemModifier mod)
    {
        var ConfluxEntrie = filter.FindEntryById(Strings.StatPoe1.Conflux);
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



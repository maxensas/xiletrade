using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Shared;

public static class Extensions
{
    public static double ToDoubleEmptyField(this string str)
    {
        return StrToDouble(str, true);
    }

    public static double ToDoubleDefault(this string str)
    {
        return StrToDouble(str, false);
    }

    /// <summary>Return true if equal to EMPTYFIELD = 99999</summary>
    public static bool IsEmpty(this double value)
    {
        return value is ModFilter.EMPTYFIELD;
    }

    /// <summary>Return true if not equal to EMPTYFIELD = 99999</summary>
    public static bool IsNotEmpty(this double value)
    {
        return value is not ModFilter.EMPTYFIELD;
    }

    /// <summary>Return true if not equal to EMPTYFIELD = 99999</summary>
    public static bool IsNotEmpty(this int value)
    {
        return value is not ModFilter.EMPTYFIELD;
    }

    private static double StrToDouble(string str, bool useEmptyfield = false)
    {
        double value = useEmptyfield ? ModFilter.EMPTYFIELD : 0;
        if (str?.Length > 0 && double.TryParse(str, CultureInfo.InvariantCulture, out double val))
        {
            value = val;
        }
        return value;
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        int pos = text.IdxOf(search);
        if (pos < 0)
        {
            return text;
        }
        return string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
    }

    public static string RemoveFirst(this string text, char search)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return string.Concat(text.AsSpan(0, pos), text.AsSpan(pos + 1));
    }

    public static string RemoveStringFromList(this string input, IEnumerable<string> list)
    {
        foreach (var txt in list)
        {
            if (input.Contain(txt))
            {
                input = input.Replace(txt, string.Empty).Trim();
                break;
            }
        }
        return input;
    }

    public static string RemoveStringFromArrayDesc(this string input, string[] array)
        => input.RemoveStringFromList(array.OrderByDescending(m => m.Length));

    /// <summary>
    /// Replace Filter Text containing [..|..] strings.
    /// </summary>
    /// <param name="filter"></param>
    /// /// <param name="gameVersion"></param>
    /// <returns></returns>
    public static FilterData ArrangeFilter(this FilterData filter, int gameVersion)
    {
        if (gameVersion is 0)
        {
            return filter;
        }
        foreach (var result in filter.Result)
        {
            // Temporary fix until GGG's update
            if (result.Label.Length is 0 && result.Entries[0] is not null 
                && result.Entries[0].ID.StartWith("rune"))
            {
                result.Label = "Rune";
            }
            foreach (var entrie in result.Entries)
            {
                entrie.Text = ParseBracketMod(entrie.Text);
            }
        }
        return filter;
    }

    /// <summary>
    /// Parse item info desc using POE2 chat links
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <returns></returns>
    public static string ArrangeItemInfoDesc(this string itemInfo) => ParseBracketMod(itemInfo);

    public static bool Contain(this string source, string toCheck) => source.Contains(toCheck, StringComparison.Ordinal);

    public static bool Contain(this string source, char toCheck) => source.Contains(toCheck, StringComparison.Ordinal);

    public static bool Contain(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.Contains(toCheck, StringComparison.Ordinal);

    public static bool StartWith(this string source, string toCheck) => source.StartsWith(toCheck, StringComparison.Ordinal);

    public static bool StartWith(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.StartsWith(toCheck, StringComparison.Ordinal);

    public static bool EndWith(this string source, string toCheck) => source.EndsWith(toCheck, StringComparison.Ordinal);

    public static bool EndWith(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.EndsWith(toCheck, StringComparison.Ordinal);

    public static bool Equal(this string source, string toCheck) => source.Equals(toCheck, StringComparison.Ordinal);

    public static int LastIdxOf(this string source, string toCheck) => source.LastIndexOf(toCheck, StringComparison.Ordinal);

    public static int LastIdxOf(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.LastIndexOf(toCheck, StringComparison.Ordinal);

    public static int IdxOf(this string source, string toCheck) => source.IndexOf(toCheck, StringComparison.Ordinal);

    public static int IdxOf(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.IndexOf(toCheck, StringComparison.Ordinal);

    public static MinMaxModel GetModel(this IEnumerable<MinMaxModel> list, StatPanel stat)
    {
        return stat switch
        {
            StatPanel.CommonItemLevel => list.First(x => x.Id is StatPanel.CommonItemLevel),
            StatPanel.CommonQuality => list.First(x => x.Id is StatPanel.CommonQuality),
            StatPanel.CommonLink => list.First(x => x.Id is StatPanel.CommonLink),
            StatPanel.CommonSocket => list.First(x => x.Id is StatPanel.CommonSocket),
            StatPanel.CommonSocketRune => list.First(x => x.Id is StatPanel.CommonSocketRune),
            StatPanel.CommonSocketGem => list.First(x => x.Id is StatPanel.CommonSocketGem),
            StatPanel.CommonRequiresLevel => list.First(x => x.Id is StatPanel.CommonRequiresLevel),
            StatPanel.CommonMemoryStrand => list.First(x => x.Id is StatPanel.CommonMemoryStrand),
            StatPanel.DamagePhysical => list.First(x => x.Id is StatPanel.DamagePhysical),
            StatPanel.DamageElemental => list.First(x => x.Id is StatPanel.DamageElemental),
            StatPanel.DamageTotal => list.First(x => x.Id is StatPanel.DamageTotal),
            StatPanel.DefenseArmour => list.First(x => x.Id is StatPanel.DefenseArmour),
            StatPanel.DefenseEnergy => list.First(x => x.Id is StatPanel.DefenseEnergy),
            StatPanel.DefenseEvasion => list.First(x => x.Id is StatPanel.DefenseEvasion),
            StatPanel.DefenseWard => list.First(x => x.Id is StatPanel.DefenseWard),
            StatPanel.TotalLife => list.First(x => x.Id is StatPanel.TotalLife),
            StatPanel.TotalResistance => list.First(x => x.Id is StatPanel.TotalResistance),
            StatPanel.TotalGlobalEs => list.First(x => x.Id is StatPanel.TotalGlobalEs),
            StatPanel.MapMoreCurrency => list.First(x => x.Id is StatPanel.MapMoreCurrency),
            StatPanel.MapMoreDivCard => list.First(x => x.Id is StatPanel.MapMoreDivCard),
            StatPanel.MapMoreScarab => list.First(x => x.Id is StatPanel.MapMoreScarab),
            StatPanel.MapPackSize => list.First(x => x.Id is StatPanel.MapPackSize),
            StatPanel.MapQuantity => list.First(x => x.Id is StatPanel.MapQuantity),
            StatPanel.MapRarity => list.First(x => x.Id is StatPanel.MapRarity),
            StatPanel.MapMoreMap => list.First(x => x.Id is StatPanel.MapMoreMap),
            StatPanel.MapMonsterRare => list.First(x => x.Id is StatPanel.MapMonsterRare),
            StatPanel.MapMonsterMagic => list.First(x => x.Id is StatPanel.MapMonsterMagic),
            StatPanel.SanctumAureus => list.First(x => x.Id is StatPanel.SanctumAureus),
            StatPanel.SanctumInspiration => list.First(x => x.Id is StatPanel.SanctumInspiration),
            StatPanel.SanctumMaxResolve => list.First(x => x.Id is StatPanel.SanctumMaxResolve),
            StatPanel.SanctumResolve => list.First(x => x.Id is StatPanel.SanctumResolve),
            _ => throw new ArgumentException("Unknown type of a StatPanel", nameof(stat))
        };
    }

    public static string FormatWithSuffix(this double value)
    {
        if (value >= 1_000_000_000)
            return ((int)(value / 1_000_000_000)).ToString() + " B";
        else if (value >= 1_000_000)
            return ((int)(value / 1_000_000)).ToString() + " M";
        else if (value >= 10_000)
            return ((int)(value / 1_000)).ToString() + " k";
        else
            return ((int)value).ToString();
    }

    private static string ParseBracketMod(string text)
    {
        var firstIdx = text.IndexOf('[');
        var secondIdx = text.IndexOf(']');
        int watchdog = 0;
        while (firstIdx >= 0 && secondIdx >= 0 && firstIdx < secondIdx)
        {
            var chunk = text.AsSpan(firstIdx + 1, secondIdx - (firstIdx + 1));
            var nestedIdx = chunk.IndexOf('|');
            if (nestedIdx is -1)
            {
                text = text.RemoveFirst('[').RemoveFirst(']');
            }
            else
            {
                var firstSub = text.AsSpan(0, firstIdx);
                var idx = text.IndexOf('|') + 1;
                var secondSub = text.AsSpan(idx, text.Length - idx);
                text = string.Concat(firstSub, secondSub).RemoveFirst(']');
            }

            firstIdx = text.IndexOf('[');
            secondIdx = text.IndexOf(']');

            watchdog++;
            if (watchdog >= 40) // Limit
            {
                break;
            }
        }
        return text;
    }
}

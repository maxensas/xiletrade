﻿using System.Globalization;
using System;
using Xiletrade.Library.Models.Serializable;

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
        return value is Modifier.EMPTYFIELD;
    }

    /// <summary>Return true if not equal to EMPTYFIELD = 99999</summary>
    public static bool IsNotEmpty(this double value)
    {
        return value is not Modifier.EMPTYFIELD;
    }

    /// <summary>Return true if not equal to EMPTYFIELD = 99999</summary>
    public static bool IsNotEmpty(this int value)
    {
        return value is not Modifier.EMPTYFIELD;
    }

    private static double StrToDouble(string str, bool useEmptyfield = false)
    {
        double value = useEmptyfield ? Modifier.EMPTYFIELD : 0;
        if (str?.Length > 0)
        {
            try
            {
                value = double.Parse(str, CultureInfo.InvariantCulture); // correction
            }
            catch (Exception)
            {
                //Helper.Debug.Trace("Exception using double parsing : " + ex.Message);
            }
        }
        return value;
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
    }

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
                && result.Entries[0].ID.StartsWith("rune", StringComparison.Ordinal))
            {
                result.Label = "Rune";
            }
            foreach (var entrie in result.Entries)
            {
                entrie.Text = ParseText(entrie.Text);
            }
        }
        return filter;
    }

    /// <summary>
    /// Parse item info desc using POE2 chat links
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <returns></returns>
    public static string ArrangeItemInfoDesc(this string itemInfo) => ParseText(itemInfo);

    //TODO: Update with Span
    private static string ParseText(string text)
    {
        var firstIdx = text.IndexOf('[');
        var secondIdx = text.IndexOf(']');
        int watchdog = 0;
        while (firstIdx >= 0 && secondIdx >= 0 && firstIdx < secondIdx)
        {
            var chunk = text.Substring(firstIdx + 1, secondIdx - (firstIdx + 1));
            var nestedIdx = chunk.IndexOf('|');
            if (nestedIdx is -1)
            {
                text = text.ReplaceFirst("[", string.Empty).ReplaceFirst("]", string.Empty);
            }
            else
            {
                var firstSub = text.Substring(0, firstIdx);
                var idx = text.IndexOf('|');
                var secondSub = text.Substring(idx + 1);
                text = (firstSub + secondSub).ReplaceFirst("]", string.Empty);
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

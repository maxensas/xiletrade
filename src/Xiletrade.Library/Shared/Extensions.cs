using System.Globalization;
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

    //TODO: Update with Span
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
            foreach (var entrie in result.Entries)
            {
                var firstIdx = entrie.Text.IndexOf('[');
                var secondIdx = entrie.Text.IndexOf(']');
                int watchdog = 0;
                while (firstIdx >= 0 && secondIdx >= 0 && firstIdx < secondIdx)
                {
                    var chunk = entrie.Text.Substring(firstIdx + 1, secondIdx - (firstIdx + 1));
                    var nestedIdx = chunk.IndexOf('|');
                    if (nestedIdx is -1)
                    {
                        entrie.Text = entrie.Text.ReplaceFirst("[", string.Empty).ReplaceFirst("]", string.Empty);
                    }
                    else
                    {
                        var firstSub = entrie.Text.Substring(0, firstIdx);
                        var idx = entrie.Text.IndexOf('|');
                        var secondSub = entrie.Text.Substring(idx + 1);
                        entrie.Text = (firstSub + secondSub).ReplaceFirst("]", string.Empty);
                    }

                    firstIdx = entrie.Text.IndexOf('[');
                    secondIdx = entrie.Text.IndexOf(']');

                    watchdog++;
                    if (watchdog >= 30) // Limit
                    {
                        break;
                    }
                }
            }
        }
        return filter;
    }
}

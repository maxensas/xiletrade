using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Shared;

public static class Extensions
{
    private const string STRING_FORMAT = "G";

    public static double ToDoubleEmptyField(this string str)
    {
        return StrToDouble(str, true);
    }

    /// <summary>Return double value if string can be parsed or 0.</summary>
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

    /// <summary>Convert double to string using 'G' format and 'InvariantCulture' provider</summary>
    public static string ToStr(this double value) => value.ToString(STRING_FORMAT, CultureInfo.InvariantCulture);

    public static string ReplaceFirst(this string text, ReadOnlySpan<char> search, ReadOnlySpan<char> replace)
    {
        int pos = text.IdxOf(search);
        if (pos < 0)
        {
            return text;
        }
        return string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
    }
    
    public static string RemoveStringFromArrayDesc(this string input, ReadOnlySpan<string> array)
        => input.AsSpan().RemoveStringFromArrayDesc(array);
    
    public static string RemoveStringFromArrayDesc(this ReadOnlySpan<char> input, ReadOnlySpan<string> array)
    {
        int maxLen = 0;

        foreach (var s in array)
            if (s.Length > maxLen)
                maxLen = s.Length;

        for (int len = maxLen; len > 0; len--)
        {
            foreach (var s in array)
            {
                if (s.Length != len)
                    continue;

                ReadOnlySpan<char> needle = s.AsSpan();
                int idx = input.IndexOf(needle);

                if (idx < 0)
                    continue;

                int newLen = input.Length - needle.Length;

                char[] buffer = new char[newLen];

                input[..idx].CopyTo(buffer);
                input[(idx + needle.Length)..].CopyTo(buffer.AsSpan(idx));

                return new string(buffer).Trim();
            }
        }

        return input.ToString();
    }

    public static bool Contain(this string source, ReadOnlySpan<char> toCheck) => source.AsSpan().Contains(toCheck, StringComparison.Ordinal);

    public static bool Contain(this string source, char toCheck) => source.Contains(toCheck, StringComparison.Ordinal);

    public static bool Contain(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.Contains(toCheck, StringComparison.Ordinal);

    public static bool StartWith(this string source, ReadOnlySpan<char> toCheck) => source.AsSpan().StartsWith(toCheck, StringComparison.Ordinal);

    public static bool StartWith(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.StartsWith(toCheck, StringComparison.Ordinal);

    public static bool EndWith(this string source, ReadOnlySpan<char> toCheck) => source.AsSpan().EndsWith(toCheck, StringComparison.Ordinal);

    public static bool EndWith(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.EndsWith(toCheck, StringComparison.Ordinal);

    public static bool Equal(this string source, ReadOnlySpan<char> toCheck) => source.AsSpan().SequenceEqual(toCheck);

    public static int LastIdxOf(this string source, ReadOnlySpan<char> toCheck) => source.AsSpan().LastIndexOf(toCheck, StringComparison.Ordinal);

    public static int LastIdxOf(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.LastIndexOf(toCheck, StringComparison.Ordinal);

    public static int IdxOf(this string source, ReadOnlySpan<char> toCheck) => source.AsSpan().IndexOf(toCheck, StringComparison.Ordinal);

    public static int IdxOf(this ReadOnlySpan<char> source, ReadOnlySpan<char> toCheck) => source.IndexOf(toCheck, StringComparison.Ordinal);

    public static int IdxOf(this ReadOnlySpan<char> source, ReadOnlySpan<char> value, int start)
    {
        if (value.Length is 0) return start;

        for (int i = start; i <= source.Length - value.Length; i++)
        {
            if (source.Slice(i, value.Length).SequenceEqual(value))
                return i;
        }

        return -1;
    }

    public static bool StartWithAny(this string source, ReadOnlySpan<char> values, char delimiter = '/')
         => source.AsSpan().StartWithAny(values, delimiter);

    public static bool StartWithAny(this ReadOnlySpan<char> source, ReadOnlySpan<char> values, char delimiter = '/')
    {
        if (source.IsEmpty || values.IsEmpty)
            return false;

        if (values.IndexOf(delimiter) < 0)
            return source.StartsWith(values, StringComparison.Ordinal);

        int start = 0;

        for (int i = 0; i <= values.Length; i++)
        {
            if (i == values.Length || values[i] == delimiter)
            {
                var slice = values[start..i];

                if (!slice.IsEmpty && source.StartsWith(slice, StringComparison.Ordinal))
                    return true;

                start = i + 1;
            }
        }

        return false;
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

    /// <summary>Count decimals in a double value</summary>
    /// <remarks>Use string conversion, does not use decimal type precision</remarks>
    public static int CountDecimals(this double value)
    {
        var str = value.ToStr();

        int dot = str.IdxOf(".");
        if (dot < 0)
            return 0;

        int end = str.Length - 1;

        while (end > dot && str[end] is '0')
            end--;

        return end - dot;
    }

    /// <summary>
    /// Processes bracketed segments in a string:
    /// - Removes brackets.
    /// - If a segment contains '|', keeps only the part after it.
    /// </summary>
    public static string ParseBracketMod(this string itemInfo, bool trim = false) => itemInfo.AsSpan().ParseBracketMod(trim);

    /// <summary>
    /// Processes bracketed segments in a string:
    /// - Removes brackets.
    /// - If a segment contains '|', keeps only the part after it.
    /// </summary>
    public static string ParseBracketMod(this ReadOnlySpan<char> text, bool trim = false)
    {
        // Output buffer (max size = input size)
        Span<char> buffer = text.Length <= 512
            ? stackalloc char[text.Length] : new char[text.Length];

        int write = 0;
        int i = 0;

        while (i < text.Length)
        {
            if (text[i] is '[')
            {
                int start = i + 1;
                int end = text[start..].IndexOf(']');

                if (end < 0)
                {
                    // No closure → we copy as is
                    buffer[write++] = text[i++];
                    continue;
                }

                end += start; // real index

                var chunk = text[start..end];
                int pipeIdx = chunk.IndexOf('|');

                if (pipeIdx >= 0)
                {
                    // keep only after '|'
                    var part = chunk[(pipeIdx + 1)..];
                    part.CopyTo(buffer[write..]);
                    write += part.Length;
                }
                else
                {
                    // Keep all content without brackets
                    chunk.CopyTo(buffer[write..]);
                    write += chunk.Length;
                }

                i = end + 1; // skip the ']'
            }
            else
            {
                buffer[write++] = text[i++];
            }
        }

        // manual Trim if requested
        if (trim)
        {
            int start = 0;
            int end = write - 1;

            while (start <= end && char.IsWhiteSpace(buffer[start]))
                start++;

            while (end >= start && char.IsWhiteSpace(buffer[end]))
                end--;

            int newLen = end - start + 1;

            return new string(buffer.Slice(start, newLen));
        }

        return new string(buffer[..write]);
    }

    public static string GetFormated(this Exception ex)
    {
        return string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace);
    }

    public static string GetTrimAfterSeparator(this ReadOnlySpan<char> line, char separator)
    {
        int idx = line.IndexOf(separator);
        return idx >= 0 ? line[(idx + 1)..].Trim().ToString() : string.Empty;
    }

    public static void SplitAndValidate(this Span<Range> range, ReadOnlySpan<char> data, ReadOnlySpan<char> separator, int expectedLineCount)
    {
        int actualCount = data.Split(range, separator, StringSplitOptions.None);

        if (actualCount != expectedLineCount)
        {
            throw new InvalidOperationException(
                $"[SplitAndValidate] Line count mismatch: Expected={expectedLineCount}, Split={actualCount}");
        }
    }

    public static int CountOccurrences(this ReadOnlySpan<char> data, ReadOnlySpan<char> separator)
    {
        int lineCount = 1;

        while (true)
        {
            int idx = data.IdxOf(separator);
            if (idx < 0)
                break;

            lineCount++;
            data = data[(idx + 2)..];
        }

        return lineCount;
    }

    /// <summary>
    /// Get english resource for specified resource key
    /// </summary>
    public static string GetEnglish(this ResourceManager rm, string key)
        => rm.GetString(key, CultureInfo.InvariantCulture);
}

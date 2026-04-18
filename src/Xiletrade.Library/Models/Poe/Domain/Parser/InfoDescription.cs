using System;
using System.Buffers;
using System.Linq;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

public sealed record InfoDescription
{
    internal bool IsPoeItem { get; }
    internal string[] Item { get; }

    public InfoDescription(ReadOnlySpan<char> itemText, bool useSb = false)
    {
        Item = useSb ? NormalizeItemTextSb(itemText) : NormalizeItemText(itemText);

        if (Item[0].StartWith(Resources.Resources.General004_Rarity)) // Fix if item class is not provided
        {
            Item[0] = $"{Resources.Resources.General126_ItemClassPrefix}{" "}{Strings.NullClass}{Strings.CRLF}{Item[0]}";
        }

        IsPoeItem = Item.Length > 1 && Item[0].StartWith(Resources.Resources.General126_ItemClassPrefix);

        if (Item[^1].Contain("~b/o") || Item[^1].Contain("~price"))
        {
            Item = Item[..^1]; // clipDataWhitoutPrice
        }
    }

    // WIP
    private static string[] NormalizeItemText(ReadOnlySpan<char> input)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(input.Length * 2);
        int len = 0;

        // 1. Normalize + remove "()" + line endings
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // remove "()"
            if (c is '(' && i + 1 < input.Length && input[i + 1] is ')')
            {
                i++;
                continue;
            }

            // normalize line endings
            if (c is '\r')
            {
                if (i + 1 < input.Length && input[i + 1] is '\n')
                {
                    buffer[len++] = '\r';
                    buffer[len++] = '\n';
                    i++;
                    continue;
                }

                buffer[len++] = '\r';
                buffer[len++] = '\n';
                continue;
            }

            if (c is '\n')
            {
                buffer[len++] = '\r';
                buffer[len++] = '\n';
                continue;
            }

            buffer[len++] = c;
        }

        Span<char> span = buffer.AsSpan(0, len);

        // 2. INLINE ParseBracketMod (fusion directe, aucune string)
        Span<char> parsed = stackalloc char[span.Length];
        int write = 0;

        int i2 = 0;
        while (i2 < span.Length)
        {
            if (span[i2] is '[')
            {
                int start = i2 + 1;
                int remainingLen = span.Length - start;

                int endRel = span.Slice(start, remainingLen).IndexOf(']');
                if (endRel < 0)
                {
                    parsed[write++] = span[i2++];
                    continue;
                }

                int end = start + endRel;

                int pipeRel = span.Slice(start, endRel).IndexOf('|');
                if (pipeRel >= 0)
                {
                    ReadOnlySpan<char> part = span[(start + pipeRel + 1)..end];
                    part.CopyTo(parsed[write..]);
                    write += part.Length;
                }
                else
                {
                    ReadOnlySpan<char> part = span[start..end];
                    part.CopyTo(parsed[write..]);
                    write += part.Length;
                }

                i2 = end + 1;
                continue;
            }

            parsed[write++] = span[i2++];
        }

        // 3. SPLIT DIRECTLY ON parsed (always span-only)
        int delimiterCount = parsed[..write].Count(Strings.ItemInfoDelimiter);
        string[] result = ArrayPool<string>.Shared.Rent(delimiterCount + 1);

        int index = 0;
        Span<char> working = parsed[..write];

        while (true)
        {
            int pos = working.IndexOf(Strings.ItemInfoDelimiter);

            ReadOnlySpan<char> part = pos >= 0 ? working[..pos] : working;

            result[index++] = part.ToString();

            if (pos < 0)
                break;

            working = working[(pos + Strings.ItemInfoDelimiter.Length)..];
        }

        // 4. compact
        string[] final = new string[index];
        Array.Copy(result, final, index);

        ArrayPool<string>.Shared.Return(result, clearArray: true);
        ArrayPool<char>.Shared.Return(buffer, clearArray: true);

        return final;
    }
    
    /// <remarks>
    /// Focus on Readability
    /// </remarks>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string[] NormalizeItemTextSb(ReadOnlySpan<char> input)
    {
        System.Text.StringBuilder sbItemText = new(input.ToString());
        // some "\r" are missing while copying directly from the game, not from website copy
        sbItemText.Replace(Strings.CRLF, Strings.LF).Replace(Strings.LF, Strings.CRLF).Replace("()", string.Empty);
        return sbItemText.ToString().AsSpan().ParseBracketMod(trim: true)
            .Split([Strings.ItemInfoDelimiter], StringSplitOptions.None);
    }
}

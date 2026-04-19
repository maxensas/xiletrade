using System;
using System.Buffers;
using System.Linq;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

public sealed record InfoDescription
{
    internal bool IsPoeItem { get; }
    internal string[] Item { get; }

    public InfoDescription(ReadOnlySpan<char> itemText)
    {
        Item = NormalizeItemText(itemText);

        // Fix if item class is not provided
        // Should never happen but GGG can forget for new items (remember uncut gems)
        if (Item[0].StartWith(Resources.Resources.General004_Rarity))
        {
            Item[0] = $"{Resources.Resources.General126_ItemClassPrefix}{" "}{Strings.NullClass}{Strings.CRLF}{Item[0]}";
        }

        IsPoeItem = Item.Length > 1 && Item[0].StartWith(Resources.Resources.General126_ItemClassPrefix);
    }

    /// <summary>
    /// Normalize CR/LF line endings, remove "()", ParseBracketMod [..|..]
    /// </summary>
    /// <param name="input"></param>
    /// <returns>String array : splited string by 'Strings.ItemInfoDelimiter'</returns>
    private static string[] NormalizeItemText(ReadOnlySpan<char> input)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(input.Length * 2);
        int len = 0;

        // 1. Normalize + remove "()" + CRLF line endings
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // remove "()"
            if (c is '(' && i + 1 < input.Length && input[i + 1] is ')')
            {
                i++;
                continue;
            }

            // INFO : some "\r" are missing while copying directly from the game, not from website copy
            // normalize line endings
            if (c is '\r')
            {
                if (i + 1 < input.Length && input[i + 1] is '\n')
                {
                    buffer[len++] = '\r';
                    buffer[len++] = '\n';
                    i++;
                }
                else
                {
                    // we keep lonely '\r' as is
                    buffer[len++] = '\r';
                }
                continue;
            }
            /*
            // Normalize lonely '\r'
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
            */

            if (c is '\n')
            {
                buffer[len++] = '\r';
                buffer[len++] = '\n';
                continue;
            }

            buffer[len++] = c;
        }

        Span<char> span = buffer.AsSpan(0, len);

        // 2. INLINE ParseBracketMod (direct merging, no strings)
        Span<char> bracketParsed = span.Length <= 2048 
            ? stackalloc char[span.Length] : new char[span.Length];
        int write = 0;
        int j = 0;
        while (j < span.Length)
        {
            if (span[j] is '[')
            {
                int start = j + 1;
                int endRel = span[start..].IndexOf(']');
                if (endRel < 0)
                {
                    bracketParsed[write++] = span[j++];
                    continue;
                }

                int end = start + endRel;
                int pipeRel = span.Slice(start, endRel).IndexOf('|');
                
                ReadOnlySpan<char> part = pipeRel >= 0 ? span[(start + pipeRel + 1)..end] : span[start..end];
                
                part.CopyTo(bracketParsed[write..]);

                write += part.Length;
                j = end + 1;
                continue;
            }
            bracketParsed[write++] = span[j++];
        }

        ReadOnlySpan<char> trimSpan = bracketParsed[..write];

        // 3. GLOBAL TRIM
        int startTrim = 0;
        int endTrim = trimSpan.Length - 1;

        while (startTrim <= endTrim && char.IsWhiteSpace(trimSpan[startTrim]))
            startTrim++;

        while (endTrim >= startTrim && char.IsWhiteSpace(trimSpan[endTrim]))
            endTrim--;

        ReadOnlySpan<char> finalSpan = startTrim > endTrim ?
            trimSpan : trimSpan.Slice(startTrim, endTrim - startTrim + 1);

        // 4. Remove bo/price 2 last lines
        int lastPos = finalSpan.LastIndexOf(Strings.ItemInfoDelimiter);
        if (lastPos >= 0)
        {
            ReadOnlySpan<char> lastLine = finalSpan[(lastPos + Strings.ItemInfoDelimiter.Length)..];
            
            if (lastLine.Contain(Strings.bo) || lastLine.Contain(Strings.price))
            {
                finalSpan = finalSpan[..lastPos];
            }
        }

        // 5. SPLIT FINAL
        int delimiterCount = finalSpan.Count(Strings.ItemInfoDelimiter);
        string[] result = new string[delimiterCount + 1];

        int index = 0;
        while (true)
        {
            int pos = finalSpan.IndexOf(Strings.ItemInfoDelimiter);
            ReadOnlySpan<char> part = pos >= 0 ? finalSpan[..pos] : finalSpan;

            // Filling result array
            result[index++] = part.ToString();

            if (pos < 0)
                break;

            finalSpan = finalSpan[(pos + Strings.ItemInfoDelimiter.Length)..];
        }

        ArrayPool<char>.Shared.Return(buffer, clearArray: true);

        return result;
    }
}

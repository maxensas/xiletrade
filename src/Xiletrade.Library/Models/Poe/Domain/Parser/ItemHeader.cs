using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

public sealed record ItemHeader
{
    internal string Class { get; }
    internal string Rarity { get; }
    internal string Name { get; }
    internal string Type { get; }

    public ItemHeader(InfoDescription infoDesc)
    {
        ReadOnlySpan<char> itemHeader = infoDesc.Item[0].AsSpan().Trim();
        int lineCount = itemHeader.CountOccurrences(Strings.CRLF);
        Span<Range> lineRanges = lineCount <= 4 ? stackalloc Range[4] : new Range[lineCount];
        lineRanges.SplitAndValidate(itemHeader, Strings.CRLF, lineCount); // Fill range values

        Class = lineCount > 0 ? itemHeader[lineRanges[0]].GetTrimAfterSeparator(':') : string.Empty;
        Rarity = lineCount > 1 ? itemHeader[lineRanges[1]].GetTrimAfterSeparator(':') : string.Empty;
        Name = lineCount > 2 && !itemHeader[lineRanges[2]].IsEmpty ? itemHeader[lineRanges[2]].Trim().ToString() : string.Empty;
        Type = GetItemType(itemHeader, lineRanges, lineCount);
    }

    private static string GetItemType(ReadOnlySpan<char> data, Span<Range> ranges, int lineCount)
    {
        if (lineCount <= 1)
            return string.Empty;

        int max = lineCount > 3 ? 3 : lineCount - 1;

        for (int i = max; i >= 1; i--)
        {
            var span = data[ranges[i]];

            if (!span.IsEmpty)
                return span.Trim().ToString();
        }

        return string.Empty;
    }
}

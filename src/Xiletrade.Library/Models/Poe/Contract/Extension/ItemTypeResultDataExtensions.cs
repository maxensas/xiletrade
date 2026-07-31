using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

internal static class ItemTypeResultDataExtensions
{
    internal static ItemTypeEntrie FindEntryByType(this ItemTypeResultData[] itemDatas,
        ReadOnlySpan<char> type)
    {
        foreach (var itemData in itemDatas)
        {
            if (itemData.Entries is null)
                continue;

            foreach (var entry in itemData.Entries)
            {
                if (entry.Type.AsSpan().SequenceEqual(type))
                {
                    return entry;
                }
            }
        }
        return null;
    }

    internal static ItemTypeEntrie FindEntryByText(this ItemTypeResultData[] itemDatas,
        ReadOnlySpan<char> text)
    {
        foreach (var itemData in itemDatas)
        {
            if (itemData.Entries is null)
                continue;

            foreach (var entry in itemData.Entries)
            {
                if (entry.Text.AsSpan().Contain(text))
                {
                    return entry;
                }
            }
        }
        return null;
    }
}

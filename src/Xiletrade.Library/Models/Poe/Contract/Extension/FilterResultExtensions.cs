using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

internal static class FilterResultExtensions
{
    internal static FilterResultEntrie FindEntry(this FilterResult filterResult,
        ReadOnlySpan<char> id, ReadOnlySpan<char> type)
    {
        var entries = filterResult.Entries;
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

    internal static FilterResultEntrie FindModEntry(this FilterResult filterResult
        , ReadOnlySpan<char> mod, bool sequenceEquality = true)
    {
        var entries = filterResult.Entries;
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
}



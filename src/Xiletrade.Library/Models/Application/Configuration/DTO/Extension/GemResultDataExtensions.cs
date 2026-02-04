using System;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class GemResultDataExtensions
{
    internal static GemResultData FindGemByName(this GemResultData[] gems,
        ReadOnlySpan<char> gemName)
    {
        for (int i = 0; i < gems.Length; i++)
        {
            var gem = gems[i];
            if (gem.Name.AsSpan().SequenceEqual(gemName))
                return gem;
        }
        return null;
    }

    internal static GemResultData FindGemByNameEn(this GemResultData[] gems,
        ReadOnlySpan<char> gemNameEn)
    {
        for (int i = 0; i < gems.Length; i++)
        {
            var gem = gems[i];
            if (gem.NameEn.AsSpan().SequenceEqual(gemNameEn))
                return gem;
        }
        return null;
    }
}

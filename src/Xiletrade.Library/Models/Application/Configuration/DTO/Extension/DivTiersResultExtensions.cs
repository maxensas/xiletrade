using System;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class DivTiersResultExtensions
{
    internal static DivTiersResult FindDivTierByTag(this DivTiersResult[] divs,
        ReadOnlySpan<char> divCardId)
    {
        for (int i = 0; i < divs.Length; i++)
        {
            var div = divs[i];
            if (div.Tag.AsSpan().SequenceEqual(divCardId))
                return div;
        }
        return null;
    }
}

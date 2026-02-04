using System;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class DustLevelExtensions
{
    internal static DustLevel FindDustByName(this DustLevel[] dusts,
        ReadOnlySpan<char> itemType)
    {
        for (int i = 0; i < dusts.Length; i++)
        {
            var dust = dusts[i];
            if (dust.Name.AsSpan().SequenceEqual(itemType))
                return dust;
        }
        return null;
    }
}

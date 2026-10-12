using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagMap
{
    // map
    internal bool IsMap { get; }
    internal bool Blight { get; }
    internal bool BlightRavaged { get; }
    internal bool Valdo { get; }

    // frag
    internal bool Fragment { get; }

    // other
    internal bool Chart { get; }
    internal bool Misc { get; }

    internal ItemFlagMap(ReadOnlySpan<char> itemClass, ReadOnlySpan<char> itemType, bool isDivCard)
    {
        Fragment = itemClass.Contain(Resources.Resources.ItemClass_mapFragments);
        IsMap = itemClass.StartWith(Resources.Resources.ItemClass_maps) && !Fragment && !isDivCard;
        Chart = itemClass.Contain(Resources.Resources.ItemClass_chart);
        Misc = itemClass.StartWith(Resources.Resources.ItemClass_miscMapItems);
        if (IsMap)
        {
            Blight = itemType.Contain(Resources.Resources.General040_Blighted);
            BlightRavaged = itemType.Contain(Resources.Resources.General100_BlightRavaged);
            Valdo = itemType.Contain(Resources.Resources.General195_ValdoMap);
        }
    }
}

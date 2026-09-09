using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagJewellery
{
    internal bool Belts { get; }
    internal bool Rings { get; }
    internal bool Amulets { get; }
    internal bool Trinkets { get; }

    // group
    internal bool IsJewellery { get; }

    internal ItemFlagJewellery(ReadOnlySpan<char> itemClass)
    {
        Rings = itemClass.StartWith(Resources.Resources.ItemClass_rings);
        Amulets = itemClass.StartWith(Resources.Resources.ItemClass_amulets);
        Belts = itemClass.StartWith(Resources.Resources.ItemClass_belts);
        Trinkets = itemClass.StartWith(Resources.Resources.ItemClass_trinkets);

        IsJewellery = Amulets || Rings || Belts || Trinkets;
    }
}

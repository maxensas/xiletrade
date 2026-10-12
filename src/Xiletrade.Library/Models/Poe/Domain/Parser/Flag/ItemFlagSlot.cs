using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagSlot
{
    // using charges
    internal bool UtilityFlask { get; }
    internal bool LifeFlask { get; }
    internal bool ManaFlask { get; }
    internal bool HybridFlask { get; }
    internal bool Charm { get; }

    // mana burn
    internal bool Tincture { get; }

    // group
    internal bool Flask { get; }

    internal ItemFlagSlot(ReadOnlySpan<char> itemClass)
    {
        UtilityFlask = itemClass.Contain(Resources.Resources.ItemClass_utilityFlask);
        LifeFlask = itemClass.Contain(Resources.Resources.ItemClass_lifeFlask);
        ManaFlask = itemClass.Contain(Resources.Resources.ItemClass_manaFlask);
        HybridFlask = itemClass.Contain(Resources.Resources.ItemClass_hybridFlasks);
        Charm = itemClass.Contain(Resources.Resources.ItemClass_charm);
        Tincture = itemClass.Contain(Resources.Resources.ItemClass_tincture);

        Flask = UtilityFlask || LifeFlask || ManaFlask || HybridFlask;
    }
}

using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagArmour
{
    internal bool BodyArmours { get; }
    internal bool Boots { get; }
    internal bool Gloves { get; }
    internal bool Helmets { get; }

    //group
    internal bool IsArmour { get; }

    internal ItemFlagArmour(ItemFlagOffhand offhand, ReadOnlySpan<char> itemClass)
    {
        BodyArmours = itemClass.Contain(Resources.Resources.ItemClass_bodyArmours);
        Boots = itemClass.Contain(Resources.Resources.ItemClass_boots);
        Gloves = itemClass.Contain(Resources.Resources.ItemClass_gloves);
        Helmets = itemClass.Contain(Resources.Resources.ItemClass_helmets);

        IsArmour = BodyArmours || Boots || Gloves || Helmets || offhand.Shield || offhand.Bucklers || offhand.Focus;
    }
}

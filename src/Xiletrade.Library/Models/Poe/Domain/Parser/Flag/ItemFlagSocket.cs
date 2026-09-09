using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed class ItemFlagSocket
{
    internal bool CanSocket { get; }

    internal bool Socketable { get; }
    internal bool TwoRuneSocketable { get; } // exceptional not corrupted
    internal bool ThreeRuneSocketable { get; } // exceptional not corrupted

    internal ItemFlagSocket(ReadOnlySpan<char> itemClass, ItemFlagArmour armour, ItemFlagWeapon weapon, ItemFlagOffhand offhand)
    {
        CanSocket = armour.IsArmour || weapon.IsWeapon || offhand.Quivers;

        // poe2 noly
        Socketable = itemClass.StartWith(Resources.Resources.ItemClass_socketable);

        TwoRuneSocketable = weapon.Wand || weapon.Daggers || weapon.RuneDaggers || weapon.Claws || weapon.OneHandAxes
            || weapon.OneHandMaces || weapon.OneHandSwords || weapon.Sceptre || weapon.Spears || weapon.Flails || weapon.Staff
            || armour.Boots || armour.Gloves || armour.Helmets || offhand.Shield || offhand.Bucklers || offhand.Focus;
        ThreeRuneSocketable = weapon.Warstaff || weapon.QuarterStaff || weapon.Bows || weapon.TwoHandSwords
            || weapon.TwoHandMaces || weapon.TwoHandAxes || weapon.Crossbows || armour.BodyArmours || weapon.Traps || weapon.Talismans;
    }
}

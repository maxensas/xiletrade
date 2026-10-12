using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagWeapon
{
    internal bool Wand { get; }
    internal bool WandConvoking { get; }
    internal bool Sceptre { get; }
    internal bool Staff { get; }
    internal bool Warstaff { get; }
    internal bool QuarterStaff { get; }
    internal bool Spears { get; }
    internal bool Bows { get; }
    internal bool ThrustingOneHandSwords { get; }
    internal bool OneHandSwords { get; }
    internal bool TwoHandSwords { get; }
    internal bool OneHandMaces { get; }
    internal bool TwoHandMaces { get; }
    internal bool OneHandAxes { get; }
    internal bool TwoHandAxes { get; }
    internal bool Daggers { get; }
    internal bool RuneDaggers { get; }
    internal bool Claws { get; }
    internal bool FishingRods { get; }
    internal bool Crossbows { get; }
    internal bool Traps { get; }
    internal bool Flails { get; }
    internal bool Talismans { get; }

    //group
    internal bool IsStave { get; }
    internal bool IsWeapon { get; }

    internal ItemFlagWeapon(ReadOnlySpan<char> itemClass, ReadOnlySpan<char> itemType)
    {
        WandConvoking = itemType.Contain(Resources.Resources.General194_ConvokingWand);

        Wand = itemClass.Contain(Resources.Resources.ItemClass_wand);
        Sceptre = itemClass.StartWith(Resources.Resources.ItemClass_sceptres);
        Staff = itemClass.Contain(Resources.Resources.ItemClass_staff);
        Warstaff = itemClass.Contain(Resources.Resources.ItemClass_warstaff);
        QuarterStaff = itemClass.StartWith(Resources.Resources.ItemClass_quarterstaves);
        Spears = itemClass.Contain(Resources.Resources.ItemClass_spears);
        ThrustingOneHandSwords = itemClass.Contain(Resources.Resources.ItemClass_thrustingOneHandSwords);
        Bows = itemClass.StartWith(Resources.Resources.ItemClass_bows);
        OneHandSwords = itemClass.StartWith(Resources.Resources.ItemClass_oneHandSwords);
        TwoHandSwords = itemClass.StartWith(Resources.Resources.ItemClass_twoHandSwords);
        OneHandMaces = itemClass.StartWith(Resources.Resources.ItemClass_oneHandMaces);
        TwoHandMaces = itemClass.StartWith(Resources.Resources.ItemClass_twoHandMaces);
        OneHandAxes = itemClass.StartWith(Resources.Resources.ItemClass_oneHandAxes);
        TwoHandAxes = itemClass.StartWith(Resources.Resources.ItemClass_twoHandAxes);
        Daggers = itemClass.StartWith(Resources.Resources.ItemClass_daggers);
        RuneDaggers = itemClass.StartWith(Resources.Resources.ItemClass_runeDaggers);
        Claws = itemClass.StartWith(Resources.Resources.ItemClass_claws);
        FishingRods = itemClass.StartWith(Resources.Resources.ItemClass_fishingRods);
        Crossbows = itemClass.StartWith(Resources.Resources.ItemClass_crossbows);
        Traps = itemClass.StartWith(Resources.Resources.ItemClass_traps);
        Flails = itemClass.StartWith(Resources.Resources.ItemClass_flails);
        Talismans = itemClass.StartWith(Resources.Resources.ItemClass_talismans);

        IsStave = Staff || Warstaff || QuarterStaff;
        IsWeapon = Wand || Sceptre || Staff || Warstaff || QuarterStaff
            || Spears || Bows || ThrustingOneHandSwords || OneHandSwords || TwoHandSwords || OneHandMaces
            || TwoHandMaces || OneHandAxes || TwoHandAxes || Daggers || RuneDaggers
            || Claws || FishingRods || Crossbows || Traps || Flails || Talismans;
    }
}

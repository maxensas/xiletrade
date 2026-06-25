using System.Collections.Generic;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class MinMaxModel(string text)
{
    public string Text { get; set; } = text;

    public string Min { get; set; } = string.Empty;

    public string Max { get; set; } = string.Empty;

    public double MinSlide { get; set; } = ModFilter.EMPTYFIELD;

    public double MinSlideDefault { get; set; } = ModFilter.EMPTYFIELD;

    public bool Selected { get; set; } = false;

    // UI display in this order
    public static Dictionary<StatPanel, MinMaxModel> CreateDictionary()
    {
        return new ()
        {
            { StatPanel.CommonItemLevel, new(Resources.Resources.General032_ItemLv) },
            { StatPanel.CommonQuality, new(Resources.Resources.Main066_tbQuality) },
            { StatPanel.CommonSocket, new(Resources.Resources.General036_Socket) },
            { StatPanel.CommonLink, new(Resources.Resources.General154_Links) },
            { StatPanel.CommonSocketRune, new(Resources.Resources.General036_Socket) },
            { StatPanel.CommonSocketGem, new(Resources.Resources.ItemClass_supportGems) },

            { StatPanel.DamageTotal, new(Resources.Resources.Main073_tbTotalDps) },
            { StatPanel.DamagePhysical, new(Resources.Resources.Main074_tbPhysDps) },
            { StatPanel.DamageElemental, new(Resources.Resources.Main075_tbElemDps) },

            { StatPanel.DefenseEnergy, new(Resources.Resources.Main069_tbEnergy) },
            { StatPanel.DefenseEvasion, new(Resources.Resources.Main070_tbEvasion) },
            { StatPanel.DefenseArmour, new(Resources.Resources.Main068_tbArmour) },
            { StatPanel.DefenseWard, new(Resources.Resources.General095_Ward) },

            { StatPanel.TotalElemResistance, new(Resources.Resources.Main076_tbTotalResist) },
            { StatPanel.TotalLife, new(Resources.Resources.Main077_tbTotalLife) },
            { StatPanel.TotalGlobalEs, new(Resources.Resources.Main078_tbGlobalES) },
            { StatPanel.TotalAttribute, new(Resources.Resources.Config180_totalAttribute) },

            { StatPanel.CommonRequiresLevel, new(Resources.Resources.General155_Requires) },
            { StatPanel.CommonMemoryStrand, new(Resources.Resources.ItemClass_memory) },

            { StatPanel.MapQuantity, new(Resources.Resources.General133_Iiq) },
            { StatPanel.MapRarity, new(Resources.Resources.General134_Iir) },
            { StatPanel.MapPackSize, new(Resources.Resources.General138_MonsterPackSize) },
            { StatPanel.MapMoreScarab, new(Resources.Resources.General140_MoreScarabs) },
            { StatPanel.MapMoreCurrency, new(Resources.Resources.General139_MoreCurrency) },
            { StatPanel.MapMoreDivCard, new(Resources.Resources.General142_MoreDivinationCards) },
            { StatPanel.MapMoreMap, new(Resources.Resources.General141_MoreMaps) },
            { StatPanel.MapMonsterRare, new(Resources.Resources.General162_RareMonsters) },
            { StatPanel.MapMonsterMagic, new(Resources.Resources.General161_MagicMonsters) },

            { StatPanel.SanctumResolve, new(Resources.Resources.General114_SanctumResolve) },
            { StatPanel.SanctumMaxResolve, new(Resources.Resources.General124_SanctumMaxResolve) },
            { StatPanel.SanctumInspiration, new(Resources.Resources.General115_SanctumInspiration) },
            { StatPanel.SanctumAureus, new(Resources.Resources.General116_SanctumAureus) },

            { StatPanel.WaystoneRarity, new(Resources.Resources.General137_ItemRarity) },
            { StatPanel.WaystoneMonsterRarity, new(Resources.Resources.General200_MonsterRarity) },
            { StatPanel.WaystoneMonsterEffectiveness, new(Resources.Resources.General201_MonsterEffectiveness) },
            { StatPanel.WaystonePackSize, new(Resources.Resources.General202_WaystonePackSize) },
            { StatPanel.WaystoneDrop, new(Resources.Resources.General163_WaystoneDrop) },
            { StatPanel.WaystoneRevives, new(Resources.Resources.General160_RevivesAvailable) }
        };
    }
}

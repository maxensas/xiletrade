using System.Collections.Generic;
using System.Globalization;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
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
            { StatPanel.CommonIntangibility, new(Resources.Resources.General218_Intangibility) },

            { StatPanel.DamageTotal, new(Resources.Resources.Main073_tbTotalDps) },
            { StatPanel.DamagePhysical, new(Resources.Resources.Main074_tbPhysDps) },
            { StatPanel.DamageElemental, new(Resources.Resources.Main075_tbElemDps) },

            { StatPanel.DefenseEnergy, new(Resources.Resources.Main069_tbEnergy) },
            { StatPanel.DefenseEvasion, new(Resources.Resources.Main070_tbEvasion) },
            { StatPanel.DefenseArmour, new(Resources.Resources.Main068_tbArmour) },
            { StatPanel.DefenseWard, new(Resources.Resources.General095_Ward) },
            { StatPanel.DefenseRunicWard, new(Resources.Resources.General208_RunicWard) },

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

            { StatPanel.GoldFound, new(Resources.Resources.General215_GoldFound) },
            { StatPanel.DeadSulphur, new(Resources.Resources.General216_DeadSulphur) },

            { StatPanel.SanctumResolve, new(Resources.Resources.General114_SanctumResolve) },
            { StatPanel.SanctumMaxResolve, new(Resources.Resources.General124_SanctumMaxResolve) },
            { StatPanel.SanctumInspiration, new(Resources.Resources.General115_SanctumInspiration) },
            { StatPanel.SanctumAureus, new(Resources.Resources.General116_SanctumAureus) },

            { StatPanel.WaystoneRarity, new(Resources.Resources.General137_ItemRarity) },
            { StatPanel.WaystoneMonsterRarity, new(Resources.Resources.General200_MonsterRarity) },
            { StatPanel.WaystoneMonsterEffectiveness, new(Resources.Resources.General201_MonsterEffectiveness) },
            { StatPanel.WaystonePackSize, new(Resources.Resources.General202_WaystonePackSize) },
            { StatPanel.WaystoneDrop, new(Resources.Resources.General163_WaystoneDrop) },
            { StatPanel.WaystoneRevives, new(Resources.Resources.General160_RevivesAvailable) },

            { StatPanel.HeistRevealedWings, new(Resources.Resources.General219_WingsRevealed) },
            { StatPanel.HeistRevealedEscapeRoutes, new(Resources.Resources.General220_EscapeRoutesRevealed) },
            { StatPanel.HeistRevealedRewardRooms, new(Resources.Resources.General221_RewardRoomsRevealed) },
            { StatPanel.HeistTotalWings, new(Resources.Resources.General237_TotalWings) },
            { StatPanel.HeistTotalEscapeRoutes, new(Resources.Resources.General238_TotalEscapeRoutes) },
            { StatPanel.HeistTotalRewardRooms, new(Resources.Resources.General239_TotalRewardRooms) },

            { StatPanel.HeistLockpicking, new(Resources.Resources.General223_Lockpicking) },
            { StatPanel.HeistDemolition, new(Resources.Resources.General224_Demolition) },
            { StatPanel.HeistCounterThaumaturgy, new(Resources.Resources.General225_CounterThaumaturgy) },
            { StatPanel.HeistTrapDisarmament, new(Resources.Resources.General226_TrapDisarmament) },
            { StatPanel.HeistAgility, new(Resources.Resources.General227_Agility) },
            { StatPanel.HeistEngineering, new(Resources.Resources.General228_Engineering) },
            { StatPanel.HeistBruteForce, new(Resources.Resources.General229_BruteForce) },
            { StatPanel.HeistPerception, new(Resources.Resources.General230_Perception) },
            { StatPanel.HeistDeception, new(Resources.Resources.General231_Deception) }
        };
    }

    internal static Dictionary<StatPanel, MinMaxModel> GetMinMax(DataManagerService dm, ItemData item)
    {
        var minMax = CreateDictionary();
        var flag = item.Flag;

        if (!item.IsPoe2)
        {
            var intangibility = item.Options.Intangibility;
            if (!string.IsNullOrEmpty(intangibility))
            {
                minMax[StatPanel.CommonIntangibility].Min = intangibility;
            }
            if (flag.Socket.CanSocket || flag.Jewellery.IsJewellery)
            {
                minMax[StatPanel.CommonMemoryStrand].Min = item.Options.MemoryStrands;
            }
            if (flag.Blueprints)
            {
                if (item.Options.HeistWings is var wings && wings.Length is 2)
                {
                    minMax[StatPanel.HeistRevealedWings].Selected = true;
                    minMax[StatPanel.HeistRevealedWings].Min = wings[0];
                    minMax[StatPanel.HeistTotalWings].Min = wings[1];
                }
                if (item.Options.HeistEscapeRoutes is var escape && escape.Length is 2)
                {
                    minMax[StatPanel.HeistRevealedEscapeRoutes].Selected = true;
                    minMax[StatPanel.HeistRevealedEscapeRoutes].Min = escape[0];
                    minMax[StatPanel.HeistTotalEscapeRoutes].Min = escape[1];
                }
                if (item.Options.HeistRewardRooms is var reward && reward.Length is 2)
                {
                    minMax[StatPanel.HeistRevealedRewardRooms].Selected = true;
                    minMax[StatPanel.HeistRevealedRewardRooms].Min = reward[0];
                    minMax[StatPanel.HeistTotalRewardRooms].Min = reward[1];
                }
            }
            if (flag.Blueprints || flag.Contracts)
            {
                if (item.Options.HeistLockpicking is var lockpick && !string.IsNullOrEmpty(lockpick))
                {
                    minMax[StatPanel.HeistLockpicking].Selected = true;
                    minMax[StatPanel.HeistLockpicking].Min = lockpick;
                }
                if (item.Options.HeistDemolition is var demolition && !string.IsNullOrEmpty(demolition))
                {
                    minMax[StatPanel.HeistDemolition].Selected = true;
                    minMax[StatPanel.HeistDemolition].Min = demolition;
                }
                if (item.Options.HeistCounterThaumaturgy is var counter && !string.IsNullOrEmpty(counter))
                {
                    minMax[StatPanel.HeistCounterThaumaturgy].Selected = true;
                    minMax[StatPanel.HeistCounterThaumaturgy].Min = counter;
                }
                if (item.Options.HeistTrapDisarmament is var trap && !string.IsNullOrEmpty(trap))
                {
                    minMax[StatPanel.HeistTrapDisarmament].Selected = true;
                    minMax[StatPanel.HeistTrapDisarmament].Min = trap;
                }
                if (item.Options.HeistAgility is var agility && !string.IsNullOrEmpty(agility))
                {
                    minMax[StatPanel.HeistAgility].Selected = true;
                    minMax[StatPanel.HeistAgility].Min = agility;
                }
                if (item.Options.HeistEngineering is var engineering && !string.IsNullOrEmpty(engineering))
                {
                    minMax[StatPanel.HeistEngineering].Selected = true;
                    minMax[StatPanel.HeistEngineering].Min = engineering;
                }
                if (item.Options.HeistBruteForce is var bruteForce && !string.IsNullOrEmpty(bruteForce))
                {
                    minMax[StatPanel.HeistBruteForce].Selected = true;
                    minMax[StatPanel.HeistBruteForce].Min = bruteForce;
                }
                if (item.Options.HeistPerception is var perception && !string.IsNullOrEmpty(perception))
                {
                    minMax[StatPanel.HeistPerception].Selected = true;
                    minMax[StatPanel.HeistPerception].Min = perception;
                }
                if (item.Options.HeistDeception is var deception && !string.IsNullOrEmpty(deception))
                {
                    minMax[StatPanel.HeistDeception].Selected = true;
                    minMax[StatPanel.HeistDeception].Min = deception;
                }
            }
        }

        if (flag.Area.SanctumResearch)
        {
            if (item.Options.Resolve is var resolve && resolve.Length is 2)
            {
                minMax[StatPanel.SanctumResolve].Min = resolve[0];
                minMax[StatPanel.SanctumMaxResolve].Max = resolve[1];
            }
            minMax[StatPanel.SanctumInspiration].Min = item.Options.Inspiration;
            minMax[StatPanel.SanctumAureus].Min = item.Options.Aureus;
        }

        var preferTier = dm.Config.Options.AutoSelectMinTierValue && !flag.Tag.Mirrored && !flag.Tag.Corrupted;
        if (!flag.Map.IsMap && !flag.Slot.Flask && item.Stats.Resistance)
        {
            var res = minMax[StatPanel.TotalElemResistance];
            res.Min = item.Stats.GetResistance(preferTier);
            if (dm.Config.Options.AutoSelectRes
                && (res.Min.ToDoubleDefault() >= 36 || flag.Jewel.IsJewel))
            {
                res.Selected = true;
            }
        }
        if (item.Stats.Life)
        {
            var life = minMax[StatPanel.TotalLife];
            life.Min = item.Stats.GetLife(preferTier);
            if (dm.Config.Options.AutoSelectLife
                && (life.Min.ToDoubleDefault() >= 40 || flag.Jewel.IsJewel))
            {
                life.Selected = true;
            }
        }
        if (item.Stats.EnergyShield)
        {
            var globalEs = minMax[StatPanel.TotalGlobalEs];
            globalEs.Min = !flag.Armour.IsArmour ? item.Stats.GetEnergyShield(preferTier) : string.Empty;
            if (!flag.Armour.IsArmour && (dm.Config.Options.AutoSelectGlobalEs
                && (globalEs.Min.ToDoubleDefault() >= 38 || flag.Jewel.IsJewel)))
            {
                globalEs.Selected = true;
            }
        }
        if (item.Stats.Attribute)
        {
            var attribute = minMax[StatPanel.TotalAttribute];
            attribute.Min = item.Stats.GetAttribute(preferTier);
            if (dm.Config.Options.AutoSelectAttr && attribute.Min.ToDoubleDefault() >= 20)
            {
                attribute.Selected = true;
            }
        }

        if (flag.Socket.CanSocket)
        {
            var socket = minMax[StatPanel.CommonSocket];
            if (socket.Min is "6" && item.State.ImmutableSockets)
            {
                socket.Selected = true;
            }
            var link = minMax[StatPanel.CommonLink];
            if (link.Min is "6")
            {
                link.Selected = true;
            }
        }

        var level = minMax[StatPanel.CommonItemLevel];

        if (flag.Gem.Uncut || flag.Wombgift || flag.UltimatumPoe2 || flag.Area.TrialCoins)
        {
            level.Min = item.Options.ItemLevel;
            level.Selected = true;
        }

        var qual = minMax[StatPanel.CommonQuality];
        if (!flag.Rarity.Unique && (flag.Slot.Flask || flag.Slot.Tincture || (flag.Rarity.Normal && item.IsPoe2)))
        {
            var iLvl = item.Options.ItemLevel;
            var baseLevelMin = item.IsPoe2 ? 82 : 84;
            if (int.TryParse(iLvl, out int result) && result >= baseLevelMin)
            {
                qual.Selected = item.Options.Quality.Length > 0
                    && int.Parse(item.Options.Quality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!item.State.ExchangeCurrency)
        {
            level.Min = flag.Gem.IsGem ? item.Options.Level : item.Options.ItemLevel;
            qual.Min = item.Options.Quality;

            if (flag.Armour.IsArmour || flag.Weapon.IsWeapon || flag.Jewellery.IsJewellery || flag.Slot.Flask || flag.Slot.Charm)
            {
                var lv = item.Options.Level;
                var req = item.Options.Requires;
                minMax[StatPanel.CommonRequiresLevel].Min = lv.Length > 0 ? lv : req;
            }

            if (flag.MercenaryWarrant)
            {
                level.Min = item.Options.MercenaryLevel;
                level.Selected = true;
            }

            if (flag.Map.IsMap)
            {
                level.Min = level.Max = item.Options.MapTier;
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;

                var mapQuant = minMax[StatPanel.MapQuantity];
                mapQuant.Min = item.Options.ItemQuantity;
                var mapRarity = minMax[StatPanel.MapRarity];
                mapRarity.Min = item.Options.ItemRarity;
                var mapPackSize = minMax[StatPanel.MapPackSize];
                mapPackSize.Min = item.Options.MonsterPackSize;
                var mapScarab = minMax[StatPanel.MapMoreScarab];
                mapScarab.Min = item.Options.MoreScarabs;
                var mapCurrency = minMax[StatPanel.MapMoreCurrency];
                mapCurrency.Min = item.Options.MoreCurrency;
                var mapDivCard = minMax[StatPanel.MapMoreDivCard];
                mapDivCard.Min = item.Options.MoreDiv;
                var mapMoreMap = minMax[StatPanel.MapMoreMap];
                mapMoreMap.Min = item.Options.MoreMaps;

                // new auto select behaviour
                if (mapQuant.Min.ToDoubleDefault() >= 100
                    && mapRarity.Min.ToDoubleDefault() >= 90
                    && mapPackSize.Min.ToDoubleDefault() >= 40)
                {
                    mapQuant.Selected = mapRarity.Selected = mapPackSize.Selected = true;
                    if (mapScarab.Min.ToDoubleDefault() >= 70)
                    {
                        mapScarab.Selected = true;
                    }
                    if (mapCurrency.Min.ToDoubleDefault() >= 70)
                    {
                        mapCurrency.Selected = true;
                    }
                    if (mapDivCard.Min.ToDoubleDefault() >= 70)
                    {
                        mapDivCard.Selected = true;
                    }
                    if (mapMoreMap.Min.ToDoubleDefault() >= 100)
                    {
                        mapMoreMap.Selected = true;
                    }
                }
            }
            if (flag.Map.Chart)
            {
                level.Text = Resources.Resources.General067_AreaLevel;
                var area = item.Options.AreaLevel;
                level.Min = area.Length > 0 ? area : item.Options.AreaLevelBis;
                level.Selected = true;

                var mapQuant = minMax[StatPanel.MapQuantity];
                mapQuant.Min = item.Options.ItemQuantity;
                var mapRarity = minMax[StatPanel.MapRarity];
                mapRarity.Min = item.Options.ItemRarity;
                var mapPackSize = minMax[StatPanel.MapPackSize];
                mapPackSize.Min = item.Options.MonsterPackSize;
                var goldFound = minMax[StatPanel.GoldFound];
                goldFound.Min = item.Options.GoldFound;
                var deadSulphur = minMax[StatPanel.DeadSulphur];
                deadSulphur.Min = item.Options.DeadSulphur;

                // auto select behaviour to be refined
                if (mapQuant.Min.ToDoubleDefault() >= 100)
                {
                    mapQuant.Selected = true;
                }
                if (mapRarity.Min.ToDoubleDefault() >= 40)
                {
                    mapRarity.Selected = true;
                }
                if (mapPackSize.Min.ToDoubleDefault() >= 30)
                {
                    mapPackSize.Selected = true;
                }
                if (goldFound.Min.ToDoubleDefault() >= 100)
                {
                    goldFound.Selected = true;
                }
                if (deadSulphur.Min.ToDoubleDefault() >= 100)
                {
                    deadSulphur.Selected = true;
                }
            }
            else if (flag.Waystones)
            {
                level.Min = level.Max = item.Options.WaystoneTier;
                level.Text = Resources.Resources.Main094_lbTier;

                minMax[StatPanel.WaystoneRevives].Max = item.Options.RevivesAvailable;

                var drop = item.Options.WaystoneDrop;
                minMax[StatPanel.WaystoneDrop].Min = drop;
                minMax[StatPanel.WaystoneDrop].Selected = drop.ToDoubleDefault() >= 100;

                var itemRarity = item.Options.ItemRarity;
                minMax[StatPanel.WaystoneRarity].Min = itemRarity;
                minMax[StatPanel.WaystoneRarity].Selected = itemRarity.ToDoubleDefault() >= 20;

                var packSize = item.Options.WaystonePackSize;
                minMax[StatPanel.WaystonePackSize].Min = packSize;
                minMax[StatPanel.WaystonePackSize].Selected = packSize.ToDoubleDefault() >= 15;

                var monsterRarity = item.Options.MonsterRarity;
                minMax[StatPanel.WaystoneMonsterRarity].Min = monsterRarity;
                minMax[StatPanel.WaystoneMonsterRarity].Selected = monsterRarity.ToDoubleDefault() >= 20;

                var effectiveness = item.Options.MonsterEffectiveness;
                minMax[StatPanel.WaystoneMonsterEffectiveness].Min = effectiveness;
                minMax[StatPanel.WaystoneMonsterEffectiveness].Selected = effectiveness.ToDoubleDefault() >= 20;
            }
            else if (flag.Gem.IsGem)
            {
                level.Selected = true;
                minMax[StatPanel.CommonQuality].Selected = item.Options.Quality.Length > 0
                    && int.Parse(item.Options.Quality, CultureInfo.InvariantCulture) > 12;
            }
            else if (item.Rule.ByType && (flag.Rarity.Normal || (flag.Rarity.Magic && item.IsPoe2)))
            {
                level.Selected = level.Min.Length > 0
                    && int.Parse(level.Min, CultureInfo.InvariantCulture) >= (item.IsPoe2 ? 82 : 83);
            }
            else if (!flag.Rarity.Unique && flag.Jewel.Cluster)
            {
                level.Selected = level.Min.Length > 0
                    && int.Parse(level.Min, CultureInfo.InvariantCulture) >= 78;
                if (level.Min.Length > 0)
                {
                    int minVal = int.Parse(level.Min, CultureInfo.InvariantCulture);
                    level.Min = minVal >= 84 ? "84" : minVal >= 78 ? "78" : level.Min;
                }
            }
        }

        if (flag.Area.Logbook || flag.Corpses || flag.Area.SanctumResearch
            || flag.Area.Chronicle || flag.Area.MirroredTablet
            || flag.Area.TrialCoins || (flag.Area.Ultimatum && item.IsPoe2)
            || (flag.Slot.Flask || flag.Slot.Tincture) && !flag.Rarity.Unique)
        {
            level.Selected = true;
        }

        if (flag.Area.IsArea)
        {
            level.Text = Resources.Resources.General067_AreaLevel;
            var area = item.Options.AreaLevel;
            level.Min = area.Length > 0 ? area : item.Options.AreaLevelBis;
        }

        if (level.Text.Length is 0)
        {
            level.Text = Resources.Resources.Main065_tbiLevel;
        }

        if (flag.Armour.IsArmour && !flag.Tag.Unidentified)
        {
            var armour = item.Options.Armour;
            var energy = item.Options.Energy;
            var evasion = item.Options.Evasion;
            var ward = item.Options.Ward;
            var runicWard = item.Options.RunicWard;

            if (armour.Length > 0)
            {
                var ar = minMax[StatPanel.DefenseArmour];
                if (dm.Config.Options.AutoSelectArEsEva) ar.Selected = true;
                ar.Min = armour;
            }
            if (energy.Length > 0)
            {
                var es = minMax[StatPanel.DefenseEnergy];
                if (dm.Config.Options.AutoSelectArEsEva) es.Selected = true;
                es.Min = energy;
            }
            if (evasion.Length > 0)
            {
                var eva = minMax[StatPanel.DefenseEvasion];
                if (dm.Config.Options.AutoSelectArEsEva) eva.Selected = true;
                eva.Min = evasion;
            }
            if (ward.Length > 0) // poe1
            {
                var wrd = minMax[StatPanel.DefenseWard];
                if (dm.Config.Options.AutoSelectArEsEva) wrd.Selected = true;
                wrd.Min = ward;
            }
            if (runicWard.Length > 0) // poe2
            {
                var wrd = minMax[StatPanel.DefenseRunicWard];
                if (dm.Config.Options.AutoSelectArEsEva) wrd.Selected = true;
                wrd.Min = runicWard;
            }
        }

        if (flag.Weapon.IsWeapon && !flag.Tag.Unidentified)
        {
            if (dm.Config.Options.AutoSelectDps && item.Damage.Total > 100)
            {
                minMax[StatPanel.DamageTotal].Selected = true;
            }
            if (item.Damage.TotalMin.Length > 0)
            {
                minMax[StatPanel.DamageTotal].Min = item.Damage.TotalMin;
            }
            if (item.Damage.PysicalMin.Length > 0)
            {
                minMax[StatPanel.DamagePhysical].Min = item.Damage.PysicalMin;
            }
            if (item.Damage.ElementalMin.Length > 0)
            {
                minMax[StatPanel.DamageElemental].Min = item.Damage.ElementalMin;
            }
        }

        return minMax;
    }
}

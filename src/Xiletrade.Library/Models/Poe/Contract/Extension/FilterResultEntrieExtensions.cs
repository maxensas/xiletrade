using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

internal static class FilterResultEntrieExtensions
{
    internal static (FilterResultEntrie Entrie, double Min, double Max) GetMinMaxEntrie(this List<FilterResultEntrie> entries, 
        DataManagerService dm, ItemModifier mod, ItemData item)
    {
        var firstMatches = RegexUtil.DecimalNoPlusPattern().Matches(mod.Parsed);

        foreach (var entrie in entries)
        {
            if (entrie.SwitchEntrieId(dm, item))
            {
                continue;
            }

            if (entries.Count > 1 && entrie.Part.Length > 0)
                continue;

            int idxMin = 0, idxMax = 0;
            bool isMin = false, isMax = false, isBreak = true;

            var secondMatches = RegexUtil.DecimalNoPlusDiezePattern().Matches(entrie.Text);
            if (firstMatches.Count == secondMatches.Count)
            {
                for (int t = 0; t < secondMatches.Count; t++)
                {
                    if (secondMatches[t].Value is "#")
                    {
                        if (!isMin)
                        {
                            isMin = true;
                            idxMin = t;
                        }
                        else if (!isMax)
                        {
                            isMax = true;
                            idxMax = t;
                        }
                    }
                    else if (firstMatches[t].Value != secondMatches[t].Value)
                    {
                        isBreak = false;
                        break;
                    }
                }
            }

            if (isBreak) //TOCHECK if does not impact negatively
            {
                var matches = RegexUtil.DecimalNoPlusPattern().Matches(mod.Parsed);
                if (item.Flag.SanctumRelic) // TO update with other unparsed values not done yet
                {
                    isMin = true;
                }
                var min = isMin && matches.Count > idxMin ? matches[idxMin].Value.ToDoubleEmptyField() : ModFilter.EMPTYFIELD;
                var max = isMax && idxMin < idxMax && matches.Count > idxMax ? matches[idxMax].Value.ToDoubleEmptyField() : ModFilter.EMPTYFIELD;
                return (entrie, min, max);
            }
        }
        return (null, ModFilter.EMPTYFIELD, ModFilter.EMPTYFIELD);
    }

    internal static bool SwitchEntrieId(this FilterResultEntrie entrie, DataManagerService dm, ItemData item)
        => item.IsPoe2 ? entrie.SwitchPoe2EntrieId(dm, item) : entrie.SwitchPoe1EntrieId(dm, item);

    // private
    private static bool SwitchPoe1EntrieId(this FilterResultEntrie entrie, DataManagerService dm, ItemData item)
    {
        bool continueLoop = false;

        if (entrie.ID.Length > 1)
        {
            if (Strings.Stat.Aura.lSkipMods.Contains(entrie.ID.Split('.')[1]))
            {
                return true;
            }
            var words = dm.Words;
            if (entrie.ID.Contain(Strings.Words.IndexableSupport))
            {
                bool isShako = words.MatchNameEn(Strings.Unique.ForbiddenShako, item.Name);
                bool isLioneye = words.MatchNameEn(Strings.Unique.LioneyesVision, item.Name);
                //bool isHungryLoop = words.MatchNameEn(Strings.Unique.TheHungryLoop, itemName);
                bool isBitter = words.MatchNameEn(Strings.Unique.Bitterdream, item.Name);

                if (!isShako && !isLioneye)
                {
                    continueLoop = true;
                }
                if (entrie.ID is Strings.Stat.SocketedPierce2 && isLioneye)
                {
                    entrie.ID = Strings.Stat.SocketedPierce1;
                }

                if (entrie.ID is Strings.Stat.SocketedInspiration2 && isBitter)
                {
                    entrie.ID = Strings.Stat.SocketedInspiration1;
                }
            }

            // TODO : REDO duplicate mod handling
            if (entrie.ID is Strings.Stat.Accuracy || entrie.ID is Strings.Stat.AccuracyLocal)
            {
                entrie.ID = item.Flag.Weapon.IsWeapon ? Strings.Stat.AccuracyLocal : Strings.Stat.Accuracy;
            }
            else if (entrie.ID is Strings.Stat.Armor || entrie.ID is Strings.Stat.ArmorLocal)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.Stat.ArmorLocal : Strings.Stat.Armor;
            }
            else if (entrie.ID is Strings.Stat.Es || entrie.ID is Strings.Stat.EsLocal)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.Stat.EsLocal : Strings.Stat.Es;
            }
            else if (entrie.ID is Strings.Stat.Eva || entrie.ID is Strings.Stat.EvaLocal)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.Stat.EvaLocal : Strings.Stat.Eva;
            }
            else if (entrie.ID is Strings.Stat.HitBlind1 || entrie.ID is Strings.Stat.HitBlind2)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.Stat.HitBlind2 : Strings.Stat.HitBlind1;
            }
            else if (entrie.ID is Strings.Stat.ImmunityIgnite1 || entrie.ID is Strings.Stat.ImmunityIgnite2)
            {
                entrie.ID = item.Flag.Rarity.Unique ? Strings.Stat.ImmunityIgnite2 : Strings.Stat.ImmunityIgnite1;
            }
            else if (entrie.ID is Strings.Stat.TriggerAssassinOld)
            {
                entrie.ID = Strings.Stat.TriggerAssassinNew;
            }
            else if (entrie.ID is Strings.Stat.IncManaReserveEffOld)
            {
                entrie.ID = Strings.Stat.IncManaReserveEffNew;
            }
            else if (entrie.ID is Strings.Stat.SupressOld)
            {
                entrie.ID = Strings.Stat.SupressNew;
            }
            else if (entrie.ID is Strings.Stat.CritFlaskChargeOld) // many rarity
            {
                entrie.ID = Strings.Stat.CritFlaskChargeNew;
            }
            else if (entrie.ID is Strings.Stat.PrecisionEfficiencyOld) // Hyrri's Truth
            {
                entrie.ID = Strings.Stat.PrecisionEfficiencyNew;
            }
            else if (entrie.ID is Strings.Stat.BlockAttack1 || entrie.ID is Strings.Stat.BlockAttack2)
            {
                entrie.ID = item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique ? Strings.Stat.BlockAttack2 : Strings.Stat.BlockAttack1;
            }
            else if (entrie.ID is Strings.Stat.BlockSpell1 || entrie.ID is Strings.Stat.BlockSpell2)
            {
                entrie.ID = item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique ? Strings.Stat.BlockSpell2 : Strings.Stat.BlockSpell1;
            }
            else if (entrie.ID is Strings.Stat.CoolDownRecovery1 || entrie.ID is Strings.Stat.CoolDownRecovery2)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.CoolDownRecovery2 : Strings.Stat.CoolDownRecovery1;
            }
            else if (entrie.ID is Strings.Stat.IncCritAgainst1 && item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique)
            {
                entrie.ID = Strings.Stat.IncCritAgainst2;
            }
            else if (entrie.ID is Strings.Stat.PeneFire || entrie.ID is Strings.Stat.PeneFireTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.PeneFireTincture : Strings.Stat.PeneFire;
            }
            else if (entrie.ID is Strings.Stat.PeneCold || entrie.ID is Strings.Stat.PeneColdTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.PeneColdTincture : Strings.Stat.PeneCold;
            }
            else if (entrie.ID is Strings.Stat.PeneLight || entrie.ID is Strings.Stat.PeneLightTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.PeneLightTincture : Strings.Stat.PeneLight;
            }
            else if (entrie.ID is Strings.Stat.ManaPerKill || entrie.ID is Strings.Stat.ManaPerKillTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.ManaPerKillTincture : Strings.Stat.ManaPerKill;
            }
            else if (entrie.ID is Strings.Stat.AoeKill || entrie.ID is Strings.Stat.AoeKillTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.AoeKillTincture : Strings.Stat.AoeKill;
            }
            else if (entrie.ID is Strings.Stat.CritFullLife || entrie.ID is Strings.Stat.CritFullLifeTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.CritFullLifeTincture : Strings.Stat.CritFullLife;
            }
            else if (entrie.ID is Strings.Stat.PhasingKill || entrie.ID is Strings.Stat.PhasingKillTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.PhasingKillTincture : Strings.Stat.PhasingKill;
            }
            else if (entrie.ID is Strings.Stat.ConcGround || entrie.ID is Strings.Stat.ConcGroundTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.Stat.ConcGroundTincture : Strings.Stat.ConcGround;
            }
            else if (entrie.ID is Strings.Stat.StrikeRange && item.Flag.Slot.Tincture)
            {
                entrie.ID = Strings.Stat.StrikeRangeTincture;
            }
            else if (entrie.ID is Strings.Stat.StrInt && item.Flag.Slot.Charm)
            {
                entrie.ID = Strings.Stat.StrIntCharm;
            }
            else if (entrie.ID is Strings.Stat.BlockDmg || entrie.ID is Strings.Stat.BlockDmgJewCharm)
            {
                entrie.ID = item.Flag.Slot.Charm || item.Flag.Jewel.IsJewel ? Strings.Stat.BlockDmgJewCharm : Strings.Stat.BlockDmg;
            }
            else if (entrie.ID is Strings.Stat.Onslaught
                || entrie.ID is Strings.Stat.OnslaughtWeaponCharm
                || entrie.ID is Strings.Stat.OnslaughtAmulet)
            {
                entrie.ID = item.Flag.Slot.Charm || item.Flag.Weapon.IsWeapon ? Strings.Stat.OnslaughtWeaponCharm
                    : item.Flag.Jewellery.Amulets && item.Flag.Rarity.Unique ? Strings.Stat.OnslaughtAmulet : Strings.Stat.Onslaught;
            }
            else if (entrie.ID is Strings.Stat.ReduceEle || entrie.ID is Strings.Stat.ReduceEleGorgon)
            {
                bool isGorgon = words.MatchNameEn(Strings.Unique.GorgonsGaze, item.Name);
                entrie.ID = isGorgon ? Strings.Stat.ReduceEleGorgon : Strings.Stat.ReduceEle;
            }
            else if (entrie.ID is Strings.Stat.ShockSpread || entrie.ID is Strings.Stat.ShockSpreadEsh)
            {
                bool isEsh = words.MatchNameEn(Strings.Unique.EshsMirror, item.Name);
                entrie.ID = isEsh ? Strings.Stat.ShockSpreadEsh : Strings.Stat.ShockSpread;
            }
            else if (entrie.ID is Strings.Stat.Zombie || entrie.ID is Strings.Stat.ZombieBones)
            {
                bool isUllr = words.MatchNameEn(Strings.Unique.BonesOfUllr, item.Name);
                entrie.ID = isUllr ? Strings.Stat.ZombieBones : Strings.Stat.Zombie;
            }
            else if (entrie.ID is Strings.Stat.Spectre || entrie.ID is Strings.Stat.SpectreBones)
            {
                bool isUllr = words.MatchNameEn(Strings.Unique.BonesOfUllr, item.Name);
                entrie.ID = isUllr ? Strings.Stat.SpectreBones : Strings.Stat.Spectre;
            }
            else if (item.Flag.Slot.Flask && item.Flag.Rarity.Unique)
            {
                bool isCinder = words.MatchNameEn(Strings.Unique.CinderswallowUrn, item.Name);
                bool isDiv = words.MatchNameEn(Strings.Unique.DivinationDistillate, item.Name);

                entrie.ID = entrie.ID is Strings.Stat.FlaskIncRarity1 && isCinder ? Strings.Stat.FlaskIncRarity2
                    : entrie.ID is Strings.Stat.FlaskIncRarity2 && isDiv ? Strings.Stat.FlaskIncRarity1
                    : entrie.ID;
            }
            else if (item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique)
            {
                if (entrie.ID is Strings.Stat.TheBlueNightmare)
                {
                    bool isBlueDream = words.MatchNameEn(Strings.Unique.TheBlueDream, item.Name);
                    if (isBlueDream)
                    {
                        entrie.ID = Strings.Stat.TheBlueDream;
                    }
                }
            }
            else if (item.Flag.Armour.IsArmour && item.Flag.Rarity.Unique)
            {
                if (entrie.ID is Strings.Stat.FireTakenOld) // The Rat Cage
                {
                    entrie.ID = Strings.Stat.FireTakenNew;
                }
                if (entrie.ID is Strings.Stat.PurityIce1) //  Doryani's Delusion
                {
                    entrie.ID = Strings.Stat.PurityIce2;
                }
                if (entrie.ID is Strings.Stat.PurityFire1) //  Doryani's Delusion
                {
                    entrie.ID = Strings.Stat.PurityFire2;
                }
                if (entrie.ID is Strings.Stat.PurityLightning1) //  Doryani's Delusion
                {
                    entrie.ID = Strings.Stat.PurityLightning2;
                }
            }
            else if (item.Flag.Area.Chronicle)
            {
                bool goContinue = true;
                for (int s = 0; s < Strings.Stat.Temple.RoomList.Length; s++)
                {
                    if (entrie.ID.Contain(Strings.Stat.Temple.RoomList[s]))
                    {
                        goContinue = false;
                        break;
                    }
                }
                if (goContinue) continueLoop = true;
            }
            else if (item.Flag.Weapon.IsWeapon && item.Flag.Rarity.Unique)
            {
                if (entrie.ID is Strings.Stat.PoisonMoreDmg1) // Darkscorn old mod
                {
                    entrie.ID = Strings.Stat.PoisonMoreDmg2;
                }
                bool isDervish = words.MatchNameEn(Strings.Unique.TheDancingDervish, item.Name);
                if (entrie.ID is Strings.Stat.Rampage && isDervish)
                {
                    continueLoop = true;
                }
                bool isTrypanon = words.MatchNameEn(Strings.Unique.ReplicaTrypanon, item.Name);
                if (entrie.ID is Strings.Stat.AccuracyLocal && isTrypanon) // this is not a revert from previous code lines
                {
                    entrie.ID = Strings.Stat.Accuracy;
                }
                bool isNetolKiss = words.MatchNameEn(Strings.Unique.UulNetolsKiss, item.Name);
                if (entrie.ID is Strings.Stat.CurseVulnerability && isNetolKiss)
                {
                    entrie.ID = Strings.Stat.CurseVulnerabilityChance;
                }
            }
            else if (item.Flag.SanctumRelic)
            {
                if (entrie.Type is not Strings.Words.Sanctum)
                {
                    continueLoop = true;
                }
            }
            else if (item.Flag.Jewel.Cluster)
            {
                if (entrie.ID is Strings.Stat.ClusterCurseEffect1)
                {
                    entrie.ID = Strings.Stat.ClusterCurseEffect2;
                }
            }
        }

        if (item.Flag.Area.Logbook)//&& implicitMod
        {
            if (!entrie.ID.Contain(Strings.Stat.Generic.LogbookBoss)
                && !entrie.ID.Contain(Strings.Stat.Generic.LogbookArea)
                && !entrie.ID.Contain(Strings.Stat.Generic.LogbookTwice))
            {
                continueLoop = true;
            }
        }

        return continueLoop;
    }

    private static bool SwitchPoe2EntrieId(this FilterResultEntrie entrie, DataManagerService dm, ItemData item)
    {
        bool continueLoop = false;

        if (entrie.ID.Length is 0)
        {
            return false;
        }

        if (item.Flag.Waystones || item.Flag.Tablet)
        {
            if (entrie.ID is Strings.StatPoe2.IncXpGain1)
            {
                entrie.ID = Strings.StatPoe2.IncXpGain2;
            }
            if (entrie.ID is Strings.StatPoe2.DeliFog1)
            {
                entrie.ID = Strings.StatPoe2.DeliFog2;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.IncXpGain2)
            {
                entrie.ID = Strings.StatPoe2.IncXpGain1;
            }
            if (entrie.ID is Strings.StatPoe2.DeliFog2)
            {
                entrie.ID = Strings.StatPoe2.DeliFog1;
            }
        }

        if (item.Flag.Slot.Flask)
        {
            if (entrie.ID is Strings.StatPoe2.IncDuration2)
            {
                entrie.ID = Strings.StatPoe2.IncDuration1;
            }
        }
        if (item.Flag.Slot.Charm)
        {
            if (entrie.ID is Strings.StatPoe2.IncDuration1)
            {
                entrie.ID = Strings.StatPoe2.IncDuration2;
            }
        }

        if (item.Flag.Rarity.Unique && item.Flag.Jewellery.Amulets)
        {
            if (entrie.ID is Strings.StatPoe2.SkillLightningBolt)
            {
                entrie.ID = Strings.StatPoe2.SkillLightningBoltUnique;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.SkillLightningBoltUnique)
            {
                entrie.ID = Strings.StatPoe2.SkillLightningBolt;
            }
        }

        if (item.Flag.Jewel.IsJewel)
        {
            if (entrie.ID is Strings.StatPoe2.RecoverManaKill1)
            {
                entrie.ID = Strings.StatPoe2.RecoverManaKill2;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.RecoverManaKill2)
            {
                entrie.ID = Strings.StatPoe2.RecoverManaKill1;
            }
        }

        if (item.Flag.Offhand.Shield)
        {
            if (entrie.ID is Strings.StatPoe2.IncBlock2)
            {
                entrie.ID = Strings.StatPoe2.IncBlock1;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.IncBlock1)
            {
                entrie.ID = Strings.StatPoe2.IncBlock2;
            }
        }

        if (item.Flag.Weapon.IsWeapon)
        {
            if (entrie.ID is Strings.StatPoe2.IncAs2)
            {
                entrie.ID = Strings.StatPoe2.IncAs1;
            }
            if (entrie.ID is Strings.StatPoe2.AccuracyRating2)
            {
                entrie.ID = Strings.StatPoe2.AccuracyRating1;
            }
            if (entrie.ID is Strings.StatPoe2.ChancePoison2)
            {
                entrie.ID = Strings.StatPoe2.ChancePoison1;
            }
            if (entrie.ID is Strings.StatPoe2.AsPerDex1 or Strings.StatPoe2.AsPerDex3)
            {
                entrie.ID = Strings.StatPoe2.AsPerDex2;
            }
            if (entrie.ID.EndWith(Strings.StatPoe2.AllAttributes1))
            {
                var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
                entrie.ID = kind.ToString() + Strings.StatPoe2.AllAttributes2;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.IncAs1)
            {
                entrie.ID = Strings.StatPoe2.IncAs2;
            }
            if (entrie.ID is Strings.StatPoe2.AccuracyRating1)
            {
                entrie.ID = Strings.StatPoe2.AccuracyRating2;
            }
            if (entrie.ID is Strings.StatPoe2.ChancePoison1)
            {
                entrie.ID = Strings.StatPoe2.ChancePoison2;
            }
            if (entrie.ID is Strings.StatPoe2.AsPerDex2 or Strings.StatPoe2.AsPerDex3)
            {
                entrie.ID = Strings.StatPoe2.AsPerDex1;
            }
            if (entrie.ID.EndWith(Strings.StatPoe2.AllAttributes2))
            {
                var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
                entrie.ID = kind.ToString() + Strings.StatPoe2.AllAttributes1;
            }
        }

        if (item.Flag.Armour.IsArmour)
        {
            if (entrie.ID is Strings.StatPoe2.IncArmour2)
            {
                entrie.ID = Strings.StatPoe2.IncArmour1;
            }
            if (entrie.ID is Strings.StatPoe2.IncEvasion2)
            {
                entrie.ID = Strings.StatPoe2.IncEvasion1;
            }
            if (entrie.ID is Strings.StatPoe2.EvasionRating1)
            {
                entrie.ID = Strings.StatPoe2.EvasionRating2;
            }
            if (entrie.ID is Strings.StatPoe2.Armour2)
            {
                entrie.ID = Strings.StatPoe2.Armour1;
            }
            if (entrie.ID is Strings.StatPoe2.EnergyShield1)
            {
                entrie.ID = Strings.StatPoe2.EnergyShield2;
            }
            if (entrie.ID is Strings.StatPoe2.IncArmourEnch2)
            {
                entrie.ID = Strings.StatPoe2.IncArmourEnch1;
            }
            if (entrie.ID is Strings.StatPoe2.IncEvasionEnch2)
            {
                entrie.ID = Strings.StatPoe2.IncEvasionEnch1;
            }
            if (entrie.ID is Strings.StatPoe2.CharmSlot1)
            {
                entrie.ID = Strings.StatPoe2.CharmSlot2;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.IncArmour1)
            {
                entrie.ID = Strings.StatPoe2.IncArmour2;
            }
            if (entrie.ID is Strings.StatPoe2.IncEvasion1)
            {
                entrie.ID = Strings.StatPoe2.IncEvasion2;
            }
            if (entrie.ID is Strings.StatPoe2.EvasionRating2)
            {
                entrie.ID = Strings.StatPoe2.EvasionRating1;
            }
            if (entrie.ID is Strings.StatPoe2.Armour1)
            {
                entrie.ID = Strings.StatPoe2.Armour2;
            }
            if (entrie.ID is Strings.StatPoe2.EnergyShield2)
            {
                entrie.ID = Strings.StatPoe2.EnergyShield1;
            }
            if (entrie.ID is Strings.StatPoe2.IncArmourEnch2)
            {
                entrie.ID = Strings.StatPoe2.IncArmourEnch1;
            }
            if (entrie.ID is Strings.StatPoe2.IncEvasionEnch1)
            {
                entrie.ID = Strings.StatPoe2.IncEvasionEnch2;
            }
            if (entrie.ID is Strings.StatPoe2.CharmSlot2)
            {
                entrie.ID = Strings.StatPoe2.CharmSlot1;
            }
        }

        //tablets
        if (entrie.ID is Strings.StatPoe2.Shrine1 or Strings.StatPoe2.Shrine2)
        {
            bool isOverseer = item.TypeEn is Strings.Tablet.Overseer;
            entrie.ID = isOverseer ? Strings.StatPoe2.Shrine1 : Strings.StatPoe2.Shrine2;
        }
        if (entrie.ID is Strings.StatPoe2.Essence1 or Strings.StatPoe2.Essence2)
        {
            bool isOverseer = item.TypeEn is Strings.Tablet.Overseer;
            entrie.ID = isOverseer ? Strings.StatPoe2.Essence1 : Strings.StatPoe2.Essence2;
        }

        //uniques
        var words = dm.Words;
        if (entrie.ID.EndWith(Strings.StatPoe2.Spirit1) || entrie.ID.EndWith(Strings.StatPoe2.Spirit2))
        {
            bool isUnborn = words.MatchNameEn(Strings.UniqueTwo.TheUnbornLich, item.Name);
            var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
            entrie.ID = kind.ToString() + (isUnborn ? Strings.StatPoe2.Spirit1 : Strings.StatPoe2.Spirit2);
        }
        if (entrie.ID.EndWith(Strings.StatPoe2.IncSpirit1) || entrie.ID.EndWith(Strings.StatPoe2.IncSpirit2))
        {
            bool isUnique = words.MatchNameEn(Strings.UniqueTwo.GripofKulemak, item.Name)
                || words.MatchNameEn(Strings.UniqueTwo.IdolofUldurn, item.Name);
            var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
            entrie.ID = kind.ToString() + (isUnique ? Strings.StatPoe2.IncSpirit1 : Strings.StatPoe2.IncSpirit2);
        }
        if (entrie.ID.EndWith(Strings.StatPoe2.RunicWard1) || entrie.ID.EndWith(Strings.StatPoe2.RunicWard2))
        {
            bool isSvalinn = words.MatchNameEn(Strings.UniqueTwo.Svalinn, item.Name);
            var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
            entrie.ID = kind.ToString() + (isSvalinn ? Strings.StatPoe2.RunicWard1 : Strings.StatPoe2.RunicWard2);
        }
        if (entrie.ID is Strings.StatPoe2.Zealot1 or Strings.StatPoe2.Zealot2)
        {
            bool isGeoFri = words.MatchNameEn(Strings.UniqueTwo.Geofri, item.Name);
            entrie.ID = isGeoFri ? Strings.StatPoe2.Zealot1 : Strings.StatPoe2.Zealot2;
        }
        if (entrie.ID is Strings.StatPoe2.Blinded1 or Strings.StatPoe2.Blinded2)
        {
            bool isVestige = words.MatchNameEn(Strings.UniqueTwo.Vestige, item.Name);
            entrie.ID = isVestige ? Strings.StatPoe2.Blinded1 : Strings.StatPoe2.Blinded2;
        }
        if (item.Flag.Rarity.Unique && entrie.ID is Strings.StatPoe2.Rarity1
            or Strings.StatPoe2.Rarity2 or Strings.StatPoe2.Rarity3) // TO FIX : Loreweave can have regular rarity mod
        {
            bool isLoreweave = words.MatchNameEn(Strings.UniqueTwo.Loreweave, item.Name);
            bool isGravebind = words.MatchNameEn(Strings.UniqueTwo.Gravebind, item.Name);
            entrie.ID = isLoreweave ? Strings.StatPoe2.Rarity1 : isGravebind ? Strings.StatPoe2.Rarity2 : Strings.StatPoe2.Rarity3;
        }
        if (entrie.ID is Strings.StatPoe2.VaalPact1 or Strings.StatPoe2.VaalPact2)
        {
            bool isAcuity = words.MatchNameEn(Strings.UniqueTwo.Acuity, item.Name);
            entrie.ID = isAcuity ? Strings.StatPoe2.VaalPact1 : Strings.StatPoe2.VaalPact2;
        }
        if (entrie.ID is Strings.StatPoe2.Daze1 or Strings.StatPoe2.Daze2)
        {
            bool isNazir = words.MatchNameEn(Strings.UniqueTwo.NazirsJudgement, item.Name);
            entrie.ID = isNazir ? Strings.StatPoe2.Daze1 : Strings.StatPoe2.Daze2;
        }
        if (entrie.ID is Strings.StatPoe2.Aftershocks1 or Strings.StatPoe2.Aftershocks2)
        {
            bool isHrimnors = words.MatchNameEn(Strings.UniqueTwo.HrimnorsHymn, item.Name);
            entrie.ID = isHrimnors ? Strings.StatPoe2.Aftershocks2 : Strings.StatPoe2.Aftershocks1;
        }
        if (entrie.ID is Strings.StatPoe2.RandomShrine1 or Strings.StatPoe2.RandomShrine2)
        {
            bool isHammer = words.MatchNameEn(Strings.UniqueTwo.TheHammerofFaith, item.Name);
            entrie.ID = isHammer ? Strings.StatPoe2.RandomShrine2 : Strings.StatPoe2.RandomShrine1;
        }
        if (entrie.ID is Strings.StatPoe2.CharmSlot3 or Strings.StatPoe2.CharmSlot2)
        {
            bool isElevore = words.MatchNameEn(Strings.UniqueTwo.Elevore, item.Name);
            entrie.ID = isElevore ? Strings.StatPoe2.CharmSlot2 : Strings.StatPoe2.CharmSlot3;
        }
        if (entrie.ID is Strings.StatPoe2.Decompose1 or Strings.StatPoe2.Decompose2)
        {
            bool isCorpsewade = words.MatchNameEn(Strings.UniqueTwo.Corpsewade, item.Name);
            entrie.ID = isCorpsewade ? Strings.StatPoe2.Decompose1 : Strings.StatPoe2.Decompose2;
        }
        //bool IsPrism() => words.FirstOrDefault(x => x.NameEn is Strings.UniqueTwo.PrismofBelief).Name == itemName;
        if (entrie.ID is Strings.StatPoe2.SkeletalSniper1)
        {
            entrie.ID = Strings.StatPoe2.SkeletalSniper2;
        }
        if (entrie.ID is Strings.StatPoe2.HeraldofBlood1)
        {
            entrie.ID = Strings.StatPoe2.HeraldofBlood2;
        }
        if (entrie.ID is Strings.StatPoe2.TamedCompanion1)
        {
            entrie.ID = Strings.StatPoe2.TamedCompanion2;
        }
        bool IsFlesh(ReadOnlySpan<char> name)
            => words.MatchNameEn(Strings.UniqueTwo.FleshCrucible, name);

        if (entrie.ID is Strings.StatPoe2.PainAttunement1 or Strings.StatPoe2.PainAttunement2)
        {
            entrie.ID = IsFlesh(item.Name) ? Strings.StatPoe2.PainAttunement1 : Strings.StatPoe2.PainAttunement2;
        }
        if (entrie.ID is Strings.StatPoe2.GiantsBlood1 or Strings.StatPoe2.GiantsBlood2)
        {
            entrie.ID = IsFlesh(item.Name) ? Strings.StatPoe2.GiantsBlood1 : Strings.StatPoe2.GiantsBlood2;
        }
        if (entrie.ID is Strings.StatPoe2.UnwaveringStance1 or Strings.StatPoe2.UnwaveringStance2)
        {
            entrie.ID = IsFlesh(item.Name) ? Strings.StatPoe2.UnwaveringStance1 : Strings.StatPoe2.UnwaveringStance2;
        }
        if (entrie.ID is Strings.StatPoe2.EldritchBattery1 or Strings.StatPoe2.EldritchBattery2)
        {
            entrie.ID = IsFlesh(item.Name) ? Strings.StatPoe2.EldritchBattery1 : Strings.StatPoe2.EldritchBattery2;
        }
        if (entrie.ID is Strings.StatPoe2.BloodMagic1 or Strings.StatPoe2.BloodMagic2)
        {
            entrie.ID = IsFlesh(item.Name) ? Strings.StatPoe2.BloodMagic1 : Strings.StatPoe2.BloodMagic2;
        }
        if (entrie.ID is Strings.StatPoe2.IronReflexes1 or Strings.StatPoe2.IronReflexes2)
        {
            entrie.ID = IsFlesh(item.Name) ? Strings.StatPoe2.IronReflexes1 : Strings.StatPoe2.IronReflexes2;
        }
        if (entrie.ID is Strings.StatPoe2.GlancingBlows1 or Strings.StatPoe2.GlancingBlows2)
        {
            entrie.ID = IsFlesh(item.Name) ? Strings.StatPoe2.GlancingBlows1 : Strings.StatPoe2.GlancingBlows2;
        }

        return continueLoop;
    }
}

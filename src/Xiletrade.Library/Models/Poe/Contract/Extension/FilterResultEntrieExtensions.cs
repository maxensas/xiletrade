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
            if (Strings.StatPoe1.Aura.lSkipMods.Contains(entrie.ID.Split('.')[1]))
            {
                return true;
            }
            var words = dm.Words;
            if (entrie.ID.Contain(Strings.Word.IndexableSupport))
            {
                bool isShako = words.MatchNameEn(Strings.Unique.ForbiddenShako, item.Name);
                bool isLioneye = words.MatchNameEn(Strings.Unique.LioneyesVision, item.Name);
                //bool isHungryLoop = words.MatchNameEn(Strings.Unique.TheHungryLoop, itemName);
                bool isBitter = words.MatchNameEn(Strings.Unique.Bitterdream, item.Name);

                if (!isShako && !isLioneye)
                {
                    continueLoop = true;
                }
                if (entrie.ID is Strings.StatPoe1.SocketedPierce2 && isLioneye)
                {
                    entrie.ID = Strings.StatPoe1.SocketedPierce1;
                }

                if (entrie.ID is Strings.StatPoe1.SocketedInspiration2 && isBitter)
                {
                    entrie.ID = Strings.StatPoe1.SocketedInspiration1;
                }
            }

            // TODO : REDO duplicate mod handling
            if (entrie.ID is Strings.StatPoe1.Accuracy || entrie.ID is Strings.StatPoe1.AccuracyLocal)
            {
                entrie.ID = item.Flag.Weapon.IsWeapon ? Strings.StatPoe1.AccuracyLocal : Strings.StatPoe1.Accuracy;
            }
            else if (entrie.ID is Strings.StatPoe1.Armor || entrie.ID is Strings.StatPoe1.ArmorLocal)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.StatPoe1.ArmorLocal : Strings.StatPoe1.Armor;
            }
            else if (entrie.ID is Strings.StatPoe1.Es || entrie.ID is Strings.StatPoe1.EsLocal)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.StatPoe1.EsLocal : Strings.StatPoe1.Es;
            }
            else if (entrie.ID is Strings.StatPoe1.Eva || entrie.ID is Strings.StatPoe1.EvaLocal)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.StatPoe1.EvaLocal : Strings.StatPoe1.Eva;
            }
            else if (entrie.ID is Strings.StatPoe1.HitBlind1 || entrie.ID is Strings.StatPoe1.HitBlind2)
            {
                entrie.ID = item.Flag.Armour.IsArmour ? Strings.StatPoe1.HitBlind2 : Strings.StatPoe1.HitBlind1;
            }
            else if (entrie.ID is Strings.StatPoe1.ImmunityIgnite1 || entrie.ID is Strings.StatPoe1.ImmunityIgnite2)
            {
                entrie.ID = item.Flag.Rarity.Unique ? Strings.StatPoe1.ImmunityIgnite2 : Strings.StatPoe1.ImmunityIgnite1;
            }
            else if (entrie.ID is Strings.StatPoe1.TriggerAssassinOld)
            {
                entrie.ID = Strings.StatPoe1.TriggerAssassinNew;
            }
            else if (entrie.ID is Strings.StatPoe1.IncManaReserveEffOld)
            {
                entrie.ID = Strings.StatPoe1.IncManaReserveEffNew;
            }
            else if (entrie.ID is Strings.StatPoe1.SupressOld)
            {
                entrie.ID = Strings.StatPoe1.SupressNew;
            }
            else if (entrie.ID is Strings.StatPoe1.CritFlaskChargeOld) // many rarity
            {
                entrie.ID = Strings.StatPoe1.CritFlaskChargeNew;
            }
            else if (entrie.ID is Strings.StatPoe1.PrecisionEfficiencyOld) // Hyrri's Truth
            {
                entrie.ID = Strings.StatPoe1.PrecisionEfficiencyNew;
            }
            else if (entrie.ID is Strings.StatPoe1.BlockAttack1 || entrie.ID is Strings.StatPoe1.BlockAttack2)
            {
                entrie.ID = item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique ? Strings.StatPoe1.BlockAttack2 : Strings.StatPoe1.BlockAttack1;
            }
            else if (entrie.ID is Strings.StatPoe1.BlockSpell1 || entrie.ID is Strings.StatPoe1.BlockSpell2)
            {
                entrie.ID = item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique ? Strings.StatPoe1.BlockSpell2 : Strings.StatPoe1.BlockSpell1;
            }
            else if (entrie.ID is Strings.StatPoe1.CoolDownRecovery1 || entrie.ID is Strings.StatPoe1.CoolDownRecovery2)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.CoolDownRecovery2 : Strings.StatPoe1.CoolDownRecovery1;
            }
            else if (entrie.ID is Strings.StatPoe1.IncCritAgainst1 && item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique)
            {
                entrie.ID = Strings.StatPoe1.IncCritAgainst2;
            }
            else if (entrie.ID is Strings.StatPoe1.PeneFire || entrie.ID is Strings.StatPoe1.PeneFireTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.PeneFireTincture : Strings.StatPoe1.PeneFire;
            }
            else if (entrie.ID is Strings.StatPoe1.PeneCold || entrie.ID is Strings.StatPoe1.PeneColdTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.PeneColdTincture : Strings.StatPoe1.PeneCold;
            }
            else if (entrie.ID is Strings.StatPoe1.PeneLight || entrie.ID is Strings.StatPoe1.PeneLightTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.PeneLightTincture : Strings.StatPoe1.PeneLight;
            }
            else if (entrie.ID is Strings.StatPoe1.ManaPerKill || entrie.ID is Strings.StatPoe1.ManaPerKillTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.ManaPerKillTincture : Strings.StatPoe1.ManaPerKill;
            }
            else if (entrie.ID is Strings.StatPoe1.AoeKill || entrie.ID is Strings.StatPoe1.AoeKillTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.AoeKillTincture : Strings.StatPoe1.AoeKill;
            }
            else if (entrie.ID is Strings.StatPoe1.CritFullLife || entrie.ID is Strings.StatPoe1.CritFullLifeTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.CritFullLifeTincture : Strings.StatPoe1.CritFullLife;
            }
            else if (entrie.ID is Strings.StatPoe1.PhasingKill || entrie.ID is Strings.StatPoe1.PhasingKillTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.PhasingKillTincture : Strings.StatPoe1.PhasingKill;
            }
            else if (entrie.ID is Strings.StatPoe1.ConcGround || entrie.ID is Strings.StatPoe1.ConcGroundTincture)
            {
                entrie.ID = item.Flag.Slot.Tincture ? Strings.StatPoe1.ConcGroundTincture : Strings.StatPoe1.ConcGround;
            }
            else if (entrie.ID is Strings.StatPoe1.StrikeRange && item.Flag.Slot.Tincture)
            {
                entrie.ID = Strings.StatPoe1.StrikeRangeTincture;
            }
            else if (entrie.ID is Strings.StatPoe1.StrInt && item.Flag.Slot.Charm)
            {
                entrie.ID = Strings.StatPoe1.StrIntCharm;
            }
            else if (entrie.ID is Strings.StatPoe1.BlockDmg || entrie.ID is Strings.StatPoe1.BlockDmgJewCharm)
            {
                entrie.ID = item.Flag.Slot.Charm || item.Flag.Jewel.IsJewel ? Strings.StatPoe1.BlockDmgJewCharm : Strings.StatPoe1.BlockDmg;
            }
            else if (entrie.ID is Strings.StatPoe1.Onslaught
                || entrie.ID is Strings.StatPoe1.OnslaughtWeaponCharm
                || entrie.ID is Strings.StatPoe1.OnslaughtAmulet)
            {
                entrie.ID = item.Flag.Slot.Charm || item.Flag.Weapon.IsWeapon ? Strings.StatPoe1.OnslaughtWeaponCharm
                    : item.Flag.Jewellery.Amulets && item.Flag.Rarity.Unique ? Strings.StatPoe1.OnslaughtAmulet : Strings.StatPoe1.Onslaught;
            }
            else if (entrie.ID is Strings.StatPoe1.ReduceEle || entrie.ID is Strings.StatPoe1.ReduceEleGorgon)
            {
                bool isGorgon = words.MatchNameEn(Strings.Unique.GorgonsGaze, item.Name);
                entrie.ID = isGorgon ? Strings.StatPoe1.ReduceEleGorgon : Strings.StatPoe1.ReduceEle;
            }
            else if (entrie.ID is Strings.StatPoe1.ShockSpread || entrie.ID is Strings.StatPoe1.ShockSpreadEsh)
            {
                bool isEsh = words.MatchNameEn(Strings.Unique.EshsMirror, item.Name);
                entrie.ID = isEsh ? Strings.StatPoe1.ShockSpreadEsh : Strings.StatPoe1.ShockSpread;
            }
            else if (entrie.ID is Strings.StatPoe1.Zombie || entrie.ID is Strings.StatPoe1.ZombieBones)
            {
                bool isUllr = words.MatchNameEn(Strings.Unique.BonesOfUllr, item.Name);
                entrie.ID = isUllr ? Strings.StatPoe1.ZombieBones : Strings.StatPoe1.Zombie;
            }
            else if (entrie.ID is Strings.StatPoe1.Spectre || entrie.ID is Strings.StatPoe1.SpectreBones)
            {
                bool isUllr = words.MatchNameEn(Strings.Unique.BonesOfUllr, item.Name);
                entrie.ID = isUllr ? Strings.StatPoe1.SpectreBones : Strings.StatPoe1.Spectre;
            }
            else if (item.Flag.Slot.Flask && item.Flag.Rarity.Unique)
            {
                bool isCinder = words.MatchNameEn(Strings.Unique.CinderswallowUrn, item.Name);
                bool isDiv = words.MatchNameEn(Strings.Unique.DivinationDistillate, item.Name);

                entrie.ID = entrie.ID is Strings.StatPoe1.FlaskIncRarity1 && isCinder ? Strings.StatPoe1.FlaskIncRarity2
                    : entrie.ID is Strings.StatPoe1.FlaskIncRarity2 && isDiv ? Strings.StatPoe1.FlaskIncRarity1
                    : entrie.ID;
            }
            else if (item.Flag.Jewel.IsJewel && item.Flag.Rarity.Unique)
            {
                if (entrie.ID is Strings.StatPoe1.TheBlueNightmare)
                {
                    bool isBlueDream = words.MatchNameEn(Strings.Unique.TheBlueDream, item.Name);
                    if (isBlueDream)
                    {
                        entrie.ID = Strings.StatPoe1.TheBlueDream;
                    }
                }
            }
            else if (item.Flag.Armour.IsArmour && item.Flag.Rarity.Unique)
            {
                if (entrie.ID is Strings.StatPoe1.FireTakenOld) // The Rat Cage
                {
                    entrie.ID = Strings.StatPoe1.FireTakenNew;
                }
                if (entrie.ID is Strings.StatPoe1.PurityIce1) //  Doryani's Delusion
                {
                    entrie.ID = Strings.StatPoe1.PurityIce2;
                }
                if (entrie.ID is Strings.StatPoe1.PurityFire1) //  Doryani's Delusion
                {
                    entrie.ID = Strings.StatPoe1.PurityFire2;
                }
                if (entrie.ID is Strings.StatPoe1.PurityLightning1) //  Doryani's Delusion
                {
                    entrie.ID = Strings.StatPoe1.PurityLightning2;
                }
            }
            else if (item.Flag.Area.Chronicle)
            {
                bool goContinue = true;
                for (int s = 0; s < Strings.StatPoe1.Temple.RoomList.Length; s++)
                {
                    if (entrie.ID.Contain(Strings.StatPoe1.Temple.RoomList[s]))
                    {
                        goContinue = false;
                        break;
                    }
                }
                if (goContinue) continueLoop = true;
            }
            else if (item.Flag.Weapon.IsWeapon && item.Flag.Rarity.Unique)
            {
                if (entrie.ID is Strings.StatPoe1.PoisonMoreDmg1) // Darkscorn old mod
                {
                    entrie.ID = Strings.StatPoe1.PoisonMoreDmg2;
                }
                bool isDervish = words.MatchNameEn(Strings.Unique.TheDancingDervish, item.Name);
                if (entrie.ID is Strings.StatPoe1.Rampage && isDervish)
                {
                    continueLoop = true;
                }
                bool isTrypanon = words.MatchNameEn(Strings.Unique.ReplicaTrypanon, item.Name);
                if (entrie.ID is Strings.StatPoe1.AccuracyLocal && isTrypanon) // this is not a revert from previous code lines
                {
                    entrie.ID = Strings.StatPoe1.Accuracy;
                }
                bool isNetolKiss = words.MatchNameEn(Strings.Unique.UulNetolsKiss, item.Name);
                if (entrie.ID is Strings.StatPoe1.CurseVulnerability && isNetolKiss)
                {
                    entrie.ID = Strings.StatPoe1.CurseVulnerabilityChance;
                }
            }
            else if (item.Flag.SanctumRelic)
            {
                if (entrie.Type is not Strings.Word.Sanctum)
                {
                    continueLoop = true;
                }
            }
            else if (item.Flag.Jewel.Cluster)
            {
                if (entrie.ID is Strings.StatPoe1.ClusterCurseEffect1)
                {
                    entrie.ID = Strings.StatPoe1.ClusterCurseEffect2;
                }
            }
        }

        if (item.Flag.Area.Logbook)//&& implicitMod
        {
            if (!entrie.ID.Contain(Strings.StatPoe1.Generic.LogbookBoss)
                && !entrie.ID.Contain(Strings.StatPoe1.Generic.LogbookArea)
                && !entrie.ID.Contain(Strings.StatPoe1.Generic.LogbookTwice))
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

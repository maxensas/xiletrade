using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed record ModFilter
{
    private readonly DataManagerService _dm;
    /// <summary>Empty fields will not be added to json</summary>
    internal const int EMPTYFIELD = 99999;

    internal ItemModifier Mod { get; }
    internal FilterResultEntrie Entrie { get; } = new();
    internal ModValue ModValue { get; } = new();
    
    internal bool IsFetched { get; }

    internal ModFilter(DataManagerService dm, ItemModifier mod, ItemData item)
    {
        _dm = dm;
        Mod = mod;

        var inputRegex = GetInputRegex(mod);
        foreach (var filter in _dm.Filter.Result)
        {
            var entries = GetEntrieList(mod, item, inputRegex, filter);
            if (entries.Any())
            {
                var (entrie, min, max) = GetMinMaxEntrie(mod, item, entries);
                if (entrie is not null)
                {
                    ModValue.ListAffix.Add(GetAffixEntrie(item, filter, entrie));
                    if (Entrie.ID == string.Empty)
                    {
                        Entrie = entrie;
                        ModValue.Min = min;
                        ModValue.Max = max;
                    }
                }
            }
            else
            {
                FilterResultEntrie entrie = null;
                if (item.Flag.Logbook && TryGetLogbookEntrie(filter, mod, out var logbookEntrie))
                {
                    entrie = logbookEntrie;
                }
                else if (_dm.Config.Options.GameVersion is 0
                    && TryGetStatWithOptionsEntrie(filter, mod, item, out var statOptionsEntrie))
                {
                    entrie = statOptionsEntrie;
                }

                if (entrie is not null)
                {
                    ModValue.ListAffix.Add(GetAffixEntrie(item, filter, entrie));
                    Entrie = entrie;
                }
            }
        }

        IsFetched = Entrie.ID != string.Empty;
    }

    private static IEnumerable<FilterResultEntrie> GetEntrieList(ItemModifier mod, ItemData item, Regex inputRegex, FilterResult filter)
    {
        var entries = filter.Entries.Where(x => inputRegex.IsMatch(x.Text));
        if (!entries.Any())
        {
            var inputSplit = mod.Parsed.Split("\\n");
            if (inputSplit.Length >= 2)
            {
                var inputRgx = new Regex("^" + inputSplit[0] + "$", RegexOptions.IgnoreCase); // not using escape ?
                entries = filter.Entries.Where(x => inputRgx.IsMatch(x.Text));
            }

            if ((item.Flag.Rare || item.Flag.Magic)
                && filter.Label == Resources.Resources.General015_Explicit && !entries.Any())
            {
                var ConfluxEntries = GetConfluxEntrieList(filter, mod);
                if (ConfluxEntries is not null)
                {
                    entries = ConfluxEntries;
                }
            }

            // multi lines mod, take some time to execute !
            if ((item.Flag.Unique || item.Flag.Magic) && !entries.Any())
            {
                entries = GetMultiLineEntrieList(mod, filter);
            }
        }

        return entries;
    }

    private static IEnumerable<FilterResultEntrie> GetConfluxEntrieList(FilterResult filter, ItemModifier mod)
    {
        var ConfluxEntries = filter.Entries.Where(x => x.ID is Strings.Stat.Conflux);
        if (ConfluxEntries.Any())
        {
            var ConfluxEntrie = ConfluxEntries.First();
            foreach (var opt in ConfluxEntrie.Option.Options)
            {
                if (ConfluxEntrie.Text.Replace("#", opt.Text) == mod.Parsed)
                {
                    return ConfluxEntries;
                }
            }
        }
        return null;
    }

    private static IEnumerable<FilterResultEntrie> GetMultiLineEntrieList(ItemModifier mod, FilterResult filter)
    {
        string modReg = RegexUtil.DecimalPattern().Replace(mod.Parsed, "#");
        var entries = filter.Entries.Where(x => x.Text.StartWith(mod.Parsed + Strings.LF) || x.Text.StartWith(modReg + Strings.LF));
        if (entries.Count() > 1 && mod.NextMod.Length > 0)
        {
            var entriesTmp = entries.Where(x => x.Text.Contain(mod.NextMod));
            if (entriesTmp.Any())
            {
                entries = entriesTmp;
            }
        }
        return entries;
    }

    private (FilterResultEntrie Entrie, double Min, double Max) GetMinMaxEntrie(ItemModifier mod, ItemData item, IEnumerable<FilterResultEntrie> entries)
    {
        var matches1 = RegexUtil.DecimalNoPlusPattern().Matches(mod.Parsed);
        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        foreach (var entrie in entries)
        {
            if (isPoe2 ? SwitchPoe2EntrieId(entrie, item.Flag, item.Name)
                : SwitchPoe1EntrieId(entrie, item.Flag, item.Name))
            {
                continue;
            }

            if (entries.Count() > 1 && entrie.Part.Length > 0)
                continue;

            int idxMin = 0, idxMax = 0;
            bool isMin = false, isMax = false, isBreak = true;

            var matches2 = RegexUtil.DecimalNoPlusDiezePattern().Matches(entrie.Text);
            if (matches1.Count == matches2.Count)
            {
                for (int t = 0; t < matches2.Count; t++)
                {
                    if (matches2[t].Value is "#")
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
                    else if (matches1[t].Value != matches2[t].Value)
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
                var min = isMin && matches.Count > idxMin ? matches[idxMin].Value.ToDoubleEmptyField() : EMPTYFIELD;
                var max = isMax && idxMin < idxMax && matches.Count > idxMax ? matches[idxMax].Value.ToDoubleEmptyField() : EMPTYFIELD;
                return (entrie, min, max);
            }
        }
        return (null, EMPTYFIELD, EMPTYFIELD);
    }

    private AffixFilterEntrie GetAffixEntrie(ItemData item, FilterResult filter, FilterResultEntrie entrie)
    {
        string lblAffix = filter.Label;
        if (_dm.Config.Options.Language > 0) lblAffix = GetTranslatedAffix(lblAffix);
        bool isCorruption = false;
        if (Strings.Stat.dicCorruption.TryGetValue(entrie.ID, out string itemClassList))
        {
            if (itemClassList.Contain(item.Flag.GetItemClass()))
            {
                lblAffix = Resources.Resources.General017_CorruptImp;
                isCorruption = true;
            }
        }
        return new(entrie.ID, lblAffix, isCorruption, item.Flag.Unique && entrie.ID.StartWith(Strings.Words.Explicit));
    }

    private static bool TryGetLogbookEntrie(FilterResult filter, ItemModifier mod
        , out FilterResultEntrie entrie)
    {
        entrie = null;
        var entrieSeek = filter.Entries.FirstOrDefault(x => x.ID.Contain(Strings.Stat.Generic.LogbookBoss));
        if (entrieSeek is not null && entrieSeek.Option.Options.Length > 0
            && entrieSeek.Option.Options.Any(opt => mod.Parsed.Contain(opt.Text)))
        {
            entrie = entrieSeek;
            return true;
        }
        entrieSeek = filter.Entries.FirstOrDefault(x => x.Text.Contain(mod.Parsed));
        if (entrieSeek is not null && entrieSeek.ID.Contain(Strings.Words.Logbook))
        {
            entrie = entrieSeek;
            return true;
        }
        return false;
    }

    private static bool TryGetStatWithOptionsEntrie(FilterResult filter, ItemModifier mod, ItemData item
        , out FilterResultEntrie entrie)
    {
        entrie = null;
        var checkList = BuildStatOptionList(filter.Label, item);
        if (checkList.Count is 0)
        {
            return false;
        }
        var entrieSeek = filter.Entries.Where(x => checkList.Contains(x.ID));
        if (entrieSeek.Any())
        {
            foreach (var resultEntrie in entrieSeek)
            {
                bool cond1 = true, cond2 = true;
                string[] testString = resultEntrie.Text.Split('#');
                if (testString.Length > 1)
                {
                    if (testString[0].Length > 0) cond1 = mod.Parsed.Contain(testString[0]);
                    if (testString[1].Length > 0) cond2 = mod.Parsed.Contain(testString[1].Split(Strings.LF)[0]); // bypass next lines
                }
                if (cond1 && cond2)
                {
                    entrie = resultEntrie;
                    return true;
                }
            }
        }
        return false;
    }

    private static Regex GetInputRegex(ItemModifier mod)
    {
        string inputRegEscape = Regex.Escape(RegexUtil.DecimalPattern().Replace(mod.Parsed, "#"));
        string inputRegPattern = RegexUtil.DiezePattern().Replace(inputRegEscape, RegexUtil.DecimalPatternDieze);
        return new Regex("^" + inputRegPattern + "$", RegexOptions.IgnoreCase);
    }

    private static List<string> BuildStatOptionList(string label, ItemData item)
    {
        var list = new List<string>();

        switch (label)
        {
            case Strings.Label.Enchant:
                if (item.Flag.Amulets) 
                { 
                    list.Add(Strings.Stat.Option.Allocate); 
                }
                if (item.Flag.Jewel) 
                { 
                    list.Add(Strings.Stat.Option.SmallPassive); 
                }
                if (item.Flag.ChargedCompass || item.Flag.Voidstone)
                {
                    list.AddRange([
                        Strings.Stat.Option.CompassHarvest,
                        Strings.Stat.Option.CompassMaster,
                        Strings.Stat.Option.CompassStrongbox,
                        Strings.Stat.Option.CompassBreach
                    ]);
                }
                break;

            case Strings.Label.Implicit when item.Flag.Map:
                list.AddRange([
                    Strings.Stat.Option.MapOccupConq,
                    Strings.Stat.Option.MapOccupElder,
                    Strings.Stat.Option.AreaInflu
                ]);
                break;

            case Strings.Label.Explicit:
                if (item.Flag.Jewel)
                {
                    list.AddRange([
                        Strings.Stat.Option.RingPassive,
                        Strings.Stat.Option.AllocateFlesh,
                        Strings.Stat.Option.AllocateFlame,
                        Strings.Stat.Option.PassivesInRadius
                    ]);
                }
                if (item.Flag.BodyArmours)
                {
                    list.Add(Strings.Stat.Option.Bestial);
                }
                break;
        }

        return list;
    }

    private bool SwitchPoe1EntrieId(FilterResultEntrie entrie, ItemFlag itemIs, string itemName)
    {
        bool continueLoop = false;

        if (entrie.ID.Length > 1)
        {
            if (Strings.Stat.Aura.lSkipMods.Contains(entrie.ID.Split('.')[1]))
            {
                return true;
            }
            var words = _dm.Words;
            if (entrie.ID.Contain(Strings.Words.IndexableSupport))
            {
                bool isShako = words.FirstOrDefault(x => x.NameEn is Strings.Unique.ForbiddenShako).Name == itemName;
                bool isLioneye = words.FirstOrDefault(x => x.NameEn is Strings.Unique.LioneyesVision).Name == itemName;
                //bool isHungryLoop = DataManager.Words.FirstOrDefault(x => x.NameEn is Strings.Unique.TheHungryLoop).Name == itemName;
                bool isBitter = words.FirstOrDefault(x => x.NameEn is Strings.Unique.Bitterdream).Name == itemName;

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
                entrie.ID = itemIs.Weapon ? Strings.Stat.AccuracyLocal : Strings.Stat.Accuracy;
            }
            else if (entrie.ID is Strings.Stat.Armor || entrie.ID is Strings.Stat.ArmorLocal)
            {
                entrie.ID = itemIs.ArmourPiece ? Strings.Stat.ArmorLocal : Strings.Stat.Armor;
            }
            else if (entrie.ID is Strings.Stat.Es || entrie.ID is Strings.Stat.EsLocal)
            {
                entrie.ID = itemIs.ArmourPiece ? Strings.Stat.EsLocal : Strings.Stat.Es;
            }
            else if (entrie.ID is Strings.Stat.Eva || entrie.ID is Strings.Stat.EvaLocal)
            {
                entrie.ID = itemIs.ArmourPiece ? Strings.Stat.EvaLocal : Strings.Stat.Eva;
            }
            else if (entrie.ID is Strings.Stat.HitBlind1 || entrie.ID is Strings.Stat.HitBlind2)
            {
                entrie.ID = itemIs.ArmourPiece ? Strings.Stat.HitBlind2 : Strings.Stat.HitBlind1;
            }
            else if (entrie.ID is Strings.Stat.ImmunityIgnite1 || entrie.ID is Strings.Stat.ImmunityIgnite2)
            {
                entrie.ID = itemIs.Unique ? Strings.Stat.ImmunityIgnite2 : Strings.Stat.ImmunityIgnite1;
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
                entrie.ID = itemIs.Jewel && itemIs.Unique ? Strings.Stat.BlockAttack2 : Strings.Stat.BlockAttack1;
            }
            else if (entrie.ID is Strings.Stat.BlockSpell1 || entrie.ID is Strings.Stat.BlockSpell2)
            {
                entrie.ID = itemIs.Jewel && itemIs.Unique ? Strings.Stat.BlockSpell2 : Strings.Stat.BlockSpell1;
            }
            else if (entrie.ID is Strings.Stat.CoolDownRecovery1 || entrie.ID is Strings.Stat.CoolDownRecovery2)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.CoolDownRecovery2 : Strings.Stat.CoolDownRecovery1;
            }
            else if (entrie.ID is Strings.Stat.IncCritAgainst1 && itemIs.Jewel && itemIs.Unique)
            {
                entrie.ID = Strings.Stat.IncCritAgainst2;
            }
            else if (entrie.ID is Strings.Stat.PeneFire || entrie.ID is Strings.Stat.PeneFireTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.PeneFireTincture : Strings.Stat.PeneFire;
            }
            else if (entrie.ID is Strings.Stat.PeneCold || entrie.ID is Strings.Stat.PeneColdTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.PeneColdTincture : Strings.Stat.PeneCold;
            }
            else if (entrie.ID is Strings.Stat.PeneLight || entrie.ID is Strings.Stat.PeneLightTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.PeneLightTincture : Strings.Stat.PeneLight;
            }
            else if (entrie.ID is Strings.Stat.ManaPerKill || entrie.ID is Strings.Stat.ManaPerKillTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.ManaPerKillTincture : Strings.Stat.ManaPerKill;
            }
            else if (entrie.ID is Strings.Stat.AoeKill || entrie.ID is Strings.Stat.AoeKillTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.AoeKillTincture : Strings.Stat.AoeKill;
            }
            else if (entrie.ID is Strings.Stat.CritFullLife || entrie.ID is Strings.Stat.CritFullLifeTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.CritFullLifeTincture : Strings.Stat.CritFullLife;
            }
            else if (entrie.ID is Strings.Stat.PhasingKill || entrie.ID is Strings.Stat.PhasingKillTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.PhasingKillTincture : Strings.Stat.PhasingKill;
            }
            else if (entrie.ID is Strings.Stat.ConcGround || entrie.ID is Strings.Stat.ConcGroundTincture)
            {
                entrie.ID = itemIs.Tincture ? Strings.Stat.ConcGroundTincture : Strings.Stat.ConcGround;
            }
            else if (entrie.ID is Strings.Stat.StrikeRange && itemIs.Tincture)
            {
                entrie.ID = Strings.Stat.StrikeRangeTincture;
            }
            else if (entrie.ID is Strings.Stat.StrInt && itemIs.Charm)
            {
                entrie.ID = Strings.Stat.StrIntCharm;
            }
            else if (entrie.ID is Strings.Stat.BlockDmg || entrie.ID is Strings.Stat.BlockDmgJewCharm)
            {
                entrie.ID = itemIs.Charm || itemIs.Jewel ? Strings.Stat.BlockDmgJewCharm : Strings.Stat.BlockDmg;
            }
            else if (entrie.ID is Strings.Stat.Onslaught 
                || entrie.ID is Strings.Stat.OnslaughtWeaponCharm 
                || entrie.ID is Strings.Stat.OnslaughtAmulet)
            {
                entrie.ID = itemIs.Charm || itemIs.Weapon ? Strings.Stat.OnslaughtWeaponCharm
                    : itemIs.Amulets && itemIs.Unique ? Strings.Stat.OnslaughtAmulet : Strings.Stat.Onslaught;
            }
            else if (entrie.ID is Strings.Stat.ReduceEle || entrie.ID is Strings.Stat.ReduceEleGorgon)
            {
                bool isGorgon = words.FirstOrDefault(x => x.NameEn is Strings.Unique.GorgonsGaze).Name == itemName;
                entrie.ID = isGorgon ? Strings.Stat.ReduceEleGorgon : Strings.Stat.ReduceEle;
            }
            else if (entrie.ID is Strings.Stat.ShockSpread || entrie.ID is Strings.Stat.ShockSpreadEsh)
            {
                bool isEsh = words.FirstOrDefault(x => x.NameEn is Strings.Unique.EshsMirror).Name == itemName;
                entrie.ID = isEsh ? Strings.Stat.ShockSpreadEsh : Strings.Stat.ShockSpread;
            }
            else if (entrie.ID is Strings.Stat.Zombie || entrie.ID is Strings.Stat.ZombieBones)
            {
                bool isUllr = words.FirstOrDefault(x => x.NameEn is Strings.Unique.BonesOfUllr).Name == itemName;
                entrie.ID = isUllr ? Strings.Stat.ZombieBones : Strings.Stat.Zombie;
            }
            else if (entrie.ID is Strings.Stat.Spectre || entrie.ID is Strings.Stat.SpectreBones)
            {
                bool isUllr = words.FirstOrDefault(x => x.NameEn is Strings.Unique.BonesOfUllr).Name == itemName;
                entrie.ID = isUllr ? Strings.Stat.SpectreBones : Strings.Stat.Spectre;
            }
            else if (itemIs.Flask && itemIs.Unique)
            {
                bool isCinder = words.FirstOrDefault(x => x.NameEn is Strings.Unique.CinderswallowUrn).Name == itemName;
                bool isDiv = words.FirstOrDefault(x => x.NameEn is Strings.Unique.DivinationDistillate).Name == itemName;

                entrie.ID = entrie.ID is Strings.Stat.FlaskIncRarity1 && isCinder ? Strings.Stat.FlaskIncRarity2
                    : entrie.ID is Strings.Stat.FlaskIncRarity2 && isDiv ? Strings.Stat.FlaskIncRarity1
                    : entrie.ID;
            }
            else if (itemIs.Jewel && itemIs.Unique)
            {
                if (entrie.ID is Strings.Stat.TheBlueNightmare)
                {
                    bool isBlueDream = words.FirstOrDefault(x => x.NameEn is Strings.Unique.TheBlueDream).Name == itemName;
                    if (isBlueDream)
                    {
                        entrie.ID = Strings.Stat.TheBlueDream;
                    }
                }
            }
            else if (itemIs.ArmourPiece && itemIs.Unique)
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
            else if (itemIs.Chronicle)
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
            else if (itemIs.Weapon && itemIs.Unique)
            {
                if (entrie.ID is Strings.Stat.PoisonMoreDmg1) // Darkscorn old mod
                {
                    entrie.ID = Strings.Stat.PoisonMoreDmg2;
                }

                bool isDervish = words.FirstOrDefault(x => x.NameEn is Strings.Unique.TheDancingDervish).Name == itemName;
                if (entrie.ID is Strings.Stat.Rampage && isDervish)
                {
                    continueLoop = true;
                }
                bool isTrypanon = words.FirstOrDefault(x => x.NameEn is Strings.Unique.ReplicaTrypanon).Name == itemName;
                if (entrie.ID is Strings.Stat.AccuracyLocal && isTrypanon) // this is not a revert from previous code lines
                {
                    entrie.ID = Strings.Stat.Accuracy;
                }
                bool isNetolKiss = words.FirstOrDefault(x => x.NameEn is Strings.Unique.UulNetolsKiss).Name == itemName;
                if (entrie.ID is Strings.Stat.CurseVulnerability && isNetolKiss)
                {
                    entrie.ID = Strings.Stat.CurseVulnerabilityChance;
                }
            }
            else if (itemIs.SanctumRelic)
            {
                if (entrie.Type is not Strings.Words.Sanctum)
                {
                    continueLoop = true;
                }
            }
        }

        if (itemIs.Logbook)//&& implicitMod
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

    private static bool SwitchPoe2EntrieId(FilterResultEntrie entrie, ItemFlag itemIs, string itemName)
    {
        bool continueLoop = false;

        if (entrie.ID.Length is 0)
        {
            return false;
        }

        if (itemIs.Waystones || itemIs.Tablet)
        {
            if (entrie.ID is Strings.StatPoe2.IncXpGain1)
            {
                entrie.ID = Strings.StatPoe2.IncXpGain2;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.IncXpGain2)
            {
                entrie.ID = Strings.StatPoe2.IncXpGain1;
            }
        }

        if (itemIs.Flask)
        {
            if (entrie.ID is Strings.StatPoe2.IncDuration2)
            {
                entrie.ID = Strings.StatPoe2.IncDuration1;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.IncDuration1)
            {
                entrie.ID = Strings.StatPoe2.IncDuration2;
            }
        }

        if (itemIs.Unique && itemIs.Amulets)
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

        if (itemIs.Jewel)
        {
            if (entrie.ID is Strings.StatPoe2.RecoverManaKill1)
            {
                entrie.ID = Strings.StatPoe2.RecoverManaKill2;
            }
            if (entrie.ID is Strings.StatPoe2.IncBlock1)
            {
                entrie.ID = Strings.StatPoe2.IncBlock2;
            }
        }
        else
        {
            if (entrie.ID is Strings.StatPoe2.RecoverManaKill2)
            {
                entrie.ID = Strings.StatPoe2.RecoverManaKill1;
            }
            if (entrie.ID is Strings.StatPoe2.IncBlock2)
            {
                entrie.ID = Strings.StatPoe2.IncBlock1;
            }
        }

        if (itemIs.Weapon)
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
        }

        if (itemIs.ArmourPiece)
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

        return continueLoop;
    }

    private static string GetTranslatedAffix(string affix)
    {
        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

        return affix == rm.GetString(Strings.Resource.Enchant, cultureEn) ? Resources.Resources.General011_Enchant
            : affix == rm.GetString(Strings.Resource.Crafted, cultureEn) ? Resources.Resources.General012_Crafted
            : affix == rm.GetString(Strings.Resource.Implicit, cultureEn) ? Resources.Resources.General013_Implicit
            : affix == rm.GetString(Strings.Resource.Pseudo, cultureEn) ? Resources.Resources.General014_Pseudo
            : affix == rm.GetString(Strings.Resource.Explicit, cultureEn) ? Resources.Resources.General015_Explicit
            : affix == rm.GetString(Strings.Resource.Fractured, cultureEn) ? Resources.Resources.General016_Fractured
            : affix == rm.GetString(Strings.Resource.CorruptImp, cultureEn) ? Resources.Resources.General017_CorruptImp
            : affix == rm.GetString(Strings.Resource.Monster, cultureEn) ? Resources.Resources.General018_Monster
            : affix == rm.GetString(Strings.Resource.Scourge, cultureEn) ? Resources.Resources.General099_Scourge
            : affix;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed record ModFilter
{
    private readonly DataManagerService _dm;
    /// <summary>Empty fields will not be added to json</summary>
    internal const int EMPTYFIELD = 99999;

    internal ItemModifier Mod { get; }
    internal FilterResultEntrie Entrie { get; private set; } = new();
    internal ModValue ModValue { get; } = new();
    
    internal bool IsFetched { get; }

    internal ModFilter(DataManagerService dm, ItemModifier mod, ItemData item, AffixFlag affix)
    {
        _dm = dm;
        Mod = mod;

        var inputRegex = GetInputRegex(mod);

        foreach (var filter in _dm.Filter.Result)
        {
            var entries = FindEntries(filter, mod, item, inputRegex);

            if (entries.Count > 0)
            {
                var (entrie, min, max) = GetMinMaxEntrie(mod, item, entries);
                if (entrie is not null)
                {
                    ModValue.ListAffix.Add(GetAffixEntrie(item, filter, entrie, affix));

                    if (Entrie.ID == string.Empty)
                    {
                        Entrie = entrie;
                        ModValue.Min = min;
                        ModValue.Max = max;
                    }
                }
                continue;
            }

            var fbEntrie = ProcessFallback(filter, mod, item);
            if (fbEntrie is not null)
            {
                ModValue.ListAffix.Add(GetAffixEntrie(item, filter, fbEntrie, affix));
                Entrie = fbEntrie;
            }
        }

        IsFetched = Entrie.ID != string.Empty;
    }

    private static List<FilterResultEntrie> FindEntries(FilterResult filter, 
        ItemModifier mod, ItemData item, Regex inputRegex)
    {
        // a) simple match with the regex
        var entries = filter.GetMatchEntriesList(inputRegex);
        if (entries.Count > 0)
            return entries;

        // b) multi-line mod (first line)
        var inputSplit = mod.Parsed.Split("\\n");
        if (inputSplit.Length >= 2)
        {
            var inputRgx = new Regex("^" + inputSplit[0] + "$", RegexOptions.IgnoreCase);
            entries = filter.GetMatchEntriesList(inputRgx);
            if (entries.Count > 0)
                return entries;
        }

        // c) ConfluxEntry
        if ((item.Flag.Rare || item.Flag.Magic) &&
            filter.Label == Resources.Resources.General015_Explicit)
        {
            var confluxEntrie = GetConfluxEntrie(filter, mod);
            if (confluxEntrie is not null)
                return new List<FilterResultEntrie> { confluxEntrie };
        }

        // d) Full Multi-line
        if (item.Flag.Unique || item.Flag.Magic)
        {
            entries = GetMultiLineEntrieList(mod, filter);
        }

        return entries ?? new List<FilterResultEntrie>();
    }

    private FilterResultEntrie ProcessFallback(FilterResult filter, 
        ItemModifier mod, ItemData item)
    {
        if (item.Flag.Logbook && TryGetLogbookEntrie(filter, mod, out var logbookEntrie))
        {
            return logbookEntrie;
        }
        else if (_dm.Config.Options.GameVersion is 0 &&
                 TryGetStatWithOptionsEntrie(filter, mod, item, out var statOptionsEntrie))
        {
            return statOptionsEntrie;
        }
        return null;
    }

    private static FilterResultEntrie GetConfluxEntrie(FilterResult filter, ItemModifier mod)
    {
        var ConfluxEntrie = filter.FindEntryById(Strings.Stat.Conflux);
        if (ConfluxEntrie is not null)
        {
            foreach (var opt in ConfluxEntrie.Option.Options)
            {
                if (ConfluxEntrie.Text.Replace("#", opt.Text) == mod.Parsed)
                {
                    return ConfluxEntrie;
                }
            }
        }
        return null;
    }

    private static List<FilterResultEntrie> GetMultiLineEntrieList(ItemModifier mod, FilterResult filter)
    {
        string modReg = RegexUtil.DecimalPattern().Replace(mod.Parsed, "#");
        var entries = filter.GetWhereStartsWithList([mod.Parsed, modReg]);
        if (entries.Count > 1 && mod.NextModInfo.ModKind.Length > 0)
        {
            var filtered = new List<FilterResultEntrie>();
            foreach (var entry in entries)
            {
                if (entry.Text.Contains(mod.NextModInfo.ModKind))
                {
                    filtered.Add(entry);
                }
            }
            if (filtered.Count > 0)
            {
                entries = filtered;
            }
        }
        return entries;
    }

    private (FilterResultEntrie Entrie, double Min, double Max) GetMinMaxEntrie(ItemModifier mod, ItemData item, List<FilterResultEntrie> entries)
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

            if (entries.Count > 1 && entrie.Part.Length > 0)
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

    private AffixFilterEntrie GetAffixEntrie(ItemData item, FilterResult filter, FilterResultEntrie entrie, AffixFlag affix)
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
        return new(entrie.ID, lblAffix, entrie.Type, isCorruption, item.Flag.Unique && entrie.ID.StartWith(Strings.Words.Explicit), isMutated: affix.Mutated);
    }

    private static bool TryGetLogbookEntrie(FilterResult filter, ItemModifier mod
        , out FilterResultEntrie entrie)
    {
        entrie = null;
        var entrieSeek = filter.FindEntryById(Strings.Stat.Generic.LogbookBoss, sequenceEquality: false);
        if (entrieSeek is not null && entrieSeek.Option.Options.Length > 0
            && entrieSeek.Option.Options.Any(opt => mod.Parsed.Contain(opt.Text)))
        {
            entrie = entrieSeek;
            return true;
        }
        entrieSeek = filter.FindEntryByType(mod.Parsed, sequenceEquality: false);
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
        var checkList = GetStatOptionList(filter.Label, item);
        if (checkList.Count is 0)
        {
            return false;
        }
        foreach (var resultEntrie in filter.Entries)
        {
            if (!checkList.Contains(resultEntrie.ID))
                continue;

            bool cond1 = true, cond2 = true;
            var testString = resultEntrie.Text.Split('#');
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
        return false;
    }

    private static Regex GetInputRegex(ItemModifier mod)
    {
        string inputRegEscape = Regex.Escape(RegexUtil.DecimalPattern().Replace(mod.Parsed, "#"));
        string inputRegPattern = RegexUtil.DiezePattern().Replace(inputRegEscape, RegexUtil.DecimalPatternDieze);
        return new Regex("^" + inputRegPattern + "$", RegexOptions.IgnoreCase);
    }

    private static List<string> GetStatOptionList(string label, ItemData item)
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
                bool isShako = words.MatchNameEn(Strings.Unique.ForbiddenShako, itemName);
                bool isLioneye = words.MatchNameEn(Strings.Unique.LioneyesVision, itemName);
                //bool isHungryLoop = words.MatchNameEn(Strings.Unique.TheHungryLoop, itemName);
                bool isBitter = words.MatchNameEn(Strings.Unique.Bitterdream, itemName);

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
                bool isGorgon = words.MatchNameEn(Strings.Unique.GorgonsGaze, itemName);
                entrie.ID = isGorgon ? Strings.Stat.ReduceEleGorgon : Strings.Stat.ReduceEle;
            }
            else if (entrie.ID is Strings.Stat.ShockSpread || entrie.ID is Strings.Stat.ShockSpreadEsh)
            {
                bool isEsh = words.MatchNameEn(Strings.Unique.EshsMirror, itemName);
                entrie.ID = isEsh ? Strings.Stat.ShockSpreadEsh : Strings.Stat.ShockSpread;
            }
            else if (entrie.ID is Strings.Stat.Zombie || entrie.ID is Strings.Stat.ZombieBones)
            {
                bool isUllr = words.MatchNameEn(Strings.Unique.BonesOfUllr, itemName);
                entrie.ID = isUllr ? Strings.Stat.ZombieBones : Strings.Stat.Zombie;
            }
            else if (entrie.ID is Strings.Stat.Spectre || entrie.ID is Strings.Stat.SpectreBones)
            {
                bool isUllr = words.MatchNameEn(Strings.Unique.BonesOfUllr, itemName);
                entrie.ID = isUllr ? Strings.Stat.SpectreBones : Strings.Stat.Spectre;
            }
            else if (itemIs.Flask && itemIs.Unique)
            {
                bool isCinder = words.MatchNameEn(Strings.Unique.CinderswallowUrn, itemName);
                bool isDiv = words.MatchNameEn(Strings.Unique.DivinationDistillate, itemName);

                entrie.ID = entrie.ID is Strings.Stat.FlaskIncRarity1 && isCinder ? Strings.Stat.FlaskIncRarity2
                    : entrie.ID is Strings.Stat.FlaskIncRarity2 && isDiv ? Strings.Stat.FlaskIncRarity1
                    : entrie.ID;
            }
            else if (itemIs.Jewel && itemIs.Unique)
            {
                if (entrie.ID is Strings.Stat.TheBlueNightmare)
                {
                    bool isBlueDream = words.MatchNameEn(Strings.Unique.TheBlueDream, itemName);
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
                bool isDervish = words.MatchNameEn(Strings.Unique.TheDancingDervish, itemName);
                if (entrie.ID is Strings.Stat.Rampage && isDervish)
                {
                    continueLoop = true;
                }
                bool isTrypanon = words.MatchNameEn(Strings.Unique.ReplicaTrypanon, itemName);
                if (entrie.ID is Strings.Stat.AccuracyLocal && isTrypanon) // this is not a revert from previous code lines
                {
                    entrie.ID = Strings.Stat.Accuracy;
                }
                bool isNetolKiss = words.MatchNameEn(Strings.Unique.UulNetolsKiss, itemName);
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

    private bool SwitchPoe2EntrieId(FilterResultEntrie entrie, ItemFlag itemIs, string itemName)
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
        //uniques
        var words = _dm.Words;
        if (entrie.ID is Strings.StatPoe2.Spirit1 or Strings.StatPoe2.Spirit2)
        {
            bool isUnborn = words.MatchNameEn(Strings.UniqueTwo.TheUnbornLich, itemName);
            entrie.ID = isUnborn ? Strings.StatPoe2.Spirit1 : Strings.StatPoe2.Spirit2;
        }
        if (entrie.ID is Strings.StatPoe2.IncSpirit1 or Strings.StatPoe2.IncSpirit2)
        {
            bool isKulemak = words.MatchNameEn(Strings.UniqueTwo.GripofKulemak, itemName);
            entrie.ID = isKulemak ? Strings.StatPoe2.IncSpirit1 : Strings.StatPoe2.IncSpirit2;
        }
        if (entrie.ID is Strings.StatPoe2.Daze1 or Strings.StatPoe2.Daze2)
        {
            bool isNazir = words.MatchNameEn(Strings.UniqueTwo.NazirsJudgement, itemName);
            entrie.ID = isNazir ? Strings.StatPoe2.Daze1 : Strings.StatPoe2.Daze2;
        }
        if (entrie.ID is Strings.StatPoe2.Aftershocks1 or Strings.StatPoe2.Aftershocks2)
        {
            bool isHrimnors = words.MatchNameEn(Strings.UniqueTwo.HrimnorsHymn, itemName);
            entrie.ID = isHrimnors ? Strings.StatPoe2.Aftershocks2 : Strings.StatPoe2.Aftershocks1;
        }
        if (entrie.ID is Strings.StatPoe2.RandomShrine1 or Strings.StatPoe2.RandomShrine2)
        {
            bool isHammer = words.MatchNameEn(Strings.UniqueTwo.TheHammerofFaith, itemName);
            entrie.ID = isHammer ? Strings.StatPoe2.RandomShrine2 : Strings.StatPoe2.RandomShrine1;
        }
        if (entrie.ID is Strings.StatPoe2.Charm1 or Strings.StatPoe2.Charm2)
        {
            bool isElevore = words.MatchNameEn(Strings.UniqueTwo.Elevore, itemName);
            entrie.ID = isElevore ? Strings.StatPoe2.Charm2 : Strings.StatPoe2.Charm1;
        }
        if (entrie.ID is Strings.StatPoe2.Decompose1 or Strings.StatPoe2.Decompose2)
        {
            bool isCorpsewade = words.MatchNameEn(Strings.UniqueTwo.Corpsewade, itemName);
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
        bool IsFlesh() => words.MatchNameEn(Strings.UniqueTwo.FleshCrucible, itemName);
        if (entrie.ID is Strings.StatPoe2.PainAttunement1 or Strings.StatPoe2.PainAttunement2)
        {
            entrie.ID = IsFlesh() ? Strings.StatPoe2.PainAttunement1 : Strings.StatPoe2.PainAttunement2;
        }
        if (entrie.ID is Strings.StatPoe2.GiantsBlood1 or Strings.StatPoe2.GiantsBlood2)
        {
            entrie.ID = IsFlesh() ? Strings.StatPoe2.GiantsBlood1 : Strings.StatPoe2.GiantsBlood2;
        }
        if (entrie.ID is Strings.StatPoe2.UnwaveringStance1 or Strings.StatPoe2.UnwaveringStance2)
        {
            entrie.ID = IsFlesh() ? Strings.StatPoe2.UnwaveringStance1 : Strings.StatPoe2.UnwaveringStance2;
        }
        if (entrie.ID is Strings.StatPoe2.EldritchBattery1 or Strings.StatPoe2.EldritchBattery2)
        {
            entrie.ID = IsFlesh() ? Strings.StatPoe2.EldritchBattery1 : Strings.StatPoe2.EldritchBattery2;
        }
        if (entrie.ID is Strings.StatPoe2.BloodMagic1 or Strings.StatPoe2.BloodMagic2)
        {
            entrie.ID = IsFlesh() ? Strings.StatPoe2.BloodMagic1 : Strings.StatPoe2.BloodMagic2;
        }
        if (entrie.ID is Strings.StatPoe2.IronReflexes1 or Strings.StatPoe2.IronReflexes2)
        {
            entrie.ID = IsFlesh() ? Strings.StatPoe2.IronReflexes1 : Strings.StatPoe2.IronReflexes2;
        }
        if (entrie.ID is Strings.StatPoe2.GlancingBlows1 or Strings.StatPoe2.GlancingBlows2)
        {
            entrie.ID = IsFlesh() ? Strings.StatPoe2.GlancingBlows1 : Strings.StatPoe2.GlancingBlows2;
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
            : affix == rm.GetString(Strings.Resource.Desecrated, cultureEn) ? Resources.Resources.General158_Desecrated
            : affix;
    }
}

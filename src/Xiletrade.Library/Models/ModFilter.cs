using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed class ModFilter
{
    internal string ID { get; private set; } = string.Empty;
    internal string Text { get; private set; } = string.Empty;
    internal string Type { get; private set; } = string.Empty;
    internal string Part { get; private set; } = string.Empty;
    internal FilterResultOption Option { get; private set; } = new FilterResultOption();

    internal bool IsFetched { get; private set; }

    internal ModFilter(string input, string[] data, int dataIndex, ItemFlag itemIs, string itemName, string itemType, string itemClass, out ModValue modVal)
    {
        modVal = new();

        string inputRegEscape = Regex.Escape(RegexUtil.DecimalPattern().Replace(input, "#"));
        string inputRegPattern = RegexUtil.DiezePattern().Replace(inputRegEscape, RegexUtil.DecimalPatternDieze);

        var inputRegex = new Regex("^" + inputRegPattern + "$", RegexOptions.IgnoreCase);

        Strings.dicPublicID.TryGetValue(itemType, out string publicID);
        publicID ??= string.Empty;

        foreach (var filterResult in DataManager.Filter.Result)
        {
            var entries = filterResult.Entries.Where(x => inputRegex.IsMatch(x.Text));
            if (!entries.Any())
            {
                var inputSplit = input.Split("\\n");
                if (inputSplit.Length >= 2)
                {
                    var inputRgx = new Regex("^" + inputSplit[0] + "$", RegexOptions.IgnoreCase); // not using escape ?
                    entries = filterResult.Entries.Where(x => inputRgx.IsMatch(x.Text));
                }

                if ((itemIs.Rare || itemIs.Magic) && filterResult.Label == Resources.Resources.General015_Explicit)
                {
                    if (!entries.Any())
                    {
                        var ConfluxEntries = filterResult.Entries.Where(x => x.ID is Strings.Stat.Conflux);
                        if (ConfluxEntries.Any())
                        {
                            var ConfluxEntrie = ConfluxEntries.First();
                            foreach (var opt in ConfluxEntrie.Option.Options)
                            {
                                if (ConfluxEntrie.Text.Replace("#", opt.Text) == input)
                                {
                                    entries = ConfluxEntries;
                                    //double optToSelect = (double)opt.ID;
                                }
                            }
                        }
                    }
                }

                // multi lines mod, take some time to execute !
                if (itemIs.Unique || itemIs.Magic) // DataManager.Config.Options.DevMode || itemIs.Watchstone
                {
                    if (!entries.Any())
                    {
                        string modReg = RegexUtil.DecimalPattern().Replace(input, "#");
                        entries = filterResult.Entries.Where(x => x.Text.StartsWith(input + Strings.LF, StringComparison.Ordinal) || x.Text.StartsWith(modReg + Strings.LF, StringComparison.Ordinal));
                        if (entries.Count() > 1)
                        {
                            if ((dataIndex + 1 < data.Length) && data[dataIndex + 1].Length > 0)
                            {
                                string modKind = RegexUtil.DecimalPattern().Replace(data[dataIndex + 1], "#");
                                var entriesTmp = entries.Where(x => x.Text.Contains(modKind, StringComparison.Ordinal));
                                if (entriesTmp.Any())
                                {
                                    entries = entriesTmp;
                                }
                            }
                        }
                    }
                }
            }
            if (entries.Any())
            {
                var matches1 = RegexUtil.DecimalNoPlusPattern().Matches(input);
                var isPoe2 = DataManager.Config.Options.GameVersion is 1;
                foreach (var entrie in entries)
                {
                    if (isPoe2 ? SwitchPoe2EntrieId(entrie, itemIs, itemName) : SwitchPoe1EntrieId(entrie, itemIs, itemName))
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

                    if (isBreak)
                    {
                        string lblAffix = filterResult.Label;
                        if (DataManager.Config.Options.Language > 0) lblAffix = Modifier.TranslateAffix(lblAffix);

                        bool isCorruption = false;
                        if (Strings.Stat.dicCorruption.TryGetValue(entrie.ID, out string intID) && publicID?.Length > 0)
                        {
                            if (intID.Contains(publicID, StringComparison.Ordinal))
                            {
                                lblAffix = Resources.Resources.General017_CorruptImp;
                                isCorruption = true;
                            }
                        }
                        modVal.ListAffix.Add(new(entrie.ID, lblAffix, isCorruption, itemIs.Unique && entrie.ID.StartsWith("explicit", StringComparison.Ordinal)));
                        if (ID == string.Empty)
                        {
                            var id_split = entrie.ID.Split('.');

                            SetFilter(entrie);

                            var matches = RegexUtil.DecimalNoPlusPattern().Matches(input);

                            if (itemIs.SanctumRelic) // TO update with other unparsed values not done yet
                            {
                                isMin = true;
                            }

                            modVal.Min = isMin && matches.Count > idxMin ? matches[idxMin].Value.ToDoubleEmptyField() : Modifier.EMPTYFIELD;
                            modVal.Max = isMax && idxMin < idxMax && matches.Count > idxMax ? matches[idxMax].Value.ToDoubleEmptyField() : Modifier.EMPTYFIELD;

                            if (entrie.ID is Strings.Stat.NecroExplicit) // invert
                            {
                                modVal.Max = modVal.Min;
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                FilterResultEntrie entrie = null;
                if (itemIs.Logbook)
                {
                    var entrieSeek = filterResult.Entries.FirstOrDefault(x => x.ID.Contains(Strings.Stat.LogbookBoss, StringComparison.Ordinal));
                    if (entrieSeek is not null && entrieSeek.Option.Options.Length > 0)
                    {
                        var entrieSeek2 =
                                from opt in entrieSeek.Option.Options
                                where input.Contains(opt.Text, StringComparison.Ordinal)
                                select entrieSeek;
                        if (entrieSeek2.Any())
                        {
                            entrie = entrieSeek2.First();
                        }
                    }

                    entrieSeek = filterResult.Entries.FirstOrDefault(x => x.Text.Contains(input, StringComparison.Ordinal));
                    if (entrieSeek is not null && entrieSeek.ID.Contains("logbook", StringComparison.Ordinal))
                    {
                        entrie = entrieSeek;
                    }
                }
                else if (DataManager.Config.Options.GameVersion is 0)
                {
                    List<string> checkList = new();
                    if (filterResult.Label is Strings.Label.Enchant)
                    {
                        if (itemClass == Resources.Resources.ItemClass_amulets)
                        {
                            checkList.Add(Strings.Stat.Allocate);
                        }
                        else if (itemClass == Resources.Resources.ItemClass_jewels)
                        {
                            checkList.Add(Strings.Stat.SmallPassive);
                        }
                        else if (itemIs.ChargedCompass || itemIs.Voidstone)
                        {
                            checkList.AddRange([Strings.Stat.CompassHarvest,
                                            Strings.Stat.CompassMaster,
                                            Strings.Stat.CompassStrongbox,
                                            Strings.Stat.CompassBreach]);
                        }
                    }
                    else if (filterResult.Label == Strings.Label.Implicit)
                    {
                        if (itemClass == Resources.Resources.ItemClass_maps)
                        {
                            checkList.AddRange([Strings.Stat.MapOccupConq,
                                            Strings.Stat.MapOccupElder,
                                            Strings.Stat.AreaInflu]);
                        }
                    }
                    else if (filterResult.Label == Strings.Label.Explicit)
                    {
                        if (itemClass == Resources.Resources.ItemClass_jewels)
                        {
                            checkList.AddRange([Strings.Stat.RingPassive,
                                            Strings.Stat.AllocateFlesh,
                                            Strings.Stat.AllocateFlame,
                                            Strings.Stat.PassivesInRadius]);
                        }
                        if (itemClass == Resources.Resources.ItemClass_bodyArmours)
                        {
                            checkList.Add(Strings.Stat.Bestial);
                        }
                    }
                    if (checkList.Count > 0)
                    {
                        var entrieSeek = filterResult.Entries.Where(x => checkList.Contains(x.ID));
                        if (entrieSeek.Any())
                        {
                            foreach (var resultEntrie in entrieSeek)
                            {
                                bool cond1 = true, cond2 = true;
                                string[] testString = resultEntrie.Text.Split('#');
                                if (testString.Length > 1)
                                {
                                    if (testString[0].Length > 0) cond1 = input.Contains(testString[0], StringComparison.Ordinal);
                                    if (testString[1].Length > 0) cond2 = input.Contains(testString[1].Split(Strings.LF)[0], StringComparison.Ordinal); // bypass next lines
                                }

                                if (cond1 && cond2)
                                {
                                    entrie = resultEntrie;
                                }
                            }
                        }
                    }
                }

                if (entrie is not null)
                {
                    string lblAffix = filterResult.Label;
                    if (DataManager.Config.Options.Language > 0) lblAffix = Modifier.TranslateAffix(lblAffix);
                    bool isCorruption = false;
                    if (Strings.Stat.dicCorruption.TryGetValue(entrie.ID, out string intID) && publicID?.Length > 0)
                    {
                        if (intID.Contains(publicID, StringComparison.Ordinal))
                        {
                            lblAffix = Resources.Resources.General017_CorruptImp;
                            isCorruption = true;
                        }
                    }
                    modVal.ListAffix.Add(new(entrie.ID, lblAffix, isCorruption, itemIs.Unique && entrie.ID.StartsWith("explicit", StringComparison.Ordinal)));
                    SetFilter(entrie);
                }
            }
        }

        IsFetched = ID != string.Empty;
    }

    private static bool SwitchPoe1EntrieId(FilterResultEntrie entrie, ItemFlag itemIs, string itemName)
    {
        bool continueLoop = false;

        if (entrie.ID.Length > 1)
        {
            if (Strings.Stat.lSkipOldMods.Contains(entrie.ID.Split('.')[1]))
            {
                return true;
            }

            if (entrie.ID.Contains("indexable_support", StringComparison.Ordinal))
            {
                bool isShako = DataManager.Words.FirstOrDefault(x => x.NameEn is "Forbidden Shako").Name == itemName;
                bool isLioneye = DataManager.Words.FirstOrDefault(x => x.NameEn is "Lioneye's Vision").Name == itemName;
                //bool isHungryLoop = DataManager.Words.FirstOrDefault(x => x.NameEn is "The Hungry Loop").Name == itemName;
                bool isBitter = DataManager.Words.FirstOrDefault(x => x.NameEn is "Bitterdream").Name == itemName;

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
            if (entrie.ID is Strings.Stat.HitBlind1 || entrie.ID is Strings.Stat.HitBlind2)
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
            else if (entrie.ID is Strings.Stat.Onslaught || entrie.ID is Strings.Stat.OnslaughtWeaponCharm)
            {
                entrie.ID = itemIs.Charm || itemIs.Weapon ? Strings.Stat.OnslaughtWeaponCharm : Strings.Stat.Onslaught;
            }
            else if (entrie.ID is Strings.Stat.ReduceEle || entrie.ID is Strings.Stat.ReduceEleGorgon)
            {
                bool isGorgon = DataManager.Words.FirstOrDefault(x => x.NameEn is "Gorgon's Gaze").Name == itemName;
                entrie.ID = isGorgon ? Strings.Stat.ReduceEleGorgon : Strings.Stat.ReduceEle;
            }
            else if (entrie.ID is Strings.Stat.ShockSpread || entrie.ID is Strings.Stat.ShockSpreadEsh)
            {
                bool isEsh = DataManager.Words.FirstOrDefault(x => x.NameEn is "Esh's Mirror").Name == itemName;
                entrie.ID = isEsh ? Strings.Stat.ShockSpreadEsh : Strings.Stat.ShockSpread;
            }
            else if (entrie.ID is Strings.Stat.Zombie || entrie.ID is Strings.Stat.ZombieBones)
            {
                bool isUllr = DataManager.Words.FirstOrDefault(x => x.NameEn is "Bones of Ullr").Name == itemName;
                entrie.ID = isUllr ? Strings.Stat.ZombieBones : Strings.Stat.Zombie;
            }
            else if (entrie.ID is Strings.Stat.Spectre || entrie.ID is Strings.Stat.SpectreBones)
            {
                bool isUllr = DataManager.Words.FirstOrDefault(x => x.NameEn is "Bones of Ullr").Name == itemName;
                entrie.ID = isUllr ? Strings.Stat.SpectreBones : Strings.Stat.Spectre;
            }
            else if (itemIs.Flask && itemIs.Unique)
            {
                bool isCinder = DataManager.Words.FirstOrDefault(x => x.NameEn is "Cinderswallow Urn").Name == itemName;
                bool isDiv = DataManager.Words.FirstOrDefault(x => x.NameEn is "Divination Distillate").Name == itemName;

                entrie.ID = entrie.ID is Strings.Stat.FlaskIncRarity1 && isCinder ? Strings.Stat.FlaskIncRarity2
                    : entrie.ID is Strings.Stat.FlaskIncRarity2 && isDiv ? Strings.Stat.FlaskIncRarity1
                    : entrie.ID;
            }
            else if (itemIs.Jewel && itemIs.Unique)
            {
                if (entrie.ID is Strings.Stat.TheBlueNightmare)
                {
                    bool isBlueDream = DataManager.Words.FirstOrDefault(x => x.NameEn is "The Blue Dream").Name == itemName;
                    if (isBlueDream)
                    {
                        entrie.ID = Strings.Stat.TheBlueDream;
                    }
                }
            }
            else if (itemIs.ArmourPiece)
            {
                if (entrie.ID is Strings.Stat.Armor)
                {
                    entrie.ID = Strings.Stat.ArmorLocal;
                }
                if (entrie.ID is Strings.Stat.Es)
                {
                    entrie.ID = Strings.Stat.EsLocal;
                }
                if (entrie.ID is Strings.Stat.Eva)
                {
                    entrie.ID = Strings.Stat.EvaLocal;
                }

                if (itemIs.Unique)
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
            }
            else if (itemIs.Chronicle)
            {
                bool goContinue = true;
                for (int s = 0; s < Strings.Stat.RoomList.Length; s++)
                {
                    if (entrie.ID.Contains(Strings.Stat.RoomList[s], StringComparison.Ordinal))
                    {
                        goContinue = false;
                        break;
                    }
                }
                if (goContinue) continueLoop = true;
            }
            else if (itemIs.Weapon)
            {
                if (entrie.ID is Strings.Stat.Accuracy)
                {
                    entrie.ID = Strings.Stat.AccuracyLocal;
                }
                if (itemIs.Unique)
                {
                    bool isDervish = DataManager.Words.FirstOrDefault(x => x.NameEn is "The Dancing Dervish").Name == itemName;
                    if (entrie.ID is Strings.Stat.Rampage && isDervish)
                    {
                        continueLoop = true;
                    }
                    bool isTrypanon = DataManager.Words.FirstOrDefault(x => x.NameEn is "Replica Trypanon").Name == itemName;
                    if (entrie.ID is Strings.Stat.AccuracyLocal && isTrypanon) // this is not a revert from previous code lines
                    {
                        entrie.ID = Strings.Stat.Accuracy;
                    }
                    bool isNetolKiss = DataManager.Words.FirstOrDefault(x => x.NameEn is "Uul-Netol's Kiss").Name == itemName;
                    if (entrie.ID is Strings.Stat.CurseVulnerability && isNetolKiss)
                    {
                        entrie.ID = Strings.Stat.CurseVulnerabilityChance;
                    }
                }
            }
            else if (itemIs.SanctumRelic)
            {
                if (entrie.Type is not "sanctum")
                {
                    continueLoop = true;
                }
            }
        }

        if (itemIs.Logbook)//&& implicitMod
        {
            if (!entrie.ID.Contains(Strings.Stat.LogbookBoss, StringComparison.Ordinal)
                && !entrie.ID.Contains(Strings.Stat.LogbookArea, StringComparison.Ordinal)
                && !entrie.ID.Contains(Strings.Stat.LogbookTwice, StringComparison.Ordinal))
            {
                continueLoop = true;
            }
        }

        return continueLoop;
    }

    //TO REDO
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
            if (entrie.ID is Strings.StatPoe2.AsPerDex1)
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
            if (entrie.ID is Strings.StatPoe2.AsPerDex2)
            {
                entrie.ID = Strings.StatPoe2.AsPerDex1;
            }
        }

        if (itemIs.ArmourPiece || itemIs.Shield || itemIs.Focus)
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

    private void SetFilter(FilterResultEntrie entrie)
    {
        ID = entrie.ID;
        Text = entrie.Text;
        Type = entrie.Type;
        Part = entrie.Part;
        Option = entrie.Option;
    }

    internal FilterResultEntrie GetSerializable()
    {
        return new FilterResultEntrie
        {
            ID = ID,
            Text = Text,
            Type = Type,
            Part = Part,
            Option = Option
        };
    }
}

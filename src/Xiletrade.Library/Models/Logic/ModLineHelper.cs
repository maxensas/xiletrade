using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;
using System.Text.RegularExpressions;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Logic;

/// <summary>Abstract helper class containing the logic used to generate a populated collection of ModLineViewModel.</summary>
internal abstract class ModLineHelper // TO REFACTOR
{
    // internal static
    internal static AsyncObservableCollection<ModLineViewModel> GetListMods(string[] clipData, ItemFlag itemIs, string itemName, string itemType, string itemClass, int idLang, out TotalStats totalStats, out Dictionary<string, string> listOptions) // TO REFACTOR
    {
        AsyncObservableCollection<ModLineViewModel> lMods = new();
        totalStats = new();
        listOptions = GetNewListOption(); // itemType, itemIs.Gem

        if (!itemIs.ShowDetail || itemIs.Gem || itemIs.SanctumResearch || itemIs.AllflameEmber || itemIs.Corpses || itemIs.TrialCoins)
        {
            for (int i = 1; i < clipData.Length; i++)
            {
                var data = clipData[i].Trim().Split(Strings.CRLF, StringSplitOptions.None);
                var sameReward = data.Where(x => x.StartsWith(Resources.Resources.General098_DeliriumReward, StringComparison.Ordinal));
                if (sameReward.Any())
                {
                    data = data.Distinct().ToArray();
                }

                if (itemIs.SanctumResearch && i == clipData.Length - 1) // at the last loop
                {
                    string[] sanctumMods = [.. GetSanctumMods(listOptions)];

                    if (sanctumMods.Length > 0)
                    {
                        Array.Resize(ref data, data.Length + sanctumMods.Length);
                        Array.Copy(sanctumMods, 0, data, data.Length - sanctumMods.Length, sanctumMods.Length);
                    }
                }

                var lSubMods = GetModsFromData(data, itemIs, itemName, itemType, itemClass, idLang, totalStats, listOptions);
                foreach (var submod in lSubMods)
                {
                    lMods.Add(submod);
                }
            }
        }
        return lMods;
    }

    // private
    private static AsyncObservableCollection<ModLineViewModel> GetModsFromData(string[] data, ItemFlag itemIs, string itemName, string itemType, string itemClass, int idLang, TotalStats totalStats, Dictionary<string, string> lOptions)
    {
        AsyncObservableCollection<ModLineViewModel> lMods = new();
        ModDescription modDesc = new();
        for (int j = 0; j < data.Length; j++)
        {
            if (data[j].Trim().Length == 0)
            {
                continue;
            }

            string unparsedData = data[j];
            AffixFlag affix = new(data[j]);
            data[j] = affix.ParseAffix(data[j]);
            var splitData = data[j].Split(':', StringSplitOptions.TrimEntries);
            if (splitData[0].Contains(Resources.Resources.General110_FoilUnique, StringComparison.Ordinal))
            {
                splitData[0] = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
            }

            if (lOptions.TryGetValue(splitData[0], out string value))
            {
                if (value.Length == 0)
                {
                    lOptions[splitData[0]] = splitData.Length > 1 ? splitData[1] : Strings.TrueOption;
                    itemIs.ItemLevel = lOptions[Resources.Resources.General032_ItemLv].Length > 0
                        || lOptions[Resources.Resources.General143_WaystoneTier].Length > 0;
                    itemIs.AreaLevel = lOptions[Resources.Resources.General067_AreaLevel].Length > 0;
                    itemIs.Weapon = lOptions[Resources.Resources.General058_PhysicalDamage].Length > 0
                        || lOptions[Resources.Resources.General059_ElementalDamage].Length > 0 || itemIs.Wand; // to update 
                }
            }
            else
            {
                if (itemIs.Gem)
                {
                    if (splitData[0].Contains(Resources.Resources.General038_Vaal, StringComparison.Ordinal))
                    {
                        lOptions[Resources.Resources.General038_Vaal] = Strings.TrueOption;
                    }
                }
                else if ((itemIs.ItemLevel || itemIs.AreaLevel || itemIs.FilledCoffin) && lMods.Count < Modifier.NB_MAX_MODS)
                {
                    if (SkipBetweenBrackets(data[j], itemIs.Ultimatum))
                    {
                        continue;
                    }

                    var desc = GetAdvancedModDesc(data[j]);
                    if (desc is not null)
                    {
                        modDesc = desc;
                        continue;
                    }

                    if (itemIs.Logbook && affix.Implicit) modDesc.Kind = Resources.Resources.General073_ModifierImplicit;

                    double tierValMin = Modifier.EMPTYFIELD, tierValMax = Modifier.EMPTYFIELD;
                    string inputData = data[j];

                    // LOW priority Bug to fix :
                    // When there is no '(x-y)' example : Adds 1 to (4–5) Lightning Damage to Spells
                    if (!itemIs.ChargedCompass && !itemIs.Voidstone && !itemIs.MirroredTablet) // to handle text : (Tier 14+) & no tier needed
                    {
                        inputData = ParseTierValues(inputData, out Tuple<double, double> minmax);
                        tierValMin = minmax.Item1;
                        tierValMax = minmax.Item2;
                    }

                    inputData = ParseUnscalableValue(inputData, out bool unscalableValue);
                    inputData = Modifier.Parse(inputData, idLang, itemName, itemIs, modDesc.Name, out bool negativeValue);
                    if (negativeValue)
                    {
                        if (tierValMin.IsNotEmpty()) tierValMin = -tierValMin;
                        if (tierValMax.IsNotEmpty()) tierValMax = -tierValMax;
                    }

                    if (inputData.StartsWith(Resources.Resources.General098_DeliriumReward, StringComparison.Ordinal))
                    {
                        inputData += " (×#)";
                    }

                    var modFilter = GetModValues(inputData, data, j, itemIs, itemName, itemType, itemClass, out AsyncObservableCollection<AffixFilterEntrie> listAffix, out double min, out double max);

                    if (modFilter is not null)
                    {
                        ModLineViewModel mod = new(modFilter, listAffix, itemIs, affix, modDesc, inputData, unparsedData, unscalableValue, tierValMin, tierValMax, min, max, idLang, negativeValue);

                        if (!itemIs.Unique)
                        {
                            FillTotalStats(totalStats, modFilter, mod.Current, idLang);
                        }

                        if (modFilter.ID.Contains(Strings.Stat.IncAs, StringComparison.Ordinal) && mod.ItemFilter.Min > 0 && mod.ItemFilter.Min < 999)
                        {
                            double val = lOptions[Strings.Stat.IncAs].ToDoubleDefault();
                            lOptions[Strings.Stat.IncAs] = (val + mod.ItemFilter.Min).ToString();
                        }
                        else if (modFilter.ID.Contains(Strings.Stat.IncPhys, StringComparison.Ordinal) && mod.ItemFilter.Min > 0 && mod.ItemFilter.Min < 9999)
                        {
                            double val = lOptions[Strings.Stat.IncPhys].ToDoubleDefault();
                            lOptions[Strings.Stat.IncPhys] = (val + mod.ItemFilter.Min).ToString();
                        }

                        lMods.Add(mod);
                    }
                }
            }
        }
        return lMods;
    }

    private static Dictionary<string, string> GetNewListOption()
    {
        Dictionary<string, string> lItemOption = new()
        {
            { Resources.Resources.General035_Quality, string.Empty },
            { Resources.Resources.General031_Lv, string.Empty },
            { Resources.Resources.General032_ItemLv, string.Empty },
            { Resources.Resources.General033_TalTier, string.Empty },
            { Resources.Resources.General034_MaTier, string.Empty },
            { Resources.Resources.General067_AreaLevel, string.Empty },
            { Resources.Resources.General036_Socket, string.Empty },
            { Resources.Resources.General055_Armour, string.Empty },
            { Resources.Resources.General056_Energy, string.Empty },
            { Resources.Resources.General057_Evasion, string.Empty },
            { Resources.Resources.General095_Ward, string.Empty },
            { Resources.Resources.General058_PhysicalDamage, string.Empty },
            { Resources.Resources.General059_ElementalDamage, string.Empty },
            { Resources.Resources.General060_ChaosDamage, string.Empty },
            { Resources.Resources.General061_AttacksPerSecond, string.Empty },
            { Resources.Resources.General041_Shaper, string.Empty },
            { Resources.Resources.General042_Elder, string.Empty },
            { Resources.Resources.General043_Crusader, string.Empty },
            { Resources.Resources.General044_Redeemer, string.Empty },
            { Resources.Resources.General045_Hunter, string.Empty },
            { Resources.Resources.General046_Warlord, string.Empty },
            { Resources.Resources.General047_Synthesis, string.Empty },
            { Resources.Resources.General037_Corrupt, string.Empty },
            { Resources.Resources.General109_Mirrored, string.Empty },
            { Resources.Resources.General110_FoilUnique, string.Empty },
            { Resources.Resources.General039_Unidentify, string.Empty },
            { Resources.Resources.General038_Vaal, string.Empty },
            { Strings.AlternateGem, string.Empty },
            { Strings.Stat.IncPhys, string.Empty },
            { Strings.Stat.IncAs, string.Empty },
            { Resources.Resources.Main154_tbFacetor, string.Empty },
            { Resources.Resources.General070_ReqSacrifice, string.Empty },
            { Resources.Resources.General071_Reward, string.Empty },
            { Resources.Resources.General099_ScourgedItem, string.Empty },
            { Resources.Resources.General114_SanctumResolve, string.Empty },
            { Resources.Resources.General115_SanctumInspiration, string.Empty },
            { Resources.Resources.General116_SanctumAureus, string.Empty },
            { Resources.Resources.General117_SanctumMinorBoons, string.Empty },
            { Resources.Resources.General118_SanctumMajorBoons, string.Empty },
            { Resources.Resources.General119_SanctumMinorAfflictions, string.Empty },
            { Resources.Resources.General120_SanctumMajorAfflictions, string.Empty },
            { Resources.Resources.General123_SanctumPacts, string.Empty },
            { Resources.Resources.General121_RewardsFloorCompletion, string.Empty },
            { Resources.Resources.General122_RewardsSanctumCompletion, string.Empty },
            { Resources.Resources.General128_Monster, string.Empty },
            { Resources.Resources.General129_CorpseLevel, string.Empty },
            { Resources.Resources.General130_MonsterCategory, string.Empty },
            { Resources.Resources.General136_ItemQuantity, string.Empty },
            { Resources.Resources.General137_ItemRarity, string.Empty },
            { Resources.Resources.General138_MonsterPackSize, string.Empty },
            { Resources.Resources.General139_MoreCurrency, string.Empty },
            { Resources.Resources.General140_MoreScarabs, string.Empty },
            { Resources.Resources.General141_MoreMaps, string.Empty },
            { Resources.Resources.General142_MoreDivinationCards, string.Empty },
            { Resources.Resources.General143_WaystoneTier, string.Empty },
            { Resources.Resources.General146_LightningDamage, string.Empty },
            { Resources.Resources.General147_CriticalHitChance, string.Empty },
            { Resources.Resources.General148_ColdDamage, string.Empty },
            { Resources.Resources.General149_FireDamage, string.Empty }
        };
        return lItemOption;
    }

    private static List<string> GetSanctumMods(Dictionary<string, string> lOptions)
    {
        List<string> lMods = new(), lEntrie = new();

        string[] majBoons = lOptions[Resources.Resources.General118_SanctumMajorBoons].Split(',', StringSplitOptions.TrimEntries);
        if (majBoons[0].Length > 0)
        {
            lEntrie.AddRange(majBoons);
        }
        string[] majAfflictions = lOptions[Resources.Resources.General120_SanctumMajorAfflictions].Split(',', StringSplitOptions.TrimEntries);
        if (majAfflictions[0].Length > 0)
        {
            lEntrie.AddRange(majAfflictions);
        }
        string[] pacts = lOptions[Resources.Resources.General123_SanctumPacts].Split(',', StringSplitOptions.TrimEntries);
        if (pacts[0].Length > 0)
        {
            lEntrie.AddRange(pacts);
        }

        /*
        StringBuilder sbMods = new(lOptions[Resources.Resources.General118_SanctumMajorBoons]);
        sbMods.AppendJoin(',', lOptions[Resources.Resources.General120_SanctumMajorAfflictions])
            .AppendJoin(',', lOptions[Resources.Resources.General123_SanctumPacts])
            .AppendJoin(',', lOptions[Resources.Resources.General121_RewardsFloorCompletion])
            .AppendJoin(',', lOptions[Resources.Resources.General122_RewardsSanctumCompletion]);
        var test = sbMods.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        */

        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var modEntry =
                    from result in DataManager.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contains(mod, StringComparison.Ordinal) && filt.Type is "sanctum"
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        lMods.Add(modTxt);
                    }
                }
            }
        }

        lEntrie = new();
        string[] floorRewards = lOptions[Resources.Resources.General121_RewardsFloorCompletion].Split(',', StringSplitOptions.TrimEntries);
        if (floorRewards[0].Length > 0)
        {
            lEntrie.AddRange(floorRewards);
        }
        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
                string modKind = RegexUtil.DecimalPattern().Replace(mod, "#").Replace("Orb ", "Orbs ").Replace("Mirror ", "Mirrors ");

                var modEntry =
                    from result in DataManager.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contains(modKind, StringComparison.Ordinal) && filt.ID.StartsWith("sanctum.sanctum_floor_reward", StringComparison.Ordinal)
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        if (match.Count == 1)
                        {
                            modTxt = modTxt.Replace("#", match[0].Value);
                        }

                        lMods.Add(modTxt);
                    }
                }
            }
        }

        lEntrie = new();
        string[] sanctumRewards = lOptions[Resources.Resources.General122_RewardsSanctumCompletion].Split(',', StringSplitOptions.TrimEntries);
        if (sanctumRewards[0].Length > 0)
        {
            lEntrie.AddRange(sanctumRewards);
        }
        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
                string modKind = RegexUtil.DecimalPattern().Replace(mod, "#").Replace("Orb ", "Orbs ").Replace("Mirror ", "Mirrors ");

                var modEntry =
                    from result in DataManager.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contains(modKind, StringComparison.Ordinal) && filt.ID.StartsWith("sanctum.sanctum_final_reward", StringComparison.Ordinal)
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        if (match.Count is 1)
                        {
                            modTxt = modTxt.Replace("#", match[0].Value);
                        }

                        lMods.Add(modTxt);
                    }
                }
            }
        }
        return lMods;
    }

    private static bool SkipBetweenBrackets(string data, bool ultimatum)
    {
        if (ultimatum)
        {
            return data.StartsWith('(') || data.EndsWith(')');
        }
        return data.StartsWith('(') && data.EndsWith(')');
    }

    private static ModDescription GetAdvancedModDesc(string data)
    {
        if (!(data.StartsWith('{') && data.EndsWith('}')))
        {
            return null;
        }
        ModDescription modDesc = new();
        string[] affixOptions = data.Split('—', StringSplitOptions.TrimEntries);

        for (int m = 0; m < affixOptions.Length; m++)
        {
            StringBuilder sb = new(affixOptions[m]);
            sb.Replace("{", string.Empty).Replace("}", string.Empty);
            affixOptions[m] = sb.ToString().Trim();
        }

        // First step : extract mod tier
        int idx1 = affixOptions[0].IndexOf('(', StringComparison.Ordinal);
        int idx2 = affixOptions[0].IndexOf(')', StringComparison.Ordinal);
        if (idx1 > -1 && idx2 > -1 && idx1 < idx2)
        {
            string tierString = affixOptions[0].Substring(idx1, idx2 - idx1 + 1);
            if (tierString.Contains(':', StringComparison.Ordinal))
            {
                //MatchCollection matchOld = Regex.Matches(tierString, @"[-]?[0-9]+\.[0-9]+|[-]?[0-9]+");
                MatchCollection match = RegexUtil.DecimalNoPlusPattern().Matches(tierString);
                if (match.Count > 0)
                {
                    _ = int.TryParse(match[0].Value, out int tier);
                    modDesc.Tier = tier;
                }
                affixOptions[0] = affixOptions[0].Replace(tierString, string.Empty).Trim();
            }
        }

        string[] affixOpt = affixOptions[0].Split('"');
        if (affixOpt.Length == 3)
        {
            StringBuilder sbAf = new();
            sbAf.Append(affixOpt[0]).Append('«').Append(affixOpt[1]).Append('»').Append(affixOpt[2]);
            affixOptions[0] = sbAf.ToString();
        }

        // Second step : extract mod generated name (between «» or "")
        idx1 = affixOptions[0].IndexOf('«', StringComparison.Ordinal);
        idx2 = affixOptions[0].IndexOf('»', StringComparison.Ordinal);
        if (idx1 > -1 && idx2 > -1 && idx1 < idx2)
        {
            string name = affixOptions[0].Substring(idx1, idx2 - idx1 + 1);
            modDesc.Name = name.Replace("«", string.Empty).Replace("»", string.Empty).Trim();
            affixOptions[0] = affixOptions[0].Replace(name, string.Empty).Trim();
        }
        // Last step
        modDesc.Kind = affixOptions[0].Replace(":", string.Empty).Trim(); // french version use ":"

        if (affixOptions.Length > 1)
        {
            modDesc.Tags = affixOptions[1];
        }
        if (affixOptions.Length > 2)
        {
            modDesc.Quality = affixOptions[2];
        }

        return modDesc;
    }

    private static string ParseTierValues(string data, out Tuple<double, double> minmax)
    {
        int watchdog = 0;
        int idx1, idx2;
        double tierValMin = Modifier.EMPTYFIELD, tierValMax = Modifier.EMPTYFIELD;
        StringBuilder sbParse = new(data);

        do
        {
            idx1 = sbParse.ToString().IndexOf('(', StringComparison.Ordinal);
            idx2 = sbParse.ToString().IndexOf(')', StringComparison.Ordinal);
            if (idx1 > -1 && idx2 > -1 && idx1 < idx2)
            {
                string tierRange = sbParse.ToString().Substring(idx1, idx2 - idx1 + 1);
                if (tierRange.Contains('-', StringComparison.Ordinal))
                {
                    string[] extract = tierRange.Replace("(", string.Empty).Replace(")", string.Empty).Split('-');
                    _ = double.TryParse(extract[0], out double tValMin);
                    _ = double.TryParse(extract[1], out double tValMax);
                    if (tValMin is 0 || tValMax is 0)
                    {
                        tierValMin = tierValMax = Modifier.EMPTYFIELD;
                    }
                    else
                    {
                        tierValMin = tierValMin.IsEmpty() ? tValMin : (tierValMin + tValMin) / 2;
                        tierValMax = tierValMax.IsEmpty() ? tValMax : (tierValMax + tValMax) / 2;
                    }
                }
                else
                {
                    string extract = tierRange.Replace("(", string.Empty).Replace(")", string.Empty);
                    _ = double.TryParse(extract, out double tVal);
                    tierValMin = tVal is 0 ? Modifier.EMPTYFIELD : tVal;
                    tierValMax = tVal is 0 ? Modifier.EMPTYFIELD : tVal;
                }
                sbParse.Replace(tierRange, string.Empty);
            }
            watchdog++;
            if (watchdog > 10)
            {
                break;
            }
        } while (idx1 is not -1 || idx2 is not -1);

        if (tierValMin.IsNotEmpty()) tierValMin = Math.Truncate(tierValMin);
        if (tierValMax.IsNotEmpty()) tierValMax = Math.Truncate(tierValMax);

        minmax = new(tierValMin, tierValMax);

        return sbParse.ToString();
    }

    private static string ParseUnscalableValue(string data, out bool unscalable)
    {
        unscalable = false;
        if (data.Split('—', StringSplitOptions.TrimEntries).Length > 1)
        {
            if (data.Split('—', StringSplitOptions.TrimEntries)[1] is Strings.UnscalableValue)
            {
                unscalable = true;
            }
        }
        return data.Split('—', StringSplitOptions.TrimEntries)[0]; // Remove : Unscalable Value - To modify if needed
    }

    private static FilterResultEntrie GetModValues(string input, string[] data, int dataIndex, ItemFlag itemIs, string itemName, string itemType, string itemClass, out AsyncObservableCollection<AffixFilterEntrie> listAffix, out double min, out double max) // TO REFACTOR
    {
        FilterResultEntrie filter = null;
        listAffix = new();
        min = max = Modifier.EMPTYFIELD;

        string inputRegEscape = Regex.Escape(RegexUtil.DecimalPattern().Replace(input, "#"));
        string inputRegPattern = RegexUtil.DiezePattern().Replace(inputRegEscape, RegexUtil.DecimalPatternDieze);

        Regex inputRegex = new("^" + inputRegPattern + "$", RegexOptions.IgnoreCase);

        Strings.dicPublicID.TryGetValue(itemType, out string publicID);
        publicID ??= string.Empty;

        foreach (FilterResult filterResult in DataManager.Filter.Result)
        {
            var entries = filterResult.Entries.Where(x => inputRegex.IsMatch(x.Text));
            if (!entries.Any())
            {
                string[] input2 = input.Split("\\n");
                if (input2.Length >= 2)
                {
                    Regex inputRegex2 = new("^" + input2[0] + "$", RegexOptions.IgnoreCase); // not using escape ?
                    entries = filterResult.Entries.Where(x => inputRegex2.IsMatch(x.Text));
                }

                if ((itemIs.Rare || itemIs.Magic) && filterResult.Label == Resources.Resources.General015_Explicit)
                {
                    if (!entries.Any())
                    {
                        var ConfluxEntries = filterResult.Entries.Where(x => x.ID == Strings.Stat.Conflux);
                        if (ConfluxEntries.Any())
                        {
                            var ConfluxEntrie = ConfluxEntries.First();
                            foreach (FilterResultOptions opt in ConfluxEntrie.Option.Options)
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
                foreach (FilterResultEntrie entrie in entries)
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
                            if (matches2[t].Value == "#")
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
                        listAffix.Add(new AffixFilterEntrie(entrie.ID, lblAffix, isCorruption, itemIs.Unique && entrie.ID.StartsWith("explicit", StringComparison.Ordinal)));
                        if (filter is null)
                        {
                            string[] id_split = entrie.ID.Split('.');

                            filter = entrie;

                            var matches = RegexUtil.DecimalNoPlusPattern().Matches(input);

                            if (itemIs.SanctumRelic) // TO update with other unparsed values not done yet
                            {
                                isMin = true;
                            }

                            min = isMin && matches.Count > idxMin ? matches[idxMin].Value.ToDoubleEmptyField() : Modifier.EMPTYFIELD;
                            max = isMax && idxMin < idxMax && matches.Count > idxMax ? matches[idxMax].Value.ToDoubleEmptyField() : Modifier.EMPTYFIELD;

                            if (entrie.ID is Strings.Stat.NecroExplicit) // invert
                            {
                                max = min;
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
                else if(DataManager.Config.Options.GameVersion is 0)
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
                            foreach (FilterResultEntrie resultEntrie in entrieSeek)
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
                    listAffix.Add(new AffixFilterEntrie(entrie.ID, lblAffix, isCorruption, itemIs.Unique && entrie.ID.StartsWith("explicit", StringComparison.Ordinal)));
                    filter = entrie;
                }
            }
        }

        return filter;
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

    private static void FillTotalStats(TotalStats stat, FilterResultEntrie modFilter, string currentValue, int idLang)
    {
        string modTextEnglish = modFilter.Text;
        if (idLang is not 0) // !("en-US")
        {
            var enResult =
                from result in DataManager.FilterEn.Result
                from Entrie in result.Entries
                where Entrie.ID == modFilter.ID
                select Entrie.Text;
            if (enResult.Any())
            {
                modTextEnglish = enResult.First();
            }
        }

        double totResist = Modifier.CalculateTotalResist(modTextEnglish, currentValue);
        if (totResist is not 0)
        {
            stat.Resistance = stat.Resistance > 0 ? stat.Resistance + totResist : totResist;
        }
        double totLife = Modifier.CalculateTotalLife(modTextEnglish, currentValue);
        if (totLife is not 0)
        {
            stat.Life = stat.Life > 0 ? stat.Life + totLife : totLife;
        }
        double totEs = Modifier.CalculateGlobalEs(modTextEnglish, currentValue);
        if (totEs is not 0)
        {
            stat.EnergyShield = stat.EnergyShield > 0 ? stat.EnergyShield + totEs : totEs;
        }
    }
}

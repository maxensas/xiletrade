﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Shared;

/// <summary>Static class only used by MainViewModelHelper class.</summary>
/// <remarks>Related to Poe modifier handling.</remarks>
internal static class Modifier
{
    /// <summary>Maximum number of mods to display.</summary>
    internal const int NB_MAX_MODS = 30;
    /// <summary>Empty fields will not be added to json</summary>
    internal const int EMPTYFIELD = 99999;
    /// <summary>Using with Levenshtein parser</summary>
    internal const int LEVENSHTEIN_DISTANCE_DIVIDER = 8; // old val: 6

    internal static string Parse(string mod, int idLang, string itemName, ItemFlag itemIs, string affixName, out bool negativeValue)
    {
        negativeValue = false;
        var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
        string modKind = RegexUtil.DecimalPattern().Replace(mod, "#");

        string[] reduced = Resources.Resources.General102_reduced.Split('/');
        string[] increased = Resources.Resources.General101_increased.Split('/');

        if (reduced.Length != increased.Length)
        {
            return mod; // should never happen
        }

        bool is_VeiledPrefix = mod == Resources.Resources.General106_VeiledPrefix;
        bool is_VeiledSuffix = mod == Resources.Resources.General107_VeiledSuffix;
        if (is_VeiledPrefix || is_VeiledSuffix)
        {
            if (affixName.Length > 0)
            {
                return affixName;
            }
            var modEntry =
                    from result in DataManager.Filter.Result
                    from filt in result.Entries
                    where filt.ID is Strings.Stat.VeiledPrefix && is_VeiledPrefix
                        || filt.ID is Strings.Stat.VeiledSuffix && is_VeiledSuffix
                    select filt.Text;
            if (modEntry.Any())
            {
                var modTxt = modEntry.First();
                if (modTxt.Length > 0)
                {
                    return modTxt;
                }
            }
            return mod;
        }

        for (int j = 0; j < reduced.Length; j++)
        {
            if (!modKind.Contains(reduced[j], StringComparison.Ordinal))
            {
                continue;
            }
            var modKindEntry =
                    from result in DataManager.Filter.Result
                    from filter in result.Entries
                    where filter.Text == modKind
                    select filter.Text;
            if (!modKindEntry.Any()) // mod with reduced stat not found
            {
                string modIncreased = modKind.Replace(reduced[j], increased[j]);
                var modEntry =
                    from result in DataManager.Filter.Result
                    from filter in result.Entries
                    where filter.Text == modIncreased
                    select filter.Text;
                if (modEntry.Any()) // mod with increased stat found
                {
                    if (match.Count > 0)
                    {
                        StringBuilder sbInc = new(modIncreased);
                        sbInc.Replace(reduced[j], increased[j]);
                        for (int i = 0; i < match.Count; i++)
                        {
                            int idx = sbInc.ToString().IndexOf('#', StringComparison.Ordinal);
                            if (idx > -1)
                            {
                                sbInc.Replace("#", "-" + match[i].Value, idx, 1);
                                mod = sbInc.ToString();
                            }
                        }
                    }
                    modKind = modIncreased.Replace("#", "-#");
                    negativeValue = true;
                    break;
                }
            }
        }

        string returnMod = mod;
        StringBuilder sb = new();
        var parseEntrie =
            from parse in DataManager.Parser.Mods
            where modKind.Contains(parse.Old, StringComparison.Ordinal) && parse.Replace == Strings.contains
                || modKind == parse.Old && parse.Replace == Strings.@equals
            select parse;
        if (parseEntrie.Any())
        {
            // mod is parsed
            if (parseEntrie.First().Replace is Strings.contains)
            {
                sb.Append(modKind);
                sb.Replace(parseEntrie.First().Old, parseEntrie.First().New);
            }
            else if (parseEntrie.First().Replace is Strings.equals)
            {
                sb.Append(parseEntrie.First().New);
            }
            else
            {
                sb.Append(modKind); // should never goes here
            }
        }
        else
        {
            // mod is not parsed using ParsingRules but with Fastenshtein
            sb.Append(ParseWithFastenshtein(modKind, itemIs));
        }

        if (modKind != sb.ToString())
        {
            if (match.Count > 0)
            {
                var lMatch = match.Select(x => x.Value).ToList();
                var lSbMatch = RegexUtil.DecimalNoPlusPattern().Matches(sb.ToString()).Select(x => x.Value);
                if (lSbMatch.Any())
                {
                    foreach (var valSbMatch in lSbMatch)
                    {
                        lMatch.Remove(valSbMatch); // remove the first and does not respect order.
                    }
                }
                foreach (var valMatch in lMatch)
                {
                    int idx = sb.ToString().IndexOf('#', StringComparison.Ordinal);
                    if (idx > -1)
                    {
                        sb.Replace("#", valMatch, idx, 1);
                    }
                }
            }
            returnMod = sb.ToString();
        }
        else
        {
            string part = string.Empty;
            if (idLang != 1) part = " "; // !StringsTable.Culture[idLang].Equals("ko-KR")
            int idxPseudo = 0;//, idxExplicit = 1, idxImplicit = 2;

            if (itemIs.Chronicle)
            {
                var filter = DataManager.Filter.Result[idxPseudo].Entries.FirstOrDefault(x => x.Text.Contains(mod, StringComparison.Ordinal));
                if (filter is not null)
                {
                    returnMod = filter.Text;
                }
                else
                {
                    // Done after to prevent new bug if filters are fixed/translated in future
                    if (mod == Resources.Resources.General068_ApexAtzoatl && idLang is not 1 and not 7) // !StringsTable.Culture[idLang].Equals("ko-KR") && !StringsTable.Culture[idLang].Equals("zh-TW")
                    {
                        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
                        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));
                        mod = rm.GetString("General068_ApexAtzoatl", cultureEn); // Using english version because GGG didnt translated 'pseudo.pseudo_temple_apex' text filter yet

                        filter = DataManager.Filter.Result[idxPseudo].Entries.FirstOrDefault(x => x.Text.Contains(mod, StringComparison.Ordinal));
                        if (filter is not null)
                        {
                            returnMod = filter.Text;
                        }
                    }
                }
            }
            else
            {
                List<string> stats = new();
                if (itemIs.Stave) //!is_jewel
                {
                    stats.Add(Strings.Stat.BlockStaffWeapon);
                }
                else if (itemIs.Shield)
                {
                    stats.Add(Strings.Stat.Block);
                }

                if (itemIs.Weapon)
                {
                    bool isBloodlust = DataManager.Words.FirstOrDefault(x => x.NameEn is "Hezmana's Bloodlust").Name == itemName;
                    if (!isBloodlust)
                    {
                        stats.Add(Strings.Stat.LifeLeech);
                    }
                    stats.Add(Strings.Stat.AddAccuracyLocal);
                    stats.Add(Strings.Stat.AttackSpeed);
                    stats.Add(Strings.Stat.ManaLeech);
                    stats.Add(Strings.Stat.PoisonHit);
                    stats.Add(Strings.Stat.IncPhysFlat);
                    stats.Add(Strings.Stat.IncLightFlat);
                    stats.Add(Strings.Stat.IncColdFlat);
                    stats.Add(Strings.Stat.IncFireFlat);
                    stats.Add(Strings.Stat.IncChaosFlat);
                }
                else if (itemIs.ArmourPiece)
                {
                    stats.Add(Strings.Stat.IncEs);
                    stats.Add(Strings.Stat.IncEva);
                    stats.Add(Strings.Stat.IncArmour);
                    stats.Add(Strings.Stat.IncAe);
                    stats.Add(Strings.Stat.IncAes);
                    stats.Add(Strings.Stat.IncEes);
                    stats.Add(Strings.Stat.IncArEes);
                    stats.Add(Strings.Stat.AddArmorFlat);
                    stats.Add(Strings.Stat.AddEsFlat);
                    stats.Add(Strings.Stat.AddEvaFlat);
                }

                if (stats.Count > 0)
                {
                    foreach (string stat in stats)
                    {
                        var resultEntry =
                            from result in DataManager.Filter.Result
                            from filter in result.Entries
                            where filter.ID.Contains(stat, StringComparison.Ordinal)
                            select filter.Text;
                        if (resultEntry.Any())
                        {
                            string modText = resultEntry.First();

                            string res = stat == Strings.Stat.Block ? Resources.Resources.General024_Shields :
                                stat == Strings.Stat.BlockStaffWeapon ? Resources.Resources.General025_Staves :
                                Resources.Resources.General023_Local;
                            string fullMod = (negativeValue ? modKind.Replace("-", string.Empty) : modKind) + part + res;
                            string fullModPositive = fullMod.Replace("#%", "+#%"); // fix (block on staff) to test longterm

                            if (modText == fullMod || modText == fullModPositive)
                            {
                                returnMod = mod + part + res;
                                break;
                            }
                        }
                    }
                }
            }
        }
        // temp fix
        if (negativeValue && DataManager.Config.Options.GameVersion is 1)
        {
            return mod;
        }
        return returnMod;
    }

    internal static string TranslateAffix(string affix)
    {
        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

        return affix == rm.GetString("General011_Enchant", cultureEn) ? Resources.Resources.General011_Enchant
            : affix == rm.GetString("General012_Crafted", cultureEn) ? Resources.Resources.General012_Crafted
            : affix == rm.GetString("General013_Implicit", cultureEn) ? Resources.Resources.General013_Implicit
            : affix == rm.GetString("General014_Pseudo", cultureEn) ? Resources.Resources.General014_Pseudo
            : affix == rm.GetString("General015_Explicit", cultureEn) ? Resources.Resources.General015_Explicit
            : affix == rm.GetString("General016_Fractured", cultureEn) ? Resources.Resources.General016_Fractured
            : affix == rm.GetString("General017_CorruptImp", cultureEn) ? Resources.Resources.General017_CorruptImp
            : affix == rm.GetString("General018_Monster", cultureEn) ? Resources.Resources.General018_Monster
            : affix == rm.GetString("General099_Scourge", cultureEn) ? Resources.Resources.General099_Scourge
            : affix;
    }

    internal static bool IsTotalStat(string modEnglish, Stat stat)
    {
        bool cond = false;
        string modLower = modEnglish.ToLowerInvariant();

        foreach (var words in stat is Stat.Life ? Strings.lTotalStatLifeUnwanted :
            stat is Stat.Es ? Strings.lTotalStatEsUnwanted : Strings.lTotalStatResistUnwanted)
        {
            cond = cond || modLower.Contains(words, StringComparison.Ordinal);
        }

        cond = (stat is Stat.Life ? modLower.Contains("to maximum life", StringComparison.Ordinal)
            || modLower.Contains("to strength", StringComparison.Ordinal) :
            stat is Stat.Es ? modLower.Contains("to maximum energy shield", StringComparison.Ordinal) :
            modLower.Contains("resistance", StringComparison.Ordinal)) && !cond;

        return cond;
    }

    internal static int CalculateTotalResist(string mod, string currentValue)
    {
        int returnVal = 0;
        if (IsTotalStat(mod, Stat.Resist)
            && double.TryParse(currentValue.Replace(".", ","), out double currentVal))
        {
            if (mod.Contains("to all Elemental Resistances", StringComparison.Ordinal))
            {
                return Convert.ToInt32(currentVal) * 3;
            }

            string modLower = mod.ToLowerInvariant();
            if (modLower.Contains("fire", StringComparison.Ordinal))
            {
                returnVal = Convert.ToInt32(currentVal);
            }
            if (modLower.Contains("cold", StringComparison.Ordinal))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (modLower.Contains("lightning", StringComparison.Ordinal))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (modLower.Contains("chaos", StringComparison.Ordinal))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
        }
        return returnVal;
    }

    internal static double CalculateTotalLife(string mod, string currentValue)
    {
        if (IsTotalStat(mod, Stat.Life)
            && double.TryParse(currentValue.Replace(".", ","), out double currentVal))
        {
            var cond = mod.ToLowerInvariant().Contains("to strength", StringComparison.Ordinal);
            return cond ? Math.Truncate(currentVal / 2) : currentVal;
        }
        return 0;
    }

    internal static int CalculateGlobalEs(string mod, string currentValue)
    {
        if (IsTotalStat(mod, Stat.Es) && int.TryParse(currentValue, out int currentVal))
        {
            return currentVal;
        }
        return 0;
    }

    internal static string ComposeModRange(string mod, double min, double tValMin, double tValMax, int idLang)
    {
        StringBuilder sbMod = new(mod);
        if (idLang > 0)
        {
            System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
            System.Resources.ResourceManager rm = new(typeof(Resources.Resources));
            string enStr = rm.GetString("General096_AddsTo", cultureEn);
            sbMod.Replace(enStr, "#"); // if mod wasnt translated
        }

        if (idLang is 1) //StringsTable.Culture[idLang].Equals("ko-KR")
        {
            sbMod.Replace("#~#", "#");
            MatchCollection match = RegexUtil.DiezeSpacePattern().Matches(sbMod.ToString());
            if (match.Count is 2)
            {
                int idx = sbMod.ToString().IndexOf("# ");
                sbMod.Remove(idx, 2);
            }
        }
        else
        {
            sbMod.Replace(Resources.Resources.General096_AddsTo, "#");
        }

        if (tValMin.IsNotEmpty() && tValMax.IsNotEmpty())
        {
            string range = "(" + tValMin + "-" + tValMax + ")";
            sbMod.Replace("#", range);
        }
        else if (min.IsNotEmpty())
        {
            sbMod.Replace("#", min.ToString());
        }

        return sbMod.ToString();
    }

    internal static string ReduceOptionText(string text)
    {
        return Strings.dicOptionText.TryGetValue(text, out string value) ? value : text;
    }

    private static string ParseWithFastenshtein(string str, ItemFlag itemIs)
    {
        int maxDistance = str.Length / GetDistanceDivider(itemIs);
        if (maxDistance is 0)
        {
            maxDistance = 1;
        }
        var entrySeek =
            from result in DataManager.Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            Fastenshtein.Levenshtein lev = new(str);

            var distance = maxDistance;
            foreach (var item in entrySeek)
            {
                int levDistance = lev.DistanceFrom(item);
                if (levDistance <= distance)
                {
                    str = item;
                    distance = levDistance - 1;
                }
            }
        }

        return str;
    }

    // WIP: probably add new rules for other item kind and distinguish according to language
    private static int GetDistanceDivider(ItemFlag itemIs)
    {
        //DataManager.Config.Options.Language
        return itemIs.Tablet ? 5 : LEVENSHTEIN_DISTANCE_DIVIDER;
    }
}

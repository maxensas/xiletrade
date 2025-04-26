using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

internal sealed record ItemModifier
{
    /// <summary>Using with Levenshtein parser</summary>
    private const int LEVENSHTEIN_DISTANCE_DIVIDER = 8; // old val: 6

    internal string Parsed { get; }
    internal double TierMin { get; } = ModFilter.EMPTYFIELD;
    internal double TierMax { get; } = ModFilter.EMPTYFIELD;
    internal bool Unscalable { get; }
    internal bool Negative { get; }

    internal ItemFlag ItemFlag { get; }
    internal int IdLang { get; }

    internal string NextMod { get; }

    internal ItemModifier(string data, string nextMod, int idLang, string itemName, string modName, ItemFlag itemFlag)
    {
        ItemFlag = itemFlag;
        IdLang = idLang;
        // LOW priority Bug to fix :
        // When there is no '(x-y)' example : Adds 1 to (4–5) Lightning Damage to Spells
        Parsed = ParseTierValues(data, out Tuple<double, double> minmax);
        TierMin = minmax.Item1;
        TierMax = minmax.Item2;

        Parsed = ParseUnscalableValue(Parsed, out bool unscalableValue);
        Unscalable = unscalableValue;
        Parsed = ParseMod(Parsed, idLang, itemName, itemFlag, modName, out bool negativeValue);
        Negative = negativeValue;
        if (negativeValue)
        {
            if (TierMin.IsNotEmpty()) TierMin = -TierMin;
            if (TierMax.IsNotEmpty()) TierMax = -TierMax;
        }

        if (Parsed.StartWith(Resources.Resources.General098_DeliriumReward))
        {
            Parsed += " (×#)";
        }

        NextMod = nextMod;
    }

    private static string ParseTierValues(string data, out Tuple<double, double> minmax)
    {
        int watchdog = 0;
        int idx1, idx2;
        double tierValMin = ModFilter.EMPTYFIELD, tierValMax = ModFilter.EMPTYFIELD;
        StringBuilder sbParse = new(data);

        do
        {
            idx1 = sbParse.ToString().IndexOf('(', StringComparison.Ordinal);
            idx2 = sbParse.ToString().IndexOf(')', StringComparison.Ordinal);
            if (idx1 > -1 && idx2 > -1 && idx1 < idx2)
            {
                string tierRange = sbParse.ToString().Substring(idx1, idx2 - idx1 + 1);
                if (tierRange.Contain('-'))
                {
                    string[] extract = tierRange.Replace("(", string.Empty).Replace(")", string.Empty).Split('-');
                    _ = double.TryParse(extract[0], out double tValMin);
                    _ = double.TryParse(extract[1], out double tValMax);
                    if (tValMin is 0 || tValMax is 0)
                    {
                        tierValMin = tierValMax = ModFilter.EMPTYFIELD;
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
                    tierValMin = tVal is 0 ? ModFilter.EMPTYFIELD : tVal;
                    tierValMax = tVal is 0 ? ModFilter.EMPTYFIELD : tVal;
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
        var dataSplit = data.Split('—', StringSplitOptions.TrimEntries);
        if (dataSplit.Length > 1)
        {
            if (dataSplit[1] is Strings.UnscalableValue)
            {
                unscalable = true;
            }
        }
        return dataSplit[0]; // Remove : Unscalable Value - To modify if needed
    }

    private static string ParseMod(string mod, int idLang, string itemName, ItemFlag itemIs, string affixName, out bool negativeValue)
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
            if (!modKind.Contain(reduced[j]))
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
            where modKind.Contain(parse.Old) && parse.Replace == Strings.contains
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
                var filter = DataManager.Filter.Result[idxPseudo].Entries.FirstOrDefault(x => x.Text.Contain(mod));
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

                        filter = DataManager.Filter.Result[idxPseudo].Entries.FirstOrDefault(x => x.Text.Contain(mod));
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
                            where filter.ID.Contain(stat)
                            select filter.Text;
                        if (!resultEntry.Any())
                        {
                            continue;
                        }

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
        // temp fix
        if (negativeValue && DataManager.Config.Options.GameVersion is 1)
        {
            return mod;
        }
        return returnMod;
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
        var seek = entrySeek.FirstOrDefault(x => x.Contain(str));
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

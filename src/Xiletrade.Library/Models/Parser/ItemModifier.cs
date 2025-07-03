using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

internal sealed record ItemModifier
{
    private readonly DataManagerService _dm;
    /// <summary>Using with Levenshtein parser</summary>
    private const int LEVENSHTEIN_DISTANCE_DIVIDER = 8; // old val: 6

    internal string Parsed { get; }
    internal double TierMin { get; } = ModFilter.EMPTYFIELD;
    internal double TierMax { get; } = ModFilter.EMPTYFIELD;
    internal bool Unscalable { get; }
    internal bool Negative { get; }

    internal ItemFlag ItemFlag { get; }
    internal Lang Lang { get; }

    internal string NextMod { get; }

    internal ItemModifier(DataManagerService dm, string data, string nextMod, string modName, ItemData item)
    {
        _dm = dm;
        ItemFlag = item.Flag;
        Lang = item.Lang;
        NextMod = nextMod;
        // LOW priority Bug to fix :
        // When there is no '(x-y)' example : Adds 1 to (4–5) Lightning Damage to Spells
        Parsed = ParseTierValues(data, out Tuple<double, double> minmax);
        TierMin = minmax.Item1;
        TierMax = minmax.Item2;

        Parsed = ParseUnscalableValue(Parsed, out bool unscalableValue);
        Unscalable = unscalableValue;
        Parsed = ParseMod(Parsed, item, modName, out bool negativeValue);
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

    private string ParseMod(string mod, ItemData item, string affixName, out bool invertedValue)
    {
        invertedValue = false;

        var staticMod = ParseStaticValueMod(mod);        
        string modKind = staticMod.Item1;
        var match = staticMod.Item2;

        var reduced = Resources.Resources.General102_reduced.Split('/');
        var increased = Resources.Resources.General101_increased.Split('/');

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
            var modEntry = _dm.Filter.Result
                .SelectMany(result => result.Entries)
                .Where(filt => (filt.ID is Strings.Stat.VeiledPrefix && is_VeiledPrefix) 
                    || (filt.ID is Strings.Stat.VeiledSuffix && is_VeiledSuffix))
                .Select(filt => filt.Text)
                .FirstOrDefault(modTxt => modTxt.Length > 0);
            if (modEntry is not null)
            {
                return modEntry;
            }
            return mod;
        }

        for (int j = 0; j < reduced.Length; j++)
        {
            if (!modKind.Contain(reduced[j]))
            {
                continue;
            }
            if (!IsModFilter(modKind)) // mod with reduced stat not found
            {
                string modIncreased = modKind.Replace(reduced[j], increased[j]);
                if (IsModFilter(modIncreased)) // mod with increased stat found
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
                    invertedValue = true;
                    break;
                }
            }
        }

        string returnMod = string.Empty;
        StringBuilder sb = new();

        var parseEntry = _dm.Parser.Mods
            .Where(parse => (modKind.Contains(parse.Old) && parse.Replace == Strings.contains) 
            || (modKind == parse.Old && parse.Replace == Strings.equals)).FirstOrDefault();
        if (parseEntry is not null)
        {
            // mod is parsed
            if (parseEntry.Replace == Strings.contains)
            {
                sb.Append(modKind);
                sb.Replace(parseEntry.Old, parseEntry.New);
            }
            else if (parseEntry.Replace == Strings.equals)
            {
                sb.Append(parseEntry.New);
            }
            else
            {
                sb.Append(modKind); // should never go here
            }
        }
        else
        {
            // mod is not parsed using ParsingRules but with Fastenshtein
            sb.Append(ParseWithFastenshtein(modKind, item.Flag));
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
            if (item.Flag.Chronicle)
            {
                int idxPseudo = 0;//, idxExplicit = 1, idxImplicit = 2;
                var filter = _dm.Filter.Result[idxPseudo].Entries.FirstOrDefault(x => x.Text.Contain(mod));
                if (filter is not null)
                {
                    returnMod = filter.Text;
                }
                else
                {
                    // Done after to prevent new bug if filters are fixed/translated in future
                    if (mod == Resources.Resources.General068_ApexAtzoatl && item.Lang is not Lang.Korean and not Lang.Taiwanese)
                    {
                        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
                        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));
                        mod = rm.GetString("General068_ApexAtzoatl", cultureEn); // Using english version because GGG didnt translated 'pseudo.pseudo_temple_apex' text filter yet

                        filter = _dm.Filter.Result[idxPseudo].Entries.FirstOrDefault(x => x.Text.Contain(mod));
                        if (filter is not null)
                        {
                            returnMod = filter.Text;
                        }
                    }
                }
            }
            if (item.Flag.Weapon || item.Flag.Shield)
            {
                returnMod = ParseWeaponAndShieldStats(mod, item, invertedValue, modKind);
            }
        }
        // temp fix, TO REDO all method
        if (invertedValue)
        {
            if (item.Flag.Weapon || item.Flag.Shield)
            {
                mod = ParseWeaponAndShieldStats(mod, item, invertedValue, modKind);
            }
            return mod;
        }
        return returnMod.Length > 0 ? returnMod : mod;
    }

    private string ParseWeaponAndShieldStats(string mod, ItemData item, bool invertedValue, string modKind)
    {
        string part = item.Lang is not Lang.Korean ? " " : string.Empty;
        var returnMod = string.Empty;
        List<string> stats = new();

        if (item.Flag.Weapon)
        {
            if (item.Flag.Stave)
            {
                stats.Add(Strings.Stat.BlockStaffWeapon);
            }
            bool isBloodlust = _dm.Words.FirstOrDefault(x => x.NameEn is "Hezmana's Bloodlust").Name == item.Name;
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
        if (item.Flag.ArmourPiece)
        {
            if (item.Flag.Shield)
            {
                stats.Add(Strings.Stat.Block);
            }
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
                var resultEntry = _dm.Filter.Result
                    .SelectMany(result => result.Entries)
                    .Where(filter => filter.ID.Contains(stat))
                    .Select(filter => filter.Text);
                if (!resultEntry.Any())
                {
                    continue;
                }

                string modText = resultEntry.First();
                string res = stat == Strings.Stat.Block ? Resources.Resources.General024_Shields :
                    stat == Strings.Stat.BlockStaffWeapon ? Resources.Resources.General025_Staves :
                    Resources.Resources.General023_Local;
                string fullMod = (invertedValue ? modKind.Replace("-", string.Empty) : modKind) + part + res;
                string fullModPositive = fullMod.Replace("#%", "+#%"); // fix (block on staff) to test longterm

                if (modText == fullMod || modText == fullModPositive)
                {
                    returnMod = mod + part + res;
                    break;
                }
            }
        }

        return returnMod.Length is 0 ? mod : returnMod;
    }

    private Tuple<string, MatchCollection> ParseStaticValueMod(string mod)
    {
        var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
        if (match.Count is 0)
        {
            return new(mod, match);
        }

        if (match.Count > 0 && IsModFilter(mod))
        {
            var emptyMatch = RegexUtil.GenerateEmptyMatch().Matches(string.Empty);
            return new(mod, emptyMatch);
        }

        if (match.Count > 1)
        {
            var lMods = new List<Tuple<string, MatchCollection>>();
            bool uniqueMatchs = match.Cast<Match>()
                .Select(m => m.Value).Distinct().Count() == match.Count;
            if (uniqueMatchs)
            {
                string modKind = RegexUtil.DecimalNoPlusPattern().Replace(mod, "#");
                lMods.Add(new(modKind, match));
                for (int i = 0; i < match.Count; i++)
                {
                    var tempMod = RegexUtil.DecimalNoPlusPattern()
                        .Replace(mod, m => m.Value != match[i].Value ? "#" : m.Value);
                    var reverseMod = RegexUtil.DecimalNoPlusPattern()
                        .Replace(mod, m => m.Value == match[i].Value ? "#" : m.Value);
                    var tempMatch = RegexUtil.DecimalNoPlusPattern().Matches(tempMod);
                    lMods.Add(new(reverseMod, tempMatch));
                }
            }

            foreach (var md in lMods)
            {
                if (IsModFilter(md.Item1))
                {
                    return new(md.Item1, md.Item2);
                }
            }
        }
        /*
        if (match.Count is 1)
        {
            string modKind = RegexUtil.DecimalPattern().Replace(mod, "#");
            if (IsModFilter(modKind))
            {
                return new(modKind, match);
            }
        }
        */
        string parsedMod = RegexUtil.DecimalPattern().Replace(mod, "#");
        return new(parsedMod, match);
    }

    private string ParseWithFastenshtein(string str, ItemFlag itemIs)
    {
        var entrySeek = _dm.Filter.Result.SelectMany(result => result.Entries);
        var seek = entrySeek.FirstOrDefault(x => x.Text.Contains(str));
        if (seek is null)
        {
            int maxDistance = str.Length / GetDistanceDivider(itemIs);
            if (maxDistance is 0)
            {
                maxDistance = 1;
            }

            var lev = new Fastenshtein.Levenshtein(str);
            var closestMatch = entrySeek
                .Select(item => new { Item = item, Distance = lev.DistanceFrom(item.Text) })
                .Where(x => x.Distance <= maxDistance)
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (closestMatch is not null 
                && !Strings.dicFastenshteinExclude.ContainsKey(closestMatch.Item.ID))
            {
                str = closestMatch.Item.Text;
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

    private bool IsModFilter(string modifier)
    {
        return _dm.Filter.Result
            .SelectMany(result => result.Entries)
            .Any(filter => filter.Text == modifier);
    }
}

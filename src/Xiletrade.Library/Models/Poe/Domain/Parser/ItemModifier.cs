using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed record ItemModifier
{
    private readonly DataManagerService _dm;
    private readonly ItemData _item;
    /// <summary>Using with Levenshtein parser</summary>
    private const int LEVENSHTEIN_DISTANCE_DIVIDER = 8; // old val: 6

    internal MatchCollection NextModMatch { get; }
    internal string NextMod { get; }
    internal string Parsed { get; }

    internal double TierMin { get; private set; } = ModFilter.EMPTYFIELD;
    internal double TierMax { get; private set; } = ModFilter.EMPTYFIELD;
    internal bool Unscalable { get; private set; }
    internal bool Negative { get; private set; }

    internal ItemModifier(DataManagerService dm, ItemData item, ReadOnlySpan<char> data, ReadOnlySpan<char> modName, string nextMod)
    {
        _dm = dm;
        _item = item;

        (NextMod, NextModMatch) = ParseStaticValueMod(nextMod);
        Parsed = ParseMod(data, modName);
    }

    private string ParseMod(ReadOnlySpan<char> data, ReadOnlySpan<char> affixName)
    {
        var parsingMod = ParseTierValues(data);
        parsingMod = ParseUnscalableValue(parsingMod);

        if (TryResolveVeiledMod(parsingMod, affixName, out string veiled))
            return veiled;
        if (TryResolveDeliriumMod(parsingMod, out string deliriumReward))
            return deliriumReward;

        (var kind, var match) = ParseStaticValueMod(parsingMod);
        (var parsedMod, var modKind) = ParseNegativeMod(parsingMod, kind, match);
        if (Negative)
        {
            if (TierMin.IsNotEmpty()) TierMin = -TierMin;
            if (TierMax.IsNotEmpty()) TierMax = -TierMax;
            return parsedMod;
        }

        StringBuilder sbMod = new(TryParseWithRules(modKind, out string parsedWithRules)
            ? parsedWithRules : ParseWithFastenshtein(modKind));
        if (modKind != sbMod.ToString())
        {
            var condNext = parsedWithRules.Length > 0 && parsedWithRules.Contain("\n") && NextModMatch.Count > 0;
            if (match.Count > 0 || condNext)
            {
                var lMatch = (condNext ? NextModMatch : match).Select(x => x.Value).ToList();
                var lSbMatch = RegexUtil.DecimalNoPlusPattern().Matches(sbMod.ToString()).Select(x => x.Value);
                if (lSbMatch.Any())
                {
                    foreach (var valSbMatch in lSbMatch)
                    {
                        lMatch.Remove(valSbMatch); // remove the first and does not respect order.
                    }
                }
                foreach (var valMatch in lMatch)
                {
                    int idx = sbMod.ToString().IndexOf('#', StringComparison.Ordinal);
                    if (idx > -1)
                    {
                        sbMod.Replace("#", valMatch, idx, 1);
                    }
                }
            }
            return sbMod.ToString();
        }
        if (_item.Flag.Chronicle && TryResolveChronicleMod(parsedMod, out string chronicleMod))
        {
            return chronicleMod;
        }
        if (_item.Flag.Weapon || _item.Flag.ArmourPiece)
        {
            return ParseWeaponAndShieldStats(parsedMod, modKind);
        }
        return parsedMod;
    }

    private string ParseTierValues(ReadOnlySpan<char> data)
    {
        double tierValMin = ModFilter.EMPTYFIELD;
        double tierValMax = ModFilter.EMPTYFIELD;

        const int MAX_ITERATIONS = 10;
        int iteration = 0;
        int start = 0;

        // We keep track of the areas to be deleted to reconstruct the text
        List<(int Start, int Length)> toRemove = new();

        while (start < data.Length && iteration++ < MAX_ITERATIONS)
        {
            int openIdx = data[start..].IndexOf('(');
            if (openIdx is -1) break;
            openIdx += start;

            int closeIdx = data[openIdx..].IndexOf(')');
            if (closeIdx is -1) break;
            closeIdx += openIdx;

            var content = data.Slice(openIdx + 1, closeIdx - openIdx - 1);

            int dashIdx = content.IndexOf('-');
            if (dashIdx is not -1)
            {
                var left = content[..dashIdx];
                var right = content[(dashIdx + 1)..];

                if (double.TryParse(left, out double min) && double.TryParse(right, out double max))
                {
                    tierValMin = tierValMin.IsEmpty() ? min : (tierValMin + min) / 2;
                    tierValMax = tierValMax.IsEmpty() ? max : (tierValMax + max) / 2;
                }
            }
            else if (double.TryParse(content, out double val))
            {
                tierValMin = val;
                tierValMax = val;
            }

            toRemove.Add((openIdx, closeIdx - openIdx + 1));
            start = closeIdx + 1;
        }

        if (tierValMin.IsNotEmpty()) TierMin = Math.Truncate(tierValMin);
        if (tierValMax.IsNotEmpty()) TierMax = Math.Truncate(tierValMax);

        // Final reconstruction
        if (toRemove.Count is 0)
            return data.ToString(); // no parentheses, we return as is

        StringBuilder sb = new();
        int last = 0;
        foreach (var (s, len) in toRemove)
        {
            if (s > last)
                sb.Append(data[last..s]);
            last = s + len;
        }

        if (last < data.Length)
            sb.Append(data[last..]);

        return sb.ToString();
    }

    private string ParseUnscalableValue(ReadOnlySpan<char> data)
    {
        int separatorIndex = data.IndexOf('—');
        if (separatorIndex is not -1)
        {
            var before = data[..separatorIndex].Trim();
            var after = data[(separatorIndex + 1)..].Trim();

            if (after.SequenceEqual(Strings.UnscalableValue.AsSpan()))
            {
                Unscalable = true;
            }

            return before.ToString();
        }
        return data.ToString();
    }

    private bool TryParseWithRules(string modKind, out string parsed)
    {
        parsed = string.Empty;

        StringBuilder sb = new();
        var parseEntry = _dm.Parser.Mods
            .Where(parse => !parse.Disabled && 
            (modKind.Contains(parse.Old) && parse.Replace is Strings.contains
            || modKind == parse.Old && parse.Replace is Strings.equals)).FirstOrDefault();
        if (parseEntry is not null)
        {
            if (parseEntry.Replace is Strings.contains)
            {
                sb.Append(modKind);
                sb.Replace(parseEntry.Old, parseEntry.New);
            }
            else if (parseEntry.Replace is Strings.equals)
            {
                sb.Append(parseEntry.New);
            }
            else
            {
                sb.Append(modKind); // should never go here
            }
            parsed = sb.ToString();
            return true;
        }
        return false;
    }

    private static bool TryResolveDeliriumMod(ReadOnlySpan<char> mod, out string deliriumReward)
    {
        deliriumReward = string.Empty;
        if (mod.StartsWith(Resources.Resources.General098_DeliriumReward.AsSpan(), StringComparison.Ordinal))
        {
            deliriumReward = mod.ToString() + " (×#)";
            return true;
        }
        return false;
    }

    private (string Parsed, string Kind) ParseNegativeMod(string parsedMod, string modKind, MatchCollection match)
    {
        var reduced = Resources.Resources.General102_reduced.Split('/');
        var increased = Resources.Resources.General101_increased.Split('/');
        if (reduced.Length != increased.Length)
        {
            return (parsedMod, modKind);
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
                    Negative = true;
                    var returnMod = parsedMod;
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
                                returnMod = sbInc.ToString();
                            }
                        }
                    }
                    return (returnMod, modIncreased.Replace("#", "-#"));
                }
            }
        }
        return (parsedMod, modKind);
    }

    private bool TryResolveVeiledMod(ReadOnlySpan<char> mod, ReadOnlySpan<char> affixName, out string result)
    {
        result = string.Empty;

        bool isVeiledPrefix = mod.SequenceEqual(Resources.Resources.General106_VeiledPrefix.AsSpan());
        bool isVeiledSuffix = mod.SequenceEqual(Resources.Resources.General107_VeiledSuffix.AsSpan());

        if (!isVeiledPrefix && !isVeiledSuffix)
            return false;

        if (!affixName.IsEmpty)
        {
            result = affixName.ToString();
            return true;
        }

        var targetId = isVeiledPrefix ? Strings.Stat.VeiledPrefix : Strings.Stat.VeiledSuffix;
        var modEntry = _dm.Filter.Result
            .SelectMany(r => r.Entries)
            .Where(f => f.ID == targetId && !string.IsNullOrWhiteSpace(f.Text))
            .Select(f => f.Text)
            .FirstOrDefault();

        result = modEntry ?? mod.ToString();
        return true;
    }

    private bool TryResolveChronicleMod(string mod, out string result)
    {
        result = string.Empty;
        var entry = _dm.Filter.Result[0].Entries.FirstOrDefault(e => e.Text.Contain(mod));
        if (entry is not null)
        {
            result = entry.Text;
            return true;
        }
        // Done after to prevent new bug if filters are fixed/translated in future
        if (mod == Resources.Resources.General068_ApexAtzoatl && _item.Lang is not (Lang.Korean or Lang.Taiwanese))
        {
            var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));
            var enMod = rm.GetString("General068_ApexAtzoatl", new CultureInfo(Strings.Culture[0]));
            if (enMod is not null)
            {
                var fallback = _dm.Filter.Result[0].Entries.FirstOrDefault(e => e.Text.Contain(enMod));
                if (fallback is not null)
                {
                    result = fallback.Text;
                    return true;
                }                    
            }
        }
        return false;
    }

    private string ParseWeaponAndShieldStats(string mod, string modKind)
    {
        string delimiter = _item.Lang is not Lang.Korean ? " " : string.Empty;
        List<string> stats = new();

        if (_item.Flag.Weapon)
        {
            if (_item.Flag.Stave)
            {
                stats.Add(Strings.Stat.Generic.BlockStaffWeapon);
            }
            bool isBloodlust = _dm.Words.FirstOrDefault(x => x.NameEn is "Hezmana's Bloodlust").Name == _item.Name;
            if (!isBloodlust)
            {
                stats.Add(Strings.Stat.Generic.LifeLeech);
            }
            stats.Add(Strings.Stat.Generic.AddAccuracyLocal);
            stats.Add(Strings.Stat.Generic.AttackSpeed);
            stats.Add(Strings.Stat.Generic.ManaLeech);
            stats.Add(Strings.Stat.Generic.PoisonHit);
            stats.Add(Strings.Stat.Generic.IncPhysFlat);
            stats.Add(Strings.Stat.Generic.IncLightFlat);
            stats.Add(Strings.Stat.Generic.IncColdFlat);
            stats.Add(Strings.Stat.Generic.IncFireFlat);
            stats.Add(Strings.Stat.Generic.IncChaosFlat);
        }
        if (_item.Flag.ArmourPiece)
        {
            if (_item.Flag.Shield)
            {
                stats.Add(Strings.Stat.Generic.Block);
            }
            stats.Add(Strings.Stat.Generic.IncEs);
            stats.Add(Strings.Stat.Generic.IncEva);
            stats.Add(Strings.Stat.Generic.IncArmour);
            stats.Add(Strings.Stat.Generic.IncAe);
            stats.Add(Strings.Stat.Generic.IncAes);
            stats.Add(Strings.Stat.Generic.IncEes);
            stats.Add(Strings.Stat.Generic.IncArEes);
            stats.Add(Strings.Stat.Generic.AddArmorFlat);
            stats.Add(Strings.Stat.Generic.AddEsFlat);
            stats.Add(Strings.Stat.Generic.AddEvaFlat);
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

                var modText = resultEntry.First();
                var suffix = stat switch
                {
                    Strings.Stat.Generic.Block => Resources.Resources.General024_Shields,
                    Strings.Stat.Generic.BlockStaffWeapon => Resources.Resources.General025_Staves,
                    _ => Resources.Resources.General023_Local
                };
                var fullMod = (Negative ? modKind.Replace("-", string.Empty) : modKind) + delimiter + suffix;
                var fullModPositive = fullMod.Replace("#%", "+#%"); // fix (block on staff) to test longterm

                if (modText == fullMod || modText == fullModPositive)
                {
                    return $"{mod}{delimiter}{suffix}";
                }
            }
        }

        return mod;
    }

    private (string Kind, MatchCollection Match) ParseStaticValueMod(string mod)
    {
        var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
        if (match.Count is 0)
        {
            return (mod, match);
        }

        if (match.Count > 0 && IsModFilter(mod))
        {
            var emptyMatch = RegexUtil.GenerateEmptyMatch().Matches(string.Empty);
            return (mod, emptyMatch);
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
                    return (md.Item1, md.Item2);
                }
            }
        }

        var kind = RegexUtil.DecimalPattern().Replace(mod, "#");
        return (kind, match);
    }

    private string ParseWithFastenshtein(string mod)
    {
        var entrySeek = _dm.Filter.Result.SelectMany(result => result.Entries);
        var seek = entrySeek.FirstOrDefault(x => x.Text.Contains(mod));
        if (seek is null)
        {
            int maxDistance = mod.Length / GetDistanceDivider();
            if (maxDistance is 0)
            {
                maxDistance = 1;
            }

            var lev = new Fastenshtein.Levenshtein(mod);
            var closestMatch = entrySeek
                .Select(item => new { Item = item, Distance = lev.DistanceFrom(item.Text) })
                .Where(x => x.Distance <= maxDistance)
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (closestMatch is not null 
                && !Strings.dicFastenshteinExclude.ContainsKey(closestMatch.Item.ID))
            {
                return closestMatch.Item.Text;
            }
        }

        return mod;
    }

    // WIP: probably add new rules for other item kind and distinguish according to language
    private int GetDistanceDivider()
    {
        //DataManager.Config.Options.Language
        return _item.Flag.Tablet ? 5 : LEVENSHTEIN_DISTANCE_DIVIDER;
    }

    private bool IsModFilter(string modifier)
    {
        return _dm.Filter.Result
            .SelectMany(result => result.Entries)
            .Any(filter => filter.Text == modifier);
    }
}

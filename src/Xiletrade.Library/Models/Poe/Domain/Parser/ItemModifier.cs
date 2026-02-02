using Raffinert.FuzzySharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Poe.Contract.Extension;
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

    internal ModInfo NextModInfo { get; }
    internal string Parsed { get; }

    internal double TierMin { get; private set; } = ModFilter.EMPTYFIELD;
    internal double TierMax { get; private set; } = ModFilter.EMPTYFIELD;
    internal bool Unscalable { get; private set; }

    internal ItemModifier(DataManagerService dm, ItemData item, ReadOnlySpan<char> data, ReadOnlySpan<char> modName, string nextMod)
    {
        _dm = dm;
        _item = item;

        NextModInfo = new ModInfo(_dm, nextMod);
        Parsed = GetParsedMod(data, modName);
    }

    private string GetParsedMod(ReadOnlySpan<char> data, ReadOnlySpan<char> affixName)
    {
        var normalizedMod = ParseUnscalableValue(ParseTierValues(data));

        if (TryResolveVeiledMod(normalizedMod, affixName, out string veiledMod))
            return veiledMod;
        if (TryResolveDeliriumMod(normalizedMod, out string deliriumRewardMod))
            return deliriumRewardMod;

        var modInfo = new ModInfoParse(_dm, normalizedMod);
        if (modInfo.IsNegative)
        {
            if (TierMin.IsNotEmpty()) TierMin = -TierMin;
            if (TierMax.IsNotEmpty()) TierMax = -TierMax;
            return modInfo.ParsedMod;
        }
        if (TryParseLayers(modInfo, out string layerMod))
        {
            return layerMod;
        }
        if (_item.Flag.Chronicle 
            && TryResolveChronicleMod(modInfo, out string chronicleMod))
        {
            return chronicleMod;
        }
        if (_item.Flag.Weapon || _item.Flag.ArmourPiece)
        {
            return ParseWeaponAndShieldStats(modInfo);
        }
        return modInfo.ParsedMod;
    }

    private bool TryParseLayers(ModInfoParse modInfo, out string returnMod)
    {
        returnMod = string.Empty;
        var intermediateMod = TryParseWithRules(modInfo, out string ruleMod) ? ruleMod
            : modInfo.IsKindFilter ? modInfo.ModKind // previously IsFilterContainMod
            : ParseWithLevenshtein(modInfo);
        if (modInfo.ModKind != intermediateMod)
        {
            var multiLineRule = ruleMod.Length > 0 && ruleMod.Contain("\n");
            returnMod = ReplaceHashes(modInfo.Match, intermediateMod, multiLineRule);
            return true;
        }
        return false;
    }

    private string ReplaceHashes(MatchCollection match, string parsed, bool multiLine)
    {
        var condNext = multiLine && NextModInfo.Match.Count > 0;
        if (match.Count is 0 && !condNext)
            return parsed;

        var lMatch = (condNext ? NextModInfo.Match : match).Select(x => x.Value).ToList();
        var lSbMatch = RegexUtil.DecimalNoPlusPattern().Matches(parsed).Select(x => x.Value);
        if (lSbMatch.Any())
        {
            foreach (var valSbMatch in lSbMatch)
            {
                lMatch.Remove(valSbMatch); // remove the first and does not respect order.
            }
        }

        var sbMod = new StringBuilder(parsed.Length);
        int matchIndex = 0;
        foreach (char c in parsed)
        {
            if (c is '#' && matchIndex < lMatch.Count)
            {
                sbMod.Append(lMatch[matchIndex]);
                matchIndex++;
            }
            else
            {
                sbMod.Append(c);
            }
        }
        return sbMod.ToString();
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

    private bool TryParseWithRules(ModInfoParse modInfo, out string returnMod)
    {
        returnMod = string.Empty;

        StringBuilder sb = new();
        var parseEntry = _dm.Parser.Mods
            .Where(parse => !parse.Disabled && 
            (modInfo.ModKind.Contain(parse.Old) && parse.Replace is Strings.contains
            || modInfo.ModKind == parse.Old && parse.Replace is Strings.equals)).FirstOrDefault();
        if (parseEntry is null)
        {
            return false;
        }

        if (parseEntry.Replace is Strings.contains)
        {
            sb.Append(modInfo.ModKind);
            sb.Replace(parseEntry.Old, parseEntry.New);
        }
        else if (parseEntry.Replace is Strings.equals)
        {
            sb.Append(parseEntry.New);
        }
        else
        {
            sb.Append(modInfo.ModKind); // should never go here
        }
        returnMod = sb.ToString();
        return true;
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
        var entry = _dm.Filter.GetFilterDataEntry(targetId, checkText: true);

        result = entry is null ? mod.ToString() : entry.Text;
        return true;
    }

    private bool TryResolveChronicleMod(ModInfoParse modInfo, out string result)
    {
        result = string.Empty;
        var entry = _dm.Filter.FindPseudoEntryContainingMod(modInfo.ParsedMod);
        if (entry is not null)
        {
            result = entry.Text;
            return true;
        }
        // Done after to prevent new bug if filters are fixed/translated in future
        if (modInfo.ParsedMod == Resources.Resources.General068_ApexAtzoatl && _item.Lang is not (Lang.Korean or Lang.Taiwanese))
        {
            var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));
            var enMod = rm.GetString("General068_ApexAtzoatl", new CultureInfo(Strings.Culture[0]));
            if (enMod is not null)
            {
                var fallback = _dm.Filter.FindPseudoEntryContainingMod(enMod);
                if (fallback is not null)
                {
                    result = fallback.Text;
                    return true;
                }                    
            }
        }
        return false;
    }

    private string ParseWeaponAndShieldStats(ModInfoParse modInfo)
    {
        string delimiter = _item.Lang is not Lang.Korean ? " " : string.Empty;
        var stats = GetWeaponShieldStats();
        if (stats.Count is 0)
        {
            return modInfo.ParsedMod;
        }

        foreach (string stat in stats)
        {
            string modText = null;
            
            var entry = _dm.Filter.GetFilterDataEntry(stat, sequenceEquality: false);
            if (entry is not null)
            {
                modText = entry.Text;
            }

            if (modText is null)
                continue;

            var suffix = stat switch
            {
                Strings.Stat.Generic.Block => Resources.Resources.General024_Shields,
                Strings.Stat.Generic.BlockStaffWeapon => Resources.Resources.General025_Staves,
                _ => Resources.Resources.General023_Local
            };
            var fullMod = (modInfo.IsNegative ? modInfo.ModKind.Replace("-", string.Empty) : modInfo.ModKind) + delimiter + suffix;
            var fullModPositive = fullMod.Replace("#%", "+#%"); // fix (block on staff) to test longterm

            if (modText == fullMod || modText == fullModPositive)
            {
                return $"{modInfo.ParsedMod}{delimiter}{suffix}";
            }
        }

        return modInfo.ParsedMod;
    }

    private List<string> GetWeaponShieldStats()
    {
        List<string> stats = new();
        if (_item.Flag.Weapon)
        {
            if (_item.Flag.Stave)
            {
                stats.Add(Strings.Stat.Generic.BlockStaffWeapon);
            }
            bool isBloodlust = _dm.Words.FirstOrDefault(x => x.NameEn is Strings.Unique.Hezmana).Name == _item.Name;
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
        return stats;
    }

    private string ParseWithLevenshtein(ModInfoParse mod)
    {
        var closestMatch = string.Empty;
        var bestDistance = int.MaxValue;
        int maxDistance = Math.Max(1, mod.ModKind.Length / GetDistanceDivider());

        using Levenshtein lev = new(mod.ModKind);

        foreach (var entry in _dm.Filter.EnumerateEntries())
        {
            if (Math.Abs(entry.Text.Length - mod.ModKind.Length) > maxDistance)
                continue;

            var distance = lev.DistanceFrom(entry.Text);

            if (distance > maxDistance)
                continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                closestMatch = entry.Text;

                if (distance is 0)
                    break;
            }
        }

        if (closestMatch.Length > 0 
            && !Strings.dicLevenshteinExclude.ContainsKey(closestMatch))
        {
            return closestMatch;
        }

        return mod.ModKind;
    }

    // WIP: probably add new rules for other item kind and distinguish according to language
    private int GetDistanceDivider()
    {
        //DataManager.Config.Options.Language
        return _item.Flag.Tablet ? 5 : LEVENSHTEIN_DISTANCE_DIVIDER;
    }
}

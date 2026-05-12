using Raffinert.FuzzySharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed record ModInfoParse : ModInfo
{
    private readonly ItemData _item;
    /// <summary>Using with Levenshtein parser</summary>
    private int LevenshteinDistanceDivider => _item.Flag.Tablet ? 5 : 8; // old val: 6

    internal string ParsedMod { get; }
    internal bool IsNegative { get; }

    /// <summary>
    /// Parse Static And Negative Mod
    /// </summary>
    /// <param name="dm"></param>
    /// <param name="mod"></param>
    internal ModInfoParse(DataManagerService dm, ItemData item, string mod) : base(dm, mod)
    {
        _item = item;
        ParsedMod = mod;

        var reduced = Resources.Resources.General102_reduced.Split('/');
        var increased = Resources.Resources.General101_increased.Split('/');
        if (reduced.Length != increased.Length)
        {
            return;
        }

        for (int j = 0; j < reduced.Length; j++)
        {
            if (!ModKind.Contain(reduced[j]))
            {
                continue;
            }
            if (!IsKindFilter) // mod with reduced stat not found
            {
                string modIncreased = ModKind.Replace(reduced[j], increased[j]);
                if (_dm.Filter.ContainModifier(modIncreased)) // mod with increased stat found
                {
                    IsNegative = true;
                    if (Match.Count > 0)
                    {
                        StringBuilder sbInc = new(modIncreased);
                        sbInc.Replace(reduced[j], increased[j]);
                        for (int i = 0; i < Match.Count; i++)
                        {
                            int idx = sbInc.ToString().IndexOf('#', StringComparison.Ordinal);
                            if (idx > -1)
                            {
                                sbInc.Replace("#", "-" + Match[i].Value, idx, 1);
                                ParsedMod = sbInc.ToString();
                            }
                        }
                    }
                    ModKind = modIncreased.Replace("#", "-#");
                    return;
                }
            }
        }
    }

    internal string ParseBasedOnItemFlag()
    {
        if (_item.Flag.Chronicle && TryResolveChronicleMod(out string chronicleMod))
        {
            return chronicleMod;
        }

        return _item.Flag.Weapon || _item.Flag.ArmourPiece ? ParseWeaponAndShieldStats() : ParsedMod;
    }

    internal bool TryParseLayers(ModInfo nextMod, out string returnMod)
    {
        returnMod = string.Empty;
        var intermediateMod = TryParseWithRules(out string ruleMod) ? ruleMod
            : IsKindFilter ? ModKind // previously IsFilterContainMod
            : ParseWithLevenshtein();
        if (ModKind != intermediateMod)
        {
            var multiLineRule = ruleMod.Length > 0 && ruleMod.Contain("\n");
            returnMod = ReplaceHashes(Match, nextMod.Match, intermediateMod, multiLineRule);
            return true;
        }
        return false;
    }

    // private
    private string ParseWeaponAndShieldStats()
    {
        string delimiter = _item.Lang is not Lang.Korean ? " " : string.Empty;
        var stats = GetWeaponShieldStats();
        if (stats.Count is 0)
        {
            return ParsedMod;
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
            var fullMod = (IsNegative ? ModKind.Replace("-", string.Empty) : ModKind) + delimiter + suffix;
            var fullModPositive = fullMod.Replace("#%", "+#%"); // fix (block on staff) to test longterm

            if (modText == fullMod || modText == fullModPositive)
            {
                return $"{ParsedMod}{delimiter}{suffix}";
            }
        }

        return ParsedMod;
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
            var isBloodlust = _dm.Words.FindWordByNameEn(Strings.Unique.Hezmana)?.Name == _item.Name;
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

    private bool TryResolveChronicleMod(out string result)
    {
        result = string.Empty;
        var entry = _dm.Filter.FindPseudoEntryContainingMod(ParsedMod);
        if (entry is not null)
        {
            result = entry.Text;
            return true;
        }
        // Done after to prevent new bug if filters are fixed/translated in future
        if (ParsedMod == Resources.Resources.General068_ApexAtzoatl && _item.Lang is not (Lang.Korean or Lang.Taiwanese))
        {
            var enMod = Resources.Resources.ResourceManager
                .GetEnglish(nameof(Resources.Resources.General068_ApexAtzoatl));
            if (_dm.Filter.FindPseudoEntryContainingMod(enMod) is var fallback && fallback is not null)
            {
                result = fallback.Text;
                return true;
            }
        }
        return false;
    }

    private static string ReplaceHashes(MatchCollection match, MatchCollection nextMatch, string parsed, bool multiLine)
    {
        var condNext = multiLine && nextMatch.Count > 0;
        if (match.Count is 0 && !condNext)
            return parsed;

        var lMatch = (condNext ? nextMatch : match).Select(x => x.Value).ToList();
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

    private string ParseWithLevenshtein()
    {
        var closestMatch = string.Empty;
        var bestDistance = int.MaxValue;
        int maxDistance = Math.Max(1, ModKind.Length / LevenshteinDistanceDivider);

        using Levenshtein lev = new(ModKind);

        foreach (var entry in _dm.Filter.EnumerateEntries())
        {
            if (Math.Abs(entry.Text.Length - ModKind.Length) > maxDistance)
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

        return ModKind;
    }

    private bool TryParseWithRules(out string returnMod)
    {
        returnMod = string.Empty;

        StringBuilder sb = new();
        var parseEntry = _dm.Parser.Mods.Where(parse => !parse.Disabled &&
            (ModKind.Contain(parse.Old) && parse.Replace is Strings.contains
            || ModKind == parse.Old && parse.Replace is Strings.equals)).FirstOrDefault();
        if (parseEntry is null)
        {
            return false;
        }

        if (parseEntry.Replace is Strings.contains)
        {
            sb.Append(ModKind);
            sb.Replace(parseEntry.Old, parseEntry.New);
        }
        else if (parseEntry.Replace is Strings.equals)
        {
            sb.Append(parseEntry.New);
        }
        else
        {
            sb.Append(ModKind); // should never go here
        }
        returnMod = sb.ToString();
        return true;
    }
}
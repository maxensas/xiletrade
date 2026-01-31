using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

internal record ModInfo
{
    private readonly DataManagerService _dm;

    internal string ModKind { get; set; }
    internal MatchCollection Match { get; }

    /// <summary>
    /// Return true if Filter contain ModKind
    /// </summary>
    internal bool IsKindFilter => ModKind.Length > 0 && IsFilterMod(ModKind);

    /// <summary>
    /// Parse Static Mod
    /// </summary>
    internal ModInfo(DataManagerService dm, string mod)
    {
        _dm = dm;
        var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
        if (match.Count is 0)
        {
            ModKind = mod;
            Match = match;
            return;
        }

        if (match.Count > 0 && IsFilterMod(mod))
        {
            var emptyMatch = RegexUtil.GenerateEmptyMatch().Matches(string.Empty);
            ModKind = mod;
            Match = emptyMatch;
            return;
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
                if (IsFilterMod(md.Item1))
                {
                    ModKind = md.Item1;
                    Match = md.Item2;
                    return;
                }
            }
        }

        ModKind = RegexUtil.DecimalPattern().Replace(mod, "#");
        Match = match;
    }

    internal bool IsFilterMod(ReadOnlySpan<char> modifier)
    {
        foreach (var result in _dm.Filter.Result)
        {
            foreach (var entry in result.Entries)
            {
                if (modifier.SequenceEqual(entry.Text.AsSpan()))
                    return true;
            }
        }
        return false;
    }

    internal bool IsFilterContainMod(ReadOnlySpan<char> modifier)
    {
        foreach (var result in _dm.Filter.Result)
        {
            foreach (var entry in result.Entries)
            {
                if (entry.Text.AsSpan().Contain(modifier))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

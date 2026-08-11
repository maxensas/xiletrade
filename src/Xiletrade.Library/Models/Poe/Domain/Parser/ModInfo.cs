using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal record ModInfo
{
    internal readonly DataManagerService _dm;

    internal string ModKind { get; set; }
    internal MatchCollection Match { get; }

    /// <summary>
    /// Return true if Filter contain ModKind
    /// </summary>
    internal bool IsKindFilter => ModKind.Length > 0 && _dm.Filter.ContainModifier(ModKind);

    /// <summary>
    /// Return ModKind with merged matches
    /// </summary>
    internal string ModKindWithMatch => ReplaceHashes(Match, null, ModKind, false);

    /// <summary>
    /// Parse Static Mod
    /// </summary>
    internal ModInfo(DataManagerService dm, string mod)
    {
        _dm = dm;
        if (mod.StartsWith('(') && mod.EndsWith(')'))
        {
            var emptyMatch = RegexUtil.GenerateEmptyMatch().Matches(string.Empty);
            ModKind = string.Empty;
            Match = emptyMatch;
            return;
        }
        var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
        if (match.Count is 0)
        {
            ModKind = mod;
            Match = match;
            return;
        }

        if (match.Count > 0 && _dm.Filter.ContainModifier(mod))
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
                if (_dm.Filter.ContainModifier(md.Item1))
                {
                    ModKind = md.Item1;
                    Match = md.Item2;
                    return;
                }
            }
        }
        // previous regex : DecimalPattern
        // current DecimalNoPlusPattern avoid useless layer parsing
        // Applies to : all mods containing the plus sign
        // TO DO : identify restrictive cases to find new avenue OR invalidate this change 
        ModKind = RegexUtil.DecimalNoPlusPattern().Replace(mod, "#");
        Match = match;
    }

    protected static string ReplaceHashes(MatchCollection match, MatchCollection nextMatch, string parsed, bool multiLine)
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
}
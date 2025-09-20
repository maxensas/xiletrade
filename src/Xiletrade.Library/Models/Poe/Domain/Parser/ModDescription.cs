using System;
using System.Text;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed class ModDescription
{
    internal bool IsParsed { get; private set; }
    internal string Kind { get; private set; } = string.Empty;
    internal string Tags { get; private set; } = string.Empty;
    internal string Name { get; private set; } = string.Empty;
    internal string Quality { get; private set; } = string.Empty;
    internal int Tier { get; private set; } = -1;

    internal ModDescription()
    {

    }

    /// <summary>
    /// Class used to parse the "advanced" mod description before the mod line.
    /// </summary>
    /// <example>
    /// { Prefix Modifier "Cruel" (Tier: 6) — Damage, Physical, Attack }
    /// 139(135-154)% increased Physical Damage
    /// </example>
    internal ModDescription(string data, bool implicitKind)
    {
        if (!(data.StartsWith('{') && data.EndsWith('}')))
        {
            return;
        }

        var affixOptions = data.Split('—', StringSplitOptions.TrimEntries);

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
            if (tierString.Contain(':'))
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(tierString);
                if (match.Count > 0)
                {
                    _ = int.TryParse(match[0].Value, out int tier);
                    Tier = tier;
                }
                affixOptions[0] = affixOptions[0].Replace(tierString, string.Empty).Trim();
            }
        }

        var affixOpt = affixOptions[0].Split('"');
        if (affixOpt.Length is 3)
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
            Name = name.Replace("«", string.Empty).Replace("»", string.Empty).Trim();
            affixOptions[0] = affixOptions[0].Replace(name, string.Empty).Trim();
        }
        // Last step
        Kind = implicitKind ? Resources.Resources.General073_ModifierImplicit
            : affixOptions[0].Replace(":", string.Empty).Trim(); // french version use ":"

        if (affixOptions.Length > 1)
        {
            Tags = affixOptions[1];
        }
        if (affixOptions.Length > 2)
        {
            Quality = affixOptions[2];
        }

        IsParsed = true;
    }
}

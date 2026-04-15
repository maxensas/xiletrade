using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed record ItemModifier
{
    private readonly DataManagerService _dm;

    internal AffixFlag Affix { get; }
    internal ModInfo NextModInfo { get; }

    internal string Parsed { get; }
    internal double TierMin { get; } = ModFilter.EMPTYFIELD;
    internal double TierMax { get; } = ModFilter.EMPTYFIELD;
    internal bool Unscalable { get; }
    internal bool IsBreakpointMod { get; }

    internal ItemModifier(DataManagerService dm, ItemData item, AffixFlag affix, string nextMod)
    {
        _dm = dm;
        Affix = affix;
        NextModInfo = new ModInfo(_dm, nextMod);
        (TierMin, TierMax) = ParseTierValues(Affix.ParsedData, out string tierParsed);
        Unscalable = ParseUnscalableValue(tierParsed, out string normalizedMod);
        Parsed = GetParsedMod(item, normalizedMod, out bool isNegative);
        if (isNegative)
        {
            if (TierMin.IsNotEmpty()) TierMin = -TierMin;
            if (TierMax.IsNotEmpty()) TierMax = -TierMax;
        }

        // extend later if needed
        IsBreakpointMod = item.Flag.Chronicle && Parsed == Resources.Resources.General177_AtzoatlObstructed;
    }

    private string GetParsedMod(ItemData item, string mod, out bool isNegative)
    {
        isNegative = false;
        if (TryResolveVeiledMod(mod, Affix, out string veiledMod))
            return veiledMod;
        if (TryResolveDeliriumMod(mod, out string deliriumRewardMod))
            return deliriumRewardMod;

        var modInfo = new ModInfoParse(_dm, item, mod);
        if (modInfo.IsNegative)
        {
            isNegative = true;
            return modInfo.ParseBasedOnItemFlag();
        }
        if (modInfo.TryParseLayers(NextModInfo, out string layerMod))
        {
            return layerMod;
        }
        return modInfo.ParseBasedOnItemFlag();
    }

    private static (double tierMin, double tierMax) ParseTierValues(ReadOnlySpan<char> data, out string parsedMod)
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

        if (tierValMin.IsNotEmpty()) tierValMin = GetTierValue(tierValMin);
        if (tierValMax.IsNotEmpty()) tierValMax = GetTierValue(tierValMax);

        // Final reconstruction
        if (toRemove.Count is 0)
        {
            parsedMod = data.ToString();
            return (tierValMin, tierValMax);
        }

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

        parsedMod = sb.ToString();
        return (tierValMin, tierValMax);
    }

    private static double GetTierValue(double value) 
        => value < 10 ? Math.Round(value, 1) : Math.Truncate(value);

    private static bool ParseUnscalableValue(ReadOnlySpan<char> data, out string parsedMod)
    {
        int separatorIndex = data.IndexOf('—');
        if (separatorIndex is not -1)
        {
            var before = data[..separatorIndex].Trim();
            var after = data[(separatorIndex + 1)..].Trim();
            parsedMod = before.ToString();
            return after.SequenceEqual(Strings.UnscalableValue.AsSpan());
        }
        parsedMod = data.ToString();
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

    private bool TryResolveVeiledMod(ReadOnlySpan<char> mod, AffixFlag affix, out string result)
    {
        result = affix.Description is not null ? affix.Description.Name : string.Empty;

        bool isVeiledPrefix = mod.SequenceEqual(Resources.Resources.General106_VeiledPrefix.AsSpan());
        bool isVeiledSuffix = mod.SequenceEqual(Resources.Resources.General107_VeiledSuffix.AsSpan());

        if (!isVeiledPrefix && !isVeiledSuffix)
            return false;
        
        if (!string.IsNullOrEmpty(result))
        {
            return true;
        }

        var targetId = isVeiledPrefix ? Strings.Stat.VeiledPrefix : Strings.Stat.VeiledSuffix;
        var entry = _dm.Filter.GetFilterDataEntry(targetId, checkText: true);

        result = entry is null ? mod.ToString() : entry.Text;
        return true;
    }
}

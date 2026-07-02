using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Extension;

internal static class ModLineExtensions
{
    internal static List<ModLine> HandleDuplicates(this List<ModLine> listMod, bool selectMinTier)
    {
        if (listMod.Count <= 1)
            return listMod;

        // Detect duplicates only (cheap first pass)
        var seen = new HashSet<string>(StringComparer.Ordinal);
        bool found = false;
        for (int i = 0; i < listMod.Count; i++)
        {
            var mod = listMod[i];
            if (mod.TierAffixKind is not (Strings.AffixKind.Prefix or Strings.AffixKind.Suffix))
                continue;

            if (!seen.Add(mod.ItemFilter.Id))
            {
                found = true;
                break;
            }
        }
        // Merge now duplicates
        return found ? listMod.MergeDuplicates(selectMinTier) : listMod;
    }

    private static List<ModLine> MergeDuplicates(this List<ModLine> listMod, bool selectMinTier)
    {
        var dict = new Dictionary<string, List<ModLine>>(listMod.Count, StringComparer.Ordinal);
        var firstIndex = new Dictionary<string, int>(StringComparer.Ordinal);

        // 1. Group + memorizing the first index
        for (int i = 0; i < listMod.Count; i++)
        {
            var mod = listMod[i];

            if (mod.TierAffixKind is not (Strings.AffixKind.Prefix or Strings.AffixKind.Suffix))
                continue;

            var id = mod.ItemFilter.Id;

            if (!dict.TryGetValue(id, out var bucket))
            {
                bucket = new List<ModLine>(1);
                dict[id] = bucket;

                // we store the first index of the group
                firstIndex[id] = i;
            }

            bucket.Add(mod);
        }

        // 2. Merge
        bool aborted = false;
        var mergedById = new Dictionary<string, ModLine>(StringComparer.Ordinal);
        foreach (var kvp in dict)
        {
            var bucket = kvp.Value;

            if (bucket.Count <= 1)
                continue;

            var modLine = GetMergedMod(bucket, selectMinTier, out bool abort);
            if (abort)
                aborted = true;

            mergedById[kvp.Key] = modLine;
        }

        // 3. strict validation 
        if (aborted || mergedById.Count is 0)
            return listMod;

        // 4. reconstruction while preserving the original order
        var result = new List<ModLine>();
        var used = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < listMod.Count; i++)
        {
            var mod = listMod[i];

            if (mod.TierAffixKind is Strings.AffixKind.Prefix or Strings.AffixKind.Suffix)
            {
                var id = mod.ItemFilter.Id;

                if (dict.TryGetValue(id, out var bucket) && bucket.Count > 1)
                {
                    // we only replace at the first index of the group
                    if (!used.Contains(id) && firstIndex[id] == i)
                    {
                        result.Add(mergedById[id]);
                        used.Add(id);
                    }
                    continue;
                }
            }
            result.Add(mod);
        }
        return result;
    }

    /// <summary>
    ///  Using LINQ not optimized / deprecated
    /// </summary>
    /// <param name="modList"></param>
    /// <param name="selectMinTier"></param>
    /// <param name="aborted"></param>
    /// <returns></returns>
    private static ModLine GetMergedModOld(List<ModLine> modList, bool selectMinTier, out bool aborted)
    {
        var mod = modList[0];
        var tier = string.Join("+", modList.Select(i => i.Tier).Distinct());
        aborted = mod.Max.Length > 0 || mod.Mod.Count(i => i is '#') is not 1;
        if (mod.CurrentVal > 0 && !aborted)
        {
            var curVal = modList.Sum(i => i.Current.ToDoubleDefault());
            var cur = curVal.ToString();
            var min = cur;
            // max not handled
            string modBis;
            if (mod.TierMin.IsNotEmpty() && mod.TierMax.IsNotEmpty() && mod.TierList.Count > 1)
            {
                var tierMin = modList.Sum(i => i.TierMin);
                var tierMax = modList.Sum(i => i.TierMax);
                var range = Math.Truncate(tierMin) + "-" + Math.Truncate(tierMax);
                modBis = mod.Mod.ReplaceFirst("#", "(" + range + ")");
                var tierList = mod.TierList.Select(x => new ToolTipItem(x)).ToList();
                tierList[0].Text = range;
                for (int i = 0; i < modList.Count && i + 1 < mod.TierList.Count; i++)
                {
                    tierList[i + 1].Text = string.Join(" ", modList[i].TierList
                        .Skip(1).Select(t => t.Text).Where(t => !string.IsNullOrEmpty(t)));
                }
                return new(modList[0], tier, curVal, cur, selectMinTier ? tierMin.ToStr() : min, 
                    modBis, tierList, tierMin, tierMax);
            }
            modBis = mod.Mod.ReplaceFirst("#", min);
            return new(modList[0], tier, curVal, cur, min, modBis);
        }
        return new(modList[0], tier);
    }

    private static ModLine GetMergedMod(List<ModLine> modList, bool selectMinTier, out bool aborted)
    {
        var mod = modList[0];

        // Distinct + Join
        var tiers = new HashSet<string>();
        var tierBuilder = new StringBuilder();
        foreach (var m in modList)
        {
            if (tiers.Add(m.Tier))
            {
                if (tierBuilder.Length > 0)
                    tierBuilder.Append('+');

                tierBuilder.Append(m.Tier);
            }
        }
        var tier = tierBuilder.ToString();

        // Count('#')
        int sharpCount = 0;
        foreach (char c in mod.Mod)
        {
            if (c is '#')
                sharpCount++;
        }

        aborted = mod.Max.Length > 0 || sharpCount is not 1;

        if (mod.CurrentVal <= 0 || aborted)
            return new(mod, tier);

        double curVal = 0;

        foreach (var m in modList)
            curVal += m.Current.ToDoubleDefault();

        var cur = curVal.ToString();
        var min = cur;

        if (mod.TierMin.IsNotEmpty() && mod.TierMax.IsNotEmpty() && mod.TierList.Count > 1)
        {
            double tierMin = 0;
            double tierMax = 0;
            foreach (var m in modList)
            {
                tierMin += m.TierMin;
                tierMax += m.TierMax;
            }

            var range = $"{Math.Truncate(tierMin)}-{Math.Truncate(tierMax)}";
            var modBis = mod.Mod.ReplaceFirst("#", "(" + range + ")");

            var tierList = new List<ToolTipItem>(mod.TierList.Count);
            foreach (var item in mod.TierList)
                tierList.Add(new(item));

            tierList[0].Text = range;

            for (int i = 0; i < modList.Count && i + 1 < mod.TierList.Count; i++)
            {
                var sb = new StringBuilder();
                for (int j = 1; j < modList[i].TierList.Count; j++)
                {
                    if (string.IsNullOrEmpty(modList[i].TierList[j].Text))
                        continue;

                    if (sb.Length > 0)
                        sb.Append(' ');

                    sb.Append(modList[i].TierList[j].Text);
                }
                tierList[i + 1].Text = sb.ToString();
            }
            return new(mod, tier, curVal, cur, selectMinTier ? tierMin.ToStr() : min, 
                modBis, tierList, tierMin, tierMax);
        }
        return new(mod, tier, curVal, cur, min, mod.Mod.ReplaceFirst("#", min));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Extension;

internal static class ModLineExtensions
{
    internal static List<ModLine> MergeSameMods(this List<ModLine> listMod)
    {
        if (listMod.Count <= 1)
        {
            return listMod;
        }

        var duplicatesIdList = listMod
            .Where(g => g.TierKind is Strings.TierKind.Prefix or Strings.TierKind.Suffix)
            .GroupBy(t => t.ItemFilter.Id).Where(g => g.Count() > 1).Select(g => g.Key);
        if (!duplicatesIdList.Any())
        {
            return listMod;
        }

        bool aborted = false;
        var groupedDuplicates = listMod
            .Where(g => g.TierKind is Strings.TierKind.Prefix or Strings.TierKind.Suffix)
            .GroupBy(t => t.ItemFilter.Id).Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.ToList());
        var mergedDupList = groupedDuplicates.Select(kvp =>
        {
            var mod = GetMergedMod(kvp.Value, out bool abort);
            if (abort)
            {
                aborted = true;
            }
            return mod;
        }).ToList();

        if (!aborted && mergedDupList.Count > 0 && mergedDupList.Count == duplicatesIdList.Count())
        {
            return [.. mergedDupList.Concat(listMod.Where(i => !duplicatesIdList.Contains(i.ItemFilter.Id)))];
        }

        return listMod;
    }

    private static ModLine GetMergedMod(List<ModLine> modList, out bool aborted)
    {
        var mod = modList[0];
        mod.Tier = string.Join("+", modList.Select(i => i.Tier).Distinct());
        aborted = mod.Max.Length > 0 || mod.Mod.Count(i => i is '#') is not 1;
        if (mod.CurrentVal > 0 && !aborted)
        {
            mod.CurrentVal = modList.Sum(i => i.Current.ToDoubleDefault());
            mod.Current = mod.CurrentVal.ToString();
            mod.Min = mod.Current;
            if (mod.TierMin.IsNotEmpty() && mod.TierMax.IsNotEmpty() && mod.TierList.Count > 1)
            {
                mod.TierMin = modList.Sum(i => i.TierMin);
                mod.TierMax = modList.Sum(i => i.TierMax);
                var range = Math.Truncate(mod.TierMin) + "-" + Math.Truncate(mod.TierMax);
                mod.ModBis = mod.Mod.ReplaceFirst("#", "(" + range + ")");
                mod.TierList[0].Text = range;
                for (int i = 0; i < modList.Count && i + 1 < mod.TierList.Count; i++)
                {
                    mod.TierList[i + 1].Text = string.Join(" ", modList[i].TierList
                        .Skip(1).Select(t => t.Text).Where(t => !string.IsNullOrEmpty(t)));
                }
            }
            else
            {
                mod.ModBis = mod.Mod.ReplaceFirst("#", mod.Min);
            }
        }
        return mod;
    }
}

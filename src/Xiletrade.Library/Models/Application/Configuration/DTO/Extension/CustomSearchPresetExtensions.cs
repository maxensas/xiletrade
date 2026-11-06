using System;
using System.Collections.Generic;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class CustomSearchPresetExtensions
{
    /// <summary>Presets added to the presets : a preset with the same name in the same game is replaced, a preset without name ignored.</summary>
    /// <returns>All the presets, null : none.</returns>
    internal static CustomSearchPreset[] Merge(this CustomSearchPreset[] presets, IEnumerable<CustomSearchPreset> added)
    {
        var list = new List<CustomSearchPreset>(presets ?? []);
        foreach (var preset in added)
        {
            if (string.IsNullOrWhiteSpace(preset?.Name))
            {
                continue;
            }
            var index = list.FindIndex(x => x.Poe2 == preset.Poe2
                && string.Equals(x.Name, preset.Name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                list[index] = preset;
                continue;
            }
            list.Add(preset);
        }
        return list.Count > 0 ? [.. list] : null;
    }

    /// <summary>
    /// Makes the preset compatible with the filters of its game : a stat id no longer in the filters is replaced
    /// by the closest one (same id with another case, then same stat with another kind), otherwise removed,
    /// as an unknown min / max filter.
    /// </summary>
    /// <param name="filter">Filters of the game of the preset.</param>
    /// <param name="report">Changes made, displayed to the user.</param>
    /// <returns>The preset was modified.</returns>
    internal static bool Repair(this CustomSearchPreset preset, FilterData filter, List<string> report)
    {
        var changed = false;
        if (preset.Stats is not null && filter?.Result is not null)
        {
            var stats = new List<CustomSearchPresetStat>(preset.Stats.Length);
            foreach (var stat in preset.Stats)
            {
                if (string.IsNullOrEmpty(stat?.Id))
                {
                    changed = true;
                    continue;
                }
                if (filter.ContainsId(stat.Id))
                {
                    stats.Add(stat);
                    continue;
                }
                changed = true;
                var match = filter.FindClosestId(stat.Id);
                if (match is null)
                {
                    report.Add(string.Format(Resources.Resources.Main293_PresetStatRemoved, preset.Name, stat.Id));
                    continue;
                }
                report.Add(string.Format(Resources.Resources.Main292_PresetStatReplaced, preset.Name, stat.Id, match));
                stat.Id = match;
                stats.Add(stat);
            }
            if (changed)
            {
                preset.Stats = stats.Count > 0 ? [.. stats] : null;
            }
        }

        if (preset.Filters is not null)
        {
            var filters = new List<CustomSearchPresetFilter>(preset.Filters.Length);
            var filtersChanged = false;
            foreach (var minMax in preset.Filters)
            {
                if (minMax is not null && minMax.TryGetPanel(out _))
                {
                    filters.Add(minMax);
                    continue;
                }
                filtersChanged = true;
                if (!string.IsNullOrEmpty(minMax?.Id))
                {
                    report.Add(string.Format(Resources.Resources.Main293_PresetStatRemoved, preset.Name, minMax.Id));
                }
            }
            if (filtersChanged)
            {
                changed = true;
                preset.Filters = filters.Count > 0 ? [.. filters] : null;
            }
        }
        return changed;
    }

    /// <summary>Min / max filter of the name of the preset filter, a number is not accepted.</summary>
    internal static bool TryGetPanel(this CustomSearchPresetFilter filter, out StatPanel panel)
    {
        var id = filter.Id;
        panel = StatPanel.NoStat;
        return !string.IsNullOrEmpty(id) && char.IsAsciiLetter(id[0])
            && Enum.TryParse(id, ignoreCase: true, out panel) && panel is not StatPanel.NoStat;
    }

    private static bool ContainsId(this FilterData filter, string id)
    {
        foreach (var entry in filter.EnumerateEntries())
        {
            if (string.Equals(entry.ID, id, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    // the order of the filters decides between several stats of another kind
    private static string FindClosestId(this FilterData filter, string id)
    {
        foreach (var entry in filter.EnumerateEntries())
        {
            if (string.Equals(entry.ID, id, StringComparison.OrdinalIgnoreCase))
            {
                return entry.ID;
            }
        }

        // same stat, another kind : explicit.stat_123 -> implicit.stat_123
        var dot = id.IndexOf('.');
        if (dot < 0 || dot == id.Length - 1)
        {
            return null;
        }
        var stat = id.AsSpan(dot + 1);
        foreach (var entry in filter.EnumerateEntries())
        {
            var entryId = entry.ID;
            if (entryId is null)
            {
                continue;
            }
            var entryDot = entryId.IndexOf('.');
            if (entryDot >= 0 && entryId.AsSpan(entryDot + 1).SequenceEqual(stat))
            {
                return entryId;
            }
        }
        return null;
    }
}

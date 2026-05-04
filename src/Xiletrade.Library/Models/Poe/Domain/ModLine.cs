using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class ModLine
{
    private readonly DataManagerService _dm;

    internal int Level { get; }
    internal int AffixIndex { get; }
    internal int OptionIndex { get; } = -1;

    internal string Mod { get; }
    internal string ModBis { get; private set; }
    internal string ModKind { get; }
    internal string Current { get; private set; }
    internal double CurrentVal { get; private set; }
    internal string Min { get; private set; }
    internal string Max { get; }

    internal string Tier { get; private set; }
    internal string TierKind { get; }
    internal string TierTag { get; } = "null";
    internal double TierMin { get; private set; } = ModFilter.EMPTYFIELD;
    internal double TierMax { get; private set; } = ModFilter.EMPTYFIELD;

    internal List<AffixFilterEntrie> AffixList { get; }
    internal List<ToolTipItem> TagList { get; }
    internal List<ToolTipItem> TierList { get; }
    internal List<string> OptionList { get; }
    internal List<int> OptionIdList { get; }

    internal ItemFilter ItemFilter { get; }

    internal ModLine(DataManagerService dm, ItemData item, ModFilter modFilter)
    {
        _dm = dm;

        var affixFlag = modFilter.Mod.Affix;
        AffixList = modFilter.ModValue.ListAffix;

        if (int.TryParse(affixFlag.Description?.Level, out int lvl))
        {
            Level = lvl;
        }
        ItemFilter = new()
        {
            Id = modFilter.Entrie.ID, // filter.Type
            Text = modFilter.Entrie.Text,
            Type = modFilter.Entrie.Type,
            Option = ModFilter.EMPTYFIELD,
            Max = modFilter.ModValue.Max,
            Min = modFilter.ModValue.Min,
            Disabled = true
        };
        if (modFilter.Entrie.Option.Options is not null) // retrieve options and select option found
        {
            int selId = -1;
            var listOpt = modFilter.Entrie.Option.Options.OrderBy(x => x.Text);
            OptionList = new();
            OptionIdList = new();
            foreach (var opt in listOpt)
            {
                string optionText = GetReducesOptionText(opt.Text);
                OptionList.Add(optionText); // fire selection changed
                OptionIdList.Add(opt.ID.Id); // fire selection changed

                string[] textLine = opt.Text.Split(Strings.LF);
                for (int l = 0; l < textLine.Length; l++)
                {
                    if (modFilter.Mod.Parsed.ToLowerInvariant().Contain(textLine[l].ToLowerInvariant()))
                    {
                        ItemFilter.Option = opt.ID.Id;
                        selId = OptionList.Count - 1;
                        break;
                    }
                }
            }
            if (selId > -1)
            {
                OptionIndex = selId;
                ItemFilter.Min = ItemFilter.Max = ModFilter.EMPTYFIELD;
            }
            else if (item.Flag.Chronicle)
            {
                OptionIndex = 1;
                ItemFilter.Min = ItemFilter.Max = ModFilter.EMPTYFIELD;
            }
        }
        AffixIndex = GetAffixIndex(affixFlag, item.Flag);
        Mod = modFilter.Entrie.Text;

        if (affixFlag.Description?.Tags?.Length > 0)
        {
            var tags = affixFlag.Description.Tags.Split(',', StringSplitOptions.TrimEntries);
            TagList = new();
            foreach (string t in tags)
            {
                TagList.Add(new(t));
            }
        }

        if (ItemFilter.Min.IsNotEmpty() && ItemFilter.Max.IsNotEmpty())
        {
            var seek = modFilter.Entrie.Text.ToCharArray();
            int dieze = 0;
            for (int h = 0; h < seek.Length; h++)
            {
                if (seek[h] is '#')
                {
                    dieze++;
                }
            }
            if (dieze is 2) // 2 '#'
            {
                ItemFilter.Min += ItemFilter.Max;
                ItemFilter.Min = Math.Truncate(ItemFilter.Min / 2 * 10 / 10);
                ItemFilter.Max = ModFilter.EMPTYFIELD;
            }
        }
        else if (ItemFilter.Min.IsNotEmpty() || ItemFilter.Max.IsNotEmpty())
        {
            var split = modFilter.Entrie.ID.Split('.');
            bool defMaxPosition = split.Length is 2 && Strings.Stat.dicDefaultPosition.ContainsKey(split[1]);
            var negativeMin = ItemFilter.Min < 0 && ItemFilter.Max.IsEmpty();
            if (negativeMin || (defMaxPosition && ItemFilter.Min > 0 && ItemFilter.Max.IsEmpty()))
            {
                ItemFilter.Max = ItemFilter.Min;
                ItemFilter.Min = ModFilter.EMPTYFIELD;
            }
        }

        if (affixFlag.Description?.AugmentPerCent > 0)
        {
            if (ItemFilter.Min.IsNotEmpty())
            {
                ItemFilter.Min += ItemFilter.Min * affixFlag.Description.AugmentPerCent / 100;
                //min = Math.Round(min, 0);
                if (ItemFilter.Min > 10 ||
                    affixFlag.Description.Tags == Resources.Resources.General029_Gem)
                {
                    ItemFilter.Min = Math.Truncate(ItemFilter.Min);
                }
            }
            else if (ItemFilter.Max.IsNotEmpty())
            {
                ItemFilter.Max += ItemFilter.Max * affixFlag.Description.AugmentPerCent / 100;
                //max = Math.Round(max, 0);
                if (ItemFilter.Max > 10)
                {
                    ItemFilter.Max = Math.Truncate(ItemFilter.Max);
                }
            }
        }
        string specifier = "G";
        Current = ItemFilter.Min.IsEmpty() ? string.Empty : ItemFilter.Min.ToString(specifier, CultureInfo.InvariantCulture);
        if (Current.Length is 0)
        {
            Current = ItemFilter.Max.IsEmpty() ? string.Empty : ItemFilter.Max.ToString(specifier, CultureInfo.InvariantCulture);
        }

        TierKind = affixFlag.Description?.TierKind;
        if (!string.IsNullOrEmpty(TierKind))
        {
            Tier = TierKind + (affixFlag.Description.Tier > -1 ? affixFlag.Description.Tier : string.Empty);
            List<ToolTipItem> tierList = new();
            if (modFilter.Mod.TierMin.IsNotEmpty() && modFilter.Mod.TierMax.IsNotEmpty())
            {
                TierMin = modFilter.Mod.TierMin;
                TierMax = modFilter.Mod.TierMax;
                string tValmin = modFilter.Mod.TierMin.ToString(specifier, CultureInfo.InvariantCulture);
                string tValmax = modFilter.Mod.TierMax.ToString(specifier, CultureInfo.InvariantCulture);
                string tip = tValmin == tValmax ? tValmin : tValmin + "-" + tValmax;
                tierList.Add(new(tip));
                if (!string.IsNullOrEmpty(affixFlag.Description.Quality))
                {
                    tierList.Add(new("(" + affixFlag.Description.Quality + ")", Resources.Resources.General035_Quality));
                }

                string tag = "tier";
                if (affixFlag.Description.Tier >= 0 && affixFlag.Description.Tier <= 3) tag += affixFlag.Description.Tier;
                TierTag = tag;
            }
            else if (modFilter.Mod.Unscalable)
            {
                tierList.Add(new(Resources.Resources.General080_UnscalableValue));
            }
            else
            {
                tierList.Add(new(Resources.Resources.General081_NoRangeValue));
            }

            if (!string.IsNullOrEmpty(affixFlag.Description.Name))
            {
                tierList.Add(new(affixFlag.Description.Name));
                if (!string.IsNullOrEmpty(affixFlag.Description.Level))
                {
                    tierList.Add(new("≥ " + affixFlag.Description.Level));
                }
            }

            if (tierList.Count > 0) TierList = tierList;
        }
        
        if (ItemFilter.Option.IsNotEmpty())
        {
            ItemFilter.Min = ItemFilter.Option;
        }
        else
        {
            var augment = affixFlag.Description is not null ? affixFlag.Description.AugmentPerCent : -1;
            ModBis = GetModRange(modFilter, item.Lang, ItemFilter.Min, augment);
        }
        
        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        var disable = modFilter.Entrie.ID.Contain(Strings.Stat.ImmunityIgnite2);
        var mods = modFilter.Entrie.ID.Contain(Strings.Stat.Generic.PassiveSkill)
            || modFilter.Entrie.ID.Contain(Strings.Stat.Generic.GrantNothing)
            || modFilter.Entrie.ID.Contain(Strings.Stat.Generic.UseRemaining)
            || modFilter.Entrie.ID.Contain(Strings.Stat.ActionSpeed)
            || modFilter.Entrie.ID.Contain(Strings.Stat.TimelessJewel);

        Min = disable || ItemFilter.Min.IsEmpty() ? string.Empty
            : modFilter.Mod.TierMin.IsNotEmpty() && _dm.Config.Options.AutoSelectMinTierValue
            && !item.Flag.Unique ? modFilter.Mod.TierMin.ToString(specifier, CultureInfo.InvariantCulture)
            : ItemFilter.Min.ToString(specifier, CultureInfo.InvariantCulture);

        Max = mods ? Min
            : ItemFilter.Max.IsEmpty() ? string.Empty
            : ItemFilter.Max.ToString(specifier, CultureInfo.InvariantCulture);

        CurrentVal = Current.ToDoubleEmptyField();

        if (IsStringOfServitude(item))
        {
            var tripledVal = CurrentVal * 3;
            Current = Min = tripledVal.ToString();
            CurrentVal = tripledVal;
        }

        ModKind = GetModKind(item, affixFlag.Description);
    }

    private static string GetModRange(ModFilter modFilter, Lang lang, double min, int augment)
    {
        StringBuilder sbMod = new(modFilter.Entrie.Text);
        if (lang is not Lang.English)
        {
            string enStr = Resources.Resources.ResourceManager
                .GetString("General096_AddsTo", CultureInfo.InvariantCulture);
            sbMod.Replace(enStr, "#"); // if mod wasnt translated
        }

        if (lang is Lang.Korean)
        {
            sbMod.Replace("#~#", "#");
            var match = RegexUtil.DiezeSpacePattern().Matches(sbMod.ToString());
            if (match.Count is 2)
            {
                int idx = sbMod.ToString().IdxOf("# ");
                sbMod.Remove(idx, 2);
            }
        }
        else
        {
            sbMod.Replace(Resources.Resources.General096_AddsTo, "#");
        }

        if (modFilter.Mod.TierMin.IsNotEmpty() && modFilter.Mod.TierMax.IsNotEmpty())
        {
            sbMod.Replace("#", GetRange(modFilter, augment));
        }
        else if (min.IsNotEmpty())
        {
            sbMod.Replace("#", min.ToString());
        }

        return sbMod.ToString();
    }

    private static string GetRange(ModFilter modFilter, int augment)
    {
        var min = augment > 0 ? Math.Truncate(modFilter.Mod.TierMin + (modFilter.Mod.TierMin / 100) * augment) : modFilter.Mod.TierMin;
        var max = augment > 0 ? Math.Truncate(modFilter.Mod.TierMax + (modFilter.Mod.TierMax / 100) * augment) : modFilter.Mod.TierMax;
        return "(" + min + "-" + max + ")";
    }

    private string GetModKind(ItemData item, ModDescription modDesc)
    {
        if (modDesc is null)
        {
            return string.Empty;
        }

        var kind = string.Empty;
        if (modDesc.AugmentPerCent > 0)
        {
            kind = Strings.ModKind.AugmentedMod;
        }
        var idStat = ItemFilter.Id.Split('.');
        if (item.Flag.Map && idStat.Length is 2 &&
            _dm.Config.DangerousMapMods.FirstOrDefault(x => x.Id.IdxOf(idStat[1]) > -1) is not null)
        {
            kind = Strings.ModKind.DangerousMod;
        }
        if (!item.Flag.Map && idStat.Length is 2 &&
            _dm.Config.RareItemMods.FirstOrDefault(x => x.Id.IdxOf(idStat[1]) > -1) is not null)
        {
            kind = Strings.ModKind.RareMod;
        }
        return kind;
    }

    private static string GetReducesOptionText(string text)
        => Strings.dicOptionText.TryGetValue(text, out string value) ? value : text;

    private bool IsStringOfServitude(ItemData item) => item.Flag.Unique && item.Flag.Belts 
        && CurrentVal is not ModFilter.EMPTYFIELD
        && _dm.Words.FindWordByNameEn(Strings.Unique.StringOfServitude)?.Name == item.Name;

    private int GetAffixIndex(AffixFlag affix, ItemFlag item)
    {
        var affixIndex = -1;
        if (AffixList.Count <= 0)
        {
            return affixIndex;
        }

        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        if (!isPoe2)
        {
            var idSplit = AffixList[0]?.ID.Split('.');
            if (idSplit?.Length is 2 && Strings.Stat.dicPseudo.TryGetValue(idSplit[1], out string value)) // Gestion des pseudo
            {
                AffixList.Add(new("pseudo." + value, Resources.Resources.General014_Pseudo));
            }
        }

        void TrySelect(ReadOnlySpan<char> resource, bool condition = true)
        {
            if (!condition || affixIndex is not -1)
                return;

            for (int i = 0; i < AffixList.Count; i++)
            {
                if (AffixList[i].Name.AsSpan().SequenceEqual(resource))
                    affixIndex = i; // last match saved
            }
        }

        // ordered
        TrySelect(Resources.Resources.General014_Pseudo,
            _dm.Config.Options.AutoSelectPseudo && !isPoe2);
        TrySelect(Resources.Resources.General011_Enchant, affix.Enchant);
        TrySelect(Resources.Resources.General016_Fractured, affix.Fractured);
        TrySelect(Resources.Resources.General012_Crafted, affix.Crafted);
        TrySelect(Resources.Resources.General099_Scourge, affix.Scourged);
        TrySelect(Resources.Resources.General018_Monster, item.CapturedBeast);
        TrySelect(Resources.Resources.General111_Sanctum, item.SanctumRelic);
        TrySelect(Resources.Resources.General013_Implicit, affix.Implicit);
        TrySelect(Resources.Resources.General145_Augment, affix.Rune);
        TrySelect(Resources.Resources.General015_Explicit);
        TrySelect(Resources.Resources.General158_Desecrated, affix.Desecrated);
        TrySelect(Resources.Resources.General016_Fractured);

        if (affixIndex is -1 && AffixList.Count is 1)
        {
            affixIndex = 0;
        }
        return affixIndex;
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

    internal static List<ModLine> MergeSameMods(List<ModLine> listMod)
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
}

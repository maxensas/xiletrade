using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed record ModLine
{
    private readonly DataManagerService _dm;

    internal int Level { get; }
    internal int AffixIndex { get; }
    internal int OptionIndex { get; } = -1;

    internal string Mod { get; }
    internal string ModBis { get; }
    internal string ModKind { get; }
    internal string Current { get; }
    internal double CurrentVal { get; }
    internal string Min { get; }
    internal string Max { get; }

    internal string Tier { get; }
    internal double TierMin { get; } = ModFilter.EMPTYFIELD;
    internal double TierMax { get; } = ModFilter.EMPTYFIELD;
    internal string TierAffixKind { get; }
    internal string TierTag { get; } = "null";

    internal bool Corruption { get; }

    internal bool ExplicitCrafted { get; }
    internal bool ExplicitFractured { get; }
    internal bool ExplicitDesecrated { get; }
    internal bool ExplicitMutated { get; }

    internal List<AffixFilterEntrie> AffixList { get; }
    internal List<ToolTipItem> TagList { get; }
    internal List<ToolTipItem> TierList { get; }
    internal List<string> OptionList { get; }
    internal List<int> OptionIdList { get; }

    internal ItemFilter ItemFilter { get; }

    private ModLine(ModLine modLine)
    {
        Level = modLine.Level;
        AffixIndex = modLine.AffixIndex;
        OptionIndex = modLine.OptionIndex;

        Mod = modLine.Mod;
        ModKind = modLine.ModKind;
        Max = modLine.Max;

        TierAffixKind = modLine.TierAffixKind;
        TierTag = modLine.TierTag;

        AffixList = modLine.AffixList;
        TagList = modLine.TagList;
        OptionList = modLine.OptionList;
        OptionIdList = modLine.OptionIdList;

        ItemFilter = modLine.ItemFilter;
    }

    internal ModLine(ModLine modLine, string tier) : this(modLine)
    {
        ModBis = modLine.ModBis;
        Current = modLine.Current;
        CurrentVal = modLine.CurrentVal;
        Min = modLine.Min;

        Tier = tier;
        TierMin = modLine.TierMin;
        TierMax = modLine.TierMax;

        TierList = modLine.TierList;
    }

    internal ModLine(ModLine modLine, string tier, double curVal, string cur, string min, string modBis) : this(modLine)
    {
        ModBis = modBis;
        Current = cur;
        CurrentVal = curVal;
        Min = min;

        Tier = tier;
        TierMin = modLine.TierMin;
        TierMax = modLine.TierMax;

        TierList = modLine.TierList;
    }

    internal ModLine(ModLine modLine, string tier, double curVal, string cur, string min, string modBis,
        List<ToolTipItem> tierList, double tierMin, double tierMax) : this(modLine)
    {
        ModBis = modBis;
        Current = cur;
        CurrentVal = curVal;
        Min = min;

        Tier = tier;
        TierMin = tierMin;
        TierMax = tierMax;

        TierList = tierList;
    }

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
                var augment = ItemFilter.Min + (ItemFilter.Min * affixFlag.Description.AugmentPerCent / 100);
                var truncate = ItemFilter.Min > 10 || affixFlag.Description.Tags == Resources.Resources.General029_Gem;
                ItemFilter.Min = truncate ? Math.Truncate(augment) 
                    : Math.Round(augment, ItemFilter.Min.CountDecimals(), MidpointRounding.ToZero);
            }
            else if (ItemFilter.Max.IsNotEmpty())
            {
                var augment = ItemFilter.Max + (ItemFilter.Max * affixFlag.Description.AugmentPerCent / 100);
                ItemFilter.Max = ItemFilter.Max > 10 ? Math.Truncate(augment)
                    : Math.Round(augment, ItemFilter.Max.CountDecimals(), MidpointRounding.ToZero);
            }
        }
        
        Current = ItemFilter.Min.IsEmpty() ? string.Empty : ItemFilter.Min.ToStr();
        if (Current.Length is 0)
        {
            Current = ItemFilter.Max.IsEmpty() ? string.Empty : ItemFilter.Max.ToStr();
        }

        var modDesc = affixFlag.Description;
        ExplicitCrafted = modDesc is not null && modDesc.IsCraft;
        ExplicitFractured = modDesc is not null && modDesc.IsFractured;
        ExplicitDesecrated = modDesc is not null && modDesc.IsDesecrated;
        ExplicitMutated = modDesc is not null && modDesc.IsMutated;
        Corruption = modDesc is not null && modDesc.IsCorruption;

        TierAffixKind = modDesc?.TierKind;
        if (!string.IsNullOrEmpty(TierAffixKind))
        {
            Tier = TierAffixKind + (modDesc.Tier > -1 ? modDesc.Tier : string.Empty);
            List<ToolTipItem> tierList = new();
            if (modFilter.Mod.TierMin.IsNotEmpty() && modFilter.Mod.TierMax.IsNotEmpty())
            {
                TierMin = modFilter.Mod.TierMin;
                TierMax = modFilter.Mod.TierMax;
                string tValmin = modFilter.Mod.TierMin.ToStr();
                string tValmax = modFilter.Mod.TierMax.ToStr();
                string tip = tValmin == tValmax ? tValmin : tValmin + "-" + tValmax;
                tierList.Add(new(tip));
                if (!string.IsNullOrEmpty(modDesc.Quality))
                {
                    tierList.Add(new("(" + modDesc.Quality + ")", Resources.Resources.General035_Quality));
                }

                string tag = "tier";
                if (modDesc.Tier >= 0 && modDesc.Tier <= 3) tag += modDesc.Tier;
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

            if (!string.IsNullOrEmpty(modDesc.Name))
            {
                tierList.Add(new(modDesc.Name));
                if (!string.IsNullOrEmpty(modDesc.Level))
                {
                    tierList.Add(new("≥ " + modDesc.Level));
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
            var augment = modDesc is not null ? modDesc.AugmentPerCent : -1;
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
            && !item.Flag.Unique && !item.Flag.Mirrored && !item.Flag.Corrupted
            ? modFilter.Mod.TierMin.ToStr() : ItemFilter.Min.ToStr();

        Max = mods ? Min : ItemFilter.Max.IsEmpty() ? string.Empty : ItemFilter.Max.ToStr();

        CurrentVal = Current.ToDoubleEmptyField();

        if (IsStringOfServitude(item))
        {
            var tripledVal = CurrentVal * 3;
            Current = Min = tripledVal.ToString();
            CurrentVal = tripledVal;
        }

        ModKind = GetModKind(item, modDesc);
    }

    private static string GetModRange(ModFilter modFilter, Lang lang, double min, int augment)
    {
        StringBuilder sbMod = new(modFilter.Entrie.Text);
        if (lang is not Lang.English)
        {
            var enStr = Resources.Resources.ResourceManager
                .GetEnglish(nameof(Resources.Resources.General096_AddsTo));
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
        if (!isPoe2)
        {
            TrySelect(Resources.Resources.General016_Fractured, affix.Fractured);
            TrySelect(Resources.Resources.General012_Crafted, affix.Crafted);
        }
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
}

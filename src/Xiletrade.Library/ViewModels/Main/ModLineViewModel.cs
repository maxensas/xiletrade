﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using System;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using System.Linq;
using Xiletrade.Library.Models.Parser;
using System.Text;
using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class ModLineViewModel : ViewModelBase
{
    private readonly DataManagerService _dm;

    [ObservableProperty]
    private AsyncObservableCollection<AffixFilterEntrie> affix = new();

    [ObservableProperty]
    private int affixIndex;

    [ObservableProperty]
    private bool affixEnable = true;

    [ObservableProperty]
    private bool affixCanBeEnabled = true;

    [ObservableProperty]
    private AsyncObservableCollection<ToolTipItem> tagTip = new();

    [ObservableProperty]
    private bool tagVisible;

    [ObservableProperty]
    private string mod;

    [ObservableProperty]
    private string modTooltip;

    [ObservableProperty]
    private bool modVisible;

    [ObservableProperty]
    private string modBis;

    [ObservableProperty]
    private string modBisTooltip;

    [ObservableProperty]
    private bool modBisVisible;

    [ObservableProperty]
    private string modKind;

    [ObservableProperty]
    private string current;

    [ObservableProperty]
    private double currentSlide;

    [ObservableProperty]
    private string tier;

    [ObservableProperty]
    private string tierKind;

    [ObservableProperty]
    private string tierTag = "null";

    [ObservableProperty]
    private AsyncObservableCollection<ToolTipItem> tierTip = new();

    [ObservableProperty]
    private string min;

    [ObservableProperty]
    private string max;

    [ObservableProperty]
    private double slideValue;

    [ObservableProperty]
    private bool isSlideReversed;

    [ObservableProperty]
    private int optionIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> option = new();

    [ObservableProperty]
    private AsyncObservableCollection<int> optionID = new();

    [ObservableProperty]
    private bool optionVisible;

    [ObservableProperty]
    private ItemFilter itemFilter;

    [ObservableProperty]
    private bool selected;

    [ObservableProperty]
    private bool preferMinMax;

    internal ModLineViewModel(DataManagerService dm, ItemData item, ModFilter modFilter, AffixFlag affix, ModDescription modDesc, bool showMinMax)
    {
        _dm = dm;
        Affix = modFilter.ModValue.ListAffix;
        ItemFilter = new()
        {
            Id = modFilter.Entrie.ID, // filter.Type
            Text = modFilter.Entrie.Text,
            Option = ModFilter.EMPTYFIELD,
            Max = modFilter.ModValue.Max,
            Min = modFilter.ModValue.Min,
            Disabled = true
        };

        if (modFilter.Entrie.Option.Options is not null) // retrieve options and select option found
        {
            int selId = -1;
            var listOpt = modFilter.Entrie.Option.Options.OrderBy(x => x.Text);
            foreach (var opt in listOpt)
            {
                string optionText = ReduceOptionText(opt.Text);
                Option.Add(optionText); // fire selection changed
                _ = int.TryParse(opt.ID.ToString(), out int idInt);
                OptionID.Add(idInt); // fire selection changed

                string[] textLine = opt.Text.Split(Strings.LF);
                for (int l = 0; l < textLine.Length; l++)
                {
                    if (modFilter.Mod.Parsed.ToLowerInvariant().Contain(textLine[l].ToLowerInvariant()))
                    {
                        ItemFilter.Option = idInt;
                        selId = Option.Count - 1;
                        break;
                    }
                }
            }
            if (selId > -1)
            {
                OptionVisible = true;
                OptionIndex = selId;
                ItemFilter.Min = ItemFilter.Max = ModFilter.EMPTYFIELD;
            }
            else if (item.Flag.Chronicle)
            {
                OptionVisible = true;
                OptionIndex = 1;
                ItemFilter.Min = ItemFilter.Max = ModFilter.EMPTYFIELD;
            }
        }

        SelectAffix(affix, item.Flag);

        Mod = modFilter.Entrie.Text.Replace(Strings.LF, " ");
        ModTooltip = modFilter.Entrie.Text;

        if (modDesc.Tags.Length > 0)
        {
            var tags = modDesc.Tags.Split(',', StringSplitOptions.TrimEntries);
            foreach (string t in tags)
            {
                TagTip.Add(new(t));
            }
            TagVisible = true;
        }

        if (item.Flag.Unique)
        {
            AffixEnable = false;
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
        else if (ItemFilter.Min.IsNotEmpty() || ItemFilter.Max.IsNotEmpty()) // TO UPDATE
        {
            var split = modFilter.Entrie.ID.Split('.');
            bool defMaxPosition = split.Length is 2 && Strings.Stat.dicDefaultPosition.ContainsKey(split[1]);
            var condNegativeTemp = _dm.Config.Options.GameVersion is 1 
                && ItemFilter.Min < 0 && ItemFilter.Max.IsEmpty() && !modFilter.Mod.Negative;
            if ((defMaxPosition && ItemFilter.Min > 0 && ItemFilter.Max.IsEmpty()) || condNegativeTemp) 
            {
                ItemFilter.Max = ItemFilter.Min;
                ItemFilter.Min = ModFilter.EMPTYFIELD;
            }
        }

        if (modDesc.Quality.Length > 0)
        {
            var match = RegexUtil.DecimalNoPlusPattern().Matches(modDesc.Quality);
            if (match.Count > 0)
            {
                _ = int.TryParse(match[0].Value, out int quality);
                if (quality > 0)
                {
                    if (ItemFilter.Min.IsNotEmpty())
                    {
                        ItemFilter.Min += ItemFilter.Min * quality / 100;
                        //min = Math.Round(min, 0);
                        if (ItemFilter.Min > 10 ||
                            modDesc.Tags == Resources.Resources.General029_Gem)
                        {
                            ItemFilter.Min = Math.Truncate(ItemFilter.Min);
                        }
                    }
                    else if (ItemFilter.Max.IsNotEmpty())
                    {
                        ItemFilter.Max += ItemFilter.Max * quality / 100;
                        //max = Math.Round(max, 0);
                        if (ItemFilter.Max > 10)
                        {
                            ItemFilter.Max = Math.Truncate(ItemFilter.Max);
                        }
                    }
                }
            }
        }
        string specifier = "G";
        Current = ItemFilter.Min.IsEmpty() ? string.Empty : ItemFilter.Min.ToString(specifier, CultureInfo.InvariantCulture);
        if (Current.Length is 0)
        {
            Current = ItemFilter.Max.IsEmpty() ? string.Empty : ItemFilter.Max.ToString(specifier, CultureInfo.InvariantCulture);
        }

        bool isImp = modDesc.Kind == Resources.Resources.General073_ModifierImplicit;
        bool isCor = modDesc.Kind == Resources.Resources.General074_ModifierCorrupt;
        bool isPre = modDesc.Kind == Resources.Resources.General075_ModifierPrefix;
        bool isPreCraft = modDesc.Kind == Resources.Resources.General076_ModifierPrefixCraft;
        bool isSuf = modDesc.Kind == Resources.Resources.General077_ModifierSuffix;
        bool isSufCraft = modDesc.Kind == Resources.Resources.General078_ModifierSuffixCraft;
        bool isUnique = modDesc.Kind == Resources.Resources.General079_ModifierUnique;

        string prefixLetter = (isPreCraft || isSufCraft) && modDesc.Tier > -1 ? Strings.TierKind.EnchantAndCraft
            : isImp || isCor ? Strings.TierKind.Implicit
            : isPre || isPreCraft ? Strings.TierKind.Prefix
            : isSuf || isSufCraft ? Strings.TierKind.Suffix
            : isUnique ? Strings.TierKind.Unique
            : string.Empty;
        TierKind = prefixLetter;

        if (prefixLetter.Length > 0)
        {
            Tier = prefixLetter + (modDesc.Tier > -1 ? modDesc.Tier : string.Empty);
            AsyncObservableCollection<ToolTipItem> dicTip = new();
            if (modFilter.Mod.TierMin.IsNotEmpty() && modFilter.Mod.TierMax.IsNotEmpty())
            {
                string tValmin = modFilter.Mod.TierMin.ToString(specifier, CultureInfo.InvariantCulture);
                string tValmax = modFilter.Mod.TierMax.ToString(specifier, CultureInfo.InvariantCulture);
                string tip = tValmin == tValmax ? tValmin : tValmin + "-" + tValmax;
                dicTip.Add(new(tip));
                if (modDesc.Quality.Length > 0)
                {
                    dicTip.Add(new("(" + modDesc.Quality + ")", "Suffix"));
                }

                string tag = "tier";
                if (modDesc.Tier >= 0 && modDesc.Tier <= 3) tag += modDesc.Tier;
                TierTag = tag;
            }
            else if (modFilter.Mod.Unscalable)
            {
                dicTip.Add(new(Resources.Resources.General080_UnscalableValue));
            }
            else
            {
                dicTip.Add(new(Resources.Resources.General081_NoRangeValue));
            }

            if (modDesc.Name.Length > 0)
            {
                dicTip.Add(new(modDesc.Name));
            }

            if (dicTip.Count > 0) TierTip = dicTip;
        }

        if (ItemFilter.Option.IsNotEmpty())
        {
            ItemFilter.Min = ItemFilter.Option;
            ModVisible = true;
        }
        else
        {
            string modWithRange = ComposeModRange(modFilter, item.Lang, ItemFilter.Min);
            ModBis = modWithRange.Replace(Strings.LF, " ");
            ModBisTooltip = modWithRange;
            ModBisVisible = true;
        }

        if (modFilter.Mod.TierMin < 0 && modFilter.Mod.TierMax < 0) // temp fix for reduce to increase mods
        {
            if (ItemFilter.Min > 0)
            {
                ItemFilter.Min = -ItemFilter.Min;
            }
            if (ItemFilter.Max.IsNotEmpty() && ItemFilter.Max > 0)
            {
                ItemFilter.Max = -ItemFilter.Max;
            }
        }
        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        var disable = modFilter.Entrie.ID.Contain(Strings.Stat.ImmunityIgnite2);
        var mods = modFilter.Entrie.ID.Contain(Strings.Stat.Generic.PassiveSkill)
            || modFilter.Entrie.ID.Contain(Strings.Stat.Generic.GrantNothing)
            || modFilter.Entrie.ID.Contain(Strings.Stat.Generic.UseRemaining)
            || modFilter.Entrie.ID.Contain(Strings.Stat.ActionSpeed)
            || modFilter.Entrie.ID.Contain(Strings.Stat.TimelessJewel);

        Min = disable || ItemFilter.Min.IsEmpty() ? string.Empty
            : modFilter.Mod.TierMin.IsNotEmpty() && _dm.Config.Options.AutoSelectMinTierValue && !isPoe2
            && !item.Flag.Unique ? modFilter.Mod.TierMin.ToString(specifier, CultureInfo.InvariantCulture)
            : ItemFilter.Min.ToString(specifier, CultureInfo.InvariantCulture);

        Max = mods ? Min
            : ItemFilter.Max.IsEmpty() ? string.Empty
            : ItemFilter.Max.ToString(specifier, CultureInfo.InvariantCulture);

        PreferMinMax = Min.Length is 0 || showMinMax;
        SlideValue = Min.ToDoubleEmptyField();
        CurrentSlide = Current.ToDoubleEmptyField();
    }

    private void SelectAffix(AffixFlag affix, ItemFlag item)
    {
        if (Affix.Count <= 0)
        {
            return;
        }

        AffixIndex = -1;
        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        if (!isPoe2)
        {
            var idSplit = Affix[0]?.ID.Split('.');
            if (idSplit?.Length is 2 && Strings.Stat.dicPseudo.TryGetValue(idSplit[1], out string value)) // Gestion des pseudo
            {
                Affix.Add(new("pseudo." + value, Resources.Resources.General014_Pseudo, false, false));
            }
        }

        void SelectAffixIndex(string affixKind)
        {
            for (int a = 0; a < Affix.Count; a++)
            {
                if (Affix[a].Name == affixKind)
                {
                    AffixIndex = a;
                }
            }
        }

        if (_dm.Config.Options.AutoSelectPseudo && !isPoe2)
        {
            SelectAffixIndex(Resources.Resources.General014_Pseudo);
        }
        if (AffixIndex is -1 && affix.Enchant)
        {
            SelectAffixIndex(Resources.Resources.General011_Enchant);
        }
        if (AffixIndex is -1 && affix.Fractured)
        {
            SelectAffixIndex(Resources.Resources.General016_Fractured);
        }
        if (AffixIndex is -1 && affix.Crafted)
        {
            SelectAffixIndex(Resources.Resources.General012_Crafted);
        }
        if (AffixIndex is -1 && affix.Scourged)
        {
            SelectAffixIndex(Resources.Resources.General099_Scourge);
        }
        if (AffixIndex is -1 && item.CapturedBeast)
        {
            SelectAffixIndex(Resources.Resources.General018_Monster);
        }
        if (AffixIndex is -1 && item.SanctumRelic)
        {
            SelectAffixIndex(Resources.Resources.General111_Sanctum);
        }

        if (AffixIndex is -1 && affix.Implicit)
        {
            SelectAffixIndex(Resources.Resources.General013_Implicit);
            if (AffixIndex is -1)
            {
                SelectAffixIndex(Resources.Resources.General017_CorruptImp);
            }
        }

        if (AffixIndex is -1 && affix.Rune)
        {
            SelectAffixIndex(Resources.Resources.General145_Rune); // change to General132_Rune when translated by GGG.
        }

        if (AffixIndex is -1)
        {
            SelectAffixIndex(Resources.Resources.General015_Explicit);
        }

        if (AffixIndex is -1)
        {
            SelectAffixIndex(Resources.Resources.General016_Fractured);
        }

        if (AffixIndex is -1 && Affix.Count is 1)
        {
            AffixIndex = 0;
        }
    }

    private static string ComposeModRange(ModFilter modFilter, Lang lang, double min)
    {
        StringBuilder sbMod = new(modFilter.Entrie.Text);
        if (lang is not Lang.English)
        {
            var cultureEn = new CultureInfo(Strings.Culture[0]);
            var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));
            string enStr = rm.GetString("General096_AddsTo", cultureEn);
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
            string range = "(" + modFilter.Mod.TierMin + "-" + modFilter.Mod.TierMax + ")";
            sbMod.Replace("#", range);
        }
        else if (min.IsNotEmpty())
        {
            sbMod.Replace("#", min.ToString());
        }

        return sbMod.ToString();
    }

    private static string ReduceOptionText(string text)
    {
        return Strings.dicOptionText.TryGetValue(text, out string value) ? value : text;
    }
}

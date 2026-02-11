using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class ModLineViewModel : ViewModelBase
{
    private readonly DataManagerService _dm;

    [ObservableProperty]
    private AsyncObservableCollection<AffixFilterEntrie> affix = new();

    [ObservableProperty]
    private int affixIndex;

    [ObservableProperty]
    private int level;

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
    private double tierMin = ModFilter.EMPTYFIELD;

    [ObservableProperty]
    private double tierMax = ModFilter.EMPTYFIELD;

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

    [RelayCommand]
    private void ToggleChecked(object commandParameter)
    {
        Selected = !Selected;
    }

    internal ModLineViewModel(DataManagerService dm, ItemData item, ModFilter modFilter, AffixFlag affixFlag, ModDescription modDesc, bool showMinMax)
    {
        _dm = dm;
        affix = modFilter.ModValue.ListAffix;
        if (int.TryParse(modDesc.Level, out int lvl))
        {
            level = lvl;
        }
        itemFilter = new()
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
            foreach (var opt in listOpt)
            {
                string optionText = ReduceOptionText(opt.Text);
                option.Add(optionText); // fire selection changed
                optionID.Add(opt.ID.Id); // fire selection changed

                string[] textLine = opt.Text.Split(Strings.LF);
                for (int l = 0; l < textLine.Length; l++)
                {
                    if (modFilter.Mod.Parsed.ToLowerInvariant().Contain(textLine[l].ToLowerInvariant()))
                    {
                        itemFilter.Option = opt.ID.Id;
                        selId = option.Count - 1;
                        break;
                    }
                }
            }
            if (selId > -1)
            {
                optionVisible = true;
                optionIndex = selId;
                itemFilter.Min = itemFilter.Max = ModFilter.EMPTYFIELD;
            }
            else if (item.Flag.Chronicle)
            {
                optionVisible = true;
                optionIndex = 1;
                itemFilter.Min = itemFilter.Max = ModFilter.EMPTYFIELD;
            }
        }

        SelectAffix(affixFlag, item.Flag);

        mod = modFilter.Entrie.Text.Replace(Strings.LF, " ");
        modTooltip = modFilter.Entrie.Text;

        if (modDesc.Tags.Length > 0)
        {
            var tags = modDesc.Tags.Split(',', StringSplitOptions.TrimEntries);
            foreach (string t in tags)
            {
                tagTip.Add(new(t));
            }
            tagVisible = true;
        }

        if (item.Flag.Unique)
        {
            affixEnable = false;
        }

        if (itemFilter.Min.IsNotEmpty() && itemFilter.Max.IsNotEmpty())
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
                itemFilter.Min += itemFilter.Max;
                itemFilter.Min = Math.Truncate(itemFilter.Min / 2 * 10 / 10);
                itemFilter.Max = ModFilter.EMPTYFIELD;
            }
        }
        else if (itemFilter.Min.IsNotEmpty() || itemFilter.Max.IsNotEmpty())
        {
            var split = modFilter.Entrie.ID.Split('.');
            bool defMaxPosition = split.Length is 2 && Strings.Stat.dicDefaultPosition.ContainsKey(split[1]);
            var negativeMin = itemFilter.Min < 0 && itemFilter.Max.IsEmpty();
            if (negativeMin || (defMaxPosition && itemFilter.Min > 0 && itemFilter.Max.IsEmpty())) 
            {
                itemFilter.Max = itemFilter.Min;
                itemFilter.Min = ModFilter.EMPTYFIELD;
                preferMinMax = true;
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
                    if (itemFilter.Min.IsNotEmpty())
                    {
                        itemFilter.Min += itemFilter.Min * quality / 100;
                        //min = Math.Round(min, 0);
                        if (itemFilter.Min > 10 ||
                            modDesc.Tags == Resources.Resources.General029_Gem)
                        {
                            itemFilter.Min = Math.Truncate(ItemFilter.Min);
                        }
                    }
                    else if (ItemFilter.Max.IsNotEmpty())
                    {
                        itemFilter.Max += itemFilter.Max * quality / 100;
                        //max = Math.Round(max, 0);
                        if (itemFilter.Max > 10)
                        {
                            itemFilter.Max = Math.Truncate(itemFilter.Max);
                        }
                    }
                }
            }
        }
        string specifier = "G";
        current = itemFilter.Min.IsEmpty() ? string.Empty : itemFilter.Min.ToString(specifier, CultureInfo.InvariantCulture);
        if (current.Length is 0)
        {
            current = itemFilter.Max.IsEmpty() ? string.Empty : itemFilter.Max.ToString(specifier, CultureInfo.InvariantCulture);
        }

        tierKind = GetPrefixLetter(modDesc.Kind, modDesc.Tier);
        if (tierKind.Length > 0)
        {
            tier = tierKind + (modDesc.Tier > -1 ? modDesc.Tier : string.Empty);
            AsyncObservableCollection<ToolTipItem> dicTip = new();
            if (modFilter.Mod.TierMin.IsNotEmpty() && modFilter.Mod.TierMax.IsNotEmpty())
            {
                tierMin = modFilter.Mod.TierMin;
                tierMax = modFilter.Mod.TierMax;
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
                tierTag = tag;
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
                if (modDesc.Level?.Length > 0)
                {
                    dicTip.Add(new("≥ " + modDesc.Level));
                }
            }

            if (dicTip.Count > 0) tierTip = dicTip;
        }

        if (itemFilter.Option.IsNotEmpty())
        {
            itemFilter.Min = itemFilter.Option;
            modVisible = true;
        }
        else
        {
            modBisTooltip = GetModRange(modFilter, item.Lang, ItemFilter.Min);
            modBis = modBisTooltip.Replace(Strings.LF, " ");
            modBisVisible = true;
        }

        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        var disable = modFilter.Entrie.ID.Contain(Strings.Stat.ImmunityIgnite2);
        var mods = modFilter.Entrie.ID.Contain(Strings.Stat.Generic.PassiveSkill)
            || modFilter.Entrie.ID.Contain(Strings.Stat.Generic.GrantNothing)
            || modFilter.Entrie.ID.Contain(Strings.Stat.Generic.UseRemaining)
            || modFilter.Entrie.ID.Contain(Strings.Stat.ActionSpeed)
            || modFilter.Entrie.ID.Contain(Strings.Stat.TimelessJewel);

        min = disable || itemFilter.Min.IsEmpty() ? string.Empty
            : modFilter.Mod.TierMin.IsNotEmpty() && _dm.Config.Options.AutoSelectMinTierValue && !isPoe2
            && !item.Flag.Unique ? modFilter.Mod.TierMin.ToString(specifier, CultureInfo.InvariantCulture)
            : itemFilter.Min.ToString(specifier, CultureInfo.InvariantCulture);

        max = mods ? min
            : itemFilter.Max.IsEmpty() ? string.Empty
            : itemFilter.Max.ToString(specifier, CultureInfo.InvariantCulture);

        preferMinMax = min.Length is 0 || showMinMax;
        slideValue = min.ToDoubleEmptyField();
        currentSlide = current.ToDoubleEmptyField();

        UpdateSosValue(item);
    }

    private static string GetPrefixLetter(ReadOnlySpan<char> kind, int tier)
    {
        bool isImp = kind.StartWith(Resources.Resources.General073_ModifierImplicit);
        bool isCor = kind.StartWith(Resources.Resources.General074_ModifierCorrupt);
        bool isEater = kind.StartWith(Resources.Resources.General170_ModifierEaterImplicit);
        bool isExarch = kind.StartWith(Resources.Resources.General171_ModifierExarchImplicit);
        bool isPre = kind.StartWith(Resources.Resources.General075_ModifierPrefix);
        bool isPreCraft = kind.StartWith(Resources.Resources.General076_ModifierPrefixCraft);
        bool isPreDesec = kind.StartWith(Resources.Resources.General169_ModifierDesecratedPrefix);
        bool isSuf = kind.StartWith(Resources.Resources.General077_ModifierSuffix);
        bool isSufCraft = kind.StartWith(Resources.Resources.General078_ModifierSuffixCraft);
        bool isSufDesec = kind.StartWith(Resources.Resources.General168_ModifierDesecratedSuffix);
        bool isUnique = kind.StartWith(Resources.Resources.General079_ModifierUnique);

        return (isPreCraft || isSufCraft) && tier > -1 ? Strings.TierKind.EnchantAndCraft
            : isImp || isCor || isEater || isExarch ? Strings.TierKind.Implicit
            : isPre || isPreCraft || isPreDesec ? Strings.TierKind.Prefix
            : isSuf || isSufCraft || isSufDesec ? Strings.TierKind.Suffix
            : isUnique ? Strings.TierKind.Unique
            : string.Empty;
    }

    private void UpdateSosValue(ItemData item) // StringOfServitude
    {
        if (item.Flag.Unique && item.Flag.Belts && CurrentSlide is not ModFilter.EMPTYFIELD
            && _dm.Words.FindWordByNameEn(Strings.Unique.StringOfServitude)?.Name == item.Name)
        {
            var tripledVal = CurrentSlide * 3;
            Current = Min = tripledVal.ToString();
            CurrentSlide = tripledVal;
        }
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
                Affix.Add(new("pseudo." + value, Resources.Resources.General014_Pseudo, null, false, false));
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
            SelectAffixIndex(Resources.Resources.General145_Augment);
        }

        if (AffixIndex is -1)
        {
            SelectAffixIndex(Resources.Resources.General015_Explicit);
        }

        if (AffixIndex is -1 && affix.Desecrated)
        {
            SelectAffixIndex(Resources.Resources.General158_Desecrated);
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

    private static string GetModRange(ModFilter modFilter, Lang lang, double min)
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

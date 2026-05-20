using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class ModLineViewModel : ViewModelBase
{
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
    private string tierTag;

    [ObservableProperty]
    private double tierMin;

    [ObservableProperty]
    private double tierMax;

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

    internal ModLineViewModel(DataManagerService dm, ModLine modLine, ItemFlag flag, Lang lang, bool isPoe2, bool showMinMax)
    {
        if (modLine.AffixList?.Count > 0)
        {
            var enableSwitch = true;
            foreach (var af in modLine.AffixList)
            {
                affix.Add(af);
                if (af.IsExplicitUnique && enableSwitch)
                {
                    enableSwitch = false;
                }
            }
            affixEnable = enableSwitch;
        }

        level = modLine.Level;
        itemFilter = modLine.ItemFilter;

        if (modLine.OptionList?.Count > 0)
        {
            foreach (var opt in modLine.OptionList)
            {
                option.Add(opt);
            }
        }
        if (modLine.OptionIdList?.Count > 0)
        {
            foreach (var id in modLine.OptionIdList)
            {
                optionID.Add(id);
            }
        }

        optionIndex = modLine.OptionIndex;
        optionVisible = optionIndex > -1;
        affixIndex = modLine.AffixIndex;
        mod = modLine.Mod.Replace(Strings.LF, " ");
        modTooltip = modLine.Mod;
        tagVisible = tagTip?.Count > 0;
        current = modLine.Current;
        tierKind = modLine.TierKind;
        tier = modLine.Tier;
        tierMin = modLine.TierMin;
        tierMax = modLine.TierMax;
        tierTag = modLine.TierTag;
        if (modLine.TierList?.Count > 0)
        {
            foreach (var tip in modLine.TierList)
            {
                tierTip.Add(tip);
            }
        }

        var showModBis = modLine.ModBis?.Length > 0;
        modBisVisible = showModBis;
        modVisible = !showModBis;
        if (showModBis)
        {
            modBis = modLine.ModBis.Replace(Strings.LF, " ");
            modBisTooltip = modLine.ModBis;
        }
        min = modLine.Min;
        max = modLine.Max;

        preferMinMax = min.Length is 0 || showMinMax;
        slideValue = min.ToDoubleEmptyField();
        currentSlide = modLine.CurrentVal;
        modKind = modLine.ModKind;
        selected = GetModSelection(dm, flag, itemFilter, affix, affixIndex, mod, level, lang, isPoe2);
        itemFilter.Disabled = !selected;
        if (selected)
        {
            if (flag.Unique)
            {
                affixCanBeEnabled = false;
                return;
            }
            affixEnable = true;
        }
    }

    // VERBOSE
    private static bool GetModSelection(DataManagerService dm, ItemFlag flag, ItemFilter filter, AsyncObservableCollection<AffixFilterEntrie> affix, int affixIndex, ReadOnlySpan<char> mod, int level, Lang lang, bool isPoe2)
    {
        var firstAffix = affix[0];
        if (firstAffix is null || affixIndex < 0
            || affixIndex > affix.Count) return false;

        bool selected = false;
        var opt = dm.Config.Options;
        var selAffix = affix[affixIndex];

        var englishMod = mod.ToString();
        if (lang is not Lang.English)
        {
            var enEntry = dm.FilterEn.GetFilterDataEntry(firstAffix.ID);
            if (enEntry is not null)
            {
                englishMod = enEntry.Text;
            }
        }

        bool condLife = opt.AutoSelectLife
            && !flag.Unique && Strings.StatTotal.IsTotalStat(englishMod, Stat.Life)
            && !englishMod.ToLowerInvariant().Contain(Strings.Words.ToStrength);
        bool condEs = opt.AutoSelectGlobalEs
            && !flag.Unique && Strings.StatTotal.IsTotalStat(englishMod, Stat.Es) && !flag.ArmourPiece;
        bool condRes = opt.AutoSelectRes
            && !flag.Unique && Strings.StatTotal.IsTotalStat(englishMod, Stat.Resist);
        bool condAttr = isPoe2 && opt.AutoSelectAttr
            && !flag.Unique && Strings.StatPoe2.IsAttribute(englishMod);

        if (selAffix.IsImplicitRegular || selAffix.IsImplicitCorruption || selAffix.IsImplicitEnch)
        {
            bool condImpAuto = opt.AutoCheckImplicits && selAffix.IsImplicitRegular || flag.Tablet;
            bool condCorruptAuto = opt.AutoCheckCorruptions && selAffix.IsImplicitCorruption;
            bool condEnchAuto = opt.AutoCheckEnchants && selAffix.IsImplicitEnch;

            bool specialImp = Strings.Stat.lSpecialImplicits.Contains(selAffix.ID)
                    || ((flag.Amulets || flag.Rings)
                    && Strings.Stat.lMagnitudeImplicits.Contains(selAffix.ID));

            if ((condImpAuto || condCorruptAuto || condEnchAuto)
                && !condLife && !condEs && !condRes && !condAttr
                || specialImp || IsInfluenced(filter.Id))
            {
                selected = true;
            }
        }

        if (opt.AutoCheckUniques && flag.Unique || opt.AutoCheckNonUniques && !flag.Unique)
        {
            bool isLogbookRare = IsLogbookRareMod(filter.Id);
            bool isCrafted = filter.Id.Contain(Strings.Stat.Generic.Crafted)
                || selAffix.IsExplicitCrafted && !opt.AutoCheckCrafted;
            if (isCrafted || flag.Logbook && !isLogbookRare)
            {
                selected = false;
            }
            else if (!flag.Invitation && !flag.Map && !flag.Waystones
                && !isCrafted && !condLife && !condEs && !condRes && !condAttr)
            {
                bool isChronicleRare = flag.Chronicle && IsChronicleRoom(firstAffix.ID);
                bool isTabletRare = flag.MirroredTablet && IsTabletRoom(firstAffix.ID);
                bool unselectPoe2Mod = isPoe2 && ShouldUnselectPoe2Mods(dm, flag, firstAffix.ID);

                if (!selAffix.IsImplicitRegular && !selAffix.IsImplicitCorruption
                    && !selAffix.IsImplicitEnch && !selAffix.IsImplicitScourge
                    && !selAffix.IsImplicitAugment && !unselectPoe2Mod
                    && (!flag.Chronicle && !flag.Ultimatum && !flag.MirroredTablet
                    || isChronicleRare || isTabletRare))
                {
                    selected = true;
                }
                // temp: Maligaro fix until GGG add filter for shock duration
                if (flag.Unique && flag.Belts && firstAffix.ID is Strings.Stat.StunOnYou)
                {
                    selected = false;
                }
            }
        }
        if (!flag.Unique && opt.AutoUnSelectBelowModLevel && level > 0 && level < opt.ModLevel)
        {
            selected = false;
        }

        return selected;
    }

    private static bool ShouldUnselectPoe2Mods(DataManagerService dm, ItemFlag flag, string id)
    {
        var opt = dm.Config.Options;
        var idSplit = id.Split('.');
        if (idSplit.Length < 2) return false;

        return (opt.AutoSelectArEsEva && flag.ArmourPiece && Strings.StatPoe2.lDefenceMods.Contains(idSplit[1]))
            || (opt.AutoSelectDps && flag.Weapon && Strings.StatPoe2.lWeaponMods.Contains(idSplit[1]));
    }

    private static bool IsInfluenced(ReadOnlySpan<char> filterId)
    {
        return filterId.SequenceEqual(Strings.Stat.Option.MapOccupConq)
            || filterId.SequenceEqual(Strings.Stat.Option.MapOccupElder)
            || filterId.SequenceEqual(Strings.Stat.Option.AreaInflu)
            || filterId.SequenceEqual(Strings.Stat.AreaInfluOrigin);
    }

    private static bool IsLogbookRareMod(ReadOnlySpan<char> id)
    {
        return id.Contain(Strings.Stat.Generic.LogbookBoss)
            || id.Contain(Strings.Stat.Generic.LogbookArea)
            || id.Contain(Strings.Stat.Generic.LogbookTwice);
    }

    private static bool IsChronicleRoom(ReadOnlySpan<char> id) =>
        id.Contain(Strings.Stat.Temple.Room01) // Apex of Atzoatl
        || id.Contain(Strings.Stat.Temple.Room11) // Doryani's Institute
        || id.Contain(Strings.Stat.Temple.Room15) // Apex of Ascension
        || id.Contain(Strings.Stat.Temple.Room17); // Locus of Corruption

    private static bool IsTabletRoom(ReadOnlySpan<char> id) =>
        id.Contain(Strings.Stat.Lake.Tablet01) // Paradise
        || id.Contain(Strings.Stat.Lake.Tablet02) // Kalandra
        || id.Contain(Strings.Stat.Lake.Tablet03) // the Sun
        || id.Contain(Strings.Stat.Lake.Tablet04); // Angling
}

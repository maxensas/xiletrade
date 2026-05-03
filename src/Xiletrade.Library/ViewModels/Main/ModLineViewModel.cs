using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main;

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
    private int optionIndex = -1;

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

    internal ModLineViewModel(ModLine modLine, bool showMinMax)
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
            AffixEnable = enableSwitch;
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
    }
}

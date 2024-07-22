using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed class ModLineViewModel : BaseViewModel
{
    private AsyncObservableCollection<AffixFilterEntrie> affix = new();
    private int affixIndex;
    private bool affixEnable = true;
    private bool affixCanBeEnabled = true;
    private AsyncObservableCollection<ToolTipItem> tagTip = new();
    private bool tagVisible;
    private string mod;
    private string modTooltip;
    private bool modVisible;
    private string modBis;
    private string modBisTooltip;
    private bool modBisVisible;
    private string modKind;
    private string current;
    private string tier;
    private string tierKind;
    private string tierTag = "null";
    private AsyncObservableCollection<ToolTipItem> tierTip = new();
    private string min;
    private string max;
    private int optionIndex;
    private AsyncObservableCollection<string> option = new();
    private AsyncObservableCollection<int> optionID = new();
    private bool optionVisible;
    private ItemFilter itemFilter;
    private bool selected;

    public AsyncObservableCollection<AffixFilterEntrie> Affix { get => affix; set => SetProperty(ref affix, value); }
    public int AffixIndex { get => affixIndex; set => SetProperty(ref affixIndex, value); }
    public bool AffixEnable { get => affixEnable; set => SetProperty(ref affixEnable, value); }
    public bool AffixCanBeEnabled { get => affixCanBeEnabled; set => SetProperty(ref affixCanBeEnabled, value); }
    public AsyncObservableCollection<ToolTipItem> TagTip { get => tagTip; set => SetProperty(ref tagTip, value); }
    public bool TagVisible { get => tagVisible; set => SetProperty(ref tagVisible, value); }
    public string Mod { get => mod; set => SetProperty(ref mod, value); }
    public string ModTooltip { get => modTooltip; set => SetProperty(ref modTooltip, value); }
    public bool ModVisible { get => modVisible; set => SetProperty(ref modVisible, value); }
    public string ModBis { get => modBis; set => SetProperty(ref modBis, value); }
    public string ModBisTooltip { get => modBisTooltip; set => SetProperty(ref modBisTooltip, value); }
    public bool ModBisVisible { get => modBisVisible; set => SetProperty(ref modBisVisible, value); }
    public string ModKind { get => modKind; set => SetProperty(ref modKind, value); }
    public string Current { get => current; set => SetProperty(ref current, value); }
    public string Tier { get => tier; set => SetProperty(ref tier, value); }
    public string TierKind { get => tierKind; set => SetProperty(ref tierKind, value); }
    public string TierTag { get => tierTag; set => SetProperty(ref tierTag, value); }
    public AsyncObservableCollection<ToolTipItem> TierTip { get => tierTip; set => SetProperty(ref tierTip, value); }
    public string Min { get => min; set => SetProperty(ref min, value); }
    public string Max { get => max; set => SetProperty(ref max, value); }
    public int OptionIndex { get => optionIndex; set => SetProperty(ref optionIndex, value); }
    public AsyncObservableCollection<string> Option { get => option; set => SetProperty(ref option, value); }
    public AsyncObservableCollection<int> OptionID { get => optionID; set => SetProperty(ref optionID, value); }
    public bool OptionVisible { get => optionVisible; set => SetProperty(ref optionVisible, value); }
    public ItemFilter ItemFilter { get => itemFilter; set => SetProperty(ref itemFilter, value); }
    public bool Selected { get => selected; set => SetProperty(ref selected, value); }
}

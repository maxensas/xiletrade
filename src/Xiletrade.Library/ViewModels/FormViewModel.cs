using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed partial class FormViewModel : ViewModelBase
{
    [ObservableProperty]
    private string itemName;

    [ObservableProperty]
    private string itemNameColor;

    [ObservableProperty]
    private string itemBaseType;

    [ObservableProperty]
    private double baseTypeFontSize = 12; // FontSize cannot be equal to 0

    [ObservableProperty]
    private string dps = string.Empty;

    [ObservableProperty]
    private string dpsTip = string.Empty;

    [ObservableProperty]
    private int corruptedIndex = -1;

    [ObservableProperty]
    private string rarityBox;

    [ObservableProperty]
    private bool byBase;

    [ObservableProperty]
    private bool allCheck;

    [ObservableProperty]
    private string detail;

    [ObservableProperty]
    private BottomFormViewModel panel = new();

    [ObservableProperty]
    private AsyncObservableCollection<ModLineViewModel> modLine = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> corruption = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> alternate = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> market = new();

    [ObservableProperty]
    private int marketIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> league = new();

    [ObservableProperty]
    private int leagueIndex;

    [ObservableProperty]
    private InfluenceItem influence = new();

    [ObservableProperty]
    private ConditionItem condition = new();

    [ObservableProperty]
    private TabViewModel tab = new();

    [ObservableProperty]
    private VisibilityViewModel visible = new();

    [ObservableProperty]
    private BulkViewModel bulk = new();

    [ObservableProperty]
    private ShopViewModel shop = new();

    [ObservableProperty]
    private RarityItem rarity = new();

    [ObservableProperty]
    private double opacity = 1.0;

    [ObservableProperty]
    private string opacityText = string.Empty;

    [ObservableProperty]
    private string priceTime = string.Empty;

    [ObservableProperty]
    private ExpanderOption expander = new();

    [ObservableProperty]
    private CheckComboOption checkComboInfluence = new();

    [ObservableProperty]
    private CheckComboOption checkComboCondition = new();

    [ObservableProperty]
    private bool freeze;

    [ObservableProperty]
    private string rateText = string.Empty;

    [ObservableProperty]
    private bool minimized;

    [ObservableProperty]
    private bool fetchDetailIsEnabled;

    public sealed partial class RarityItem : ViewModelBase
    {
        [ObservableProperty]
        private string item;

        [ObservableProperty]
        private int index;

        [ObservableProperty]
        private AsyncObservableCollection<string> comboBox = new();
    }

    public sealed partial class InfluenceItem : ViewModelBase
    {
        [ObservableProperty]
        private bool shaper;

        [ObservableProperty]
        private bool elder;

        [ObservableProperty]
        private bool crusader;

        [ObservableProperty]
        private bool redeemer;

        [ObservableProperty]
        private bool hunter;

        [ObservableProperty]
        private bool warlord;

        [ObservableProperty]
        private string shaperText = string.Empty;

        [ObservableProperty]
        private string elderText = string.Empty;

        [ObservableProperty]
        private string crusaderText = string.Empty;

        [ObservableProperty]
        private string redeemerText = string.Empty;

        [ObservableProperty]
        private string hunterText = string.Empty;

        [ObservableProperty]
        private string warlordText = string.Empty;
    }

    public sealed partial class ConditionItem : ViewModelBase
    {
        [ObservableProperty]
        private bool freePrefix;

        [ObservableProperty]
        private bool freeSuffix;

        [ObservableProperty]
        private bool socketColors;

        [ObservableProperty]
        private string freePrefixText = string.Empty;

        [ObservableProperty]
        private string freeSuffixText = string.Empty;

        [ObservableProperty]
        private string socketColorsText = string.Empty;

        [ObservableProperty]
        private string freePrefixToolTip = string.Empty;

        [ObservableProperty]
        private string freeSuffixToolTip = string.Empty;

        [ObservableProperty]
        private string socketColorsToolTip = string.Empty;
    }

    public sealed partial class ExpanderOption : ViewModelBase
    {
        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        private double width = 40;
    }

    public sealed partial class CheckComboOption : ViewModelBase
    {
        [ObservableProperty]
        private string text = string.Empty;

        [ObservableProperty]
        private string toolTip = string.Empty;

        [ObservableProperty]
        private string none = string.Empty;
    }
}

using System.Collections.Specialized;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed class FormViewModel : BaseViewModel
{
    private string itemName;
    private string itemNameColor;
    private string itemBaseType;
    private double baseTypeFontSize = 12; // FontSize cannot be equal to 0
    private string dps = string.Empty;
    private string dpsTip = string.Empty;
    private int corruptedIndex = -1;
    private string rarityBox;
    private bool byBase;
    private bool allCheck;
    private string detail;
    private BottomFormViewModel panel = new();
    private AsyncObservableCollection<ModLineViewModel> modLine = new();
    private AsyncObservableCollection<string> corruption = new();
    private AsyncObservableCollection<string> alternate = new();
    private AsyncObservableCollection<string> market = new();
    private int marketIndex;
    private AsyncObservableCollection<string> league = new();
    private int leagueIndex;
    private InfluenceItem influence = new();
    private ConditionItem condition = new();
    private TabViewModel tab = new();
    private VisibilityViewModel visible = new();
    private BulkViewModel bulk = new();
    private ShopViewModel shop = new();
    private RarityItem rarity = new();
    private double opacity = 1.0;
    private string opacityText = string.Empty;
    private string priceTime = string.Empty;
    private ExpanderOption expander = new();
    private CheckComboOption checkComboInfluence = new();
    private CheckComboOption checkComboCondition = new();
    private bool freeze;
    private string rateText = string.Empty;
    private bool minimized;
    private bool fetchDetailIsEnabled;

    public string ItemName { get => itemName; set => SetProperty(ref itemName, value); }
    public string ItemNameColor { get => itemNameColor; set => SetProperty(ref itemNameColor, value); }
    public string ItemBaseType { get => itemBaseType; set => SetProperty(ref itemBaseType, value); }
    public double BaseTypeFontSize { get => baseTypeFontSize; set => SetProperty(ref baseTypeFontSize, value); }
    public string Dps { get => dps; set => SetProperty(ref dps, value); }
    public string DpsTip { get => dpsTip; set => SetProperty(ref dpsTip, value); }
    public int CorruptedIndex { get => corruptedIndex; set => SetProperty(ref corruptedIndex, value); }
    public string RarityBox { get => rarityBox; set => SetProperty(ref rarityBox, value); }
    public bool ByBase { get => byBase; set => SetProperty(ref byBase, value); }
    public bool AllCheck { get => allCheck; set => SetProperty(ref allCheck, value); }
    public string Detail { get => detail; set => SetProperty(ref detail, value); }
    public BottomFormViewModel Panel { get => panel; set => SetProperty(ref panel, value); }
    public AsyncObservableCollection<ModLineViewModel> ModLine { get => modLine; set => SetProperty(ref modLine, value); }
    public AsyncObservableCollection<string> Corruption { get => corruption; set => SetProperty(ref corruption, value); }
    public AsyncObservableCollection<string> Alternate { get => alternate; set => SetProperty(ref alternate, value); }
    public AsyncObservableCollection<string> Market { get => market; set => SetProperty(ref market, value); }
    public int MarketIndex { get => marketIndex; set => SetProperty(ref marketIndex, value); }
    public AsyncObservableCollection<string> League { get => league; set => SetProperty(ref league, value); }
    public int LeagueIndex { get => leagueIndex; set => SetProperty(ref leagueIndex, value); }
    public InfluenceItem Influence { get => influence; set => SetProperty(ref influence, value); }
    public ConditionItem Condition { get => condition; set => SetProperty(ref condition, value); }
    public TabViewModel Tab { get => tab; set => SetProperty(ref tab, value); }
    public VisibilityViewModel Visible { get => visible; set => SetProperty(ref visible, value); }
    public BulkViewModel Bulk { get => bulk; set => SetProperty(ref bulk, value); }
    public ShopViewModel Shop { get => shop; set => SetProperty(ref shop, value); }
    public RarityItem Rarity { get => rarity; set => SetProperty(ref rarity, value); }
    public double Opacity { get => opacity; set => SetProperty(ref opacity, value); }
    public string OpacityText { get => opacityText; set => SetProperty(ref opacityText, value); }
    public string PriceTime { get => priceTime; set => SetProperty(ref priceTime, value); }
    public ExpanderOption Expander { get => expander; set => SetProperty(ref expander, value); }
    public CheckComboOption CheckComboInfluence { get => checkComboInfluence; set => SetProperty(ref checkComboInfluence, value); }
    public CheckComboOption CheckComboCondition { get => checkComboCondition; set => SetProperty(ref checkComboCondition, value); }
    public bool Freeze { get => freeze; set => SetProperty(ref freeze, value); }
    public string RateText { get => rateText; set => SetProperty(ref rateText, value); }
    public bool Minimized { get => minimized; set => SetProperty(ref minimized, value); }
    public bool FetchDetailIsEnabled { get => fetchDetailIsEnabled; set => SetProperty(ref fetchDetailIsEnabled, value); }

    public sealed class RarityItem : BaseViewModel
    {
        private string item;
        private int index;
        private AsyncObservableCollection<string> comboBox = new();

        public string Item { get => item; set => SetProperty(ref item, value); }
        public int Index { get => index; set => SetProperty(ref index, value); }
        public AsyncObservableCollection<string> ComboBox { get => comboBox; set => SetProperty(ref comboBox, value); }
    }

    public sealed class InfluenceItem : BaseViewModel
    {
        private bool shaper;
        private bool elder;
        private bool crusader;
        private bool redeemer;
        private bool hunter;
        private bool warlord;
        private string shaperText = string.Empty;
        private string elderText = string.Empty;
        private string crusaderText = string.Empty;
        private string redeemerText = string.Empty;
        private string hunterText = string.Empty;
        private string warlordText = string.Empty;

        public bool Shaper { get => shaper; set => SetProperty(ref shaper, value); }
        public bool Elder { get => elder; set => SetProperty(ref elder, value); }
        public bool Crusader { get => crusader; set => SetProperty(ref crusader, value); }
        public bool Redeemer { get => redeemer; set => SetProperty(ref redeemer, value); }
        public bool Hunter { get => hunter; set => SetProperty(ref hunter, value); }
        public bool Warlord { get => warlord; set => SetProperty(ref warlord, value); }
        public string ShaperText { get => shaperText; set => SetProperty(ref shaperText, value); }
        public string ElderText { get => elderText; set => SetProperty(ref elderText, value); }
        public string CrusaderText { get => crusaderText; set => SetProperty(ref crusaderText, value); }
        public string RedeemerText { get => redeemerText; set => SetProperty(ref redeemerText, value); }
        public string HunterText { get => hunterText; set => SetProperty(ref hunterText, value); }
        public string WarlordText { get => warlordText; set => SetProperty(ref warlordText, value); }
    }

    public sealed class ConditionItem : BaseViewModel
    {
        private bool freePrefix;
        private bool freeSuffix;
        private bool socketColors;
        private string freePrefixText = string.Empty;
        private string freeSuffixText = string.Empty;
        private string socketColorsText = string.Empty;
        private string freePrefixToolTip = string.Empty;
        private string freeSuffixToolTip = string.Empty;
        private string socketColorsToolTip = string.Empty;

        public bool FreePrefix { get => freePrefix; set => SetProperty(ref freePrefix, value); }
        public bool FreeSuffix { get => freeSuffix; set => SetProperty(ref freeSuffix, value); }
        public bool SocketColors { get => socketColors; set => SetProperty(ref socketColors, value); }
        public string FreePrefixText { get => freePrefixText; set => SetProperty(ref freePrefixText, value); }
        public string FreeSuffixText { get => freeSuffixText; set => SetProperty(ref freeSuffixText, value); }
        public string SocketColorsText { get => socketColorsText; set => SetProperty(ref socketColorsText, value); }
        public string FreePrefixToolTip { get => freePrefixToolTip; set => SetProperty(ref freePrefixToolTip, value); }
        public string FreeSuffixToolTip { get => freeSuffixToolTip; set => SetProperty(ref freeSuffixToolTip, value); }
        public string SocketColorsToolTip { get => socketColorsToolTip; set => SetProperty(ref socketColorsToolTip, value); }
    }

    public sealed class ExpanderOption : BaseViewModel
    {
        private bool isExpanded;
        private double width = 40;

        public bool IsExpanded { get => isExpanded; set => SetProperty(ref isExpanded, value); }
        public double Width { get => width; set => SetProperty(ref width, value); }
    }

    public sealed class CheckComboOption : BaseViewModel
    {
        private string text = string.Empty;
        private string toolTip = string.Empty;
        private string none = string.Empty;

        public string Text { get => text; set => SetProperty(ref text, value); }
        public string ToolTip { get => toolTip; set => SetProperty(ref toolTip, value); }
        public string None { get => none; set => SetProperty(ref none, value); }
    }
}

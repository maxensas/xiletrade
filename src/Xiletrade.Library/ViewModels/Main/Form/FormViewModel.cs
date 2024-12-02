using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.ViewModels.Main.Exchange;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form;

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
    private PanelViewModel panel = new();

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
    private InfluenceViewModel influence = new();

    [ObservableProperty]
    private ConditionViewModel condition = new();

    [ObservableProperty]
    private TabViewModel tab = new();

    [ObservableProperty]
    private VisibilityViewModel visible = new();

    [ObservableProperty]
    private BulkViewModel bulk = new();

    [ObservableProperty]
    private ShopViewModel shop = new();

    [ObservableProperty]
    private RarityViewModel rarity = new();

    [ObservableProperty]
    private double opacity = 1.0;

    [ObservableProperty]
    private string opacityText = string.Empty;

    [ObservableProperty]
    private string priceTime = string.Empty;

    [ObservableProperty]
    private ExpanderViewModel expander = new();

    [ObservableProperty]
    private CheckComboViewModel checkComboInfluence = new();

    [ObservableProperty]
    private CheckComboViewModel checkComboCondition = new();

    [ObservableProperty]
    private bool freeze;

    [ObservableProperty]
    private string rateText = string.Empty;

    [ObservableProperty]
    private bool minimized;

    [ObservableProperty]
    private bool fetchDetailIsEnabled;

    [ObservableProperty]
    private bool sameUser;

    [ObservableProperty]
    private bool chaosDiv;

    [ObservableProperty]
    private bool autoClose;
}

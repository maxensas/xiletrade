using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main.Exchange;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class FormViewModel : ViewModelBase
{
    [ObservableProperty]
    private string itemName;

    [ObservableProperty]
    private string itemNameColor = string.Empty;

    [ObservableProperty]
    private string itemBaseType = string.Empty;

    [ObservableProperty]
    private string itemBaseTypeColor = string.Empty;

    [ObservableProperty]
    private double baseTypeFontSize = 12; // FontSize cannot be equal to 0

    [ObservableProperty]
    private string dps = string.Empty;

    [ObservableProperty]
    private string dpsTip = string.Empty;

    [ObservableProperty]
    private int corruptedIndex = 0;

    [ObservableProperty]
    private string rarityBox;

    [ObservableProperty]
    private bool byBase;

    [ObservableProperty]
    private bool allCheck;

    [ObservableProperty]
    private string detail = string.Empty;

    [ObservableProperty]
    private PanelViewModel panel = new();

    [ObservableProperty]
    private AsyncObservableCollection<ModLineViewModel> modLine = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> corruption = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private AsyncObservableCollection<string> alternate = new() { Resources.Resources.General005_Any, Resources.Resources.General001_Anomalous, Resources.Resources.General002_Divergent,
        Resources.Resources.General003_Phantasmal }; // obsolete

    [ObservableProperty]
    private AsyncObservableCollection<string> market = new() { "online", Strings.any };

    [ObservableProperty]
    private int marketIndex = 0;

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

    public FormViewModel(bool useBulk = false)
    {
        // Init using data
        InitLeagues();
        Opacity = DataManager.Config.Options.Opacity;
        AutoClose = DataManager.Config.Options.Autoclose;
        CorruptedIndex = DataManager.Config.Options.AutoSelectCorrupt ? 2 : 0;
        SameUser = DataManager.Config.Options.HideSameOccurs;
        Visible.Poeprices = DataManager.Config.Options.Language is 0;

        OpacityText = Opacity * 100 + "%";

        if (useBulk)
        {
            ItemBaseType = Resources.Resources.Main032_cbTotalExchange;
            ItemBaseTypeColor = Strings.Color.Moccasin;
            Tab.BulkEnable = true;
            Tab.BulkSelected = true;
            Tab.ShopEnable = true;
            Tab.ShopSelected = false;
            Visible.Wiki = false;
            Visible.BtnPoeDb = false;
            ItemName = string.Empty;
            BaseTypeFontSize = 16;
        }
    }

    private void InitLeagues()
    {
        AsyncObservableCollection<string> listLeague = new();

        if (DataManager.League.Result.Length >= 2)
        {
            foreach (LeagueResult res in DataManager.League.Result)
            {
                listLeague.Add(res.Id);
            }
        }
        League = listLeague;
        int idx = listLeague.IndexOf(DataManager.Config.Options.League);
        LeagueIndex = idx > -1 ? idx : 0;
    }
}

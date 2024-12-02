using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Logic;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;
using Xiletrade.Library.ViewModels.Main.Form;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private FormViewModel form = new();

    [ObservableProperty]
    private ResultViewModel result = new();

    [ObservableProperty]
    private NinjaButtonViewModel ninjaButton = new();

    [ObservableProperty]
    private string notifyName;

    internal MainLogic Logic { get; private set; }

    public MainCommand Commands { get; private set; }
    public TrayMenuCommand TrayCommands { get; private set; }

    public List<MouseGestureCom> GestureList { get; set; } = new();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        Logic = new(this, serviceProvider);
        Commands = new(this, serviceProvider);
        TrayCommands = new(this, serviceProvider);
        NotifyName = "Xiletrade " + Common.GetFileVersion();
        GestureList.Add(new MouseGestureCom(Commands.WheelIncrementCommand, ModifierKey.None, MouseWheelDirection.Up));
        GestureList.Add(new MouseGestureCom(Commands.WheelIncrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Up));
        GestureList.Add(new MouseGestureCom(Commands.WheelIncrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Up));
        GestureList.Add(new MouseGestureCom(Commands.WheelDecrementCommand, ModifierKey.None, MouseWheelDirection.Down));
        GestureList.Add(new MouseGestureCom(Commands.WheelDecrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Down));
        GestureList.Add(new MouseGestureCom(Commands.WheelDecrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Down));
    }

    internal void ResetViewModel(bool useBulk)
    {
        Form.Minimized = false;
        Form.Expander.IsExpanded = false;

        Form.Opacity = DataManager.Config.Options.Opacity;
        Form.AutoClose = DataManager.Config.Options.Autoclose;
        Form.OpacityText = Form.Opacity * 100 + "%";

        Logic.Task.Price.Buffer.NinjaChaosEqGet = -1;
        Logic.Task.Price.Buffer.NinjaChaosEqPay = -1;

        Result.Quick.Price = string.Empty;
        Result.Quick.PriceBis = string.Empty;
        Result.Detail.Price = string.Empty;
        Result.Detail.PriceBis = string.Empty;

        Form.PriceTime = string.Empty;
        Form.Panel.Common.Sockets.RedColor = string.Empty;
        Form.Panel.Common.Sockets.GreenColor = string.Empty;
        Form.Panel.Common.Sockets.BlueColor = string.Empty;
        Form.Panel.Common.Sockets.WhiteColor = string.Empty;
        Form.Panel.Common.Sockets.LinkMin = string.Empty;
        Form.Panel.Common.Sockets.SocketMin = string.Empty;
        Form.Panel.Common.Sockets.LinkMax = string.Empty;
        Form.Panel.Common.Sockets.SocketMax = string.Empty;
        Form.Panel.Common.ItemLevel.Min = string.Empty;
        Form.Panel.Common.ItemLevel.Max = string.Empty;
        Form.Panel.Common.Quality.Min = string.Empty;
        Form.Panel.Common.Quality.Max = string.Empty;
        Form.Detail = string.Empty;
        Form.Panel.Defense.Armour.Min = string.Empty;
        Form.Panel.Defense.Armour.Max = string.Empty;
        Form.Panel.Defense.Energy.Min = string.Empty;
        Form.Panel.Defense.Energy.Max = string.Empty;
        Form.Panel.Defense.Evasion.Min = string.Empty;
        Form.Panel.Defense.Evasion.Max = string.Empty;
        Form.Panel.Defense.Ward.Min = string.Empty;
        Form.Panel.Defense.Ward.Max = string.Empty;
        Form.Panel.Damage.Total.Min = string.Empty;
        Form.Panel.Damage.Total.Max = string.Empty;
        Form.Panel.Damage.Physical.Min = string.Empty;
        Form.Panel.Damage.Physical.Max = string.Empty;
        Form.Panel.Damage.Elemental.Min = string.Empty;
        Form.Panel.Damage.Elemental.Max = string.Empty;
        Form.Panel.Total.Resistance.Min = string.Empty;
        Form.Panel.Total.Resistance.Max = string.Empty;
        Form.Panel.Total.Life.Min = string.Empty;
        Form.Panel.Total.Life.Max = string.Empty;
        Form.Panel.Total.GlobalEs.Min = string.Empty;
        Form.Panel.Total.GlobalEs.Max = string.Empty;
        Form.Panel.FacetorMin = string.Empty;
        Form.Panel.FacetorMax = string.Empty;
        Form.Panel.Reward.Text = string.Empty;
        Form.Panel.Reward.Tip = string.Empty;
        Form.Panel.Reward.FgColor = string.Empty;

        Form.Panel.Sanctum.Resolve.Min = string.Empty;
        Form.Panel.Sanctum.Resolve.Max = string.Empty;
        Form.Panel.Sanctum.MaximumResolve.Min = string.Empty;
        Form.Panel.Sanctum.MaximumResolve.Max = string.Empty;
        Form.Panel.Sanctum.Inspiration.Min = string.Empty;
        Form.Panel.Sanctum.Inspiration.Max = string.Empty;
        Form.Panel.Sanctum.Aureus.Min = string.Empty;
        Form.Panel.Sanctum.Aureus.Max = string.Empty;

        Form.Panel.Map.Quantity.Min = string.Empty;
        Form.Panel.Map.Quantity.Max = string.Empty;
        Form.Panel.Map.Rarity.Min = string.Empty;
        Form.Panel.Map.Rarity.Max = string.Empty;
        Form.Panel.Map.PackSize.Min = string.Empty;
        Form.Panel.Map.PackSize.Max = string.Empty;
        Form.Panel.Map.MoreScarab.Min = string.Empty;
        Form.Panel.Map.MoreScarab.Max = string.Empty;
        Form.Panel.Map.MoreCurrency.Min = string.Empty;
        Form.Panel.Map.MoreCurrency.Max = string.Empty;
        Form.Panel.Map.MoreDivCard.Min = string.Empty;
        Form.Panel.Map.MoreDivCard.Max = string.Empty;
        Form.Panel.Map.MoreMap.Min = string.Empty; // it's parsed but not used in view/request.
        Form.Panel.Map.MoreMap.Max = string.Empty; // it's parsed but not used in view/request.

        //ViewModel.Form.Panel.Common.ItemLevelLabel = Resources.Resources.Main065_tbiLevel;
        Result.Bulk.Total = Resources.Resources.Main032_cbTotalExchange;

        Form.AllCheck = false; // call events
        Form.Panel.Common.ItemLevel.Selected = false;
        Form.Panel.Common.Quality.Selected = false;
        Form.Panel.Common.Sockets.Selected = false;
        Form.Panel.Defense.Armour.Selected = false;
        Form.Panel.Defense.Energy.Selected = false;
        Form.Panel.Defense.Evasion.Selected = false;
        Form.Panel.Defense.Ward.Selected = false;
        Form.Panel.Damage.Total.Selected = false;
        Form.Panel.Damage.Physical.Selected = false;
        Form.Panel.Damage.Elemental.Selected = false;
        Form.Panel.Total.Resistance.Selected = false;
        Form.Panel.Total.Life.Selected = false;
        Form.Panel.Total.GlobalEs.Selected = false;
        Form.Panel.Sanctum.Resolve.Selected = false;
        Form.Panel.Sanctum.MaximumResolve.Selected = false;
        Form.Panel.Sanctum.Inspiration.Selected = false;
        Form.Panel.Sanctum.Aureus.Selected = false;
        Form.Panel.Map.Quantity.Selected = false;
        Form.Panel.Map.Rarity.Selected = false;
        Form.Panel.Map.PackSize.Selected = false;
        Form.Panel.Map.MoreScarab.Selected = false;
        Form.Panel.Map.MoreCurrency.Selected = false;
        Form.Panel.Map.MoreDivCard.Selected = false;
        Form.Panel.Map.MoreMap.Selected = false;

        //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(StringsTable.Culture[mConfigData.Options.Language]);
        CultureInfo cultureRefresh = CultureInfo.CreateSpecificCulture(Strings.Culture[DataManager.Config.Options.Language]);
        Thread.CurrentThread.CurrentUICulture = cultureRefresh;
        TranslationViewModel.Instance.CurrentCulture = cultureRefresh;

        Form.Bulk.Get.Category = InitCategory();
        Form.Bulk.Pay.Category = InitCategory();
        Form.Shop.Exchange.Category = InitCategory();

        Form.Corruption = new AsyncObservableCollection<string> { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

        Form.Rarity.ComboBox = new AsyncObservableCollection<string> { Resources.Resources.General005_Any, Resources.Resources.General009_Normal, Resources.Resources.General008_Magic,
        Resources.Resources.General007_Rare, Resources.Resources.General006_Unique, Resources.Resources.General110_FoilUnique, Resources.Resources.General010_AnyNU };

        // obsolete
        Form.Alternate = new AsyncObservableCollection<string> { Resources.Resources.General005_Any, Resources.Resources.General001_Anomalous, Resources.Resources.General002_Divergent,
        Resources.Resources.General003_Phantasmal };

        Form.Visible.Detail = false;
        Form.Visible.HeaderMod = true;
        Form.Visible.Quality = true;
        Form.Visible.Wiki = true;
        Form.Visible.BtnPoeDb = true;
        Form.Visible.BulkLastSearch = false;
        Form.Visible.SanctumFields = false;
        Form.Visible.MapStats = false;
        Form.Visible.Reward = false;

        Form.Bulk.Get.TierVisible = false;
        Form.Bulk.Pay.TierVisible = false;
        Form.Bulk.Get.CurrencyVisible = false;
        Form.Bulk.Pay.CurrencyVisible = false;

        Form.Visible.CheckAll = true;
        Form.Visible.Corrupted = true;
        Form.Visible.AlternateGem = false;
        Form.Visible.Facetor = false;

        Form.CorruptedIndex = DataManager.Config.Options.AutoSelectCorrupt ? 2 : 0;
        Form.SameUser = DataManager.Config.Options.HideSameOccurs;

        Form.Condition.FreePrefixText = Resources.Resources.Main174_cbFreePrefix;
        Form.Condition.FreePrefixToolTip = Resources.Resources.Main175_cbFreePrefixTip;
        Form.Condition.FreeSuffixText = Resources.Resources.Main176_cbFreeSuffix;
        Form.Condition.FreeSuffixToolTip = Resources.Resources.Main177_cbFreeSuffixTip;
        Form.Condition.SocketColorsText = Resources.Resources.Main209_cbSocketColors;
        Form.Condition.SocketColorsToolTip = Resources.Resources.Main210_cbSocketColorsTip;
        Form.Influence.ShaperText = Resources.Resources.Main037_Shaper;
        Form.Influence.ElderText = Resources.Resources.Main038_Elder;
        Form.Influence.CrusaderText = Resources.Resources.Main039_Crusader;
        Form.Influence.RedeemerText = Resources.Resources.Main040_Redeemer;
        Form.Influence.HunterText = Resources.Resources.Main041_Hunter;
        Form.Influence.WarlordText = Resources.Resources.Main042_Warlord;

        Form.Influence.Shaper = false;
        Form.Influence.Elder = false;
        Form.Influence.Crusader = false;
        Form.Influence.Redeemer = false;
        Form.Influence.Hunter = false;
        Form.Influence.Warlord = false;
        Form.Condition.FreePrefix = false;
        Form.Condition.FreeSuffix = false;
        Form.Condition.SocketColors = false;

        Form.Visible.Influences = true;
        Form.Visible.Conditions = false;

        Form.CheckComboInfluence.Text = Resources.Resources.Main036_None;
        Form.CheckComboInfluence.None = Resources.Resources.Main036_None;
        Form.CheckComboInfluence.ToolTip = null;
        Form.CheckComboCondition.Text = Resources.Resources.Main036_None;
        Form.CheckComboCondition.None = Resources.Resources.Main036_None;
        Form.CheckComboCondition.ToolTip = null;

        Form.Bulk.Get.CategoryIndex = 0;
        Form.Bulk.Get.CurrencyIndex = 0;
        Form.Bulk.Pay.CategoryIndex = 0;
        Form.Bulk.Pay.CurrencyIndex = 0;
        Form.Shop.Exchange.CategoryIndex = 0;
        Form.Shop.Exchange.CurrencyIndex = 0;

        Form.Bulk.Get.Image = null;
        Form.Bulk.Pay.Image = null;

        Form.ItemNameColor = string.Empty;

        Form.CorruptedIndex = 0;

        Form.Bulk.Get.CategoryIndex = 0;
        Form.Bulk.Pay.CategoryIndex = 0;
        Form.Shop.Exchange.CategoryIndex = 0;
        Form.Panel.AlternateGemIndex = 0;

        Result.DetailList.Clear();
        Result.BulkList.Clear();
        Result.BulkOffers.Clear();
        Result.ShopList.Clear();

        Form.Shop.GetList.Clear();
        Form.Shop.PayList.Clear();

        Form.Bulk.Pay.Currency.Clear();
        Form.Bulk.Get.Currency.Clear();

        Form.Bulk.Get.Search = string.Empty;
        Form.Bulk.Pay.Search = string.Empty;
        Form.Shop.Exchange.Search = string.Empty;

        Form.Dps = string.Empty;
        Form.DpsTip = string.Empty;
        Form.Visible.Damage = false;
        Form.Visible.Defense = false;
        Form.Visible.Total = false;

        Form.Visible.HiddablePanel = false;

        Form.Visible.SynthesisBlight = Form.Panel.SynthesisBlight = false;
        Form.Visible.BlightRavaged = Form.Panel.BlighRavaged = false;
        Form.Visible.Scourged = Form.Panel.Scourged = false;

        Form.Panel.UseBorderThickness = true;
        Form.Visible.ModSet = false;

        Form.Panel.Row.ArmourMaxHeight = 0;
        Form.Panel.Row.WeaponMaxHeight = 0;
        Form.Panel.Row.TotalMaxHeight = 0;
        Form.Panel.Col.FirstMaxWidth = 14;
        Form.Panel.Col.LastMinWidth = 86;

        Form.Visible.Sockets = false;

        Form.Visible.Ward = false;
        Form.Visible.Armour = false;
        Form.Visible.Energy = false;
        Form.Visible.Evasion = false;

        Form.Visible.PanelForm = true;

        // Tab items
        Form.Tab.QuickSelected = Form.Tab.DetailSelected = Form.Tab.BulkSelected = Form.Tab.ShopSelected = Form.Tab.PoePriceSelected = false;
        Form.Tab.QuickEnable = Form.Tab.DetailEnable = Form.Tab.BulkEnable = Form.Tab.ShopEnable = Form.Tab.PoePriceEnable = false;
        Result.PoepricesList.Clear();

        NinjaButton.Visible = false;

        // poeprices only in english
        Form.Visible.Poeprices = DataManager.Config.Options.Language is 0;

        //DataManager.UniquesImplicits = null;
        //DataManager.UniquesMods = null;

        Form.Visible.ByBase = true;

        Form.Visible.Rarity = true;
        Form.Rarity.Index = 0;

        Form.ItemBaseType = string.Empty;
        Form.BaseTypeFontSize = 12;

        Form.Market = new() { "online", Strings.any };
        Form.MarketIndex = 0;

        InitLeagues();
        Form.Bulk.Stock = "1";
        Form.Shop.Stock = "1";

        Form.Tab.QuickSelected = true;

        Result.Bulk.Price = Resources.Resources.Main001_PriceSelect;
        Result.Bulk.PriceBis = string.Empty;
        Result.Shop.Price = Resources.Resources.Main001_PriceSelect;
        Result.Shop.PriceBis = string.Empty;

        Form.Visible.PanelStat = true;

        Form.ChaosDiv = false;

        Form.ModLine = new();

        Logic.MetamorphMods = new();

        if (useBulk)
        {
            Form.Tab.BulkEnable = true;
            Form.Tab.BulkSelected = true;
            Form.Tab.ShopEnable = true;
            Form.Tab.ShopSelected = false;

            Form.Visible.Wiki = false;
            Form.Visible.BtnPoeDb = false;
            Form.ItemName = string.Empty;
            Form.ItemBaseType = Resources.Resources.Main032_cbTotalExchange;
            Form.BaseTypeFontSize = 16;
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
        Form.League = listLeague;
        int idx = listLeague.IndexOf(DataManager.Config.Options.League);
        Form.LeagueIndex = idx > -1 ? idx : 0;
    }

    private static AsyncObservableCollection<string> InitCategory()
    {
        return new AsyncObservableCollection<string> { Resources.Resources.Main043_Choose, Resources.Resources.Main044_MainCur, Resources.Resources.Main207_ExoticCurrency, Resources.Resources.Main045_OtherCur, //Resources.Resources.Main149_Shards,
        Resources.Resources.Main046_MapFrag, Resources.Resources.Main047_Stones, Resources.Resources.Main198_ScoutingReports, Resources.Resources.Main208_MemoryLine, Resources.Resources.Main186_Expedition, Resources.Resources.Main048_Delirium, Resources.Resources.Main049_Catalysts,
        Resources.Resources.Main050_Oils, Resources.Resources.Main051_Incubators, Resources.Resources.Main052_Scarabs, Resources.Resources.Main053_Fossils,
        Resources.Resources.Main054_Essences, Resources.Resources.Main211_AncestorCurrency, Resources.Resources.Main212_Sanctum, //Resources.Resources.Main213_Crucible,
        Resources.Resources.Main055_Divination, Resources.Resources.Main056_Maps, Resources.Resources.Main179_UniqueMaps, Resources.Resources.Main216_BossMaps, Resources.Resources.Main217_BlightedMaps,
        Resources.Resources.Main219_Beasts, Resources.Resources.Main218_Heist, Resources.Resources.General132_Rune
        };
    }
}

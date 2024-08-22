using System.Globalization;
using System.Threading;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Library.Models.Logic;

/// <summary>Class containing logic used to reset main viewmodel to original state.</summary>
internal sealed class MainResetHelper // code from previous release, TODO : test perf when making new MainViewModel object.
{
    private static MainViewModel Vm { get; set; }

    internal MainResetHelper(MainViewModel vm)
    {
        Vm = vm;
    }
    
    internal void ResetMainViewModel()
    {
        Reset(Vm);
    }

    // private methods
    private static void Reset(MainViewModel vm)
    {
        vm.Form.Minimized = false;
        vm.Form.Expander.IsExpanded = false;

        vm.Form.Opacity = DataManager.Config.Options.Opacity;
        vm.AutoClose = DataManager.Config.Options.Autoclose;
        vm.Form.OpacityText = vm.Form.Opacity * 100 + "%";

        vm.Logic.Task.Price.Buffer.NinjaChaosEqGet = -1;
        vm.Logic.Task.Price.Buffer.NinjaChaosEqPay = -1;

        vm.Result.Quick.Price = string.Empty;
        vm.Result.Quick.PriceBis = string.Empty;
        vm.Result.Detail.Price = string.Empty;
        vm.Result.Detail.PriceBis = string.Empty;

        vm.Form.PriceTime = string.Empty;
        vm.Form.Panel.Common.Sockets.RedColor = string.Empty;
        vm.Form.Panel.Common.Sockets.GreenColor = string.Empty;
        vm.Form.Panel.Common.Sockets.BlueColor = string.Empty;
        vm.Form.Panel.Common.Sockets.WhiteColor = string.Empty;
        vm.Form.Panel.Common.Sockets.LinkMin = string.Empty;
        vm.Form.Panel.Common.Sockets.SocketMin = string.Empty;
        vm.Form.Panel.Common.Sockets.LinkMax = string.Empty;
        vm.Form.Panel.Common.Sockets.SocketMax = string.Empty;
        vm.Form.Panel.Common.ItemLevel.Min = string.Empty;
        vm.Form.Panel.Common.ItemLevel.Max = string.Empty;
        vm.Form.Panel.Common.Quality.Min = string.Empty;
        vm.Form.Panel.Common.Quality.Max = string.Empty;
        vm.Form.Detail = string.Empty;
        vm.Form.Panel.Defense.Armour.Min = string.Empty;
        vm.Form.Panel.Defense.Armour.Max = string.Empty;
        vm.Form.Panel.Defense.Energy.Min = string.Empty;
        vm.Form.Panel.Defense.Energy.Max = string.Empty;
        vm.Form.Panel.Defense.Evasion.Min = string.Empty;
        vm.Form.Panel.Defense.Evasion.Max = string.Empty;
        vm.Form.Panel.Defense.Ward.Min = string.Empty;
        vm.Form.Panel.Defense.Ward.Max = string.Empty;
        vm.Form.Panel.Damage.Total.Min = string.Empty;
        vm.Form.Panel.Damage.Total.Max = string.Empty;
        vm.Form.Panel.Damage.Physical.Min = string.Empty;
        vm.Form.Panel.Damage.Physical.Max = string.Empty;
        vm.Form.Panel.Damage.Elemental.Min = string.Empty;
        vm.Form.Panel.Damage.Elemental.Max = string.Empty;
        vm.Form.Panel.Total.Resistance.Min = string.Empty;
        vm.Form.Panel.Total.Resistance.Max = string.Empty;
        vm.Form.Panel.Total.Life.Min = string.Empty;
        vm.Form.Panel.Total.Life.Max = string.Empty;
        vm.Form.Panel.Total.GlobalEs.Min = string.Empty;
        vm.Form.Panel.Total.GlobalEs.Max = string.Empty;
        vm.Form.Panel.FacetorMin = string.Empty;
        vm.Form.Panel.FacetorMax = string.Empty;
        vm.Form.Panel.Reward.Text = string.Empty;
        vm.Form.Panel.Reward.Tip = string.Empty;
        vm.Form.Panel.Reward.FgColor = string.Empty;

        vm.Form.Panel.Sanctum.Resolve.Min = string.Empty;
        vm.Form.Panel.Sanctum.Resolve.Max = string.Empty;
        vm.Form.Panel.Sanctum.MaximumResolve.Min = string.Empty;
        vm.Form.Panel.Sanctum.MaximumResolve.Max = string.Empty;
        vm.Form.Panel.Sanctum.Inspiration.Min = string.Empty;
        vm.Form.Panel.Sanctum.Inspiration.Max = string.Empty;
        vm.Form.Panel.Sanctum.Aureus.Min = string.Empty;
        vm.Form.Panel.Sanctum.Aureus.Max = string.Empty;

        vm.Form.Panel.Map.Quantity.Min = string.Empty;
        vm.Form.Panel.Map.Quantity.Max = string.Empty;
        vm.Form.Panel.Map.Rarity.Min = string.Empty;
        vm.Form.Panel.Map.Rarity.Max = string.Empty;
        vm.Form.Panel.Map.PackSize.Min = string.Empty;
        vm.Form.Panel.Map.PackSize.Max = string.Empty;

        //ViewModel.Form.Panel.Common.ItemLevelLabel = Resources.Resources.Main065_tbiLevel;
        vm.Result.Bulk.Total = Resources.Resources.Main032_cbTotalExchange;

        vm.Form.AllCheck = false; // call events
        vm.Form.Panel.Common.ItemLevel.Selected = false;
        vm.Form.Panel.Common.Quality.Selected = false;
        vm.Form.Panel.Common.Sockets.Selected = false;
        vm.Form.Panel.Defense.Armour.Selected = false;
        vm.Form.Panel.Defense.Energy.Selected = false;
        vm.Form.Panel.Defense.Evasion.Selected = false;
        vm.Form.Panel.Defense.Ward.Selected = false;
        vm.Form.Panel.Damage.Total.Selected = false;
        vm.Form.Panel.Damage.Physical.Selected = false;
        vm.Form.Panel.Damage.Elemental.Selected = false;
        vm.Form.Panel.Total.Resistance.Selected = false;
        vm.Form.Panel.Total.Life.Selected = false;
        vm.Form.Panel.Total.GlobalEs.Selected = false;
        vm.Form.Panel.Sanctum.Resolve.Selected = false;
        vm.Form.Panel.Sanctum.MaximumResolve.Selected = false;
        vm.Form.Panel.Sanctum.Inspiration.Selected = false;
        vm.Form.Panel.Sanctum.Aureus.Selected = false;
        vm.Form.Panel.Map.Quantity.Selected = false;
        vm.Form.Panel.Map.Rarity.Selected = false;
        vm.Form.Panel.Map.PackSize.Selected = false;

        //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(StringsTable.Culture[mConfigData.Options.Language]);
        CultureInfo cultureRefresh = CultureInfo.CreateSpecificCulture(Strings.Culture[DataManager.Config.Options.Language]);
        Thread.CurrentThread.CurrentUICulture = cultureRefresh;
        TranslationViewModel.Instance.CurrentCulture = cultureRefresh;

        vm.Form.Bulk.Get.Category = InitCategory();
        vm.Form.Bulk.Pay.Category = InitCategory();
        vm.Form.Shop.Exchange.Category = InitCategory();

        vm.Form.Corruption = new AsyncObservableCollection<string> { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

        vm.Form.Rarity.ComboBox = new AsyncObservableCollection<string> { Resources.Resources.General005_Any, Resources.Resources.General009_Normal, Resources.Resources.General008_Magic,
        Resources.Resources.General007_Rare, Resources.Resources.General006_Unique, Resources.Resources.General110_FoilUnique, Resources.Resources.General010_AnyNU };

        // obsolete
        vm.Form.Alternate = new AsyncObservableCollection<string> { Resources.Resources.General005_Any, Resources.Resources.General001_Anomalous, Resources.Resources.General002_Divergent,
        Resources.Resources.General003_Phantasmal };

        vm.Form.Visible.Detail = false;
        vm.Form.Visible.HeaderMod = true;
        vm.Form.Visible.Quality = true;

        vm.Form.Visible.Wiki = true;

        vm.Form.Visible.BtnPoeDb = true;

        vm.Form.Visible.BulkLastSearch = false;

        vm.Form.Visible.SanctumFields = false;

        vm.Form.Visible.MapStats = false;

        vm.Form.Visible.Reward = false;

        vm.Form.Bulk.Get.TierVisible = false;
        vm.Form.Bulk.Pay.TierVisible = false;
        vm.Form.Bulk.Get.CurrencyVisible = false;
        vm.Form.Bulk.Pay.CurrencyVisible = false;

        vm.Form.Visible.CheckAll = true;
        vm.Form.Visible.Corrupted = true;
        vm.Form.Visible.AlternateGem = false;
        vm.Form.Visible.Facetor = false;

        vm.Form.CorruptedIndex = DataManager.Config.Options.AutoSelectCorrupt ? 2 : 0;
        vm.SameUser = DataManager.Config.Options.HideSameOccurs;

        vm.Form.Condition.FreePrefixText = Resources.Resources.Main174_cbFreePrefix;
        vm.Form.Condition.FreePrefixToolTip = Resources.Resources.Main175_cbFreePrefixTip;
        vm.Form.Condition.FreeSuffixText = Resources.Resources.Main176_cbFreeSuffix;
        vm.Form.Condition.FreeSuffixToolTip = Resources.Resources.Main177_cbFreeSuffixTip;
        vm.Form.Condition.SocketColorsText = Resources.Resources.Main209_cbSocketColors;
        vm.Form.Condition.SocketColorsToolTip = Resources.Resources.Main210_cbSocketColorsTip;
        vm.Form.Influence.ShaperText = Resources.Resources.Main037_Shaper;
        vm.Form.Influence.ElderText = Resources.Resources.Main038_Elder;
        vm.Form.Influence.CrusaderText = Resources.Resources.Main039_Crusader;
        vm.Form.Influence.RedeemerText = Resources.Resources.Main040_Redeemer;
        vm.Form.Influence.HunterText = Resources.Resources.Main041_Hunter;
        vm.Form.Influence.WarlordText = Resources.Resources.Main042_Warlord;

        vm.Form.Influence.Shaper = false;
        vm.Form.Influence.Elder = false;
        vm.Form.Influence.Crusader = false;
        vm.Form.Influence.Redeemer = false;
        vm.Form.Influence.Hunter = false;
        vm.Form.Influence.Warlord = false;
        vm.Form.Condition.FreePrefix = false;
        vm.Form.Condition.FreeSuffix = false;
        vm.Form.Condition.SocketColors = false;

        vm.Form.Visible.Influences = true;
        vm.Form.Visible.Conditions = false;

        vm.Form.CheckComboInfluence.Text = Resources.Resources.Main036_None;
        vm.Form.CheckComboInfluence.None = Resources.Resources.Main036_None;
        vm.Form.CheckComboInfluence.ToolTip = null;
        vm.Form.CheckComboCondition.Text = Resources.Resources.Main036_None;
        vm.Form.CheckComboCondition.None = Resources.Resources.Main036_None;
        vm.Form.CheckComboCondition.ToolTip = null;

        vm.Form.Bulk.Get.CategoryIndex = 0;
        vm.Form.Bulk.Get.CurrencyIndex = 0;
        vm.Form.Bulk.Pay.CategoryIndex = 0;
        vm.Form.Bulk.Pay.CurrencyIndex = 0;
        vm.Form.Shop.Exchange.CategoryIndex = 0;
        vm.Form.Shop.Exchange.CurrencyIndex = 0;

        vm.Form.Bulk.Get.Image = null;
        vm.Form.Bulk.Pay.Image = null;

        vm.Form.ItemNameColor = string.Empty;

        vm.Form.CorruptedIndex = 0;

        vm.Form.Bulk.Get.CategoryIndex = 0;
        vm.Form.Bulk.Pay.CategoryIndex = 0;
        vm.Form.Shop.Exchange.CategoryIndex = 0;
        vm.Form.Panel.AlternateGemIndex = 0;

        vm.Result.DetailList.Clear();
        vm.Result.BulkList.Clear();
        vm.Result.BulkOffers.Clear();
        vm.Result.ShopList.Clear();

        vm.Form.Shop.GetList.Clear();
        vm.Form.Shop.PayList.Clear();

        vm.Form.Bulk.Pay.Currency.Clear();
        vm.Form.Bulk.Get.Currency.Clear();

        vm.Form.Bulk.Get.Search = string.Empty;
        vm.Form.Bulk.Pay.Search = string.Empty;
        vm.Form.Shop.Exchange.Search = string.Empty;

        vm.Form.Dps = string.Empty;
        vm.Form.DpsTip = string.Empty;
        vm.Form.Visible.Damage = false;
        vm.Form.Visible.Defense = false;
        vm.Form.Visible.Total = false;

        vm.Form.Visible.HiddablePanel = false;

        vm.Form.Visible.SynthesisBlight = vm.Form.Panel.SynthesisBlight = false;
        vm.Form.Visible.BlightRavaged = vm.Form.Panel.BlighRavaged = false;
        vm.Form.Visible.Scourged = vm.Form.Panel.Scourged = false;

        vm.Form.Panel.UseBorderThickness = true;
        vm.Form.Visible.ModSet = false;

        vm.Form.Panel.Row.ArmourMaxHeight = 0;
        vm.Form.Panel.Row.WeaponMaxHeight = 0;
        vm.Form.Panel.Row.TotalMaxHeight = 0;
        vm.Form.Panel.Col.FirstMaxWidth = 14;
        vm.Form.Panel.Col.LastMinWidth = 86;

        vm.Form.Visible.Sockets = false;

        vm.Form.Visible.Ward = false;
        vm.Form.Visible.Armour = false;
        vm.Form.Visible.Energy = false;
        vm.Form.Visible.Evasion = false;

        vm.Form.Visible.PanelForm = true;

        // Tab items
        vm.Form.Tab.QuickSelected = vm.Form.Tab.DetailSelected = vm.Form.Tab.BulkSelected = vm.Form.Tab.ShopSelected = vm.Form.Tab.PoePriceSelected = false;
        vm.Form.Tab.QuickEnable = vm.Form.Tab.DetailEnable = vm.Form.Tab.BulkEnable = vm.Form.Tab.ShopEnable = vm.Form.Tab.PoePriceEnable = false;
        vm.Result.PoepricesList.Clear();

        vm.NinjaButton.Visible = false;

        // poeprices only in english
        vm.Form.Visible.Poeprices = DataManager.Config.Options.Language is 0;

        //DataManager.UniquesImplicits = null;
        //DataManager.UniquesMods = null;

        vm.Form.Visible.ByBase = true;

        vm.Form.Visible.Rarity = true;
        vm.Form.Rarity.Index = 0;

        vm.Form.ItemBaseType = string.Empty;
        vm.Form.BaseTypeFontSize = 12;

        vm.Form.Market = new() { "online", Strings.any };
        vm.Form.MarketIndex = 0;

        InitLeagues(vm);
        vm.Form.Bulk.Stock = "1";
        vm.Form.Shop.Stock = "1";

        vm.Form.Tab.QuickSelected = true;

        vm.Result.Bulk.Price = Resources.Resources.Main001_PriceSelect;
        vm.Result.Bulk.PriceBis = string.Empty;
        vm.Result.Shop.Price = Resources.Resources.Main001_PriceSelect;
        vm.Result.Shop.PriceBis = string.Empty;

        vm.Form.Visible.PanelStat = true;

        vm.ChaosDiv = false;

        vm.Form.ModLine = new();

        vm.Logic.MetamorphMods = new();
    }

    private static void InitLeagues(MainViewModel vm)
    {
        AsyncObservableCollection<string> listLeague = new();

        if (DataManager.League.Result.Length >= 2)
        {
            foreach (LeagueResult res in DataManager.League.Result)
            {
                listLeague.Add(res.Id);
            }
        }
        vm.Form.League = listLeague;
        int idx = listLeague.IndexOf(DataManager.Config.Options.League);
        vm.Form.LeagueIndex = idx > -1 ? idx : 0;
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

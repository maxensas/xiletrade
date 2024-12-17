using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Exchange;

namespace Xiletrade.Library.ViewModels.Command;

public sealed partial class MainCommand : ViewModelBase
{
    private static MainViewModel Vm { get; set; }
    private static IServiceProvider _serviceProvider;
    private static bool _blockSelectBulk = false;

    public MainCommand(MainViewModel vm, IServiceProvider serviceProvider)
    {
        Vm = vm;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private static void OpenSearch(object commandParameter)
    {
        //CloseFading();
        string sEntity;
        string[] exchange = new string[2];
        string market = Vm.Form.Market[Vm.Form.MarketIndex];
        //string league = Restr.ServerType ;
        string league = Vm.Form.League[Vm.Form.LeagueIndex];

        if (Vm.Form.Tab.BulkSelected)
        {
            if (Vm.Form.Bulk.Pay.CurrencyIndex > 0 || Vm.Form.Bulk.Get.CurrencyIndex > 0)
            {
                if (Vm.Form.Bulk.Pay.CurrencyIndex > 0)
                {
                    BaseResultData tmpBase = DataManager.Bases.FirstOrDefault(y => y.Name == Vm.Form.Bulk.Pay.Currency[Vm.Form.Bulk.Pay.CurrencyIndex]);
                    if (tmpBase is null)
                    {
                        exchange[0] = Vm.Form.GetExchangeCurrencyTag(ExchangeType.Pay);
                    }
                }
                if (Vm.Form.Bulk.Get.CurrencyIndex > 0)
                {
                    BaseResultData tmpBase = DataManager.Bases.FirstOrDefault(y => y.Name == Vm.Form.Bulk.Get.Currency[Vm.Form.Bulk.Get.CurrencyIndex]);
                    if (tmpBase is null)
                    {
                        exchange[1] = Vm.Form.GetExchangeCurrencyTag(ExchangeType.Get);
                    }
                }
                if (exchange[0] is null && exchange[1] is null)
                {
                    return;
                }

                bool isInteger = int.TryParse(Vm.Form.Bulk.Stock, out int minimumStock);
                if (!isInteger)
                {
                    minimumStock = 1;
                    Vm.Form.Bulk.Stock = "1";
                }

                Exchange change = new();
                change.ExchangeData.Status.Option = market;
                change.ExchangeData.Minimum = minimumStock;
                if (exchange[0] is not null)
                {
                    change.ExchangeData.Have = [exchange[0]];
                }
                if (exchange[1] is not null)
                {
                    change.ExchangeData.Want = [exchange[1]];
                }

                string url = Strings.ExchangeUrl + league + "/?q=" + Uri.EscapeDataString(Json.Serialize<Exchange>(change));
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Failed to open PoE search window.", MessageStatus.Error);
                }
            }
            return;
        }

        if (Vm.Form.Tab.ShopSelected)
        {
            var curGetList = from list in Vm.Form.Shop.GetList select list.ToolTip;
            var curPayList = from list in Vm.Form.Shop.PayList select list.ToolTip;
            if (curGetList.Any() && curPayList.Any())
            {
                bool isInteger = int.TryParse(Vm.Form.Shop.Stock, out int minimumStock);
                if (!isInteger)
                {
                    minimumStock = 1;
                    Vm.Form.Shop.Stock = "1";
                }

                Exchange change = new();
                change.ExchangeData.Status.Option = market;
                change.ExchangeData.Have = curPayList.ToArray();
                change.ExchangeData.Want = curGetList.ToArray();
                change.ExchangeData.Minimum = minimumStock;
                //change.ExchangeData.Collapse = true;
                change.Engine = "new";
                var test = Json.Serialize<Exchange>(change);
                string url = Strings.ExchangeUrl + league + "/?q=" + Uri.EscapeDataString(Json.Serialize<Exchange>(change));

                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Failed to open PoE search window.", MessageStatus.Error);
                }
            }
            return;
        }
        if (Vm.Form.Tab.QuickSelected || Vm.Form.Tab.DetailSelected)
        {
            sEntity = Json.GetSerialized(Vm.Form.GetXiletradeItem(), Vm.CurrentItem, false, market);

            if (sEntity?.Length > 0)
            {
                OpenSearchTask(sEntity, league);
            }
        }
    }

    private static void OpenSearchTask(string sEntity, string league)
    {
        Task.Run(() =>
        {
            string result = string.Empty;
            try
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                result = service.SendHTTP(sEntity, Strings.TradeApi + league, Client.Trade).Result;
                if (result.Length > 0)
                {
                    ResultData resultData = Json.Deserialize<ResultData>(result);// voir
                    string url = Strings.TradeUrl + league + "/" + resultData.Id;
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is HttpRequestException exception)
                {
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show("Cannot open search in browser : \n" + exception.Message, "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                }
            }
        });
    }

    [RelayCommand]
    private static void SearchPoeprices(object commandParameter) => Vm.Logic.Task.UpdatePoePricesTab();

    [RelayCommand]
    private static void OpenNinja(object commandParameter) => Vm.Logic.Task.OpenNinjaTask();

    [RelayCommand]
    private static void OpenWiki(object commandParameter) => Vm.Logic.Task.OpenWikiTask();

    [RelayCommand]
    private static void OpenPoeDb(object commandParameter) => Vm.Logic.Task.OpenPoedbTask();

    [RelayCommand]
    private static void OpenDonateUrl(object commandParameter)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = Strings.UrlPaypalDonate, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(Resources.Resources.Main126_PaypalFail, "Redirection to paypal failed ", MessageStatus.Warning);
        }
    }

    [RelayCommand]
    private static void RefreshSearch(object commandParameter)
    {
        try
        {
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
            if (Vm.Form.Tab.QuickSelected || Vm.Form.Tab.DetailSelected)
            {
                Vm.Logic.Task.UpdateItemPrices(minimumStock: 1);
                return;
            }
            if (Vm.Form.Tab.BulkSelected)
            {
                if (Vm.Form.Bulk.Pay.CurrencyIndex > 0 && Vm.Form.Bulk.Get.CurrencyIndex > 0)
                {
                    if (!int.TryParse(Vm.Form.Bulk.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        Vm.Form.Bulk.Stock = "1";
                    }
                    //getMoreBulk.IsEnabled = true;
                    Vm.Form.Bulk.Get.ImageLast = Vm.Form.Bulk.Get.Image;
                    Vm.Form.Bulk.Pay.ImageLast = Vm.Form.Bulk.Pay.Image;
                    Vm.Form.Visible.BulkLastSearch = true;

                    Vm.Logic.Task.UpdateItemPrices(minimumStock, true);
                    Vm.Logic.Task.UpdateNinjaChaosEq();
                    return;
                }

                Vm.Result.Bulk.Price = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                Vm.Result.Bulk.PriceBis = string.Empty;
                return;
            }
            if (Vm.Form.Tab.ShopSelected)
            {
                if (Vm.Form.Shop.GetList.Count > 0 && Vm.Form.Shop.PayList.Count > 0)
                {
                    if (!int.TryParse(Vm.Form.Shop.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        Vm.Form.Shop.Stock = "1";
                    }
                    Vm.Logic.Task.UpdateItemPrices(minimumStock, true);
                    return;
                }

                Vm.Result.Shop.Price = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                Vm.Result.Shop.PriceBis = string.Empty;
            }
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Refreshing search error", MessageStatus.Error);
        }
    }

    [RelayCommand]
    private static void Fetch(object commandParameter)
    {
        Vm.Form.FetchDetailIsEnabled = false;
        Vm.Logic.Task.FetchDetailResults();
    }

    [RelayCommand]
    private static void InvertBulk(object commandParameter)
    {
        int idxCategory = Vm.Form.Bulk.Get.CategoryIndex;
        int idxCurrency = Vm.Form.Bulk.Get.CurrencyIndex;
        int idxTier = Vm.Form.Bulk.Get.TierIndex;

        Vm.Form.Bulk.Get.CategoryIndex = Vm.Form.Bulk.Pay.CategoryIndex;
        if (Vm.Form.Bulk.Get.CategoryIndex > 0)
        {
            if (Vm.Form.Bulk.Pay.TierIndex >= 0)
            {
                Vm.Form.Bulk.Get.TierIndex = Vm.Form.Bulk.Pay.TierIndex;
            }
            Vm.Form.Bulk.Get.CurrencyIndex = Vm.Form.Bulk.Pay.CurrencyIndex;
        }

        Vm.Form.Bulk.Pay.CategoryIndex = idxCategory;
        if (idxCategory > 0)
        {
            if (idxTier >= 0)
            {
                Vm.Form.Bulk.Pay.TierIndex = idxTier;
            }
            Vm.Form.Bulk.Pay.CurrencyIndex = idxCurrency;
        }
    }

    [RelayCommand]
    private static void SetModCurrent(object commandParameter) => Vm.Form.SetModCurrent();

    [RelayCommand]
    private static void SetModTier(object commandParameter) => Vm.Form.SetModTier();

    [RelayCommand]
    public static void CheckCondition(object commandParameter)
    {
        if (!Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.FreeSuffix && !Vm.Form.Condition.SocketColors)
        {
            Vm.Form.CheckComboCondition.Text = Resources.Resources.Main036_None;
            Vm.Form.CheckComboCondition.ToolTip = null;
            return;
        }

        bool prefixOnly = Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.FreeSuffix && !Vm.Form.Condition.SocketColors;
        bool suffixOnly = Vm.Form.Condition.FreeSuffix && !Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.SocketColors;
        bool colorsOnly = Vm.Form.Condition.SocketColors && !Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.FreeSuffix;
        if (prefixOnly)
        {
            Vm.Form.CheckComboCondition.Text = Vm.Form.Condition.FreePrefixText;
            Vm.Form.CheckComboCondition.ToolTip = Vm.Form.Condition.FreePrefixToolTip;
            return;
        }
        if (suffixOnly)
        {
            Vm.Form.CheckComboCondition.Text = Vm.Form.Condition.FreeSuffixText;
            Vm.Form.CheckComboCondition.ToolTip = Vm.Form.Condition.FreeSuffixToolTip;
            return;
        }
        if (colorsOnly)
        {
            Vm.Form.CheckComboCondition.Text = Vm.Form.Condition.SocketColorsText;
            Vm.Form.CheckComboCondition.ToolTip = Vm.Form.Condition.SocketColorsToolTip;
            return;
        }

        List<KeyValuePair<bool, string>> condList = new();
        condList.Add(new(Vm.Form.Condition.FreePrefix, Vm.Form.Condition.FreePrefixToolTip));
        condList.Add(new(Vm.Form.Condition.FreeSuffix, Vm.Form.Condition.FreeSuffixToolTip));
        condList.Add(new(Vm.Form.Condition.SocketColors, Vm.Form.Condition.SocketColorsToolTip));

        int nbCond = 0;
        StringBuilder toolTip = new();
        foreach (var cond in condList)
        {
            if (cond.Key)
            {
                nbCond++;
                if (toolTip.Length > 0)
                {
                    toolTip.AppendLine(); // "\n"
                }
                toolTip.Append(cond.Value);
            }
        }

        if (nbCond > 0)
        {
            Vm.Form.CheckComboCondition.Text = nbCond.ToString();
            Vm.Form.CheckComboCondition.ToolTip = toolTip.ToString();
        }
    }

    [RelayCommand]
    public static void CheckInfluence(object commandParameter)
    {
        string textVal = string.Empty;
        int checks = 0;
        foreach (KeyValuePair<string, bool> inf in Vm.Form.GetInfluenceSate())
        {
            if (inf.Value)
            {
                checks++;
                if (textVal.Length > 0) textVal += " & ";
                textVal += inf.Key;
            }
        }

        if (textVal.Length > 0)
        {
            Vm.Form.CheckComboInfluence.Text = checks == 1 ? textVal : checks.ToString();
            Vm.Form.CheckComboInfluence.ToolTip = textVal;
            return;
        }
        Vm.Form.CheckComboInfluence.Text = Resources.Resources.Main036_None;
        Vm.Form.CheckComboInfluence.ToolTip = null;
    }

    [RelayCommand]
    private static void Change(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            ExchangeViewModel exVm = @string.StartsWith("get", StringComparison.Ordinal) ? Vm.Form.Bulk.Get :
                @string.StartsWith("pay", StringComparison.Ordinal) ? Vm.Form.Bulk.Pay :
                @string.StartsWith("shop", StringComparison.Ordinal) ? Vm.Form.Shop.Exchange : null;
            if (exVm is null)
            {
                return;
            }

            if (exVm.CategoryIndex > 0 && exVm.CurrencyIndex > 0)
            {
                string tier = null;
                if (exVm.TierIndex > 0)
                {
                    tier = exVm.Tier[exVm.TierIndex].ToLowerInvariant().Replace("t", string.Empty);
                }
                exVm.Image = Common.GetCurrencyImageUri(exVm.Currency[exVm.CurrencyIndex], tier);
            }
            if (exVm.CurrencyIndex == 0)
            {
                exVm.Image = null;
            }
        }
    }

    [RelayCommand]
    private static void ResetBulkImage(object commandParameter)
    {
        _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        if (commandParameter is string str)
        {
            var exVm = str.StartsWith("get", StringComparison.Ordinal) ? Vm.Form.Bulk.Get :
                str.StartsWith("pay", StringComparison.Ordinal) ? Vm.Form.Bulk.Pay :
                str.StartsWith("shop", StringComparison.Ordinal) ? Vm.Form.Shop.Exchange : null;
            if (exVm is null)
            {
                return;
            }
            if (str.EndsWith("nothing", StringComparison.Ordinal))
            {
                exVm.CategoryIndex = 0;
                exVm.CurrencyIndex = 0;
                exVm.Search = string.Empty;

                return;
            }

            bool isChaos = str.EndsWith("chaos", StringComparison.Ordinal);
            bool isExalt = str.EndsWith("exalt", StringComparison.Ordinal);
            bool isDivine = str.EndsWith("divine", StringComparison.Ordinal);

            if (isChaos || isExalt || isDivine)
            {
                exVm.CategoryIndex = 1;

                var cur = from result in DataManager.Currencies
                          from Entrie in result.Entries
                          where ((isChaos && Entrie.Id is "chaos") 
                          || (isExalt && Entrie.Id is "exalted") 
                          || (isDivine && Entrie.Id is "divine"))
                          select (Entrie.Text);
                if (cur.Any())
                {
                    int idx = exVm.Currency.IndexOf(cur.First());
                    if (idx >= 0)
                    {
                        exVm.CurrencyIndex = idx;
                    }
                }
            }
        }
    }

    // TODO prefer LINQ over foreach loops & remove else instructions as much as possible
    [RelayCommand]
    private static void SelectBulk(object commandParameter)
    {
        if (commandParameter is not string @string || _blockSelectBulk)
        {
            return;
        }
        _blockSelectBulk = true;

        int idLang = DataManager.Config.Options.Language;
        
        bool isGet = @string.Contains("get", StringComparison.Ordinal);
        bool isPay = @string.Contains("pay", StringComparison.Ordinal);
        bool isShop = @string.Contains("shop", StringComparison.Ordinal);
        bool isTier = @string.Contains("tier", StringComparison.Ordinal);

        var exchange = isGet ? Vm.Form.Bulk.Get 
            : isPay ? Vm.Form.Bulk.Pay 
            : isShop ? Vm.Form.Shop.Exchange : null;
        if (exchange is null)
        {
            return;
        }
        exchange.Image = null;

        if (exchange.CategoryIndex > 0)
        {
            if (!isTier)
            {
                bool isMap = exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main056_Maps
                        || exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main179_UniqueMaps
                        || exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main217_BlightedMaps;
                bool isDiv = exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main055_Divination;

                exchange.TierVisible = isDiv || isMap;
                if (isDiv || isMap)
                {
                    if (isDiv)
                    {
                        exchange.Tier = new() { "T1", "T2", "T3", "T4" };
                        exchange.TierIndex = 0;
                    }
                    else if (isMap)
                    {
                        exchange.Tier = new()
                        {
                            "T1","T2","T3","T4","T5",
                            "T6","T7","T8","T9","T10",
                            "T11","T12","T13","T14","T15","T16","T17"
                        };
                        exchange.TierIndex = 0;
                    }
                }
            }

            AsyncObservableCollection<string> listBulk = new();
            listBulk.Add("------------------------------------------------");
            exchange.CurrencyIndex = 0;

            string selValue = exchange.Category[exchange.CategoryIndex];
            string searchKind = GetSearchKind(selValue);

            if (searchKind.Length > 0)
            {
                IEnumerable<CurrencyResultData> tmpCurr = searchKind is Strings.Delve ?
                    DataManager.Currencies.Where(x => x.Id.Contains(searchKind, StringComparison.Ordinal))
                    : DataManager.Currencies.Where(x => x.Id.Equals(searchKind, StringComparison.Ordinal));

                foreach (CurrencyResultData resultDat in tmpCurr)
                {
                    foreach (CurrencyEntrie entrieDat in resultDat.Entries)
                    {
                        if (entrieDat.Text.Length == 0 || entrieDat.Id is Strings.sep)
                        {
                            continue;
                        }
                        
                        bool addItem = false;

                        if (searchKind is Strings.CurrencyType.Maps or Strings.CurrencyType.MapsUnique or Strings.CurrencyType.MapsBlighted)
                        {
                            if (exchange.TierIndex >= 0)
                            {
                                string tier = Strings.tierPrefix + exchange.Tier[exchange.TierIndex].Replace("T", string.Empty);
                                
                                addItem = entrieDat.Id.EndsWith(tier,StringComparison.Ordinal);
                            }
                        }
                        else if (searchKind is Strings.CurrencyType.Cards)
                        {
                            DivTiersResult tmpDiv = DataManager.DivTiers.FirstOrDefault(x => x.Tag == entrieDat.Id);
                            if (tmpDiv is not null)
                            {
                                if (exchange.TierIndex >= 0)
                                {
                                    string tierVal = exchange.Tier[exchange.TierIndex].ToLowerInvariant().Replace("t", string.Empty);
                                    addItem = tierVal == tmpDiv.Tier;
                                }
                            }
                            else
                            {
                                if (!exchange.Tier.Contains(Resources.Resources.Main016_TierNothing))
                                {
                                    exchange.Tier.Add(Resources.Resources.Main016_TierNothing);
                                }

                                if (exchange.TierIndex >= 0)
                                {
                                    addItem = exchange.Tier[exchange.TierIndex].ToLowerInvariant() == Resources.Resources.Main016_TierNothing; // to check
                                }
                            }
                        }
                        else if (searchKind is Strings.CurrencyType.Currency)
                        {
                            bool is_mainCur = Strings.dicMainCur.TryGetValue(entrieDat.Id, out string curVal2);
                            bool is_exoticCur = Strings.dicExoticCur.TryGetValue(entrieDat.Id, out string curVal3);
                            addItem = selValue == Resources.Resources.Main044_MainCur ? is_mainCur && !is_exoticCur
                                : selValue == Resources.Resources.Main207_ExoticCurrency ? !is_mainCur && is_exoticCur
                                : selValue == Resources.Resources.Main045_OtherCur ? !is_mainCur && !is_exoticCur
                                : addItem;
                        }
                        else if (searchKind is Strings.CurrencyType.Fragments)
                        {
                            bool is_scarab = entrieDat.Id.Contains(Strings.scarab);
                            bool is_stone = Strings.dicStones.TryGetValue(entrieDat.Id, out string stoneVal);
                            addItem = selValue == Resources.Resources.Main047_Stones ? (is_stone && !is_scarab)
                                : selValue == Resources.Resources.Main046_MapFrag ? (!is_stone && !is_scarab)
                                : selValue == Resources.Resources.Main052_Scarabs ? (!is_stone && is_scarab)
                                : addItem;
                        }
                        else
                        {
                            addItem = true;
                        }

                        if (addItem) listBulk.Add(entrieDat.Text);
                    }
                }
            }
            exchange.Currency = listBulk;
            exchange.CurrencyVisible = true;
        }
        else
        {
            exchange.TierVisible = false;
            exchange.CurrencyVisible = false;
        }

        if (isGet)
        {
            Vm.Form.Bulk.Get = exchange;
        }
        else if (isPay)
        {
            Vm.Form.Bulk.Pay = exchange;
        }
        else if (isShop)
        {
            Vm.Form.Shop.Exchange = exchange;
        }

        _blockSelectBulk = false;
    }

    private static string GetSearchKind(string selValue)
    {
        return (selValue == Resources.Resources.Main044_MainCur
            || selValue == Resources.Resources.Main207_ExoticCurrency
            || selValue == Resources.Resources.Main045_OtherCur) ? Strings.CurrencyType.Currency :
            (selValue == Resources.Resources.Main046_MapFrag
            || selValue == Resources.Resources.Main047_Stones
            || selValue == Resources.Resources.Main052_Scarabs) ? Strings.CurrencyType.Fragments :
            selValue == Resources.Resources.Main208_MemoryLine ? Strings.CurrencyType.MemoryLine :
            selValue == Resources.Resources.Main186_Expedition ? Strings.CurrencyType.Expedition :
            selValue == Resources.Resources.Main048_Delirium ? Strings.CurrencyType.DeliriumOrbs :
            selValue == Resources.Resources.Main049_Catalysts ? Strings.CurrencyType.Catalysts :
            selValue == Resources.Resources.Main050_Oils ? Strings.CurrencyType.Oils :
            selValue == Resources.Resources.Main051_Incubators ? Strings.CurrencyType.Incubators :
            selValue == Resources.Resources.Main053_Fossils ? Strings.Delve :
            selValue == Resources.Resources.Main054_Essences ? Strings.CurrencyType.Essences :
            selValue == Resources.Resources.Main211_AncestorCurrency ? Strings.CurrencyType.Ancestor :
            selValue == Resources.Resources.Main212_Sanctum ? Strings.CurrencyType.Sanctum :
            selValue == Resources.Resources.Main198_ScoutingReports ? Strings.CurrencyType.ScoutingReport :
            selValue == Resources.Resources.Main055_Divination ? Strings.CurrencyType.Cards :
            selValue == Resources.Resources.Main200_SentinelCurrency ? Strings.CurrencyType.Sentinel :
            selValue == Resources.Resources.Main056_Maps ? Strings.CurrencyType.Maps :
            selValue == Resources.Resources.Main179_UniqueMaps ? Strings.CurrencyType.MapsUnique :
            selValue == Resources.Resources.Main216_BossMaps ? Strings.CurrencyType.MapsSpecial :
            selValue == Resources.Resources.Main217_BlightedMaps ? Strings.CurrencyType.MapsBlighted :
            selValue == Resources.Resources.Main218_Heist ? Strings.CurrencyType.Heist :
            selValue == Resources.Resources.Main219_Beasts ? Strings.CurrencyType.Beasts :
            selValue == Resources.Resources.General132_Rune ? Strings.CurrencyType.Runes :
            string.Empty;
    }

    [RelayCommand]
    private static void SearchCurrency(object commandParameter)
    {
        if (commandParameter is string strParam)
        {
            Vm.SearchCurrency(strParam);
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        }
    }

    [RelayCommand]
    private static void AddShopList(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            var shopList = @string.Contains("get", StringComparison.Ordinal) ? Vm.Form.Shop.GetList :
                @string.Contains("pay", StringComparison.Ordinal) ? Vm.Form.Shop.PayList : null;
            if (shopList is null)
            {
                return;
            }
            if (Vm.Form.Shop.Exchange.CategoryIndex > 0 && Vm.Form.Shop.Exchange.CurrencyIndex > 0)
            {
                string currency = Vm.Form.Shop.Exchange.Currency[Vm.Form.Shop.Exchange.CurrencyIndex];
                bool addItem = true;
                foreach (var item in shopList)
                {
                    if (item.Content == currency)
                    {
                        addItem = false;
                        break;
                    }
                }
                if (addItem)
                {
                    shopList.Add(new ListItemViewModel { Index = shopList.Count, Content = currency, FgColor = Strings.Color.Azure, ToolTip = Vm.Form.GetExchangeCurrencyTag(ExchangeType.Shop) });
                }
            }
        }
    }

    [RelayCommand]
    private static void ResetShopLists(object commandParameter)
    {
        Vm.Form.Shop.PayList.Clear();
        Vm.Form.Shop.GetList.Clear();
    }

    [RelayCommand]
    private static void InvertShopLists(object commandParameter)
    {
        var tempList = Vm.Form.Shop.PayList;
        Vm.Form.Shop.PayList = Vm.Form.Shop.GetList;
        Vm.Form.Shop.GetList = tempList;
    }

    [RelayCommand]
    private static void ClearFocus(object commandParameter) => _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();

    [RelayCommand]
    private static void SwitchTab(object commandParameter)
    {
        if (commandParameter is string tab)
        {
            if (tab is "quick" && Vm.Form.Tab.DetailEnable)
            {
                Vm.Form.Tab.DetailSelected = true;
            }
            if (tab is "detail")
            {
                if (Vm.Form.Tab.BulkEnable)
                {
                    Vm.Form.Tab.BulkSelected = true;
                    return;
                }
                Vm.Form.Tab.QuickSelected = true;
            }
            if (tab is "bulk")
            {
                Vm.Form.Tab.ShopSelected = true;
            }
            if (tab is "shop")
            {
                if (Vm.Form.Tab.QuickEnable)
                {
                    Vm.Form.Tab.QuickSelected = true;
                    return;
                }
                Vm.Form.Tab.BulkSelected = true;
            }
        }
    }

    [RelayCommand]
    public static void WheelIncrement(object commandParameter) => WheelAdjustValue(commandParameter, 1);

    [RelayCommand]
    public static void WheelIncrementTenth(object commandParameter) => WheelAdjustValue(commandParameter, 0.1);

    [RelayCommand]
    public static void WheelIncrementHundredth(object commandParameter) => WheelAdjustValue(commandParameter, 0.01);

    [RelayCommand]
    public static void WheelDecrement(object commandParameter) => WheelAdjustValue(commandParameter, -1);

    [RelayCommand]
    public static void WheelDecrementTenth(object commandParameter) => WheelAdjustValue(commandParameter, -0.1);

    [RelayCommand]
    public static void WheelDecrementHundredth(object commandParameter) => WheelAdjustValue(commandParameter, -0.01);

    private static void WheelAdjustValue(object param, double value) 
        => _serviceProvider.GetRequiredService<INavigationService>().UpdateControlValue(param, value);

    [RelayCommand]
    private static void SelectMod(object commandParameter)
        => _serviceProvider.GetRequiredService<INavigationService>().UpdateControlValue(commandParameter);

    [RelayCommand]
    private static void AutoClose(object commandParameter)
    {
        DataManager.Config.Options.Autoclose = Vm.Form.AutoClose;
    }

    [RelayCommand]
    private static void UpdateOpacity(object commandParameter)
    {
        if (DataManager.Instance is not null && DataManager.Config is not null)
        {
            Vm.Form.OpacityText = (Vm.Form.Opacity * 100) + "%";
            DataManager.Config.Options.Opacity = Vm.Form.Opacity;
        }
    }

    [RelayCommand]
    private static void ExpanderExpand(object commandParameter)
    {
        Vm.Form.Expander.Width = 214;
    }

    [RelayCommand]
    private static void ExpanderCollapse(object commandParameter)
    {
        Vm.Form.Expander.Width = 40;
        string configToSave = Json.Serialize<ConfigData>(DataManager.Config);
        DataManager.Save_Config(configToSave, "cfg");
    }

    [RelayCommand]
    private static void CheckAllMods(object commandParameter)
    {
        if (Vm.Form.ModLine.Count > 0)
        {
            foreach (ModLineViewModel mod in Vm.Form.ModLine)
            {
                mod.Selected = Vm.Form.AllCheck;
            }
        }
    }

    [RelayCommand]
    private static void SelectBulkIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Result.SelectedIndex.Bulk = idx;
        }
    }

    [RelayCommand]
    private static void ShowBulkWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = Vm.Result.BulkList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = Vm.Result.BulkOffers[idx];
            _serviceProvider.GetRequiredService<INavigationService>().ShowWhisperView(data);
        }
    }

    [RelayCommand]
    private static void SelectShopIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Result.SelectedIndex.Shop = idx;
        }
    }

    [RelayCommand]
    private static void ShowShopWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = Vm.Result.ShopList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = Vm.Result.ShopOffers[idx];
            _serviceProvider.GetRequiredService<INavigationService>().ShowWhisperView(data);
        }
    }

    [RelayCommand]
    private static void RemoveGetList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Form.Shop.GetList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in Vm.Form.Shop.GetList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    [RelayCommand]
    private static void RemovePayList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Form.Shop.PayList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in Vm.Form.Shop.PayList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    [RelayCommand]
    private static void UpdateMinimized(object commandParameter)
    {
        Vm.Form.Minimized = !Vm.Form.Minimized;
    }

    [RelayCommand]
    private static void WindowLoaded(object commandParameter)
    {
        _serviceProvider.GetRequiredService<INavigationService>().SetMainHandle(commandParameter);
    }

    [RelayCommand]
    private static void WindowClosed(object commandParameter)
    {
        //nothing
    }

    [RelayCommand]
    private static void WindowDeactivated(object commandParameter)
    {
        if (!Vm.Form.Tab.BulkSelected && !Vm.Form.Tab.ShopSelected
            && DataManager.Config.Options.Autoclose)
        {
            _serviceProvider.GetRequiredService<INavigationService>().CloseMainView();
        }
    }
}

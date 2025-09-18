using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.CoE.Domain;
using Xiletrade.Library.Models.DB.Domain;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Prices.Contract;
using Xiletrade.Library.Models.Wiki.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Exchange;

namespace Xiletrade.Library.ViewModels.Command;

public sealed partial class MainCommand : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    
    private static bool BlockSelectBulk { get; set; } = false;

    private readonly MainViewModel _vm;
    private readonly DataManagerService _dm;

    public MainCommand(MainViewModel vm, IServiceProvider serviceProvider)
    {
        _vm = vm;
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
    }

    [RelayCommand]
    private async Task OpenSearch(object commandParameter)
    {
        string market = _vm.Form.Market[_vm.Form.MarketIndex];
        string league = _vm.Form.League[_vm.Form.LeagueIndex];

        if (_vm.Form.Tab.BulkSelected)
        {
            await OpenBulkSearchTask(market, league);
            return;
        }
        if (_vm.Form.Tab.ShopSelected)
        {
            await OpenShopSearchTask(market, league);
            return;
        }
        if (_vm.Form.Tab.QuickSelected || _vm.Form.Tab.DetailSelected)
        {
            try
            {
                var sEntity = Json.GetSerialized(_dm, _vm.Form.GetXiletradeItem(), _vm.Item, false, market);
                if (sEntity?.Length > 0)
                {
                    await OpenSearchTask(sEntity, league);
                }
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "JSON serialization error", MessageStatus.Error);
            }
        }
    }

    private Task OpenBulkSearchTask(string market, string league)
    {
        return Task.Run(() =>
        {
            if (_vm.Form.Bulk.Pay.CurrencyIndex < 1 || _vm.Form.Bulk.Get.CurrencyIndex < 1)
            {
                return;
            }

            string[] exchange = new string[2];
            if (_vm.Form.Bulk.Pay.CurrencyIndex > 0)
            {
                var tmpBase = _dm.Bases.FirstOrDefault(y => y.Name == _vm.Form.Bulk.Pay.Currency[_vm.Form.Bulk.Pay.CurrencyIndex]);
                if (tmpBase is null)
                {
                    exchange[0] = _vm.Form.GetExchangeCurrencyTag(ExchangeType.Pay);
                }
            }
            if (_vm.Form.Bulk.Get.CurrencyIndex > 0)
            {
                var tmpBase = _dm.Bases.FirstOrDefault(y => y.Name == _vm.Form.Bulk.Get.Currency[_vm.Form.Bulk.Get.CurrencyIndex]);
                if (tmpBase is null)
                {
                    exchange[1] = _vm.Form.GetExchangeCurrencyTag(ExchangeType.Get);
                }
            }
            if (exchange[0] is null && exchange[1] is null)
            {
                return;
            }

            bool isInteger = int.TryParse(_vm.Form.Bulk.Stock, out int minimumStock);
            if (!isInteger)
            {
                minimumStock = 1;
                _vm.Form.Bulk.Stock = "1";
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
        });
    }

    private Task OpenShopSearchTask(string market, string league)
    {
        return Task.Run(() =>
        {
            var curGetList = from list in _vm.Form.Shop.GetList select list.ToolTip;
            var curPayList = from list in _vm.Form.Shop.PayList select list.ToolTip;
            if (curGetList.Any() && curPayList.Any())
            {
                bool isInteger = int.TryParse(_vm.Form.Shop.Stock, out int minimumStock);
                if (!isInteger)
                {
                    minimumStock = 1;
                    _vm.Form.Shop.Stock = "1";
                }

                Exchange change = new();
                change.ExchangeData.Status.Option = market;
                change.ExchangeData.Have = [.. curPayList];
                change.ExchangeData.Want = [.. curGetList];
                change.ExchangeData.Minimum = minimumStock;
                //change.ExchangeData.Collapse = true;
                change.Engine = "new";

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
        });
    }

    private static Task OpenSearchTask(string sEntity, string league)
    {
        return Task.Run(() =>
        {
            string result = string.Empty;
            try
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                result = service.SendHTTP(sEntity, Strings.TradeApi + league, Client.Trade).Result;
                if (result.Length > 0)
                {
                    var resultData = Json.Deserialize<ResultData>(result);
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
    private async Task SearchPoeprices(object commandParameter) => await SearchPoepricesTask();

    private Task SearchPoepricesTask()
    {
        return Task.Run(() =>
        {
            string errorMsg = string.Empty;
            List<Tuple<string, string>> lines = new();
            try
            {
                _vm.Result.PoepricesList.Clear();
                _vm.Result.PoepricesList.Add(new("Waiting response from poeprices.info ..." ));

                var net = _serviceProvider.GetRequiredService<NetService>();
                string result = net.SendHTTP(null, Strings.ApiPoePrice + _dm.Config.Options.League + "&i=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_vm.ClipboardText)), Client.PoePrice).Result;
                if (result is null || result.Length is 0)
                {
                    errorMsg = "Http request error : www.poeprices.info cannot respond, please try again later.";
                    return;
                }
                var jsonData = Json.Deserialize<PoePrices>(result);
                if (jsonData is null)
                {
                    errorMsg = "Json deserialize error : difference between Xiletrade and poeprices json format.";
                    return;
                }
                if (jsonData.Error is not 0)
                {
                    errorMsg = "Issue with Poeprices.info, error received: " + jsonData.ErrorMsg;
                    return;
                }

                lines.Add(new("Result from poeprices.info website :", string.Empty));

                _ = double.TryParse(jsonData.PredConfidenceScore.ToString(), out double score);
                lines.Add(new("Confidence score : " + string.Format("{0:0.00}", score) + "%", score >= 90 ? Strings.Color.LimeGreen : Strings.Color.Red));

                if (jsonData.Min is not 0.0)
                    lines.Add(new("Min price : " + string.Format("{0:0.0}", jsonData.Min) + " " + jsonData.Currency, Strings.Color.LimeGreen));
                if (jsonData.Max is not 0.0)
                    lines.Add(new("Max price : " + string.Format("{0:0.0}", jsonData.Max) + " " + jsonData.Currency, Strings.Color.LimeGreen));

                if (jsonData.PredExplantion is not null && jsonData.PredExplantion.Length > 0)
                {
                    lines.Add(new("Weight:    Mod: ", Strings.Color.LightGray));
                    foreach (Array items in jsonData.PredExplantion)
                    {
                        lines.Add(new("  " + string.Format("{0:0.00}", items.GetValue(1)) + "       " + items.GetValue(0), Strings.Color.LightGray));
                    }
                }
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                if (ex.InnerException is HttpRequestException exception)
                {
                    service.Show(ex.Message, "Poeprices error code : " + exception.StatusCode, MessageStatus.Information);
                    return;
                }
                service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "UTF8 Deserialize error", MessageStatus.Error);
            }
            finally
            {
                if (errorMsg.Length > 0)
                {
                    lines.Add(new(errorMsg, Strings.Color.Red));
                }

                _vm.Result.PoepricesList.Clear();
                foreach (var line in lines)
                {
                    _vm.Result.PoepricesList.Add(new(line.Item1, line.Item2 ));
                }
            }
        });
    }

    [RelayCommand]
    private void OpenNinja(object commandParameter) => _vm.OpenUrlTask(_vm.Ninja.GetFullUrl(), UrlType.Ninja);

    [RelayCommand]
    private void OpenWiki(object commandParameter)
    {
        var poeWiki = new PoeWiki(_dm, _vm.Item);
        _vm.OpenUrlTask(poeWiki.Link, UrlType.PoeWiki);
    }

    [RelayCommand]
    private void OpenPoeDb(object commandParameter)
    {
        var poeDb = new PoeDb(_dm, _vm.Item);
        _vm.OpenUrlTask(poeDb.Link, UrlType.PoeDb);
    }

    [RelayCommand]
    private void OpenCraftOfExile(object commandParameter)
    {
        var coe = new CraftOfExile(_vm.ClipboardText);
        _vm.OpenUrlTask(coe.Link, UrlType.CraftOfExile);
    }

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
    private void RefreshSearch(object commandParameter)
    {
        try
        {
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
            _vm.Result.InitData();
            if (_vm.Form.Tab.QuickSelected || _vm.Form.Tab.DetailSelected)
            {
                _vm.UpdatePrices(minimumStock: 1);
                return;
            }
            if (_vm.Form.Tab.BulkSelected)
            {
                if (_vm.Form.Bulk.Pay.CurrencyIndex > 0 && _vm.Form.Bulk.Get.CurrencyIndex > 0)
                {
                    if (!int.TryParse(_vm.Form.Bulk.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        _vm.Form.Bulk.Stock = "1";
                    }
                    _vm.Form.Bulk.Get.ImageLast = _vm.Form.Bulk.Get.Image;
                    _vm.Form.Bulk.Pay.ImageLast = _vm.Form.Bulk.Pay.Image;
                    _vm.Form.Visible.BulkLastSearch = true;

                    _vm.UpdatePrices(minimumStock, true);
                    if (!_vm.Form.IsPoeTwo)
                    {
                        UpdateBulkNinjaTask();
                    }
                    return;
                }

                _vm.Result.Bulk.RightString = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                _vm.Result.Bulk.LeftString = string.Empty;
                return;
            }
            if (_vm.Form.Tab.ShopSelected)
            {
                if (_vm.Form.Shop.GetList.Count > 0 && _vm.Form.Shop.PayList.Count > 0)
                {
                    if (!int.TryParse(_vm.Form.Shop.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        _vm.Form.Shop.Stock = "1";
                    }
                    _vm.UpdatePrices(minimumStock, true);
                    return;
                }

                _vm.Result.Shop.RightString = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                _vm.Result.Shop.LeftString = string.Empty;
            }
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Refreshing search error", MessageStatus.Error);
        }
    }

    [RelayCommand]
    private async Task Fetch(object commandParameter)
    {
        _vm.Form.FetchDetailIsEnabled = false;
        _vm.Result.Detail.Total = "Fetching new results...";
        var market = _vm.Form.Market[_vm.Form.MarketIndex];
        var sameUser = _vm.Form.SameUser;
        var token = _vm.TaskManager.GetPriceToken();

        ResultBar result = null;
        try
        {
            result = await Task.Run(() => _vm.Result.FetchWithApi(20, market, sameUser, token), token); // maxFetch is set to 20 by default !
        }
        catch (InvalidOperationException ex)
        {
            result = new(emptyLine: true);
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Invalid operation", MessageStatus.Error);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is HttpRequestException exception)
            {
                result = new(exception, false);
            }
        }
        _vm.Result.RefreshResultBar(false, result);
    }

    [RelayCommand]
    private void InvertBulk(object commandParameter)
    {
        int idxCategory = _vm.Form.Bulk.Get.CategoryIndex;
        int idxCurrency = _vm.Form.Bulk.Get.CurrencyIndex;
        int idxTier = _vm.Form.Bulk.Get.TierIndex;

        _vm.Form.Bulk.Get.CategoryIndex = _vm.Form.Bulk.Pay.CategoryIndex;
        if (_vm.Form.Bulk.Get.CategoryIndex > 0)
        {
            if (_vm.Form.Bulk.Pay.TierIndex >= 0)
            {
                _vm.Form.Bulk.Get.TierIndex = _vm.Form.Bulk.Pay.TierIndex;
            }
            _vm.Form.Bulk.Get.CurrencyIndex = _vm.Form.Bulk.Pay.CurrencyIndex;
        }

        _vm.Form.Bulk.Pay.CategoryIndex = idxCategory;
        if (idxCategory > 0)
        {
            if (idxTier >= 0)
            {
                _vm.Form.Bulk.Pay.TierIndex = idxTier;
            }
            _vm.Form.Bulk.Pay.CurrencyIndex = idxCurrency;
        }
    }

    [RelayCommand]
    private void SetModCurrent(object commandParameter) => _vm.Form.SetModCurrent();

    [RelayCommand]
    private void SetModTier(object commandParameter) => _vm.Form.SetModTier();

    [RelayCommand]
    public void CheckCondition(object commandParameter)
    {
        if (!_vm.Form.Condition.FreePrefix && !_vm.Form.Condition.FreeSuffix && !_vm.Form.Condition.SocketColors)
        {
            _vm.Form.CheckComboCondition.Text = Resources.Resources.Main036_None;
            _vm.Form.CheckComboCondition.ToolTip = null;
            return;
        }

        bool prefixOnly = _vm.Form.Condition.FreePrefix && !_vm.Form.Condition.FreeSuffix && !_vm.Form.Condition.SocketColors;
        bool suffixOnly = _vm.Form.Condition.FreeSuffix && !_vm.Form.Condition.FreePrefix && !_vm.Form.Condition.SocketColors;
        bool colorsOnly = _vm.Form.Condition.SocketColors && !_vm.Form.Condition.FreePrefix && !_vm.Form.Condition.FreeSuffix;
        if (prefixOnly)
        {
            _vm.Form.CheckComboCondition.Text = _vm.Form.Condition.FreePrefixText;
            _vm.Form.CheckComboCondition.ToolTip = _vm.Form.Condition.FreePrefixToolTip;
            return;
        }
        if (suffixOnly)
        {
            _vm.Form.CheckComboCondition.Text = _vm.Form.Condition.FreeSuffixText;
            _vm.Form.CheckComboCondition.ToolTip = _vm.Form.Condition.FreeSuffixToolTip;
            return;
        }
        if (colorsOnly)
        {
            _vm.Form.CheckComboCondition.Text = _vm.Form.Condition.SocketColorsText;
            _vm.Form.CheckComboCondition.ToolTip = _vm.Form.Condition.SocketColorsToolTip;
            return;
        }

        List<KeyValuePair<bool, string>> condList = new();
        condList.Add(new(_vm.Form.Condition.FreePrefix, _vm.Form.Condition.FreePrefixToolTip));
        condList.Add(new(_vm.Form.Condition.FreeSuffix, _vm.Form.Condition.FreeSuffixToolTip));
        condList.Add(new(_vm.Form.Condition.SocketColors, _vm.Form.Condition.SocketColorsToolTip));

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
            _vm.Form.CheckComboCondition.Text = nbCond.ToString();
            _vm.Form.CheckComboCondition.ToolTip = toolTip.ToString();
        }
    }

    [RelayCommand]
    public void CheckInfluence(object commandParameter)
    {
        
        string influences = _vm.Form.Influence.GetSate(" & ");
        int checks = influences.AsSpan().Count('&');
        if (influences.Length > 0)
        {
            _vm.Form.CheckComboInfluence.Text = checks is 0 ? influences : (checks + 1).ToString();
            _vm.Form.CheckComboInfluence.ToolTip = influences;
            return;
        }
        _vm.Form.CheckComboInfluence.Text = Resources.Resources.Main036_None;
        _vm.Form.CheckComboInfluence.ToolTip = null;
    }

    [RelayCommand]
    internal void Change(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            ExchangeViewModel exVm = @string.StartWith("get") ? _vm.Form.Bulk.Get :
                @string.StartWith("pay") ? _vm.Form.Bulk.Pay :
                @string.StartWith("shop") ? _vm.Form.Shop.Exchange : null;
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
                exVm.Image = Common.GetCurrencyImageUri(_dm, exVm.Currency[exVm.CurrencyIndex], tier);
            }
            if (exVm.CurrencyIndex is 0)
            {
                exVm.Image = null;
            }
        }
    }

    [RelayCommand]
    private void ResetBulkImage(object commandParameter)
    {
        _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        if (commandParameter is string str)
        {
            var exVm = str.StartWith("get") ? _vm.Form.Bulk.Get :
                str.StartWith("pay") ? _vm.Form.Bulk.Pay :
                str.StartWith("shop") ? _vm.Form.Shop.Exchange : null;
            if (exVm is null)
            {
                return;
            }
            if (str.EndWith("nothing"))
            {
                exVm.CategoryIndex = 0;
                exVm.CurrencyIndex = 0;
                exVm.Search = string.Empty;

                return;
            }

            bool isChaos = str.EndWith("chaos");
            bool isExalt = str.EndWith("exalt");
            bool isDivine = str.EndWith("divine");

            if (isChaos || isExalt || isDivine)
            {
                exVm.CategoryIndex = 1;
                var curText = _dm.Currencies.SelectMany(result => result.Entries)
                    .Where(e => (isChaos && e.Id is "chaos") || (isExalt && e.Id is "exalted") 
                        || (isDivine && e.Id is "divine"))
                    .Select(e => e.Text).FirstOrDefault();
                if (curText is not null)
                {
                    int idx = exVm.Currency.IndexOf(curText);
                    if (idx >= 0)
                    {
                        exVm.CurrencyIndex = idx;
                    }
                }
            }
        }
    }

    [RelayCommand]
    internal void SelectBulk(object commandParameter)
    {
        if (commandParameter is not string @string || BlockSelectBulk)
        {
            return;
        }
        BlockSelectBulk = true;

        int idLang = _dm.Config.Options.Language;
        
        bool isGet = @string.Contain("get");
        bool isPay = @string.Contain("pay");
        bool isShop = @string.Contain("shop");
        bool isTier = @string.Contain("tier");

        var exchange = isGet ? _vm.Form?.Bulk.Get 
            : isPay ? _vm.Form?.Bulk.Pay 
            : isShop ? _vm.Form?.Shop.Exchange : null;
        if (exchange is null)
        {
            return;
        }
        exchange.Image = null;

        if (exchange.CategoryIndex > 0)
        {
            if (!isTier && _dm.Config.Options.GameVersion is 0)
            {
                bool isMap = exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main056_Maps
                        || exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main179_UniqueMaps
                        || exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main217_BlightedMaps;
                bool isDiv = exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main055_Divination;

                exchange.TierVisible = isDiv || isMap;
                if (isDiv || isMap)
                {
                    exchange.Tier = isDiv ? new(Strings.BulkStrings.DivinationCardTier) : new(Strings.BulkStrings.MapTierPoe1);
                    exchange.TierIndex = 0;
                }
            }

            AsyncObservableCollection<string> listBulk = new();
            listBulk.Add(Strings.BulkStrings.Delimiter);
            exchange.CurrencyIndex = 0;

            string selValue = exchange.Category[exchange.CategoryIndex];
            string searchKind = GetSearchKind(selValue);

            if (searchKind.Length > 0)
            {
                var listSelect = searchKind is Strings.Delve ?
                    _dm.Currencies.Where(x => x.Id.Contain(searchKind))
                    : _dm.Currencies.Where(x => x.Id.Equal(searchKind));
                foreach (var resultData in listSelect)
                {
                    foreach (var currency in resultData.Entries)
                    {
                        if (currency.Text.Length is 0 || currency.Id is Strings.sep)
                        {
                            continue;
                        }

                        bool addItem = false;

                        if (searchKind is Strings.CurrencyTypePoe1.Maps or Strings.CurrencyTypePoe1.MapsUnique or Strings.CurrencyTypePoe1.MapsBlighted)
                        {
                            if (exchange.TierIndex >= 0)
                            {
                                string tier = Strings.tierPrefix + exchange.Tier[exchange.TierIndex].Replace("T", string.Empty);

                                addItem = currency.Id.EndWith(tier);
                            }
                        }
                        else if (searchKind is Strings.CurrencyTypePoe1.Cards)
                        {
                            var tmpDiv = _dm.DivTiers.FirstOrDefault(x => x.Tag == currency.Id);
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
                                    addItem = exchange.Tier[exchange.TierIndex].Equals(Resources.Resources.Main016_TierNothing, StringComparison.InvariantCultureIgnoreCase);
                                }
                            }
                        }
                        else if (searchKind is Strings.CurrencyTypePoe1.Currency)
                        {
                            bool is_mainCur = Strings.dicMainCur.TryGetValue(currency.Id, out string curVal2);
                            bool is_exoticCur = Strings.dicExoticCur.TryGetValue(currency.Id, out string curVal3);
                            addItem = selValue == Resources.Resources.Main044_MainCur ? is_mainCur && !is_exoticCur
                                : selValue == Resources.Resources.Main207_ExoticCurrency ? !is_mainCur && is_exoticCur
                                : selValue == Resources.Resources.Main045_OtherCur ? !is_mainCur && !is_exoticCur
                                : addItem;
                        }
                        else if (searchKind is Strings.CurrencyTypePoe1.Fragments)
                        {
                            bool is_scarab = currency.Id.Contain(Strings.scarab);
                            bool is_stone = Strings.dicStones.TryGetValue(currency.Id, out string stoneVal);
                            addItem = selValue == Resources.Resources.Main047_Stones ? (is_stone && !is_scarab)
                                : selValue == Resources.Resources.Main046_MapFrag ? (!is_stone && !is_scarab)
                                : selValue == Resources.Resources.Main052_Scarabs ? (!is_stone && is_scarab)
                                : addItem;
                        }
                        else
                        {
                            addItem = true;
                        }

                        if (addItem) listBulk.Add(currency.Text);
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
            _vm.Form.Bulk.Get = exchange;
        }
        else if (isPay)
        {
            _vm.Form.Bulk.Pay = exchange;
        }
        else if (isShop)
        {
            _vm.Form.Shop.Exchange = exchange;
        }

        BlockSelectBulk = false;
    }

    private Task UpdateBulkNinjaTask()
    {
        return Task.Run(() =>
        {
            try
            {
                _vm.TaskManager.NinjaTask?.Wait();

                string tipGet = _vm.Form.Bulk.Get.Currency[_vm.Form.Bulk.Get.CurrencyIndex];
                string tagGet = string.Empty;
                string tipPay = _vm.Form.Bulk.Pay.Currency[_vm.Form.Bulk.Pay.CurrencyIndex];
                string tagPay = string.Empty;

                if (_dm.Config.Options.Language is not 8 and not 9) // ! tw & ! cn
                {
                    string translatedGet = Common.TranslateCurrency(_dm, _vm.Form.Bulk.Get.Currency[_vm.Form.Bulk.Get.CurrencyIndex]);
                    if (translatedGet is Strings.ChaosOrb)
                    {
                        _vm.Result.Data.NinjaEq.ChaosGet = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (_vm.Form.Bulk.Get.Tier.Count > 0)
                        {
                            tier = _vm.Form.Bulk.Get.Tier[_vm.Form.Bulk.Get.TierIndex].ToLowerInvariant();
                        }

                        _vm.Result.Data.NinjaEq.ChaosGet = _vm.Ninja.GetChaosEq(_vm.Form.League[_vm.Form.LeagueIndex], translatedGet, tier);
                    }

                    if (_vm.Result.Data.NinjaEq.ChaosGet > 0 && translatedGet is not Strings.ChaosOrb)
                    {
                        tipGet = "1 " + _vm.Form.Bulk.Get.Currency[_vm.Form.Bulk.Get.CurrencyIndex] + " = " + _vm.Result.Data.NinjaEq.ChaosGet.ToString() + " chaos";
                        tagGet = "ninja";
                    }

                    string translatedPay = Common.TranslateCurrency(_dm, _vm.Form.Bulk.Pay.Currency[_vm.Form.Bulk.Pay.CurrencyIndex]);
                    if (translatedPay is Strings.ChaosOrb)
                    {
                        _vm.Result.Data.NinjaEq.ChaosPay = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (_vm.Form.Bulk.Pay.Tier.Count > 0)
                        {
                            tier = _vm.Form.Bulk.Pay.Tier[_vm.Form.Bulk.Pay.TierIndex].Replace("T", string.Empty);
                        }

                        _vm.Result.Data.NinjaEq.ChaosPay = _vm.Ninja.GetChaosEq(_vm.Form.League[_vm.Form.LeagueIndex], translatedPay, tier);
                    }

                    if (_vm.Result.Data.NinjaEq.ChaosPay > 0 && translatedPay is not Strings.ChaosOrb)
                    {
                        tipPay = "1 " + _vm.Form.Bulk.Pay.Currency[_vm.Form.Bulk.Pay.CurrencyIndex] + " = " + _vm.Result.Data.NinjaEq.ChaosPay.ToString() + " chaos";
                        tagPay = "ninja";
                    }
                }
                _vm.Form.Bulk.Get.ImageLastToolTip = tipGet;
                _vm.Form.Bulk.Get.ImageLastTag = tagGet;
                _vm.Form.Bulk.Pay.ImageLastToolTip = tipPay;
                _vm.Form.Bulk.Pay.ImageLastTag = tagPay;
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Exception encountered : getting chaos equivalent", MessageStatus.Error);
            }
        });
    }

    private string GetSearchKind(string selValue)
    {
        if (_dm.Config.Options.GameVersion is 1)
        {
            return (selValue == Resources.Resources.Main044_MainCur
            || selValue == Resources.Resources.Main045_OtherCur) ? Strings.CurrencyTypePoe2.Currency :
            selValue == Resources.Resources.Main046_MapFrag ? Strings.CurrencyTypePoe2.Fragments :
            selValue == Resources.Resources.General132_Rune ? Strings.CurrencyTypePoe2.Runes :
            selValue == Resources.Resources.Main054_Essences ? Strings.CurrencyTypePoe2.Essences :
            selValue == Resources.Resources.ItemClass_sanctumRelic ? Strings.CurrencyTypePoe2.Relics :
            selValue == Resources.Resources.General069_Ultimatum ? Strings.CurrencyTypePoe2.Ultimatum :
            selValue == Resources.Resources.Main049_Catalysts ? Strings.CurrencyTypePoe2.BreachCatalyst :
            selValue == Resources.Resources.Main186_Expedition ? Strings.CurrencyTypePoe2.Expedition :
            selValue == Resources.Resources.ItemClass_omen ? Strings.CurrencyTypePoe2.Ritual :
            selValue == Resources.Resources.Main236_Delirium ? Strings.CurrencyTypePoe2.Delirium :
            selValue == Resources.Resources.ItemClass_maps ? Strings.CurrencyTypePoe2.Waystones :
            selValue == Resources.Resources.Main229_Talismans ? Strings.CurrencyTypePoe2.Talismans :
            selValue == Resources.Resources.Main230_VaultKeys ? Strings.CurrencyTypePoe2.VaultKeys :
            selValue == Resources.Resources.Main235_AbyssalBones ? Strings.CurrencyTypePoe2.Abyss :
            selValue == Resources.Resources.Main237_UncutGems ? Strings.CurrencyTypePoe2.UncutGems :
            selValue == Resources.Resources.Main238_LineageGems ? Strings.CurrencyTypePoe2.LineageSupportGems :
            string.Empty;
        }

        return (selValue == Resources.Resources.Main044_MainCur
            || selValue == Resources.Resources.Main207_ExoticCurrency
            || selValue == Resources.Resources.Main045_OtherCur) ? Strings.CurrencyTypePoe1.Currency :
            (selValue == Resources.Resources.Main046_MapFrag
            || selValue == Resources.Resources.Main047_Stones
            || selValue == Resources.Resources.Main052_Scarabs) ? Strings.CurrencyTypePoe1.Fragments :
            selValue == Resources.Resources.Main186_Expedition ? Strings.CurrencyTypePoe1.Expedition :
            selValue == Resources.Resources.Main048_Delirium ? Strings.CurrencyTypePoe1.DeliriumOrbs :
            selValue == Resources.Resources.Main049_Catalysts ? Strings.CurrencyTypePoe1.Catalysts :
            selValue == Resources.Resources.Main050_Oils ? Strings.CurrencyTypePoe1.Oils :
            selValue == Resources.Resources.Main051_Incubators ? Strings.CurrencyTypePoe1.Incubators :
            selValue == Resources.Resources.Main053_Fossils ? Strings.Delve :
            selValue == Resources.Resources.Main054_Essences ? Strings.CurrencyTypePoe1.Essences :
            selValue == Resources.Resources.Main211_AncestorCurrency ? Strings.CurrencyTypePoe1.Ancestor :
            selValue == Resources.Resources.Main212_Sanctum ? Strings.CurrencyTypePoe1.Sanctum :
            selValue == Resources.Resources.Main198_ScoutingReports ? Strings.CurrencyTypePoe1.ScoutingReport :
            selValue == Resources.Resources.Main055_Divination ? Strings.CurrencyTypePoe1.Cards :
            selValue == Resources.Resources.Main200_SentinelCurrency ? Strings.CurrencyTypePoe1.Sentinel :
            selValue == Resources.Resources.Main056_Maps ? Strings.CurrencyTypePoe1.Maps :
            selValue == Resources.Resources.Main179_UniqueMaps ? Strings.CurrencyTypePoe1.MapsUnique :
            selValue == Resources.Resources.Main216_BossMaps ? Strings.CurrencyTypePoe1.MapsSpecial :
            selValue == Resources.Resources.Main217_BlightedMaps ? Strings.CurrencyTypePoe1.MapsBlighted :
            selValue == Resources.Resources.Main218_Heist ? Strings.CurrencyTypePoe1.Heist :
            selValue == Resources.Resources.Main219_Beasts ? Strings.CurrencyTypePoe1.Beasts :
            selValue == Resources.Resources.General132_Rune ? Strings.CurrencyTypePoe1.Runegrafts :
            selValue == Resources.Resources.ItemClass_allflame ? Strings.CurrencyTypePoe1.AllflameEmbers :
            string.Empty;
    }

    [RelayCommand]
    private async Task SearchCurrency(object commandParameter)
    {
        if (commandParameter is string strParam)
        {
            var exVm = strParam is "get" ? _vm.Form.Bulk.Get :
            strParam is "pay" ? _vm.Form.Bulk.Pay :
            strParam is "shop" ? _vm.Form.Shop.Exchange : null;
            if (exVm is not null)
            {
                if (exVm.Search.Length >= 1)
                {
                    await _vm.Form.SelectExchangeCurrency(strParam + "/contains", exVm.Search);
                }
                else
                {
                    exVm.CategoryIndex = 0;
                    exVm.CurrencyIndex = 0;
                }
            }

            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        }
    }

    [RelayCommand]
    private void AddShopList(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            var shopList = @string.Contain("get") ? _vm.Form.Shop.GetList :
                @string.Contain("pay") ? _vm.Form.Shop.PayList : null;
            if (shopList is null)
            {
                return;
            }
            if (_vm.Form.Shop.Exchange.CategoryIndex > 0 && _vm.Form.Shop.Exchange.CurrencyIndex > 0)
            {
                string currency = _vm.Form.Shop.Exchange.Currency[_vm.Form.Shop.Exchange.CurrencyIndex];
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
                    shopList.Add(new(shopList.Count, currency, _vm.Form.GetExchangeCurrencyTag(ExchangeType.Shop), Strings.Color.Azure));
                }
            }
        }
    }

    [RelayCommand]
    private void ResetShopLists(object commandParameter)
    {
        _vm.Form.Shop.PayList.Clear();
        _vm.Form.Shop.GetList.Clear();
    }

    [RelayCommand]
    private void InvertShopLists(object commandParameter)
    {
        var tempList = _vm.Form.Shop.PayList;
        _vm.Form.Shop.PayList = _vm.Form.Shop.GetList;
        _vm.Form.Shop.GetList = tempList;
    }

    [RelayCommand]
    private static void ClearFocus(object commandParameter) => _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();

    [RelayCommand]
    private void SwitchTab(object commandParameter)
    {
        if (commandParameter is string tab)
        {
            if (tab is "quick" && _vm.Form.Tab.DetailEnable)
            {
                _vm.Form.Tab.DetailSelected = true;
            }
            if (tab is "detail")
            {
                if (_vm.Form.Tab.BulkEnable)
                {
                    _vm.Form.Tab.BulkSelected = true;
                    return;
                }
                _vm.Form.Tab.QuickSelected = true;
            }
            if (tab is "bulk")
            {
                _vm.Form.Tab.ShopSelected = true;
            }
            if (tab is "shop")
            {
                if (_vm.Form.Tab.QuickEnable)
                {
                    _vm.Form.Tab.QuickSelected = true;
                    return;
                }
                _vm.Form.Tab.BulkSelected = true;
            }
        }
    }

    [RelayCommand]
    public static void WheelIncrement(object commandParameter) 
        => WheelAdjustValue(commandParameter, 1);

    [RelayCommand]
    public static void WheelIncrementTenth(object commandParameter) 
        => WheelAdjustValue(commandParameter, 0.1);

    [RelayCommand]
    public static void WheelIncrementHundredth(object commandParameter) 
        => WheelAdjustValue(commandParameter, 0.01);

    [RelayCommand]
    public static void WheelDecrement(object commandParameter) 
        => WheelAdjustValue(commandParameter, -1);

    [RelayCommand]
    public static void WheelDecrementTenth(object commandParameter) 
        => WheelAdjustValue(commandParameter, -0.1);

    [RelayCommand]
    public static void WheelDecrementHundredth(object commandParameter) 
        => WheelAdjustValue(commandParameter, -0.01);

    private static void WheelAdjustValue(object param, double value) 
        => _serviceProvider.GetRequiredService<INavigationService>().UpdateControlValue(param, value);

    [RelayCommand]
    private static void SelectMod(object commandParameter)
        => _serviceProvider.GetRequiredService<INavigationService>().UpdateControlValue(commandParameter);

    [RelayCommand]
    private void AutoClose(object commandParameter)
    {
        _dm.Config.Options.Autoclose = _vm.Form.AutoClose;
    }

    [RelayCommand]
    private void UpdateOpacity(object commandParameter)
    {
        if (_dm.Config is not null)
        {
            _dm.Config.Options.Opacity = _vm.Form.Opacity;
        }
    }

    [RelayCommand]
    private void ExpanderCollapse(object commandParameter)
    {
        var configToSave = Json.Serialize<ConfigData>(_dm.Config);
        _dm.SaveConfiguration(configToSave);
    }

    [RelayCommand]
    private void CheckAllMods(object commandParameter)
    {
        if (_vm.Form.ModList.Count is 0)
        {
            return;
        }
        foreach (var mod in _vm.Form.ModList)
        {
            mod.Selected = _vm.Form.AllCheck;
        }
    }

    [RelayCommand]
    private void ShowMinMaxMods(object commandParameter)
    {
        if (_vm.Form.ModList.Count is 0)
        {
            return;
        }
        foreach (var mod in _vm.Form.ModList)
        {
            if (mod.Min.Length > 0)
            {
                mod.PreferMinMax = _vm.ShowMinMax;
            }
        }
    }

    [RelayCommand]
    private void SelectBulkIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            _vm.Result.SelectedIndex.Bulk = idx;
        }
    }

    [RelayCommand]
    private void ShowBulkWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = _vm.Result.BulkList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = _vm.Result.BulkOffers[idx];
            _serviceProvider.GetRequiredService<INavigationService>().ShowWhisperView(data);
        }
    }

    [RelayCommand]
    private void SelectShopIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            _vm.Result.SelectedIndex.Shop = idx;
        }
    }

    [RelayCommand]
    private void ShowShopWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = _vm.Result.ShopList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = _vm.Result.ShopOffers[idx];
            _serviceProvider.GetRequiredService<INavigationService>().ShowWhisperView(data);
        }
    }

    [RelayCommand]
    private void RemoveGetList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            _vm.Form.Shop.GetList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in _vm.Form.Shop.GetList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    [RelayCommand]
    private void RemovePayList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            _vm.Form.Shop.PayList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in _vm.Form.Shop.PayList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    [RelayCommand]
    private void UpdateMinimized(object commandParameter)
    {
        _vm.Form.Minimized = !_vm.Form.Minimized;
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
    private void WindowDeactivated(object commandParameter)
    {
        if (_vm.Form is not null && !_vm.Form.Tab.BulkSelected && !_vm.Form.Tab.ShopSelected
            && _dm.Config.Options.Autoclose)
        {
            _serviceProvider.GetRequiredService<INavigationService>().CloseMainView();
        }
    }

    [RelayCommand]
    private static void WindowActivated(object commandParameter)
    {
        //nothing
    }
}

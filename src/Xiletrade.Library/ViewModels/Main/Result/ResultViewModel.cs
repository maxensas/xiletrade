using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Prices.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Result;

// ICollection
public sealed partial class ResultViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly MainViewModel _vm;
    private readonly DataManagerService _dm;

    //private bool IsPoe2 => _dm.Config.Options.GameVersion is 1;

    private int language = -1;
    private string _bulkFormat;
    private string _shopFormat;
    private string _shopAccountFormat;
    internal string BulkFormat { get { InitFormat(); return _bulkFormat; } }
    internal string ShopFormat { get { InitFormat(); return _shopFormat; } }
    internal string ShopAccountFormat { get { InitFormat(); return _shopAccountFormat; } }

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> detailList = new();

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> bulkList = new();

    [ObservableProperty]
    private AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> bulkOffers = new();

    [ObservableProperty]
    private AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> shopOffers = new();

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> poepricesList = new();

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> shopList = new();

    [ObservableProperty]
    private ResultBarViewModel quick = new(price: string.Empty, total: string.Empty);

    [ObservableProperty]
    private ResultBarViewModel detail = new(price: string.Empty, total: string.Empty);

    [ObservableProperty]
    private ResultBarViewModel bulk = new(price: Resources.Resources.Main001_PriceSelect, total: string.Empty);

    [ObservableProperty]
    private ResultBarViewModel shop = new(price: Resources.Resources.Main001_PriceSelect, total: string.Empty);
    /*
    [ObservableProperty]
    private ResultBarViewModel custom = new(price: string.Empty, total: string.Empty);
    */
    [ObservableProperty]
    private ResultRateViewModel rate = new();

    [ObservableProperty]
    private ResultListIndexViewModel selectedIndex = new();

    // model
    internal PricingData Data { get; private set; } = new();

    public ResultViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _vm = _serviceProvider.GetRequiredService<MainViewModel>();
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
    }

    [RelayCommand]
    private async Task SearchPoeprices(object commandParameter)
    {
        string errorMsg = string.Empty;
        List<Tuple<string, string>> lines = new();
        try
        {
            PoepricesList.Clear();
            PoepricesList.Add(new("Waiting response from poeprices.info ..."));

            var net = _serviceProvider.GetRequiredService<NetService>();
            string result = await net.SendHTTP(Strings.ApiPoePrice + _dm.Config.Options.League
                + "&i=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_vm.ClipboardText)), Client.PoePrice);
            if (result is null || result.Length is 0)
            {
                errorMsg = "Http request error : www.poeprices.info cannot respond, please try again later.";
                return;
            }
            var jsonData = _dm.Json.Deserialize<PoePrices>(result);
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

            var score = jsonData.PredConfidenceScore.Score;
            lines.Add(new("Confidence score : " + string.Format("{0:0.00}", score) + "%", score >= 90 ? Strings.Color.LimeGreen : Strings.Color.Red));

            if (jsonData.Min is not 0.0)
                lines.Add(new("Min price : " + string.Format("{0:0.0}", jsonData.Min) + " " + jsonData.Currency, Strings.Color.LimeGreen));
            if (jsonData.Max is not 0.0)
                lines.Add(new("Max price : " + string.Format("{0:0.0}", jsonData.Max) + " " + jsonData.Currency, Strings.Color.LimeGreen));

            if (jsonData.PredExplantion is not null && jsonData.PredExplantion.Length > 0)
            {
                lines.Add(new("Weight:   Mod: ", Strings.Color.LightGray));
                foreach (Array items in jsonData.PredExplantion)
                {
                    double.TryParse(items.GetValue(1).ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double weight);
                    lines.Add(new(string.Format("{0:0.00}", weight) + "     " + items.GetValue(0), Strings.Color.LightGray));
                }
            }
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            if (ex.InnerException is HttpRequestException exception)
            {
                ms.Show(ex.GetFormated(), "Poeprices error code : " + exception.StatusCode, MessageStatus.Information);
                return;
            }
            ms.Show(ex.GetFormated(), "UTF8 Deserialize error", MessageStatus.Error);
        }
        finally
        {
            if (errorMsg.Length > 0)
            {
                lines.Add(new(errorMsg, Strings.Color.Red));
            }

            PoepricesList.Clear();
            foreach (var line in lines)
            {
                PoepricesList.Add(new(line.Item1, line.Item2));
            }
        }
    }

    [RelayCommand]
    private static async Task TravelToHideout(object commandParameter)
    {
        if (commandParameter is null)
        {
            return;
        }

        if (commandParameter is SaleInfo saleInfo)
        {
            if (saleInfo.HideoutToken is null || saleInfo.HideoutToken.Length is 0)
            {
                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                ms.Show("Cannot travel to hideout : " + "\n\nYour POESESSID is missing or expired !" +
                    "\n\nFor advanced users : You can manually update your POESESSID " +
                    "under settings by using the developer manager in the authentication section.",
                    "This feature requires authentication", MessageStatus.Exclamation);
                return;
            }

            try
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                var sEntity = $"{{\"token\":\"{saleInfo.HideoutToken}\"}}";
                //var urlRef = Strings.TradeUrl + _vm.Form.League[_vm.Form.LeagueIndex] + "/" + _vm.Result.Data.ResultData.Id;
                var result = await service.SendHTTP(sEntity, Strings.WhisperApi, Client.Trade, isXml: true);
                if (result.Length > 0)
                {
                    //{"success":true}
                }
            }
            catch (Exception ex)
            {
                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                if (ex is HttpRequestException exception)
                {
                    if (exception.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        return; // can be normal
                    }
                    if (exception.StatusCode is System.Net.HttpStatusCode.BadRequest)
                    {
                        ms.Show("Cannot travel to hideout :" + "\n\nIs your account correctly connected ?"
                        , "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                        return;
                    }
                    if (exception.StatusCode is System.Net.HttpStatusCode.Forbidden)
                    {
                        ms.Show("Cannot travel to hideout.", "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                        return;
                    }
                    ms.Show("Cannot travel to hideout : " + "\n\nYour POESESSID is probably missing or expired !" +
                        "\n\nYou can manually update it under settings by using the developer manager in the authentication section.",
                        "ERROR Code : " + exception.StatusCode, MessageStatus.Error);

                }
                ms.Show("Cannot travel to hideout :\n\n" +
                    string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace),
                    "Unknown error encountered", MessageStatus.Error);
            }
        }
    }

    // internal methods
    internal void InitData()
    {
        Data = new();
        Rate.ShowMin = false;
    }

    internal void ClearLists()
    {
        DetailList.Clear();
        BulkList.Clear();
        BulkOffers.Clear();
        ShopOffers.Clear();
        PoepricesList.Clear();
        ShopList.Clear();
    }
    
    internal void UpdateWithPoeApi(PricingInfo pricingInfo)
    {
        string urlApi = string.Empty;
        string sEntity = null;
        try
        {
            if (pricingInfo.IsTradeEntity)
            {
                sEntity = pricingInfo.TradeEntity;
                urlApi = Strings.TradeApi;
                Data.StatDetail = new();
            }
            else if (pricingInfo.IsExchangeEntity)
            {
                var change = new Models.Poe.Contract.Exchange();
                change.ExchangeData.Status.Option = pricingInfo.Market;
                change.ExchangeData.Minimum = pricingInfo.MinimumStock;
                change.Engine = "new";
                change.ExchangeData.Have = pricingInfo.ExchangeHave;
                change.ExchangeData.Want = pricingInfo.ExchangeWant;

                sEntity = _dm.Json.Serialize<Models.Poe.Contract.Exchange>(change);
                urlApi = Strings.ExchangeApi;
                Data.StatBulk = new();
            }
            if (sEntity is null || sEntity.Length is 0)
            {
                return;
            }
            var token = _vm.TaskManager.GetPriceToken(initCts: true);
            _vm.TaskManager.PriceTask = RunPriceTask(pricingInfo, sEntity, urlApi, token);
        }
        catch(Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Error encountered while serializing Exchange object...", MessageStatus.Error);
        }
    }

    internal void UpdateResultWithPoeApi(int minimumStock)
    {
        try
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();

            int maxFetch = 0;
            var entity = new List<string>[2];

            _vm.Form.FetchDetailIsEnabled = false;

            if (_vm.Form.Tab.QuickSelected || _vm.Form.Tab.DetailSelected)
            {
                Detail.Total = Resources.Resources.Main005_PriceResearch;
                Quick.RightString = Detail.RightString = Resources.Resources.Main006_PriceCheck;
                Quick.LeftString = Detail.LeftString = string.Empty;
                Quick.Total = string.Empty;
                DetailList.Clear();

                maxFetch = (int)dm.Config.Options.SearchFetchDetail;
            }
            else if (_vm.Form.Tab.BulkSelected)
            {
                Bulk.RightString = Resources.Resources.Main003_PriceFetching;
                Bulk.Total = Resources.Resources.Main005_PriceResearch;
                Bulk.LeftString = string.Empty;
                BulkList.Clear();
                BulkOffers.Clear();

                if (_vm.Form.ItemExchange.Bulk.Pay.CurrencyIndex > 0 
                    && _vm.Form.ItemExchange.Bulk.Get.CurrencyIndex > 0)
                {
                    entity[0] = new() { _vm.Form.ItemExchange.GetExchangeCurrencyTag(ExchangeType.Pay) };
                    entity[1] = new() { _vm.Form.ItemExchange.GetExchangeCurrencyTag(ExchangeType.Get) };
                    maxFetch = (int)dm.Config.Options.SearchFetchBulk;
                }
            }
            else if (_vm.Form.Tab.ShopSelected)
            {
                Shop.RightString = Resources.Resources.Main003_PriceFetching;
                Shop.Total = Resources.Resources.Main005_PriceResearch;
                Shop.LeftString = string.Empty;
                ShopList.Clear();
                ShopOffers.Clear();

                var curGetList = from list in _vm.Form.ItemExchange.Shop.GetList select list.ToolTip;
                var curPayList = from list in _vm.Form.ItemExchange.Shop.PayList select list.ToolTip;
                if (!curGetList.Any() || !curPayList.Any())
                {
                    return;
                }
                entity[0] = [.. curPayList];
                entity[1] = [.. curGetList];
            }

            if (entity[0] is null)
            {
                entity[0] = new() { _vm.GetSerialized(_vm.Form.Market[_vm.Form.MarketIndex], useSaleType: true) };
            }
            var isExchange = _vm.Item is not null && _vm.Item.State.ExchangeCurrency; // quick or detail
            var usePoeApi = _vm.Form.Tab.BulkSelected || _vm.Form.Tab.ShopSelected || !isExchange;
            if (usePoeApi)
            {
                var priceInfo = new PricingInfo(entity, _vm.Form.League[_vm.Form.LeagueIndex]
                    , _vm.Form.Market[_vm.Form.MarketIndex], minimumStock, maxFetch, _vm.Form.SameUser, _vm.Form.Tab.BulkSelected);
                UpdateWithPoeApi(priceInfo);
                return;
            }
            RefreshResultBar(false, new(state: ResultBarSate.Unimplemented));
        }
        catch (Exception ex)
        {
            throw new Exception("Exception encountered : method UpdateItemPrices", ex);
        }
    }

    internal async Task<ResultBar> FetchWithApi(int maxFetch, string market, bool hideSameUser, CancellationToken token)
    {
        CurrencyFetch currencys = new();
        try
        {
            int fetchNbMax = 10; // previously 5
            int beginFetch = Data.StatDetail.Begin;
            var dataToFetch = Data.ResultData;
            int resultCount = dataToFetch.Result.Length;
            if (dataToFetch.Result.Length > 0)
            {
                int nbFetch = 0;
                while (nbFetch < maxFetch)
                {
                    token.ThrowIfCancellationRequested();
                    var data = new string[fetchNbMax];
                    if (beginFetch >= dataToFetch.Result.Length)
                        break;

                    for (int i = 0; i < fetchNbMax; i++)
                    {
                        if (beginFetch >= dataToFetch.Result.Length)
                            break;

                        data[i] = dataToFetch.Result[beginFetch];
                        nbFetch++;
                        beginFetch++;
                    }

                    string url = Strings.FetchApi + string.Join(",", data) + "?query=" + dataToFetch.Id;

                    _serviceProvider.GetRequiredService<PoeApiService>().ApplyCooldown();
                    var service = _serviceProvider.GetRequiredService<NetService>();
                    string sResult = await service.SendHTTP(url, Client.Trade); // use cooldown
#if DEBUG
                    var logger = _serviceProvider.GetRequiredService<ILogger<ResultViewModel>>();
                    logger.LogInformation("Recovered result from Fetch API..");
#endif
                    if (sResult.Length > 0)
                    {
                        currencys.Add(FillDetailVm(hideSameUser, sResult, token));
#if DEBUG
                        logger.LogInformation("Data fetched into vm...");
#endif
                    }
                }
            }

            Data.StatDetail.Begin = beginFetch;
            Data.StatDetail.ResultCount = resultCount;
            Data.StatDetail.Unpriced += currencys.Unpriced;
            Data.StatDetail.Total += currencys.Total;

            if (dataToFetch.Total is 0 || currencys.ListCur.Count is 0)
            {
                return new(state: ResultBarSate.NoResult); // useSecondLine: false
            }
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException or OperationCanceledException)
            {
                return new(ex, abort: true);
            }
            bool abort = ex.Message.ToLowerInvariant().Contain("thread");
            if (abort || ex is ThreadAbortException 
                or HttpRequestException or TimeoutException or JsonException)
            {
                return new(ex, abort);
            }
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "FetchResults() : Error encountered while fetching data...", MessageStatus.Error);
            return new(state: ResultBarSate.NoResult); // added
        }
        return new(_dm, currencys.ListCur, _vm.Form.Tab.QuickSelected);
    }

    internal void RefreshResultBar(bool exchange, ResultBar result)
    {
        if (UpdateResultBar(exchange, result))
        {
            return;
        }
        
        int removed = Data.StatDetail.Total - Data.StatDetail.ResultLoaded;
        int unpriced = Data.StatDetail.Unpriced;

        if (Data.ResultBar is null)
        {
            Data.ResultBar = result;
        }
        else
        {
            Data.ResultBar.Update(_dm, result);
        }

        Rate.ShowMin = Data.ResultBar.IsFetched;
        if (Data.ResultBar.IsFetched)
        {
            Rate.MinAmount = Data.ResultBar.Min.Amount.ToString();
            Rate.MinCurrency = Data.ResultBar.Min.Label;
            Rate.MinImage = Data.ResultBar.Min.Uri;
        }

        Rate.ShowMax = Data.ResultBar.IsMany;
        if (Data.ResultBar.IsMany)
        {
            Rate.MaxAmount = Data.ResultBar.Max.Amount.ToString();
            Rate.MaxCurrency = Data.ResultBar.Max.Label;
            Rate.MaxImage = Data.ResultBar.Max.Uri;
        }

        Quick.RightString = Detail.RightString = Data.ResultBar.FirstLine;
        Quick.LeftString = Data.ResultBar.SecondLine;

        if (Data.StatDetail.Begin > 0)
        {
            Detail.LeftString = Resources.Resources.Main017_Results + " : " + (Data.StatDetail.Begin - (removed + unpriced))
                + " " + Resources.Resources.Main018_ResultsDisplay + " / " + Data.StatDetail.Begin + " " + Resources.Resources.Main019_ResultsFetched;
            bool isRemoved = removed > 0;
            bool isUnpriced = unpriced > 0;
            if (isRemoved || isUnpriced)
            {
                Detail.LeftString += Strings.LF + Resources.Resources.Main010_PriceProcessed + " : ";
                if (isRemoved)
                {
                    Detail.LeftString += removed + " " + Resources.Resources.Main025_ResultsAgregate;
                    if (unpriced > 0) Detail.LeftString += Strings.LF + "          ";
                }
                if (isUnpriced)
                {
                    Detail.LeftString += unpriced + " " + Resources.Resources.Main026_ResultsUnpriced;
                }
            }
            if (Data.StatDetail.Begin < Data.StatDetail.ResultCount)
            {
                _vm.Form.FetchDetailIsEnabled = true;
            }
        }
        else
        {
            Detail.LeftString = string.Empty;
        }

        Detail.Total = Data.ResultData is null ? "ERROR : Can not retreive data from official website !" : string.Empty;
        
        if (Data.ResultData is not null)
        {
            Detail.LeftString += Strings.LF + Resources.Resources.Main027_ResultsTotal + " : " 
                + Data.StatDetail.ResultCount + " " + Resources.Resources.Main020_ResultsListed
                + " / " + Data.ResultData.Total + " " + Resources.Resources.Main021_ResultsMatch;
        }

        Quick.Total = Data.StatDetail.Total > 0
            /*&& !Quick.Total.Contain(Resources.Resources.Main011_PriceBase)*/ ?
            Resources.Resources.Main011_PriceBase + " " + (Data.StatDetail.Begin - (removed + unpriced)) + " "
            + Resources.Resources.Main017_Results.ToLowerInvariant() : string.Empty;
    }

    // private methods
    private void InitFormat()
    {
        if (_dm.Config.Options.Language != language)
        {
            //_bulkFormat = "{0,5} {1,-1} {2,5} {3}   " + Resources.Resources.Main014_ListStock + ": {4,-8} " + Resources.Resources.Main013_ListName + ": {5}";
            _bulkFormat = "{0,5} {1,-1} {2,5} {3}   " + Resources.Resources.Main014_ListStock + ": {4,-8} {5}";
            _shopFormat = Resources.Resources.Main014_ListStock + " : {0,-8} {1,20} {2,-4} ⇐ {3,4} {4}";
            _shopAccountFormat = Resources.Resources.Main206_tabItemShop + "  : {0} ({1})";
            language = _dm.Config.Options.Language;
        }
    }

    private async Task RunPriceTask(PricingInfo pricingInfo, string sEntity, string urlApi, CancellationToken token)
    {
        ResultBar result = null;
        try
        {
            _serviceProvider.GetRequiredService<PoeApiService>().ApplyCooldown();
            var netService = _serviceProvider.GetRequiredService<NetService>();
            var sResult = await netService.SendHTTP(sEntity, urlApi + pricingInfo.League, Client.Trade); // use cooldown

            token.ThrowIfCancellationRequested();
            if (sResult.Length > 0)
            {
                if (sResult.Contain("total\":false"))
                {
                    result = new(state: ResultBarSate.BadLeague);
                    return;
                }
                if (sResult.Contain("total\":0"))
                {
                    result = new(state: ResultBarSate.NoResult);
                    return;
                }

                if (pricingInfo.IsExchangeEntity)
                {
                    _serviceProvider.GetRequiredService<PoeApiService>().ApplyCooldown();
                    var bulkData = _dm.Json.Deserialize<BulkData>(sResult);
                    result = pricingInfo.IsSimpleBulk ? FillBulkVm(bulkData, pricingInfo) : FillShopVm(bulkData);
                    return;
                }
                Data.ResultData = _dm.Json.Deserialize<ResultData>(sResult);
                result = await FetchWithApi(pricingInfo.MaximumFetch, pricingInfo.Market, pricingInfo.HideSameUser, token);
                return;
            }
            result = new(state: ResultBarSate.NoData);
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException or OperationCanceledException)
            {
                result = new(ex, abort: true);
                return;
            }
            if (ex is HttpRequestException or TimeoutException or JsonException)
            {
                result = new(ex, false);
                return;
            }

            result = new(emptyLine: true);
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Error encountered while updating price...", MessageStatus.Error);
        }
        finally
        {
            RefreshResultBar(pricingInfo.IsExchangeEntity, result);
        }
    }

    private CurrencyFetch FillDetailVm(bool hideSameUser, ReadOnlySpan<char> sResult, CancellationToken token)
    {
        var cur = new CurrencyFetch();
        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        var fetchData = _dm.Json.Deserialize<FetchData>(sResult, replace: true);
        for (int i = 0; i < fetchData.Result.Length; i++)
        {
            token.ThrowIfCancellationRequested();

            var info = fetchData.Result[i];
            if (info is null)
                break;

            if (info.Listing.Price is null)
            {
                cur.Unpriced++;
            }

            var cond = info.Listing.Price is not null && info.Listing.Price.Amount > 0;
            if (!cond)
            {
                continue;
            }

            string account = info.Listing.Account.Name;
            bool addedData = false;
            string ageIndex = GetAgeIndex(info.Listing.Indexed);
            string key = info.Listing.Price.Currency;
            string keyName = key;
            double amount = 0; // int val formating
            amount = info.Listing.Price.Amount;
            int tempFetch = Data.StatDetail.ResultLoaded;
            string curShort = ReplaceCurrencyChars(keyName);
            var age = ageIndex.Split('-');
            // need non-async
            bool addItem = true;
            if (_vm.Form.SameUser && DetailList.Count >= 1)
            {
                var lbi = DetailList[^1]; // liPriceDetail.Items.Count - 1]
                if (lbi.Content.Contain(account))
                {
                    addItem = false;
                }
            }

            if (addItem)
            {
                var entry = _dm.Currencies.FindEntryById(curShort);
                var curInfo = entry is null ? new CurrencyInfo(curShort, isPoe2)
                    : new CurrencyInfo(curShort, entry.Img, isPoe2);
                var saleInfo = new SaleInfo(amount, GetQuality(info), age[0], age[1], account, info.Listing.HideoutToken);
                var saleItem = new SaleItem(_dm, info.Item);
                
                DetailList.Add(new(saleItem, saleInfo, curInfo, info.Listing.Account.Status));
                Data.StatDetail.ResultLoaded++;
            }
            else
            {
                int iLastInd = DetailList.Count - 1;
                if (iLastInd >= 0)
                {
                    var lbi = DetailList[^1]; // liPriceDetail.Items.Count - 1]
                    int itemCount = 0;
                    int idCount = lbi.Content.IndexOf(Resources.Resources.Main015_ListCount, StringComparison.Ordinal);
                    if (idCount > 0)
                    {
                        var idx = idCount + Resources.Resources.Main015_ListCount.Length;
                        string trimmed = lbi.Content.AsSpan(idx, lbi.Content.Length - idx).ToString().Trim();
                        itemCount = int.Parse(trimmed.AsSpan(0, trimmed.IndexOf(' ')).ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }

                    itemCount = itemCount is 0 ? 2 : itemCount + 1;
                    DetailList.RemoveAt(iLastInd); // Remove last record from same user account found
                    var count = Resources.Resources.Main015_ListCount + ": " + itemCount;
                    var saleInfo = new SaleInfo(amount, count, age[0], age[1], account, info.Listing.HideoutToken);
                    var entry = _dm.Currencies.FindEntryById(curShort);
                    var curInfo = entry is null ? new CurrencyInfo(curShort, isPoe2)
                        : new CurrencyInfo(curShort, entry.Img, isPoe2);
                    var saleItem = new SaleItem(_dm, info.Item);

                    DetailList.Add(new(saleItem, saleInfo, curInfo, info.Listing.Account.Status));
                }
            }
            key = amount + " " + key; // not using round
            if (tempFetch < Data.StatDetail.ResultLoaded) addedData = true;

            if (!hideSameUser || addedData && !token.IsCancellationRequested)
            {
                if (cur.ListCur.TryGetValue(key, out int value))
                    cur.ListCur[key] = ++value;
                else
                    cur.ListCur.Add(key, 1);
            }
            cur.Total++;
        }
        return cur;
    }

    private static string GetQuality(FetchDataInfo info)
    {
        var qual = info.Item.Properties?.FirstOrDefault(x => x.Name.Contain(Strings.ItemApi.Quality));
        if (qual?.Values[0] is not null)
        {
            var match = RegexUtil.DecimalNoPlusDiezePattern().Matches(qual.Values[0].Item1);
            if (match.Count is 1)
            {
                return match[0].Value;
            }
        }
        return string.Empty;
    }

    private ResultBar FillBulkVm(BulkData data, PricingInfo pricingInfo)
    {
        try
        {
            Dictionary<string, int> currencys = new();

            int total = 0;
            int resultCount = data.Result.Count;
            if (data.Result.Count > 0)
            {
                foreach (var valData in data.Result.Values)
                {
                    if (valData.Listing.Offers.Length is 0)
                    {
                        continue;
                    }

                    string account = valData.Listing.Account.Name;

                    string buyerCurrency = valData.Listing.Offers[0].Exchange.Currency;
                    string buyerCurrencyWhisper = valData.Listing.Offers[0].Exchange.Whisper;
                    double buyerAmount = valData.Listing.Offers[0].Exchange.Amount;
                    string sellerCurrency = valData.Listing.Offers[0].Item.Currency;
                    string sellerCurrencyWhisper = valData.Listing.Offers[0].Item.Whisper;
                    double sellerAmount = valData.Listing.Offers[0].Item.Amount;
                    int sellerStock = valData.Listing.Offers[0].Item.Stock;
                    string whisper = valData.Listing.Whisper;
                    StringBuilder sbWhisper = new(whisper);

                    string varPos1 = "{0}", varPos2 = "{1}"; // to update for handling multiple offers
                    if (sellerCurrencyWhisper.Contain(varPos1))
                    {
                        sbWhisper.Replace(varPos1, sellerCurrencyWhisper);
                    }

                    if (buyerCurrencyWhisper.Contain(varPos1))
                    {
                        sbWhisper.Replace(varPos2, buyerCurrencyWhisper.Replace(varPos1, varPos2));
                    }

                    string charName = valData.Listing.Account.LastCharacterName;
                    string key = valData.Listing.Offers[0].Exchange.Currency;
                    double amount = 0;
                    amount = valData.Listing.Offers[0].Exchange.Amount;
                    string keyName = key;

                    sbWhisper.Append('/').Append(sellerAmount).Append('/').Append(sellerCurrency).Append('/').Append(buyerAmount).Append('/').Append(buyerCurrency).Append('/').Append(sellerStock).Append('/').Append(charName);
                    string content = string.Format(BulkFormat, sellerAmount, ReplaceCurrencyChars(sellerCurrency), buyerAmount, ReplaceCurrencyChars(buyerCurrency), sellerStock, charName); // account
                    string tag = string.Empty;
                    string tip = null;
                    if (Data.NinjaEq.ChaosGet > 0 && Data.NinjaEq.ChaosPay > 0)
                    {
                        double ratio = Math.Round(sellerAmount * Data.NinjaEq.ChaosGet / (buyerAmount * Data.NinjaEq.ChaosPay), 2);
                        tip = Resources.Resources.Main195_Ratio + " : " + ratio;
                        tag = Strings.Emoji.GetNinjaTag(ratio);
                    }
                    BulkList.Add(new(BulkList.Count, content, tip, tag, valData.Listing.Account.Status));
                    BulkOffers.Add(new(valData.Listing, valData.Listing.Offers[0]));

                    Data.StatBulk.ResultLoaded++;

                    string replace = @"$3`$2";

                    string cur0 = RegexUtil.LetterTimelessPattern().Replace(pricingInfo.ExchangeCurrency[0], replace);
                    string cur1 = RegexUtil.LetterTimelessPattern().Replace(pricingInfo.ExchangeCurrency[1], replace);

                    key = amount < 1 ? Math.Round(1 / amount, 1) + " " + cur1 : Math.Round(amount, 1) + " " + cur0;

                    if (currencys.TryGetValue(key, out int value))
                        currencys[key] = ++value;
                    else
                        currencys.Add(key, 1);

                    total++;
                }
            }

            Data.StatBulk.ResultCount = resultCount;
            if (data.Total is 0)
            {
                return new(state: ResultBarSate.NoResult);
            }
        }
        catch (Exception ex)
        {
            bool abort = ex.Message.ToLowerInvariant().Contain("thread");
            if (abort || ex.InnerException is ThreadAbortException or HttpRequestException or TimeoutException or JsonException)
            {
                return new(ex, abort);
            }
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "FillBulkWindow() : Error encountered while fetching data...", MessageStatus.Error);
            return new(state: ResultBarSate.NoResult); // added
        }
        return new();
    }

    private ResultBar FillShopVm(BulkData data)
    {
        try
        {
            int total = 0;
            int resultCount = data.Result.Count;
            if (data.Result.Count > 0)
            {
                foreach (var valData in data.Result.Values)
                {
                    if (valData.Listing.Offers.Length is 0)
                    {
                        continue;
                    }

                    //string account = valData.Listing.Account.Name;
                    var itemList = new List<ResultListItemViewModel>();
                    var whisperList = new List<Tuple<FetchDataListing, OfferInfo>>();
                    foreach (var offer in valData.Listing.Offers)
                    {
                        string buyerCurrency = offer.Exchange.Currency;
                        string buyerCurrencyWhisper = offer.Exchange.Whisper;
                        double buyerAmount = offer.Exchange.Amount;
                        string sellerCurrency = offer.Item.Currency;
                        string sellerCurrencyWhisper = offer.Item.Whisper;
                        double sellerAmount = offer.Item.Amount;
                        int sellerStock = offer.Item.Stock;
                        string key = offer.Exchange.Currency;
                        double amount = 0;
                        amount = offer.Exchange.Amount;
                        string keyName = key;
                        string ageIndex = string.Empty;
                        string charName = valData.Listing.Account.LastCharacterName;
                        StringBuilder sbWhisper = new(valData.Listing.Whisper);

                        string varPos1 = "{0}", varPos2 = "{1}"; // to update for handling multiple offers
                        if (sellerCurrencyWhisper.Contain(varPos1))
                        {
                            sbWhisper.Replace(varPos1, sellerCurrencyWhisper);
                        }
                        if (buyerCurrencyWhisper.Contain(varPos1))
                        {
                            sbWhisper.Replace(varPos2, buyerCurrencyWhisper.Replace(varPos1, varPos2));
                        }
                        sbWhisper.Append('/').Append(sellerAmount).Append('/').Append(sellerCurrency).Append('/').Append(buyerAmount).Append('/').Append(buyerCurrency).Append('/').Append(sellerStock).Append('/').Append(charName);

                        string content = string.Format(ShopFormat, sellerStock, ReplaceCurrencyChars(sellerCurrency), sellerAmount, buyerAmount, ReplaceCurrencyChars(buyerCurrency));
                        itemList.Add(new(content, valData.Listing.Account.Status));
                        whisperList.Add(new(valData.Listing, offer));

                        total++;
                    }

                    string cont = string.Format(ShopAccountFormat, valData.Listing.Account.LastCharacterName, valData.Listing.Account.Name);
                    ShopList.Add(new(ShopList.Count, cont, valData.Listing.Account.Status));
                    ShopOffers.Add(new(valData.Listing, null));

                    foreach (var item in itemList)
                    {
                        item.Index = ShopList.Count;
                        ShopList.Add(item);
                    }
                    foreach (var whisper in whisperList)
                    {
                        ShopOffers.Add(whisper);
                    }
                }
            }

            if (data.Total is 0)
            {
                return new(state: ResultBarSate.NoResult);
            }
        }
        catch (Exception ex)
        {
            bool abort = ex.Message.ToLowerInvariant().Contain("thread");
            if (abort || ex.InnerException is ThreadAbortException or HttpRequestException or TimeoutException)
            {
                return new(ex, abort);
            }
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "FillShopWindow() : Error encountered while fetching data...", MessageStatus.Error);
            return new(state: ResultBarSate.NoResult); // added
        }
        return new();
    }

    /// <summary>
    /// Update Result bar viewmodel
    /// </summary>
    /// <remarks>
    /// Return true to stop the update process
    /// </remarks>
    /// <param name="exchange"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private bool UpdateResultBar(bool exchange, ResultBar result)
    {
        if (result is null)
        {
            return true;
        }

        if (result.IsEmpty)
        {
            if (exchange)
            {
                if (_vm.Form.Tab.BulkSelected)
                {
                    Bulk.RightString = result.FirstLine;
                    Bulk.LeftString = result.SecondLine;
                }
                if (_vm.Form.Tab.ShopSelected)
                {
                    Shop.RightString = result.FirstLine;
                    Shop.LeftString = result.SecondLine;
                }
            }
            else
            {
                if (Data.StatDetail?.Total > 0)
                {
                    return false;
                }
                Quick.RightString = Detail.RightString = result.FirstLine;
                Quick.LeftString = Detail.LeftString = result.SecondLine;
                Detail.Total = string.Empty;
            }

            return true;
        }

        if (exchange)
        {
            UpdateExchange();
            return true;
        }

        return false;
    }

    private void UpdateExchange()
    {
        if (_vm.Form.Tab.BulkSelected)
        {
            Bulk.RightString = Resources.Resources.Main002_PriceLoaded;
            Bulk.LeftString = Resources.Resources.Main017_Results + " : " + Data.StatBulk.ResultLoaded + " "
                + Resources.Resources.Main018_ResultsDisplay + " / " + Data.StatBulk.ResultCount + " "
                + Resources.Resources.Main020_ResultsListed
                + Strings.LF + Strings.LF + Resources.Resources.Main004_PriceRefresh;
            Bulk.Total = string.Empty;
            return;
        }
        if (_vm.Form.Tab.ShopSelected)
        {
            Shop.RightString = Resources.Resources.Main002_PriceLoaded;
            Shop.LeftString = Resources.Resources.Main004_PriceRefresh;
        }
    }

    private static string GetAgeIndex(string indexTime)
    {
        DateTime indexTimeDate = DateTime.Parse(indexTime);
        TimeSpan intervalIndex = DateTime.Now - indexTimeDate;

        if (intervalIndex.Days > 0)
        {
            int daysCount = intervalIndex.Days;
            if (daysCount >= 365)
            {
                double yearCount = Math.Round((double)daysCount / 365, MidpointRounding.ToEven);
                return yearCount.ToString() + "-" + (yearCount > 1 ? Resources.Resources.Main140_Years : Resources.Resources.Main139_Year);// + " ago";
            }
            if (daysCount >= 30)
            {
                double monthCount = Math.Round((double)daysCount / 30, MidpointRounding.ToEven);
                return monthCount.ToString() + "-" + (monthCount > 1 ? Resources.Resources.Main138_Months : Resources.Resources.Main137_Month);// + " ago";
            }
            if (daysCount >= 1 && daysCount < 30)
            {
                return daysCount.ToString() + "-" + (daysCount > 1 ? Resources.Resources.Main136_Days : Resources.Resources.Main135_Day);// + " ago";
            }
        }
        if (intervalIndex.Hours > 0)
        {
            int hoursCount = intervalIndex.Hours;
            return hoursCount.ToString() + "-" + (hoursCount > 1 ? Resources.Resources.Main134_Hours : Resources.Resources.Main133_Hour);// + " ago";
        }
        if (intervalIndex.Minutes > 0)
        {
            int minutesCount = intervalIndex.Minutes;
            return minutesCount.ToString() + "-" + (minutesCount > 1 ? Resources.Resources.Main132_Minutes : Resources.Resources.Main131_Minute);// + " ago";
        }
        if (intervalIndex.Seconds > 0)
        {
            int secondsCount = intervalIndex.Seconds;
            return secondsCount.ToString() + "-" + (secondsCount > 1 ? Resources.Resources.Main130_Seconds : Resources.Resources.Main129_Second);// + " ago";
        }
        return "0-error";
    }

    private static string ReplaceCurrencyChars(string entry)
    {
        StringBuilder sb = new(entry);

        foreach (var item in Strings.dicCurrencyChars.Keys)
        {
            sb.Replace(item, Strings.dicCurrencyChars[item]);
        }

        return sb.ToString();
    }
}

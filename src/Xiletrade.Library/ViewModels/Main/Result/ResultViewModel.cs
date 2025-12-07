using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
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
    private AsyncObservableCollection<ListItemViewModel> detailList = new();

    [ObservableProperty]
    private AsyncObservableCollection<ListItemViewModel> bulkList = new();

    [ObservableProperty]
    private AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> bulkOffers = new();

    [ObservableProperty]
    private AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> shopOffers = new();

    [ObservableProperty]
    private AsyncObservableCollection<ListItemViewModel> poepricesList = new();

    [ObservableProperty]
    private AsyncObservableCollection<ListItemViewModel> shopList = new();

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

    // internal methods
    internal void InitData()
    {
        Data = new();
        Rate.ShowMin = false;
    }
    
    internal void UpdateWithApi(PricingInfo pricingInfo)
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
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Error encountered while serializing Exchange object...", MessageStatus.Error);
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
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FetchResults() : Error encountered while fetching data...", MessageStatus.Error);
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
            //var sResult = TestGetEmptyResult();
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
                    result = pricingInfo.IsSimpleBulk ? FillBulkVm(bulkData, pricingInfo) : FillShopVm(bulkData, pricingInfo);
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
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Error encountered while updating price...", MessageStatus.Error);
        }
        finally
        {
            RefreshResultBar(pricingInfo.IsExchangeEntity, result);
        }
    }

    private CurrencyFetch FillDetailVm(bool hideSameUser, ReadOnlySpan<char> sResult, CancellationToken token)
    {
        var cur = new CurrencyFetch();

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
            string pad = age[1] is "일" or "초" or "분" or "天" ? string.Empty.PadRight(1)
                : age[1] is "ชั่วโมง" ? string.Empty.PadRight(2)
                : string.Empty;
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
                string content = string.Format(Strings.DetailListFormat1, amount, curShort, age[0], age[1], pad, Resources.Resources.Main013_ListName, account);
                DetailList.Add(new(content, info.Item, Strings.Status.GetColorStatus(info.Listing.Account.Status)));
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
                    pad = _dm.Config.Options.Language is 0 or 5 or 6 ? pad.PadRight(2)  // en, ru, pt
                        : _dm.Config.Options.Language is 2 or 3 ? pad.PadRight(4) // fr, es
                        : _dm.Config.Options.Language is 4 or 8 or 9 ? pad.PadRight(1) // de, tw, cn
                        : _dm.Config.Options.Language is 7 ? pad.PadRight(5) // th
                        : pad;

                    string content = string.Format(Strings.DetailListFormat2, amount, curShort, age[0], age[1], pad, Resources.Resources.Main015_ListCount, itemCount, Resources.Resources.Main013_ListName, account);
                    DetailList.RemoveAt(iLastInd); // Remove last record from same user account found
                    DetailList.Add(new(content, info.Item, Strings.Status.GetColorStatus(info.Listing.Account.Status)));
                }
            }

            //key = Math.Round(amount - 0.1) + " " + key;
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
                    BulkList.Add(new(BulkList.Count, content, tip, tag, Strings.Status.GetColorStatus(valData.Listing.Account.Status, isBulkTheme: true)));
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
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FillBulkWindow() : Error encountered while fetching data...", MessageStatus.Error);
            return new(state: ResultBarSate.NoResult); // added
        }
        return new();
    }

    private ResultBar FillShopVm(BulkData data, PricingInfo pricingInfo)
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
                    var itemList = new List<ListItemViewModel>();
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
                        itemList.Add(new(content, Strings.Status.GetColorStatus(valData.Listing.Account.Status, isBulkTheme: true)));
                        whisperList.Add(new(valData.Listing, offer));

                        total++;
                    }

                    string cont = string.Format(ShopAccountFormat, valData.Listing.Account.LastCharacterName, valData.Listing.Account.Name);
                    ShopList.Add(new(ShopList.Count, cont, Strings.Status.GetColorStatus(valData.Listing.Account.Status, isShopTheme: true)));
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
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FillShopWindow() : Error encountered while fetching data...", MessageStatus.Error);
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
                if (Data.StatDetail.Total > 0)
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

    private static string TestGetEmptyResult()
    {
        return @"{""id"":""jm5EqYlFX"",""complexity"":30,""result"":[],""total"":0}";
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.Services;

/// <summary>Service used to handle behaviours when querying PoE trade APIs.</summary>
/// <remarks>Responsible for updating ResultViewModel based on API's results</remarks>
public sealed class PoeApiService
{
    private readonly ILogger<PoeApiService> _logger;
    private readonly DataManagerService _dm;
    private readonly INetService _net;
    private readonly IMessageAdapterService _message;
    private readonly MainViewModel _vm;

    // Lazzy initialization of the service.
    public PoeApiService(ILogger<PoeApiService> logger, DataManagerService dm, 
        IMessageAdapterService message, INetService net, MainViewModel vm)
    {
        _logger = logger;
        _dm = dm;
        _net = net;
        _message = message;
        _vm = vm;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    internal async Task<ResultBar> UpdateResult(ResultViewModel rVm, PricingInfo pricingInfo)
    {
        string urlApi = string.Empty;
        string sEntity = null;
        try
        {
            if (pricingInfo.IsTradeEntity)
            {
                sEntity = pricingInfo.TradeEntity;
                urlApi = Strings.Api.Trade;
                rVm.Data.StatDetail = new();
            }
            else if (pricingInfo.IsExchangeEntity)
            {
                var change = new Exchange();
                change.ExchangeData.Status.Option = pricingInfo.Market;
                change.ExchangeData.Minimum = pricingInfo.MinimumStock;
                change.Engine = "new";
                change.ExchangeData.Have = pricingInfo.ExchangeHave;
                change.ExchangeData.Want = pricingInfo.ExchangeWant;

                sEntity = _dm.Json.Serialize<Exchange>(change);
                urlApi = Strings.Api.Exchange;
                rVm.Data.StatBulk = new();
            }
            if (sEntity is null || sEntity.Length is 0)
            {
                return new ResultBar(state: ResultBarSate.NoData);
            }
            var token = _vm.TaskManager.GetPriceToken(initCts: true);
            _vm.TaskManager.PriceTask = RunPriceTask(_vm.Result, pricingInfo, sEntity, urlApi, token);
            return await _vm.TaskManager.PriceTask;
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Error encountered while serializing Exchange object...", MessageStatus.Error);
        }

        return new ResultBar(state: ResultBarSate.NoResult);
    }

    internal async Task<ResultBar> FetchResult(ResultViewModel rVm, int maxFetch, string market, bool hideSameUser, CancellationToken token)
    {
        CurrencyFetch currencys = new();
        try
        {
            int fetchNbMax = 10; // previously 5
            int beginFetch = rVm.Data.StatDetail.Begin;
            var dataToFetch = rVm.Data.ResultData;
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

                    string url = Strings.Api.Fetch + string.Join(",", data) + "?query=" + dataToFetch.Id;

                    string sResult = await _net.SendHTTP(url, Client.Trade); // use cooldown
#if DEBUG
                    _logger.LogInformation("Recovered result from Fetch API..");
#endif
                    if (sResult.Length > 0)
                    {
                        currencys.Add(FillDetailVm(rVm, hideSameUser, sResult, token));
#if DEBUG
                        _logger.LogInformation("Data fetched into vm...");
#endif
                    }
                }
            }

            rVm.Data.StatDetail.Begin = beginFetch;
            rVm.Data.StatDetail.ResultCount = resultCount;
            rVm.Data.StatDetail.Unpriced += currencys.Unpriced;
            rVm.Data.StatDetail.Total += currencys.Total;

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
            _message.Show(ex.GetFormated(), "FetchResults() : Error encountered while fetching data...", MessageStatus.Error);
            return new(state: ResultBarSate.NoResult); // added
        }
        return new(_dm, currencys.ListCur, _vm.Form.Tab.QuickSelected);
    }

    // private

    private async Task<ResultBar> RunPriceTask(ResultViewModel rVm, PricingInfo pricingInfo,
        string sEntity, string urlApi, CancellationToken token)
    {
        try
        {
            var sResult = await _net.SendHTTP(sEntity, urlApi + pricingInfo.League, Client.Trade); // use cooldown

            token.ThrowIfCancellationRequested();
            if (sResult.Length > 0)
            {
                if (sResult.Contain("total\":false"))
                {
                    return new(state: ResultBarSate.BadLeague);
                }
                if (sResult.Contain("total\":0"))
                {
                    return new(state: ResultBarSate.NoResult);
                }

                if (pricingInfo.IsExchangeEntity)
                {
                    var bulkData = _dm.Json.Deserialize<BulkData>(sResult);
                    return pricingInfo.IsSimpleBulk ? FillBulkVm(rVm, bulkData, pricingInfo) : FillShopVm(rVm, bulkData);
                }
                rVm.Data.ResultData = _dm.Json.Deserialize<ResultData>(sResult);
                return await FetchResult(rVm, pricingInfo.MaximumFetch, pricingInfo.Market, pricingInfo.HideSameUser, token);
            }
            return new(state: ResultBarSate.NoData);
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException or OperationCanceledException)
            {
                return new(ex, abort: true);
            }
            if (ex is HttpRequestException or TimeoutException or JsonException)
            {
                return new(ex, false);
            }

            _message.Show(ex.GetFormated(), "Error encountered while updating price...", MessageStatus.Error);
        }
        return new(emptyLine: true);
    }

    // TODO: move responsibility to a viewmodel dedicated to Quick and Detail
    private CurrencyFetch FillDetailVm(ResultViewModel rVm, bool hideSameUser, ReadOnlySpan<char> sResult, CancellationToken token)
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
            int tempFetch = rVm.Data.StatDetail.ResultLoaded;
            string curShort = ReplaceCurrencyChars(keyName);
            var age = ageIndex.Split('-');
            // need non-async
            bool addItem = true;
            if (_vm.Form.SameUser && rVm.DetailList.Count >= 1)
            {
                var lbi = rVm.DetailList[^1]; // liPriceDetail.Items.Count - 1]
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

                rVm.DetailList.Add(new(saleItem, saleInfo, curInfo, info.Listing.Account.Status));
                rVm.Data.StatDetail.ResultLoaded++;
            }
            else
            {
                int iLastInd = rVm.DetailList.Count - 1;
                if (iLastInd >= 0)
                {
                    var lbi = rVm.DetailList[^1]; // liPriceDetail.Items.Count - 1]
                    int itemCount = 0;
                    int idCount = lbi.Content.IndexOf(Resources.Resources.Main015_ListCount, StringComparison.Ordinal);
                    if (idCount > 0)
                    {
                        var idx = idCount + Resources.Resources.Main015_ListCount.Length;
                        string trimmed = lbi.Content.AsSpan(idx, lbi.Content.Length - idx).ToString().Trim();
                        itemCount = int.Parse(trimmed.AsSpan(0, trimmed.IndexOf(' ')).ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }

                    itemCount = itemCount is 0 ? 2 : itemCount + 1;
                    rVm.DetailList.RemoveAt(iLastInd); // Remove last record from same user account found
                    var count = Resources.Resources.Main015_ListCount + ": " + itemCount;
                    var saleInfo = new SaleInfo(amount, count, age[0], age[1], account, info.Listing.HideoutToken);
                    var entry = _dm.Currencies.FindEntryById(curShort);
                    var curInfo = entry is null ? new CurrencyInfo(curShort, isPoe2)
                        : new CurrencyInfo(curShort, entry.Img, isPoe2);
                    var saleItem = new SaleItem(_dm, info.Item);

                    rVm.DetailList.Add(new(saleItem, saleInfo, curInfo, info.Listing.Account.Status));
                }
            }
            key = amount + " " + key; // not using round
            if (tempFetch < rVm.Data.StatDetail.ResultLoaded) addedData = true;

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

    // TODO: move responsibility to a viewmodel dedicated to Bulk
    private ResultBar FillBulkVm(ResultViewModel rVm, BulkData data, PricingInfo pricingInfo)
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
                    string content = string.Format(rVm.BulkFormat, sellerAmount, ReplaceCurrencyChars(sellerCurrency), buyerAmount, ReplaceCurrencyChars(buyerCurrency), sellerStock, charName); // account
                    string tag = string.Empty;
                    string tip = null;
                    if (rVm.Data.NinjaEq.ChaosGet > 0 && rVm.Data.NinjaEq.ChaosPay > 0)
                    {
                        double ratio = Math.Round(sellerAmount * rVm.Data.NinjaEq.ChaosGet / (buyerAmount * rVm.Data.NinjaEq.ChaosPay), 2);
                        tip = Resources.Resources.Main195_Ratio + " : " + ratio;
                        tag = Strings.Emoji.GetNinjaTag(ratio);
                    }
                    rVm.BulkList.Add(new(rVm.BulkList.Count, content, tip, tag, valData.Listing.Account.Status));
                    rVm.BulkOffers.Add(new(valData.Listing, valData.Listing.Offers[0]));

                    rVm.Data.StatBulk.ResultLoaded++;

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

            rVm.Data.StatBulk.ResultCount = resultCount;
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
            _message.Show(ex.GetFormated(), "FillBulkWindow() : Error encountered while fetching data...", MessageStatus.Error);
            return new(state: ResultBarSate.NoResult); // added
        }
        return new();
    }

    // TODO: move responsibility to a viewmodel dedicated to Shop
    private ResultBar FillShopVm(ResultViewModel rVm, BulkData data)
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

                        string content = string.Format(rVm.ShopFormat, sellerStock, ReplaceCurrencyChars(sellerCurrency), sellerAmount, buyerAmount, ReplaceCurrencyChars(buyerCurrency));
                        itemList.Add(new(content, valData.Listing.Account.Status));
                        whisperList.Add(new(valData.Listing, offer));

                        total++;
                    }

                    string cont = string.Format(rVm.ShopAccountFormat, valData.Listing.Account.LastCharacterName, valData.Listing.Account.Name);
                    rVm.ShopList.Add(new(rVm.ShopList.Count, cont, valData.Listing.Account.Status));
                    rVm.ShopOffers.Add(new(valData.Listing, null));

                    foreach (var item in itemList)
                    {
                        item.Index = rVm.ShopList.Count;
                        rVm.ShopList.Add(item);
                    }
                    foreach (var whisper in whisperList)
                    {
                        rVm.ShopOffers.Add(whisper);
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
            _message.Show(ex.GetFormated(), "FillShopWindow() : Error encountered while fetching data...", MessageStatus.Error);
            return new(state: ResultBarSate.NoResult); // added
        }
        return new();
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

        foreach (var item in Strings.Collection.dicCurrencyChars.Keys)
        {
            sb.Replace(item, Strings.Collection.dicCurrencyChars[item]);
        }

        return sb.ToString();
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
}

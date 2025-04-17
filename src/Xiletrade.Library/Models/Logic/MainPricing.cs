using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Http;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;
using System.Threading.Tasks;

namespace Xiletrade.Library.Models.Logic;

/// <summary>class used to fetch price data into the main viewmodel.</summary>
internal sealed class MainPricing
{
    private static IServiceProvider _serviceProvider;
    private static MainViewModel Vm { get; set; }
    private static readonly StringListFormat StrFormat = new();
    
    internal PricingWatch Watch { get; private set; } = new();
    internal PricingCooldown CoolDown { get; private set; }

    internal MainPricing(MainViewModel vm, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Vm = vm;
        CoolDown = new(Vm, _serviceProvider);
    }

    // internal methods
    internal void UpdateVmWithApi(PricingInfo pricingInfo, CancellationToken token)
    {
        PricingResult result = null;
        string urlString = string.Empty;
        string sEntity = null;
        bool exchange = false;
        bool simpleBulk = false;

        if (pricingInfo.Entity[0]?.Count is 1 && pricingInfo.Entity[1] is null)
        {
            sEntity = pricingInfo.Entity[0][0];
            urlString = Strings.TradeApi;
            for (int i = 0; i < 5; i++)
            {
                Vm.Result.Data.StatsFetchDetail[i] = 0;
            }
        }
        else if (pricingInfo.Entity[0]?.Count >= 1 && pricingInfo.Entity[1]?.Count >= 1)
        {
            simpleBulk = Vm.Form.Tab.BulkSelected;
            if (simpleBulk)
            {
                Vm.Result.Data.ExchangeCurrency = [pricingInfo.Entity[0][0], pricingInfo.Entity[1][0]];
            }

            Exchange change = new();
            change.ExchangeData.Status.Option = pricingInfo.Market;
            change.ExchangeData.Minimum = pricingInfo.MinimumStock;
            //change.ExchangeData.Collapse = simpleBulk ? "false" : "true"; // Collapse parameter not needed anymore
            change.Engine = "new";
            if (pricingInfo.Entity[0] is not null)
            {
                change.ExchangeData.Have = [.. pricingInfo.Entity[0]];
            }
            if (pricingInfo.Entity[1] is not null)
            {
                change.ExchangeData.Want = [.. pricingInfo.Entity[1]];
            }

            sEntity = Json.Serialize<Exchange>(change); 

            urlString = Strings.ExchangeApi;
            exchange = true;

            for (int i = 0; i < 5; i++)
            {
                Vm.Result.Data.StatsFetchBulk[i] = 0;
            }
        }
        if (sEntity is null || sEntity.Length is 0)
        {
            return;
        }

        try
        {
            CoolDown.Apply();
            var service = _serviceProvider.GetRequiredService<NetService>();
            string sResult = service.SendHTTP(sEntity, urlString + pricingInfo.League, Client.Trade).Result; // use cooldown
            int idLang = DataManager.Config.Options.Language;

            if (sResult.Length > 0)
            {
                if (sResult.Contains("total\":false", StringComparison.Ordinal))
                {
                    result = new(state: PricingResultSate.BadLeague);
                    return;
                }
                if (sResult.Contains("total\":0", StringComparison.Ordinal))
                {
                    result = new(state: PricingResultSate.NoResult);
                    return;
                }

                if (exchange)
                {
                    CoolDown.Apply();
                    var bulkData = Json.Deserialize<BulkData>(sResult);
                    result = simpleBulk ? FillBulkVm(bulkData, pricingInfo.Market) : FillShopVm(bulkData, pricingInfo.Market);
                    return;
                }
                Vm.Result.Data.DataToFetchDetail = Json.Deserialize<ResultData>(sResult);
                result = FetchDetail(pricingInfo.MaximumFetch, pricingInfo.Market, pricingInfo.HideSameUser, token);
                return;
            }
            result = new(state: PricingResultSate.NoData);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is HttpRequestException or TimeoutException)
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
            if (!token.IsCancellationRequested)
            {
                Vm.Result.RefreshResultBar(exchange, result);
            }
        }
    }

    internal PricingResult FetchDetail(int maxFetch, string market, bool hideSameUser, CancellationToken token)
    {
        CurrencyFetch currencys = new();
        try
        {
            //int resultsLoaded = 0;
            int fetchNbMax = 10; // previously 5
            int beginFetch = Vm.Result.Data.StatsFetchDetail[0];
            var dataToFetch = Vm.Result.Data.DataToFetchDetail;
            int resultCount = dataToFetch.Result.Length;
            if (dataToFetch.Result.Length > 0)
            {
                int nbFetch = 0;
                while (nbFetch < maxFetch)
                {
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

                    CoolDown.Apply();
                    var service = _serviceProvider.GetRequiredService<NetService>();
                    string sResult = service.SendHTTP(null, url, Client.Trade).Result; // use cooldown

                    if (sResult.Length > 0)
                    {
                        currencys = FillDetailVm(market, hideSameUser, sResult, token);
                    }
                }
            }

            Vm.Result.Data.StatsFetchDetail[0] = beginFetch;
            //Vm.Result.Data.StatsFetchDetail[1] += resultsLoaded;
            Vm.Result.Data.StatsFetchDetail[2] = resultCount;
            Vm.Result.Data.StatsFetchDetail[3] += currencys.Unpriced;
            Vm.Result.Data.StatsFetchDetail[4] += currencys.Total;
            //Vm.Result.Data.StatsFetchDetail[5] += (total - resultsLoaded);

            if (dataToFetch.Total is 0 || currencys.ListCur.Count is 0)
            {
                return new PricingResult(state: PricingResultSate.NoResult); // useSecondLine: false
            }
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException or OperationCanceledException)
            {
                return new PricingResult(ex, abort: true);
            }
            bool abort = ex.Message.ToLowerInvariant().Contains("thread", StringComparison.Ordinal);
            if (abort || ex.InnerException is ThreadAbortException or HttpRequestException or TimeoutException)
            {
                return new PricingResult(ex, abort);
            }
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FetchResults() : Error encountered while fetching data...", MessageStatus.Error);
            return new PricingResult(state: PricingResultSate.NoResult); // added
        }
        return new PricingResult(currencys.ListCur);
    }

    private static CurrencyFetch FillDetailVm(string market, bool hideSameUser, string sResult, CancellationToken token)
    {
        var cur = new CurrencyFetch();
        StringBuilder sbJson = new(sResult);
        // Handle bad stash names, cryptisk does not resolve :
        sbJson.Replace("\\\\\",", "\",").Replace("name:,", "\"name\":\"\",");

        var fetchData = Json.Deserialize<FetchData>(sbJson.ToString());

        for (int i = 0; i < fetchData.Result.Length; i++)
        {
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
            string onlineStatus = market is Strings.any && info.Listing.Account.Online is null ?
                Strings.Offline : Strings.Online;

            bool addedData = false;
            string ageIndex = GetAgeIndex(info.Listing.Indexed);
            string key = info.Listing.Price.Currency;
            string keyName = key;
            double amount = 0; // int val formating
            amount = info.Listing.Price.Amount;

            token.ThrowIfCancellationRequested();

            int tempFetch = Vm.Result.Data.StatsFetchDetail[1];
            string curShort = ReplaceCurrencyChars(keyName);
            var age = ageIndex.Split('-');
            string pad = age[1] is "일" or "초" or "분" or "天" ? string.Empty.PadRight(1)
                : age[1] is "ชั่วโมง" ? string.Empty.PadRight(2)
                : string.Empty;
            // need non-async
            bool addItem = true;
            if (Vm.Form.SameUser && Vm.Result.DetailList.Count >= 1)
            {
                var lbi = Vm.Result.DetailList[^1]; // liPriceDetail.Items.Count - 1]
                if (lbi.Content.Contains(account, StringComparison.Ordinal))
                {
                    addItem = false;
                }
            }

            if (addItem)
            {
                string content = string.Format(Strings.DetailListFormat1, amount, curShort, age[0], age[1], pad, Resources.Resources.Main013_ListName, account);
                Vm.Result.DetailList.Add(new() { Content = content, FgColor = onlineStatus == Strings.Online ? Strings.Color.LimeGreen : Strings.Color.Red });
                Vm.Result.Data.StatsFetchDetail[1]++;
            }
            else
            {
                int iLastInd = Vm.Result.DetailList.Count - 1;
                if (iLastInd >= 0)
                {
                    var lbi = Vm.Result.DetailList[^1]; // liPriceDetail.Items.Count - 1]

                    int itemCount = 0;
                    int idCount = lbi.Content.IndexOf(Resources.Resources.Main015_ListCount, StringComparison.Ordinal);
                    if (idCount > 0)
                    {
                        string subLb = lbi.Content[(idCount + Resources.Resources.Main015_ListCount.Length)..].Trim();
                        idCount = subLb.IndexOf(' ');
                        string subLb3 = subLb[..idCount];
                        itemCount = int.Parse(subLb3, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    itemCount = itemCount is 0 ? 2 : itemCount + 1;

                    pad = DataManager.Config.Options.Language is 0 or 5 or 6 ? pad.PadRight(2)  // en, ru, pt
                        : DataManager.Config.Options.Language is 2 or 3 ? pad.PadRight(4) // fr, es
                        : DataManager.Config.Options.Language is 4 or 8 or 9 ? pad.PadRight(1) // de, tw, cn
                        : DataManager.Config.Options.Language is 7 ? pad.PadRight(5) // th
                        : pad;

                    string content = string.Format(Strings.DetailListFormat2, amount, curShort, age[0], age[1], pad, Resources.Resources.Main015_ListCount, itemCount, Resources.Resources.Main013_ListName, account);
                    Vm.Result.DetailList.RemoveAt(iLastInd); // Remove last record from same user account found
                    Vm.Result.DetailList.Add(new() { Content = content, FgColor = onlineStatus == Strings.Online ? Strings.Color.LimeGreen : Strings.Color.Red });
                }
            }

            //key = Math.Round(amount - 0.1) + " " + key;
            key = amount + " " + key; // not using round
            if (tempFetch < Vm.Result.Data.StatsFetchDetail[1]) addedData = true;

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

    // private methods
    private PricingResult FillBulkVm(BulkData data, string market)
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
                    if (valData.Listing.Offers.Length > 0)
                    {
                        string account = valData.Listing.Account.Name;

                        string onlineStatus = Strings.Online;
                        if (market is Strings.any)
                        {
                            if (valData.Listing.Account.Online is null)
                                onlineStatus = Strings.Offline;
                        }

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

                        if (sellerCurrencyWhisper.Contains(varPos1, StringComparison.Ordinal))
                        {
                            sbWhisper.Replace(varPos1, sellerCurrencyWhisper);
                        }

                        if (buyerCurrencyWhisper.Contains(varPos1, StringComparison.Ordinal))
                        {
                            sbWhisper.Replace(varPos2, buyerCurrencyWhisper.Replace(varPos1, varPos2));
                        }

                        string status = valData.Listing.Account.Online is not null ?
                            valData.Listing.Account.Online.Status : Strings.Offline;

                        string charName = valData.Listing.Account.LastCharacterName;
                        string key = valData.Listing.Offers[0].Exchange.Currency;
                        double amount = 0;
                        amount = valData.Listing.Offers[0].Exchange.Amount;
                        string keyName = key;

                        sbWhisper.Append('/').Append(sellerAmount).Append('/').Append(sellerCurrency).Append('/').Append(buyerAmount).Append('/').Append(buyerCurrency).Append('/').Append(sellerStock).Append('/').Append(charName);
                        string content = string.Format(StrFormat.Bulk, sellerAmount, ReplaceCurrencyChars(sellerCurrency), buyerAmount, ReplaceCurrencyChars(buyerCurrency), sellerStock, charName); // account
                        string tag = string.Empty;
                        string tip = null;
                        if (Vm.Result.Data.NinjaChaosEqGet > 0 && Vm.Result.Data.NinjaChaosEqPay > 0)
                        {
                            double ratio = Math.Round(sellerAmount * Vm.Result.Data.NinjaChaosEqGet / (buyerAmount * Vm.Result.Data.NinjaChaosEqPay), 2);
                            tip = Resources.Resources.Main195_Ratio + " : " + ratio;
                            tag = ratio >= 1.2 ? "emoji_vhappy" : ratio >= 1 ? "emoji_happy" : ratio >= 0.90 ? "emoji_neutral" : ratio >= 0.80 ? "emoji_crying" : "emoji_angry";
                        }

                        Vm.Result.BulkList.Add(new(){ Index = Vm.Result.BulkList.Count, Content = content, ToolTip = tip, Tag = tag, FgColor = onlineStatus == Strings.Online ? status == Strings.afk ? Strings.Color.YellowGreen : Strings.Color.LimeGreen : Strings.Color.Red });
                        Vm.Result.BulkOffers.Add(new(valData.Listing, valData.Listing.Offers[0]));

                        Vm.Result.Data.StatsFetchBulk[1]++;

                        string replace = @"$3`$2";
                        string cur0 = RegexUtil.LetterTimelessPattern().Replace(Vm.Result.Data.ExchangeCurrency[0], replace);
                        string cur1 = RegexUtil.LetterTimelessPattern().Replace(Vm.Result.Data.ExchangeCurrency[1], replace);

                        key = amount < 1 ? Math.Round(1 / amount, 1) + " " + cur1 : Math.Round(amount, 1) + " " + cur0;

                        if (currencys.TryGetValue(key, out int value))
                            currencys[key] = ++value;
                        else
                            currencys.Add(key, 1);

                        total++;
                    }
                }
            }

            Vm.Result.Data.StatsFetchBulk[2] = resultCount;
            if (data.Total is 0)
            {
                return new PricingResult(state: PricingResultSate.NoResult);
            }
        }
        catch (Exception ex)
        {
            bool abort = ex.Message.ToLowerInvariant().Contains("thread", StringComparison.Ordinal);
            if (abort || ex.InnerException is ThreadAbortException or HttpRequestException or TimeoutException)
            {
                return new PricingResult(ex, abort);
            }
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FillBulkWindow() : Error encountered while fetching data...", MessageStatus.Error);
            return new PricingResult(state: PricingResultSate.NoResult); // added
        }
        return new PricingResult();
    }

    private PricingResult FillShopVm(BulkData data, string market)
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

                    string account = valData.Listing.Account.Name;
                    string onlineStatus = Strings.Online;
                    if (market is Strings.any && valData.Listing.Account.Online is null)
                    {
                        onlineStatus = Strings.Offline;
                    }

                    string status = valData.Listing.Account.Online is not null ?
                        valData.Listing.Account.Online.Status : Strings.Offline;

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

                        string key = string.Empty;
                        double amount = 0;
                        string keyName = string.Empty;
                        string ageIndex = string.Empty;
                        string whisper = string.Empty;
                        string charName = string.Empty;

                        whisper = valData.Listing.Whisper;

                        StringBuilder sbWhisper = new(whisper);

                        string varPos1 = "{0}", varPos2 = "{1}"; // to update for handling multiple offers

                        if (sellerCurrencyWhisper.Contains(varPos1, StringComparison.Ordinal))
                        {
                            sbWhisper.Replace(varPos1, sellerCurrencyWhisper);
                        }

                        if (buyerCurrencyWhisper.Contains(varPos1, StringComparison.Ordinal))
                        {
                            sbWhisper.Replace(varPos2, buyerCurrencyWhisper.Replace(varPos1, varPos2));
                        }

                        charName = valData.Listing.Account.LastCharacterName;
                        key = offer.Exchange.Currency;
                        amount = offer.Exchange.Amount;

                        keyName = key;

                        sbWhisper.Append('/').Append(sellerAmount).Append('/').Append(sellerCurrency).Append('/').Append(buyerAmount).Append('/').Append(buyerCurrency).Append('/').Append(sellerStock).Append('/').Append(charName);
                        string content = string.Empty;
                        string tag = string.Empty;
                        string tip = null;
                        content = string.Format(StrFormat.Shop, sellerStock, ReplaceCurrencyChars(sellerCurrency), sellerAmount, buyerAmount, ReplaceCurrencyChars(buyerCurrency));
                        itemList.Add(new(){ Content = content, ToolTip = tip, Tag = tag, FgColor = onlineStatus == Strings.Online ? status is Strings.afk ? Strings.Color.YellowGreen : Strings.Color.LimeGreen : Strings.Color.Red });
                        whisperList.Add(new(valData.Listing, offer));

                        total++;
                    }

                    string cont = string.Format(StrFormat.ShopAccount, valData.Listing.Account.LastCharacterName, valData.Listing.Account.Name);
                    Vm.Result.ShopList.Add(new() { Index = Vm.Result.ShopList.Count, Content = cont, ToolTip = null, Tag = string.Empty, FgColor = onlineStatus == Strings.Online ? status is Strings.afk ? Strings.Color.Yellow : Strings.Color.DeepSkyBlue : Strings.Color.DarkRed });
                    Vm.Result.ShopOffers.Add(new(valData.Listing, null));

                    foreach (var item in itemList)
                    {
                        item.Index = Vm.Result.ShopList.Count;
                        Vm.Result.ShopList.Add(item);
                    }
                    foreach (var whisper in whisperList)
                    {
                        Vm.Result.ShopOffers.Add(whisper);
                    }
                }
            }

            if (data.Total is 0)
            {
                return new PricingResult(state: PricingResultSate.NoResult);
            }
        }
        catch (Exception ex)
        {
            bool abort = ex.Message.ToLowerInvariant().Contains("thread", StringComparison.Ordinal);
            if (abort || ex.InnerException is ThreadAbortException or HttpRequestException or TimeoutException)
            {
                return new PricingResult(ex, abort);
            }
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FillShopWindow() : Error encountered while fetching data...", MessageStatus.Error);
            return new PricingResult(state: PricingResultSate.NoResult); // added
        }
        return new PricingResult();
    }
    
    private static string GetAgeIndex(string indexTime)
    {
        DateTime indexTimeDate = DateTime.Parse(indexTime);
        TimeSpan intervalIndex = DateTime.Now - indexTimeDate;
        //int idLang = DataManager.Config.Options.Language;
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Http;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Logic;

/// <summary>class used to fetch price data into the main viewmodel.</summary>
internal sealed class MainPricing
{
    private static IServiceProvider _serviceProvider;
    private static MainViewModel Vm { get; set; }
    private static readonly StringListFormat StrFormat = new();
    
    internal PricingWatch Watch { get; private set; } = new();
    internal PricingDataBuffer Buffer { get; private set; } = new();
    internal PricingCooldown CoolDown { get; private set; }

    internal MainPricing(MainViewModel vm, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Vm = vm;
        CoolDown = new(Vm, _serviceProvider);
    }

    // internal methods

    // TODO : remake with real json object instead of dirty strings.
    internal void UpdateVmWithApi(List<string>[] entity, string league, string market, int minimumStock, int maxFetch, bool hideSameUser, CancellationToken token)
    {
        string[] result = [string.Empty, string.Empty];
        string urlString = string.Empty;
        string sEntity = null;
        bool exchange = false;
        bool simpleBulk = false;
        //int totalWantResult = listCount*5;
        if (entity[0]?.Count == 1 && entity[1] is null)
        {
            sEntity = entity[0][0];

            urlString = Strings.TradeApi[DataManager.Config.Options.Language];
            //beginFetch[0] = 0;
            for (int i = 0; i < 5; i++)
            {
                Buffer.StatsFetchDetail[i] = 0;
            }
        }
        else if (entity[0]?.Count >= 1 && entity[1]?.Count >= 1)
        {
            //simpleBulk = entity[0].Count == 1 && entity[1].Count == 1;
            simpleBulk = Vm.Form.Tab.BulkSelected; //&& !Vm.Form.Tab.ShopSelected
            if (simpleBulk)
            {
                Buffer.ExchangeCurrency = [entity[0][0], entity[1][0]];
            }

            StringBuilder sbPay = new(",\"have\":["), sbGet = new(",\"want\":[");
            bool appended = false;
            foreach (var str in entity[0])
            {
                if (str.Length > 0)
                {
                    if (appended)
                    {
                        sbPay.Append(',');
                    }
                    sbPay.Append('"').Append(str).Append('"');
                    appended = true;
                }
            }
            sbPay.Append(']');

            appended = false;
            foreach (var str in entity[1])
            {
                if (str.Length > 0)
                {
                    if (appended)
                    {
                        sbGet.Append(',');
                    }
                    sbGet.Append('"').Append(str).Append('"');
                    appended = true;
                }
            }
            sbGet.Append(']');

            /*
            if (entity[0][0].Length > 0)
            {
                entity[0][0] = ",\"have\":[\"" + entity[0][0] + "\"]";
            }
            if (entity[1][0].Length > 0)
            {
                entity[1][0] = ",\"want\":[\"" + entity[1][0] + "\"]";
            }*/

            //sEentity = "{\"exchange\":{\"status\":{\"option\":\"online\"},\"have\":[\"chaos\"],\"want\":[\"exalted\",\"gcp\"],\"minimum\":1,\"collapse\":true},\"engine\":\"new\"}";
            //sEentity = "{\"exchange\":{\"status\":{\"option\":\"online\"},\"have\":[\"chaos\"],\"want\":[\"exalted\"],\"minimum\":1,\"collapse\":false},\"engine\":\"new\"}";
            //sEentity = "{\"query\":{\"status\":{\"option\":\"online\"},\"have\":[\"exalted\"],\"want\":[\"chaos\"]},\"sort\":{\"want\":\"asc\"},\"engine\":\"new\"}";
            // "engine" : "new"|"legacy"
            sEntity = string.Format(
                    "{{\"query\":{{\"status\":{{\"option\":\"{0}\"}}{1}{2}, \"minimum\": {3},\"collapse\": {4}}},\"engine\":\"new\"}}",
                    market,
                    sbPay.ToString(),//entity[0][0],
                    sbGet.ToString(),//entity[1][0],
                    minimumStock,
                    simpleBulk ? "false" : "true"
                );
            // "{{\"exchange\":{{\"status\":{{\"option\":\"{0}\"}},\"have\":[\"{1}\"],\"want\":[\"{2}\"], \"minimum\": {3}}}}}",

            urlString = Strings.ExchangeApi[DataManager.Config.Options.Language];
            exchange = true;
            //beginFetch[1] = 0;
            for (int i = 0; i < 5; i++)
            {
                Buffer.StatsFetchBulk[i] = 0;
            }
        }

        if (sEntity?.Length > 0)
        {
            try
            {
                CoolDown.Apply();
                var service = _serviceProvider.GetRequiredService<NetService>();
                string sResult = service.SendHTTP(sEntity, urlString + league, Client.Trade).Result; // use cooldown
                int idLang = DataManager.Config.Options.Language;

                if (sResult.Length > 0)
                {
                    if (sResult.Contains("total\":false", StringComparison.Ordinal))
                    {
                        result[1] = "ERROR " + Resources.Resources.Main029_Error1bis; // "ERROR while retrieving data.";
                        result[0] = Resources.Resources.Main028_Error1; //"Wrong league set ?";
                    }
                    else if (sResult.Contains("total\":0", StringComparison.Ordinal))
                    {
                        result[1] = "NORESULT";
                        result[0] = Resources.Resources.Main008_PriceNoResult;
                    }
                    else
                    {
                        if (exchange)
                        {
                            BulkData bulkData = Json.Deserialize<BulkData>(sResult);
                            result = simpleBulk ? FillBulkVm(bulkData, market) : FillShopVm(bulkData, market);
                        }
                        else
                        {
                            Buffer.DataToFetchDetail = Json.Deserialize<ResultData>(sResult);
                            result = FillDetailVm(maxFetch, market, hideSameUser, token);
                        }

                    }
                }
                else
                {
                    // to check : never go in this loop
                    result[0] = Resources.Resources.Main030_Error2; //"No Data";
                    result[1] = "ERROR " + Resources.Resources.Main031_Error2bis; //"ERROR contacting trade website.";
                }
            }
            catch (Exception ex)
            {
                //if (ex.StatusCode == HttpStatusCode.Unauthorized)
                if (ex.InnerException is HttpRequestException exception)
                {
                    string[] mess = exception.Message.Split(':');
                    result[0] = "The request encountered" + Strings.LF + "an exception. [A]";
                    result[1] = mess.Length > 1 ? "ERROR : Code " + mess[1].Trim() : exception.Message;
                }
                else if (ex.InnerException is TimeoutException except)
                {
                    result[0] = "The request has expired";
                    result[1] = except.Message.Length > 24 ?
                        except.Message[..24].Trim() + Strings.LF + except.Message[24..].Trim() : except.Message;
                }
                else
                {
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Error encountered while updating price...", MessageStatus.Error);
                }
            }

            if (!token.IsCancellationRequested)
            {
                Vm.Logic.RefreshViewModelStatus(exchange, result);
            }
        }
    }

    // TODO Refactor
    internal string[] FillDetailVm(int maxFetch, string market, bool hideSameUser, CancellationToken token)
    {
        string[] result = [Resources.Resources.Main007_PriceWaiting, string.Empty];
        int idLang = DataManager.Config.Options.Language;
        try
        {
            //int resultsLoaded = 0;
            Dictionary<string, int> currencys = new();
            int beginFetch;
            int fetchNbMax;
            ResultData dataToFetch;
            fetchNbMax = 10; // previously 5
            dataToFetch = Buffer.DataToFetchDetail;
            beginFetch = Buffer.StatsFetchDetail[0];

            int total = 0, unpriced = 0;
            int resultCount = dataToFetch.Result.Length;
            if (dataToFetch.Result.Length > 0)
            {
                int nbFetch = 0;
                while (nbFetch < maxFetch)
                {
                    string[] tmp = new string[fetchNbMax];
                    if (beginFetch >= dataToFetch.Result.Length)
                        break;

                    for (int i = 0; i < fetchNbMax; i++)
                    {

                        if (beginFetch >= dataToFetch.Result.Length)
                            break;

                        tmp[i] = dataToFetch.Result[beginFetch];
                        nbFetch++;
                        beginFetch++;
                    }

                    string url = Strings.FetchApi[DataManager.Config.Options.Language] + string.Join(",", tmp) + "?query=" + dataToFetch.Id;

                    CoolDown.Apply();
                    var service = _serviceProvider.GetRequiredService<NetService>();
                    string sResult = service.SendHTTP(null, url, Client.Trade).Result; // use cooldown

                    if (sResult.Length > 0)
                    {
                        StringBuilder sbJson = new(sResult);
                        // Handle bad stash names, cryptisk does not resolve :
                        sbJson.Replace("\\\\\",", "\",").Replace("name:,", "\"name\":\"\",");

                        var fetchData = Json.Deserialize<FetchData>(sbJson.ToString());

                        for (int i = 0; i < fetchData.Result.Length; i++)
                        {

                            if (fetchData.Result[i] is null)
                                break;

                            if (fetchData.Result[i].Listing.Price is null)
                            {
                                unpriced++;
                            }

                            if (fetchData.Result[i].Listing.Price is not null && fetchData.Result[i].Listing.Price.Amount > 0)
                            {
                                string account = fetchData.Result[i].Listing.Account.Name;
                                string onlineStatus = Strings.Online;
                                if (market is Strings.any)
                                {
                                    if (fetchData.Result[i].Listing.Account.Online is null)
                                        onlineStatus = Strings.Offline;
                                }

                                string buyerCurrency = string.Empty;
                                double buyerAmount = 0;
                                string sellerCurrency = string.Empty;
                                double sellerAmount = 0;
                                int sellerStock = 0;

                                string key = string.Empty;
                                double amount = 0;
                                string keyName = string.Empty;
                                string ageIndex = string.Empty;
                                //string whisper = string.Empty;
                                string charName = string.Empty;
                                string status = string.Empty;

                                ageIndex = GetAgeIndex(fetchData.Result[i].Listing.Indexed); // Recover age
                                key = fetchData.Result[i].Listing.Price.Currency;
                                amount = fetchData.Result[i].Listing.Price.Amount;

                                keyName = key;

                                bool addedData = false;
                                int tempFetch;

                                if (token.IsCancellationRequested)
                                {
                                    return ["Abort called before the end", "Application (Task) ERROR "];
                                }

                                tempFetch = Buffer.StatsFetchDetail[1];

                                string curShort = ReplaceCurrencyChars(keyName);
                                string[] age = ageIndex.Split('-');
                                string pad = age[1] is "일" or "초" or "분" or "天" ? string.Empty.PadRight(1)
                                    : age[1] is "ชั่วโมง" ? string.Empty.PadRight(2)
                                    : string.Empty;
                                // need non-async
                                bool addItem = true;
                                if (Vm.SameUser && Vm.Result.DetailList.Count >= 1)
                                {
                                    ListItemViewModel lbi = Vm.Result.DetailList[^1]; // liPriceDetail.Items.Count - 1]
                                    if (lbi.Content.Contains(account, StringComparison.Ordinal))
                                    {
                                        addItem = false;
                                    }
                                }

                                if (addItem)
                                {
                                    string content = string.Format(Strings.DetailListFormat1, amount, curShort, age[0], age[1], pad, Resources.Resources.Main013_ListName, account);
                                    Vm.Result.DetailList.Add(new ListItemViewModel { Content = content, FgColor = onlineStatus == Strings.Online ? Strings.Color.LimeGreen : Strings.Color.Red });
                                    Buffer.StatsFetchDetail[1]++;
                                }
                                else
                                {
                                    int iLastInd = Vm.Result.DetailList.Count - 1;
                                    if (iLastInd >= 0)
                                    {
                                        ListItemViewModel lbi = Vm.Result.DetailList[^1]; // liPriceDetail.Items.Count - 1]

                                        int itemCount = 0;
                                        int idCount = lbi.Content.IndexOf(Resources.Resources.Main015_ListCount, StringComparison.Ordinal);
                                        if (idCount > 0)
                                        {
                                            string subLb = lbi.Content[(idCount + Resources.Resources.Main015_ListCount.Length)..].Trim();
                                            //string subLb = lbi.Content.ToString().Substring(idCount + Resources.Resources.Main015_ListCount.Length + 1).Trim();
                                            idCount = subLb.IndexOf(' ');
                                            string subLb3 = subLb[..idCount];
                                            itemCount = int.Parse(subLb3, System.Globalization.CultureInfo.InvariantCulture);
                                            //Int32.TryParse(subLb3, out itemCount);
                                        }

                                        itemCount = itemCount is 0 ? 2 : itemCount + 1;

                                        pad = DataManager.Config.Options.Language is 0 or 5 or 6 ? pad.PadRight(2)  // en, ru, pt
                                            : DataManager.Config.Options.Language is 2 or 3 ? pad.PadRight(4) // fr, es
                                            : DataManager.Config.Options.Language is 4 or 8 or 9 ? pad.PadRight(1) // de, tw, cn
                                            : DataManager.Config.Options.Language is 7 ? pad.PadRight(5) // th
                                            : pad;

                                        string content = string.Format(Strings.DetailListFormat2, amount, curShort, age[0], age[1], pad, Resources.Resources.Main015_ListCount, itemCount, Resources.Resources.Main013_ListName, account);
                                        Vm.Result.DetailList.RemoveAt(iLastInd); // Remove last record from same user account found
                                        Vm.Result.DetailList.Add(new ListItemViewModel { Content = content, FgColor = onlineStatus == Strings.Online ? Strings.Color.LimeGreen : Strings.Color.Red });
                                    }
                                }

                                //key = Math.Round(amount - 0.1) + " " + key;
                                key = amount + " " + key; // not using round
                                if (tempFetch < Buffer.StatsFetchDetail[1]) addedData = true;

                                if (!hideSameUser || addedData && !token.IsCancellationRequested)
                                {
                                    if (currencys.TryGetValue(key, out int value))
                                        currencys[key] = ++value;
                                    else
                                        currencys.Add(key, 1);
                                }
                                total++;
                            }
                        }
                    }
                }

                if (currencys.Count > 0) // Only first time : && Global.StatsFetchDetail[4] < total
                {
                    List<KeyValuePair<string, int>> myList = new(currencys);
                    string first = myList[0].Key;
                    string last = myList[^1].Key; // myList.Count - 1

                    myList.Sort(
                        delegate (KeyValuePair<string, int> firstPair,
                        KeyValuePair<string, int> nextPair)
                        {
                            return -1 * firstPair.Value.CompareTo(nextPair.Value);
                        }
                    );

                    KeyValuePair<string, int> firstKey = myList[^1]; // myList.Count - 1
                    if (myList.Count > 1 && (firstKey.Value == 1 || firstKey.Value == 2 && first == firstKey.Key))
                    {
                        int idx = myList.Count - 2;

                        if (firstKey.Value == 1 || myList[idx].Value == 1)
                            idx = (int)Math.Truncate((double)myList.Count / 2);

                        firstKey = myList[idx];
                    }

                    //result = Regex.Replace(first + " ~ " + last, @"(timeless-)?([a-z]{3})[a-z\-]+\-([a-z]+)", @"$3`$2");
                    //StringsTable.MainStrings.ListStock[idLang]
                    string concatPrice = first != last ? first + " (" + Resources.Resources.Main022_ResultsMin + ")"
                        + Strings.LF + last + " (" + Resources.Resources.Main023_ResultsMax + ")"
                        : first + Strings.LF + Resources.Resources.Main141_ResultsSingle; // single price

                    result[0] = RegexUtil.LetterTimelessPattern().Replace(concatPrice, @"$3`$2");

                    int lineCount = 0, records = 0;
                    for (int i = 0; i < myList.Count; i++)
                    {
                        if (myList[i].Value >= 2 && lineCount < 3)
                        {
                            if (lineCount > 0) result[1] += Strings.LF;
                            result[1] += myList[i].Key + " : " + myList[i].Value + " " + Resources.Resources.Main024_ResultsSales; // sales
                            lineCount++;
                        }
                        records += myList[i].Value;
                    }
                    result[1] = RegexUtil.LetterTimelessPattern().Replace(result[1].TrimEnd(',', ' '), @"$3`$2");
                    /*
                    if (records < 10 && lineCount < 3)
                    {
                        if (lineCount > 0) result[1] += "\n";
                        result[1] += "Few results found on the market, more info on detail Tab.";
                    }
                    */
                    if (result[1].Length == 0 && records < 10) result[1] = Resources.Resources.Main012_PriceFew;//"Few results founds on the market\nPlease check detailed Tab.";
                }
            }

            Buffer.StatsFetchDetail[0] = beginFetch;
            //Buffer.StatsFetchDetail[1] += resultsLoaded;
            Buffer.StatsFetchDetail[2] = resultCount;
            Buffer.StatsFetchDetail[3] += unpriced;
            Buffer.StatsFetchDetail[4] += total;
            //Buffer.StatsFetchDetail[5] += (total - resultsLoaded);

            if (dataToFetch.Total == 0 || currencys.Count == 0)
            {
                return [Resources.Resources.Main008_PriceNoResult, string.Empty];//"There is no results."
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is ThreadAbortException || ex.Message.ToLowerInvariant().Contains("thread", StringComparison.Ordinal))
            {
                return ["Abort called before the end", "Application (Thread) ERROR "];
            }
            if (ex.InnerException is HttpRequestException exception)
            {
                string[] mess = exception.Message.Split(':');
                return [ "The request encountered" + Strings.LF + "an exception. [B]",
                    mess.Length > 1 ? "ERROR : Code " + mess[1].Trim() : exception.Message ];
            }
            if (ex.InnerException is TimeoutException except)
            {
                return [ "The request has expired", except.Message.Length > 24 ?
                    except.Message[..24].Trim() + Strings.LF + except.Message[24..].Trim() : except.Message ];
            }
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FetchResults() : Error encountered while fetching data...", MessageStatus.Error);
        }
        return result;
    }
    
    // private methods
    private string[] FillBulkVm(BulkData data, string market)
    {
        CoolDown.Apply();
        string[] result = [Resources.Resources.Main007_PriceWaiting, string.Empty];
        try
        {
            //int resultsLoaded = 0;
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

                        string key = string.Empty;
                        double amount = 0;
                        string keyName = string.Empty;
                        string ageIndex = string.Empty;
                        string whisper = string.Empty;
                        string charName = string.Empty;
                        string status = string.Empty;

                        whisper = valData.Listing.Whisper?.ToString(); // to test

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

                        status = valData.Listing.Account.Online != null ?
                            valData.Listing.Account.Online.Status : Strings.Offline;

                        charName = valData.Listing.Account.LastCharName;
                        key = valData.Listing.Offers[0].Exchange.Currency;
                        amount = valData.Listing.Offers[0].Exchange.Amount;

                        keyName = key;

                        sbWhisper.Append('/').Append(sellerAmount).Append('/').Append(sellerCurrency).Append('/').Append(buyerAmount).Append('/').Append(buyerCurrency).Append('/').Append(sellerStock).Append('/').Append(charName);
                        string content = string.Format(StrFormat.Bulk, sellerAmount, ReplaceCurrencyChars(sellerCurrency), buyerAmount, ReplaceCurrencyChars(buyerCurrency), sellerStock, charName); // account
                        string tag = string.Empty;
                        string tip = null;
                        if (Buffer.NinjaChaosEqGet > 0 && Buffer.NinjaChaosEqPay > 0)
                        {
                            double ratio = Math.Round(sellerAmount * Buffer.NinjaChaosEqGet / (buyerAmount * Buffer.NinjaChaosEqPay), 2);
                            tip = Resources.Resources.Main195_Ratio + " : " + ratio;
                            tag = ratio >= 1.2 ? "emoji_vhappy" : ratio >= 1 ? "emoji_happy" : ratio >= 0.90 ? "emoji_neutral" : ratio >= 0.80 ? "emoji_crying" : "emoji_angry";
                        }

                        Vm.Result.BulkList.Add(new ListItemViewModel { Index = Vm.Result.BulkList.Count, Content = content, ToolTip = tip, Tag = tag, FgColor = onlineStatus == Strings.Online ? status == Strings.afk ? Strings.Color.YellowGreen : Strings.Color.LimeGreen : Strings.Color.Red });
                        Vm.Result.BulkOffers.Add(new Tuple<FetchDataListing, OfferInfo>(valData.Listing, valData.Listing.Offers[0]));//sbWhisper.ToString()

                        Buffer.StatsFetchBulk[1]++;

                        string replace = @"$3`$2";
                        string cur0 = RegexUtil.LetterTimelessPattern().Replace(Buffer.ExchangeCurrency[0], replace);
                        string cur1 = RegexUtil.LetterTimelessPattern().Replace(Buffer.ExchangeCurrency[1], replace);

                        key = amount < 1 ? Math.Round(1 / amount, 1) + " " + cur1 : Math.Round(amount, 1) + " " + cur0;

                        if (currencys.TryGetValue(key, out int value))
                            currencys[key] = ++value;
                        else
                            currencys.Add(key, 1);

                        total++;
                    }
                }
            }

            Buffer.StatsFetchBulk[2] = resultCount;

            if (data.Total == 0)
            {
                result[0] = Resources.Resources.Main008_PriceNoResult;//"There is no results.";
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is ThreadAbortException || ex.Message.ToLowerInvariant().Contains("thread", StringComparison.Ordinal))
            {
                return ["Abort called before the end", "Application (Thread) ERROR "];
            }
            if (ex.InnerException is HttpRequestException exception)
            {
                string[] mess = exception.Message.Split(':');
                return [ "The request encountered" + Strings.LF + "an exception. [B]",
                    mess.Length > 1 ? "ERROR : Code " + mess[1].Trim() : exception.Message ];
            }
            if (ex.InnerException is TimeoutException except)
            {
                return [ "The request has expired", except.Message.Length > 24 ?
                    except.Message[..24].Trim() + Strings.LF + except.Message[24..].Trim() : except.Message ];
            }
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FillBulkWindow() : Error encountered while fetching data...", MessageStatus.Error);
        }

        return result;
    }

    private string[] FillShopVm(BulkData data, string market)
    {
        CoolDown.Apply();
        string[] result = [Resources.Resources.Main007_PriceWaiting, string.Empty];
        try
        {
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

                        string status = valData.Listing.Account.Online != null ?
                            valData.Listing.Account.Online.Status : Strings.Offline;

                        List<ListItemViewModel> itemList = new();
                        List<Tuple<FetchDataListing, OfferInfo>> whisperList = new();
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

                            whisper = valData.Listing.Whisper?.ToString(); // to test

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

                            charName = valData.Listing.Account.LastCharName;
                            key = offer.Exchange.Currency;
                            amount = offer.Exchange.Amount;

                            keyName = key;

                            sbWhisper.Append('/').Append(sellerAmount).Append('/').Append(sellerCurrency).Append('/').Append(buyerAmount).Append('/').Append(buyerCurrency).Append('/').Append(sellerStock).Append('/').Append(charName);
                            string content = string.Empty;
                            string tag = string.Empty;
                            string tip = null;
                            content = string.Format(StrFormat.Shop, sellerStock, ReplaceCurrencyChars(sellerCurrency), sellerAmount, buyerAmount, ReplaceCurrencyChars(buyerCurrency));
                            itemList.Add(new ListItemViewModel { Content = content, ToolTip = tip, Tag = tag, FgColor = onlineStatus == Strings.Online ? status is Strings.afk ? Strings.Color.YellowGreen : Strings.Color.LimeGreen : Strings.Color.Red });
                            whisperList.Add(new Tuple<FetchDataListing, OfferInfo>(valData.Listing, offer));

                            total++;
                        }

                        string cont = string.Format(StrFormat.ShopAccount, valData.Listing.Account.LastCharName, valData.Listing.Account.Name);
                        Vm.Result.ShopList.Add(new ListItemViewModel { Index = Vm.Result.ShopList.Count, Content = cont, ToolTip = null, Tag = string.Empty, FgColor = onlineStatus == Strings.Online ? status is Strings.afk ? Strings.Color.Yellow : Strings.Color.DeepSkyBlue : Strings.Color.DarkRed });
                        Vm.Result.ShopOffers.Add(new Tuple<FetchDataListing, OfferInfo>(valData.Listing, null));

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
            }

            //Global.StatsFetchBulk[2] = resultCount;

            if (data.Total == 0)
            {
                result[0] = Resources.Resources.Main008_PriceNoResult;//"There is no results.";
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is ThreadAbortException || ex.Message.ToLowerInvariant().Contains("thread", StringComparison.Ordinal))
            {
                return ["Abort called before the end", "Application (Thread) ERROR "];
            }
            if (ex.InnerException is HttpRequestException exception)
            {
                string[] mess = exception.Message.Split(':');
                return [ "The request encountered" + Strings.LF + "an exception. [B]",
                    mess.Length > 1 ? "ERROR : Code " + mess[1].Trim() : exception.Message ];
            }
            if (ex.InnerException is TimeoutException except)
            {
                return [ "The request has expired", except.Message.Length > 24 ?
                    except.Message[..24].Trim() + Strings.LF + except.Message[24..].Trim() : except.Message ];
            }
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "FillShopWindow() : Error encountered while fetching data...", MessageStatus.Error);
        }

        return result;
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

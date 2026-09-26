using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Prices.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultViewModel(IMessageAdapterService message, INetService net, 
    PoeApiService poeApi, DataManagerService dm, MainViewModel vm ) : ViewModelBase
{
    private readonly IMessageAdapterService _message = message;
    private readonly INetService _net = net;
    private readonly PoeApiService _poeApi = poeApi;
    private readonly DataManagerService _dm = dm;
    private readonly MainViewModel _vm = vm;

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

    [ObservableProperty]
    private bool fetchDetailIsEnabled;

    // model
    internal PricingData Data { get; private set; } = new();

    [RelayCommand]
    private async Task SearchPoeprices(object commandParameter)
    {
        string errorMsg = string.Empty;
        List<Tuple<string, string>> lines = new();
        try
        {
            PoepricesList.Clear();
            PoepricesList.Add(new("Waiting response from poeprices.info ..."));

            var result = await _net.SendHTTP(Strings.Api.PoePrice + _dm.Config.Options.League
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
            if (ex.InnerException is HttpRequestException exception)
            {
                _message.Show(ex.GetFormated(), "Poeprices error code : " + exception.StatusCode, MessageStatus.Information);
                return;
            }
            _message.Show(ex.GetFormated(), "UTF8 Deserialize error", MessageStatus.Error);
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
    private async Task TravelToHideout(object commandParameter)
    {
        if (commandParameter is null)
        {
            return;
        }

        if (commandParameter is SaleInfo saleInfo)
        {
            if (saleInfo.HideoutToken is null || saleInfo.HideoutToken.Length is 0)
            {
                _message.Show("Cannot travel to hideout : " + "\n\nYour POESESSID is missing or expired !" +
                    "\n\nFor advanced users : You can manually update your POESESSID " +
                    "under settings by using the developer manager in the authentication section.",
                    "This feature requires authentication", MessageStatus.Exclamation);
                return;
            }

            try
            {
                var sEntity = $"{{\"token\":\"{saleInfo.HideoutToken}\"}}";
                //var urlRef = Strings.TradeUrl + _vm.Form.League[_vm.Form.LeagueIndex] + "/" + _vm.Result.Data.ResultData.Id;
                var result = await _net.SendHTTP(sEntity, Strings.Api.Whisper, Client.Trade, isXml: true);
                if (result.Length > 0)
                {
                    //{"success":true}
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException exception)
                {
                    if (exception.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        return; // can be normal
                    }
                    if (exception.StatusCode is System.Net.HttpStatusCode.BadRequest)
                    {
                        _message.Show("Cannot travel to hideout :" + "\n\nIs your account correctly connected ?"
                        , "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                        return;
                    }
                    if (exception.StatusCode is System.Net.HttpStatusCode.Forbidden)
                    {
                        _message.Show("Cannot travel to hideout.", "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                        return;
                    }
                    _message.Show("Cannot travel to hideout : " + "\n\nYour POESESSID is probably missing or expired !" +
                        "\n\nYou can manually update it under settings by using the developer manager in the authentication section.",
                        "ERROR Code : " + exception.StatusCode, MessageStatus.Error);

                }
                _message.Show("Cannot travel to hideout :\n\n" +
                    string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace),
                    "Unknown error encountered", MessageStatus.Error);
            }
        }
    }

    [RelayCommand]
    private async Task Fetch(object commandParameter) // detail view
    {
        FetchDetailIsEnabled = false;
        Detail.Total = "Fetching new results...";
        var market = _vm.Form.Market[_vm.Form.MarketIndex];
        var sameUser = _vm.Form.SameUser;
        var token = _vm.TaskManager.GetPriceToken();

        ResultBar result = null;
        try
        {
            result = await _poeApi.FetchResult(this, maxFetch: 20, market, sameUser, token); // maxFetch is set to 20 by default !
        }
        catch (InvalidOperationException ex)
        {
            result = new(emptyLine: true);
            _message.Show(ex.GetFormated(), "Invalid operation", MessageStatus.Error);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is HttpRequestException exception)
            {
                result = new(exception, false);
            }
        }
        RefreshResultBar(false, result);
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

    internal async void UpdateWithApiAsync(int minimumStock)
    {
        try
        {
            int maxFetch = 0;
            var entity = new List<string>[2];

            FetchDetailIsEnabled = false;

            if (_vm.Form.Tab.QuickSelected || _vm.Form.Tab.DetailSelected)
            {
                Detail.Total = Resources.Resources.Main005_PriceResearch;
                Quick.RightString = Detail.RightString = Resources.Resources.Main006_PriceCheck;
                Quick.LeftString = Detail.LeftString = string.Empty;
                Quick.Total = string.Empty;
                DetailList.Clear();

                maxFetch = (int)_dm.Config.Options.SearchFetchDetail;
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
                    entity[0] = new() { _vm.Form.ItemExchange.GetCurrencyTag(ExchangeType.Pay) };
                    entity[1] = new() { _vm.Form.ItemExchange.GetCurrencyTag(ExchangeType.Get) };
                    maxFetch = (int)_dm.Config.Options.SearchFetchBulk;
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
            var isCurrency = _vm.Item is not null && _vm.Item.State.ExchangeCurrency; // quick or detail
            var isItemExchange = _vm.Form.Tab.BulkSelected || _vm.Form.Tab.ShopSelected;
            var usePoeApi = isItemExchange || !isCurrency;
            ResultBar resultApi = null;
            if (usePoeApi)
            {
                var priceInfo = new PricingInfo(entity, _vm.Form.League[_vm.Form.LeagueIndex]
                    , _vm.Form.Market[_vm.Form.MarketIndex], minimumStock, maxFetch, _vm.Form.SameUser, _vm.Form.Tab.BulkSelected);
                resultApi = await _poeApi.UpdateResult(_vm.Result, priceInfo);
            }
            RefreshResultBar(isItemExchange, resultApi ?? new(state: ResultBarSate.Unimplemented));
        }
        catch (Exception ex)
        {
            throw new Exception("Exception encountered : method UpdateItemPrices", ex);
        }
    }

    internal void RefreshResultBar(bool exchange, ResultBar result)
    {
        if (UpdateResultBar(exchange, result))
        {
            return;
        }

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

        int removed = Data.StatDetail.Total - Data.StatDetail.ResultLoaded;
        int unpriced = Data.StatDetail.Unpriced;
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
                FetchDetailIsEnabled = true;
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
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Result;

// ICollection
public sealed partial class ResultViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

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
    private ResultBarViewModel bulk = new(price: Resources.Resources.Main001_PriceSelect, total: Resources.Resources.Main032_cbTotalExchange);

    [ObservableProperty]
    private ResultBarViewModel shop = new(price: Resources.Resources.Main001_PriceSelect, total: string.Empty);

    [ObservableProperty]
    private ResultRateViewModel rate = new();

    [ObservableProperty]
    private ResultListIndexViewModel selectedIndex = new();

    // model
    internal PricingData Data { get; private set; } = new();

    public ResultViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal void InitData()
    {
        Data = new();
        Rate.ShowMin = false;
    }

    internal void RefreshResultBar(MainViewModel vm, bool exchange, PricingResult result)
    {
        if (result is null)
        {
            return;
        }
        if (UpdateResultBarWithEmptyResult(vm, exchange, result))
        {
            return;
        }
        if (exchange)
        {
            UpdateExchangeResultBar(vm);
            return;
        }

        int removed = Data.StatsFetchDetail[4] - Data.StatsFetchDetail[1];
        int unpriced = Data.StatsFetchDetail[3];

        if (Data.Result is null)
        {
            Data.Result = result;
        }
        else
        {
            Data.Result.UpdateResult(result);
        }

        Rate.ShowMin = Data.Result.IsFetched;
        if (Data.Result.IsFetched)
        {
            Rate.MinAmount = Data.Result.Min.Amount.ToString();
            Rate.MinCurrency = Data.Result.Min.Label;
            Rate.MinImage = Data.Result.Min.Uri;
        }

        Rate.ShowMax = Data.Result.IsMany;
        if (Data.Result.IsMany)
        {
            Rate.MaxAmount = Data.Result.Max.Amount.ToString();
            Rate.MaxCurrency = Data.Result.Max.Label;
            Rate.MaxImage = Data.Result.Max.Uri;
        }

        Quick.RightString = Detail.RightString = Data.Result.FirstLine;
        Quick.LeftString = Data.Result.SecondLine;

        if (Data.StatsFetchDetail[0] > 0)
        {
            Detail.LeftString = Resources.Resources.Main017_Results + " : " + (Data.StatsFetchDetail[0] - (removed + unpriced))
                + " " + Resources.Resources.Main018_ResultsDisplay + " / " + Data.StatsFetchDetail[0] + " " + Resources.Resources.Main019_ResultsFetched;
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
            if (Data.StatsFetchDetail[0] < Data.StatsFetchDetail[2])
            {
                vm.Form.FetchDetailIsEnabled = true;
            }
        }
        else
        {
            Detail.LeftString = string.Empty;
        }

        Detail.Total = Data.DataToFetchDetail is not null ?
            Resources.Resources.Main027_ResultsTotal + " : " + Data.StatsFetchDetail[2] + " " + Resources.Resources.Main020_ResultsListed
            + " / " + Data.DataToFetchDetail.Total + " " + Resources.Resources.Main021_ResultsMatch
            : "ERROR : Can not retreive data from official website !";

        Quick.Total = Data.StatsFetchDetail[4] > 0
            && !Quick.Total.Contains(Resources.Resources.Main011_PriceBase, StringComparison.Ordinal) ?
            Resources.Resources.Main011_PriceBase + " " + (Data.StatsFetchDetail[0] - (removed + unpriced)) + " "
            + Resources.Resources.Main017_Results.ToLowerInvariant() : string.Empty;
    }

    private bool UpdateResultBarWithEmptyResult(MainViewModel vm, bool exchange, PricingResult result)
    {
        var isEmpty = result.IsEmpty;
        if (isEmpty)
        {
            if (exchange)
            {
                if (vm.Form.Tab.BulkSelected)
                {
                    Bulk.RightString = result.FirstLine;
                    Bulk.LeftString = result.SecondLine;
                }
                if (vm.Form.Tab.ShopSelected)
                {
                    Shop.RightString = result.FirstLine;
                    Shop.LeftString = result.SecondLine;
                }
            }
            else
            {
                Quick.RightString = Detail.RightString = result.FirstLine;
                Quick.LeftString = Detail.LeftString = result.SecondLine;
            }
        }
        return isEmpty;
    }

    private void UpdateExchangeResultBar(MainViewModel vm)
    {
        if (vm.Form.Tab.BulkSelected)
        {
            Bulk.RightString = Resources.Resources.Main002_PriceLoaded;
            Bulk.LeftString = Resources.Resources.Main004_PriceRefresh;
            var str = Resources.Resources.Main017_Results + " : " + Data.StatsFetchBulk[1] + " "
                + Resources.Resources.Main018_ResultsDisplay + " / " + Data.StatsFetchBulk[2] + " "
                + Resources.Resources.Main020_ResultsListed;
            Bulk.Total = str;
            return;
        }
        if (vm.Form.Tab.ShopSelected)
        {
            Shop.RightString = Resources.Resources.Main002_PriceLoaded;
            Shop.LeftString = Resources.Resources.Main004_PriceRefresh;
        }
    }
}

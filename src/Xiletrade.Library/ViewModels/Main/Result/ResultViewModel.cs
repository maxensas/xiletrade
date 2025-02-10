using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.ViewModels.Main.Result;

// ICollection
public sealed partial class ResultViewModel : ViewModelBase
{
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

    public ResultViewModel()
    {
        
    }

    internal void InitData()
    {
        Data = new();
        Rate.ShowMin = false;
    }
}

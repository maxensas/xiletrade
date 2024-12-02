using CommunityToolkit.Mvvm.ComponentModel;
using System;
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
    private ResultPriceViewModel quick = new();

    [ObservableProperty]
    private ResultPriceViewModel detail = new();

    [ObservableProperty]
    private ResultPriceViewModel bulk = new();

    [ObservableProperty]
    private ResultPriceViewModel shop = new();

    [ObservableProperty]
    private ResultListIndexViewModel selectedIndex = new();
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.ViewModels;

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
    private PriceViewModel quick = new();

    [ObservableProperty]
    private PriceViewModel detail = new();

    [ObservableProperty]
    private PriceViewModel bulk = new();

    [ObservableProperty]
    private PriceViewModel shop = new();

    [ObservableProperty]
    private ListIndexViewModel selectedIndex = new();

    public sealed partial class PriceViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string price = string.Empty;

        [ObservableProperty]
        private string priceBis = string.Empty;

        [ObservableProperty]
        private string total = string.Empty;
    }

    public sealed partial class ListIndexViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int bulk;

        [ObservableProperty]
        private int shop;
    }
}

using System;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.ViewModels;

// ICollection
public sealed class ResultViewModel : BaseViewModel
{
    private AsyncObservableCollection<ListItemViewModel> detailList = new();
    private AsyncObservableCollection<ListItemViewModel> bulkList = new();
    private AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> bulkOffers = new();
    private AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> shopOffers = new();
    private AsyncObservableCollection<ListItemViewModel> poepricesList = new();
    private AsyncObservableCollection<ListItemViewModel> shopList = new();
    private PriceViewModel quick = new();
    private PriceViewModel detail = new();
    private PriceViewModel bulk = new();
    private PriceViewModel shop = new();
    private ListIndexViewModel selectedIndex = new();

    public AsyncObservableCollection<ListItemViewModel> DetailList { get => detailList; set => SetProperty(ref detailList, value); }
    public AsyncObservableCollection<ListItemViewModel> BulkList { get => bulkList; set => SetProperty(ref bulkList, value); }
    public AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> BulkOffers { get => bulkOffers; set => SetProperty(ref bulkOffers, value); }
    public AsyncObservableCollection<Tuple<FetchDataListing, OfferInfo>> ShopOffers { get => shopOffers; set => SetProperty(ref shopOffers, value); }
    public AsyncObservableCollection<ListItemViewModel> PoepricesList { get => poepricesList; set => SetProperty(ref poepricesList, value); }
    public AsyncObservableCollection<ListItemViewModel> ShopList { get => shopList; set => SetProperty(ref shopList, value); }
    public PriceViewModel Quick { get => quick; set => SetProperty(ref quick, value); }
    public PriceViewModel Detail { get => detail; set => SetProperty(ref detail, value); }
    public PriceViewModel Bulk { get => bulk; set => SetProperty(ref bulk, value); }
    public PriceViewModel Shop { get => shop; set => SetProperty(ref shop, value); }
    public ListIndexViewModel SelectedIndex { get => selectedIndex; set => SetProperty(ref selectedIndex, value); }

    public sealed class PriceViewModel : BaseViewModel
    {
        private string price = string.Empty;
        private string priceBis = string.Empty;
        private string total = string.Empty;

        public string Price { get => price; set => SetProperty(ref price, value); }
        public string PriceBis { get => priceBis; set => SetProperty(ref priceBis, value); }
        public string Total { get => total; set => SetProperty(ref total, value); }
    }

    public sealed class ListIndexViewModel : BaseViewModel
    {
        private int bulk;
        private int shop;

        public int Bulk { get => bulk; set => SetProperty(ref bulk, value); }
        public int Shop { get => shop; set => SetProperty(ref shop, value); }
    }
}

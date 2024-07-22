using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed class ShopViewModel : BaseViewModel
{
    private string stock = "1";
    private ExchangeViewModel exchange = new();
    private AsyncObservableCollection<ListItemViewModel> getList = new();
    private AsyncObservableCollection<ListItemViewModel> payList = new();

    public ExchangeViewModel Exchange { get => exchange; set => SetProperty(ref exchange, value); }
    public string Stock { get => stock; set => SetProperty(ref stock, value); }
    public AsyncObservableCollection<ListItemViewModel> GetList { get => getList; set => SetProperty(ref getList, value); }
    public AsyncObservableCollection<ListItemViewModel> PayList { get => payList; set => SetProperty(ref payList, value); }
}

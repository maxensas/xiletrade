namespace Xiletrade.Library.ViewModels;

public sealed class TabViewModel : BaseViewModel
{
    private bool quickEnable;
    private bool quickSelected;
    private bool detailEnable;
    private bool detailSelected;
    private bool bulkEnable;
    private bool bulkSelected;
    private bool shopEnable;
    private bool shopSelected;
    private bool poePriceEnable;
    private bool poePriceSelected;

    public bool QuickEnable { get => quickEnable; set => SetProperty(ref quickEnable, value); }
    public bool QuickSelected { get => quickSelected; set => SetProperty(ref quickSelected, value); }
    public bool DetailEnable { get => detailEnable; set => SetProperty(ref detailEnable, value); }
    public bool DetailSelected { get => detailSelected; set => SetProperty(ref detailSelected, value); }
    public bool BulkEnable { get => bulkEnable; set => SetProperty(ref bulkEnable, value); }
    public bool BulkSelected { get => bulkSelected; set => SetProperty(ref bulkSelected, value); }
    public bool ShopEnable { get => shopEnable; set => SetProperty(ref shopEnable, value); }
    public bool ShopSelected { get => shopSelected; set => SetProperty(ref shopSelected, value); }
    public bool PoePriceEnable { get => poePriceEnable; set => SetProperty(ref poePriceEnable, value); }
    public bool PoePriceSelected { get => poePriceSelected; set => SetProperty(ref poePriceSelected, value); }
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultPriceViewModel : ViewModelBase
{
    public ResultPriceViewModel(string price, string total)
    {
        Price = price;
        Total = total;
    }

    [ObservableProperty]
    private string price = string.Empty;

    [ObservableProperty]
    private string priceBis = string.Empty;

    [ObservableProperty]
    private string total;
}

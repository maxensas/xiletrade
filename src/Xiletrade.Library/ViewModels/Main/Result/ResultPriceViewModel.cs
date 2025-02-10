using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultBarViewModel : ViewModelBase
{
    public ResultBarViewModel(string price, string total)
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

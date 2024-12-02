using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultPriceViewModel : ViewModelBase
{
    [ObservableProperty]
    private string price = string.Empty;

    [ObservableProperty]
    private string priceBis = string.Empty;

    [ObservableProperty]
    private string total = string.Empty;
}

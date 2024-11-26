using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public sealed partial class BulkViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool autoSelect;

    [ObservableProperty]
    private string args;

    [ObservableProperty]
    private string currency;

    [ObservableProperty]
    private string tier;

    [ObservableProperty]
    private string stock = "1";

    [ObservableProperty]
    private ExchangeViewModel get = new();

    [ObservableProperty]
    private ExchangeViewModel pay = new();
}

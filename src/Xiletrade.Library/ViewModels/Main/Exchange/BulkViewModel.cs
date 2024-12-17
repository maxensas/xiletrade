using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class BulkViewModel(IServiceProvider serviceProvider) : ViewModelBase
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
    private ExchangeViewModel get = new(serviceProvider);

    [ObservableProperty]
    private ExchangeViewModel pay = new(serviceProvider);
}

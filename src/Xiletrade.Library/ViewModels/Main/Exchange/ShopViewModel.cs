using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ShopViewModel(IServiceProvider serviceProvider) : ViewModelBase
{
    [ObservableProperty]
    private string stock = "1";

    [ObservableProperty]
    private ExchangeViewModel exchange = new(serviceProvider);

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> getList = new();

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> payList = new();
}

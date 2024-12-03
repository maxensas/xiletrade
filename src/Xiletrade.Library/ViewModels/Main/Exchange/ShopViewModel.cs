﻿using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ShopViewModel : ViewModelBase
{
    [ObservableProperty]
    private string stock = "1";

    [ObservableProperty]
    private ExchangeViewModel exchange = new();

    [ObservableProperty]
    private AsyncObservableCollection<ListItemViewModel> getList = new();

    [ObservableProperty]
    private AsyncObservableCollection<ListItemViewModel> payList = new();
}
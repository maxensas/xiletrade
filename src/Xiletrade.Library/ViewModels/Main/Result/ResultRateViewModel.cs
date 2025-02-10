using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultRateViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool showMin;

    [ObservableProperty]
    private string minAmount = string.Empty;

    [ObservableProperty]
    private string minCurrency = string.Empty;

    [ObservableProperty]
    private Uri minImage = null;

    [ObservableProperty]
    private bool showMax;

    [ObservableProperty]
    private string maxAmount = string.Empty;

    [ObservableProperty]
    private string maxCurrency = string.Empty;

    [ObservableProperty]
    private Uri maxImage = null;
}

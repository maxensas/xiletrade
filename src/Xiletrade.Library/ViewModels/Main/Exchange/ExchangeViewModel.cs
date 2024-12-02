using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ExchangeViewModel : ViewModelBase
{
    [ObservableProperty]
    private AsyncObservableCollection<string> category = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> currency = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> tier = new();

    [ObservableProperty]
    private int categoryIndex;

    [ObservableProperty]
    private int currencyIndex;

    [ObservableProperty]
    private int tierIndex;

    [ObservableProperty]
    private Uri image;

    [ObservableProperty]
    private Uri imageLast;

    [ObservableProperty]
    private string imageLastToolTip;

    [ObservableProperty]
    private string imageLastTag;

    [ObservableProperty]
    private bool tierVisible;

    [ObservableProperty]
    private bool currencyVisible;

    [ObservableProperty]
    private string search = string.Empty;
}

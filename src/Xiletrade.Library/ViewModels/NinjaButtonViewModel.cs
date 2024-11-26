﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public sealed partial class NinjaButtonViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool visible;

    [ObservableProperty]
    private string price;

    [ObservableProperty]
    private double valWidth = 0;

    [ObservableProperty]
    private double btnWidth = 0;

    [ObservableProperty]
    private string imageName;

    [ObservableProperty]
    private string imgLeftRightMargin;
}

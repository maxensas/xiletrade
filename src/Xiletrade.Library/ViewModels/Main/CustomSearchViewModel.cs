using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class CustomSearchViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string search;

    public CustomSearchViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
}

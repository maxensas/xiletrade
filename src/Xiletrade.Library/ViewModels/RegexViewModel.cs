using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.ViewModels;

public sealed partial class RegexViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string regex;

    public RegexViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private void RemoveRegex(object commandParameter)
    {
        if (Id is 0)
        {
            return;
        }
        if (commandParameter is RegexManagerViewModel vm)
        {
            vm.RegexList.Remove(this);
        }
    }

    [RelayCommand]
    private void CopyRegex(object commandParameter)
    {
        // copy regex to poe window search bar.
        _serviceProvider.GetRequiredService<ClipboardService>().SendRegex(Regex);
    }
}

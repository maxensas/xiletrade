using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Xiletrade.Library.ViewModels;

public sealed partial class RegexViewModel : ViewModelBase
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string regex;

    [RelayCommand]
    private static void RemoveRegex(object commandParameter)
    {

    }

    [RelayCommand]
    private static void CopyRegex(object commandParameter)
    {
        // copy regex to poe window search bar.
    }
}

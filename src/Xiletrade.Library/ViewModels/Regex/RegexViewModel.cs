using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.ViewModels.Regex;

[ViewModelCreation(ViewModelCreation.Container)]
public sealed partial class RegexViewModel(IClipboardService clipboard) : ViewModelBase
{
    private readonly IClipboardService _clipboard = clipboard;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string regex;

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
        _clipboard.SendRegex(Regex);
    }
}

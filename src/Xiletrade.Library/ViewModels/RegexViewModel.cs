using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Shared;

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
        ClipboardHelper.SendRegex(Regex);
    }
}

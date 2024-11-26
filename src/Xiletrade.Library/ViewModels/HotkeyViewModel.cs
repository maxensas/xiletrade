using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed partial class HotkeyViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isEnable;

    [ObservableProperty]
    private string hotkey = string.Empty;

    [ObservableProperty]
    private string val = string.Empty;

    [ObservableProperty]
    private AsyncObservableCollection<string> list;

    [ObservableProperty]
    private int listIndex;
}

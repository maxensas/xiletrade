using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public sealed partial class ListItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string content;

    [ObservableProperty]
    private string toolTip;

    [ObservableProperty]
    private string tag;

    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private string fgColor;
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class ExpanderViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isExpanded;

    [ObservableProperty]
    private double width = 40;
}

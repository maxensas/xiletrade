using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class MinMaxViewModel : ViewModelBase
{
    [ObservableProperty]
    private string min = string.Empty;

    [ObservableProperty]
    private string max = string.Empty;

    [ObservableProperty]
    private bool selected = false;
}

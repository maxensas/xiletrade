using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelRowViewModel : ViewModelBase
{
    [ObservableProperty]
    private double armourMaxHeight = 0;

    [ObservableProperty]
    private double weaponMaxHeight = 0;

    [ObservableProperty]
    private double totalMaxHeight = 0;
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelRowViewModel : ViewModelBase
{
    [ObservableProperty]
    private double armourMaxHeight;

    [ObservableProperty]
    private double weaponMaxHeight;

    [ObservableProperty]
    private double totalMaxHeight;
}

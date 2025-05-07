using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelRowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool showArmour;

    [ObservableProperty]
    private bool showWeapon;

    [ObservableProperty]
    private bool showTotal;
}

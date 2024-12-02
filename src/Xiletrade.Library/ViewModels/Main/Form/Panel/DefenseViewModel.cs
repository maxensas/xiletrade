using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class DefenseViewModel : ViewModelBase
{
    [ObservableProperty]
    private MinMaxViewModel armour = new();

    [ObservableProperty]
    private MinMaxViewModel energy = new();

    [ObservableProperty]
    private MinMaxViewModel evasion = new();

    [ObservableProperty]
    private MinMaxViewModel ward = new();
}

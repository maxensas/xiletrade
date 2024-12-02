using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class DamageViewModel : ViewModelBase
{
    [ObservableProperty]
    private MinMaxViewModel total = new();

    [ObservableProperty]
    private MinMaxViewModel physical = new();

    [ObservableProperty]
    private MinMaxViewModel elemental = new();
}
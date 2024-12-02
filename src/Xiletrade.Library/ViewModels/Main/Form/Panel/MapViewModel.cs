using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class MapViewModel : ViewModelBase
{
    [ObservableProperty]
    private MinMaxViewModel quantity = new();

    [ObservableProperty]
    private MinMaxViewModel rarity = new();

    [ObservableProperty]
    private MinMaxViewModel packSize = new();

    [ObservableProperty]
    private MinMaxViewModel moreScarab = new();

    [ObservableProperty]
    private MinMaxViewModel moreCurrency = new();

    [ObservableProperty]
    private MinMaxViewModel moreDivCard = new();

    [ObservableProperty]
    private MinMaxViewModel moreMap = new();
}

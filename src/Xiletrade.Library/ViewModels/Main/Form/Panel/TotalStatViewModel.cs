using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class TotalStatViewModel : ViewModelBase
{
    [ObservableProperty]
    private MinMaxViewModel resistance = new();

    [ObservableProperty]
    private MinMaxViewModel life = new();

    [ObservableProperty]
    private MinMaxViewModel globalEs = new();
}

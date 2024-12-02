using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class CommonStatViewModel : ViewModelBase
{
    [ObservableProperty]
    private string itemLevelLabel = string.Empty;

    [ObservableProperty]
    private MinMaxViewModel itemLevel = new();

    [ObservableProperty]
    private MinMaxViewModel quality = new();

    [ObservableProperty]
    private SocketViewModel sockets = new();
}

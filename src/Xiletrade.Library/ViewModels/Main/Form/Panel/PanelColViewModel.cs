using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelColViewModel : ViewModelBase
{
    [ObservableProperty]
    private double firstMaxWidth = 14;

    [ObservableProperty]
    private double lastMinWidth = 86;
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool synthesisBlight;

    [ObservableProperty]
    private bool blighRavaged;

    [ObservableProperty]
    private string synthesisBlightLabel = "Synthblight";// = string.Empty;

    [ObservableProperty]
    private string blighRavagedtLabel = "Ravaged";// = string.Empty;

    [ObservableProperty]
    private string facetorMin = string.Empty;

    [ObservableProperty]
    private string facetorMax = string.Empty;

    [ObservableProperty]
    private SocketViewModel sockets = new();

    [ObservableProperty]
    private RewardViewModel reward = new();

    [ObservableProperty]
    private RowViewModel row = new();
}

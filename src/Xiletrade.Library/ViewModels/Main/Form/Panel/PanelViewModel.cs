using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

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
    private RewardViewModel reward;

    [ObservableProperty]
    private RowViewModel row = new();

    public PanelViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        reward = new(_serviceProvider);
    }
}

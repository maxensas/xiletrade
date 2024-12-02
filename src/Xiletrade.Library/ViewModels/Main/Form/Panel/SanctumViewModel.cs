using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class SanctumViewModel : ViewModelBase
{
    [ObservableProperty]
    private MinMaxViewModel resolve = new();

    [ObservableProperty]
    private MinMaxViewModel maximumResolve = new();

    [ObservableProperty]
    private MinMaxViewModel inspiration = new();

    [ObservableProperty]
    private MinMaxViewModel aureus = new();
}

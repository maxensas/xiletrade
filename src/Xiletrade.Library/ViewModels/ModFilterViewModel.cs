using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public sealed partial class ModFilterViewModel : ViewModelBase
{
    [ObservableProperty]
    private int num;

    [ObservableProperty]
    private string id;

    [ObservableProperty]
    private string type;

    [ObservableProperty]
    private string text;
}

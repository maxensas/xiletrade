using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultListIndexViewModel : ViewModelBase
{
    [ObservableProperty]
    private int bulk;

    [ObservableProperty]
    private int shop;
}

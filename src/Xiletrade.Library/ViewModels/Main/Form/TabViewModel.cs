using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class TabViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool quickEnable;

    [ObservableProperty]
    private bool quickSelected;

    [ObservableProperty]
    private bool detailEnable;

    [ObservableProperty]
    private bool detailSelected;

    [ObservableProperty]
    private bool bulkEnable;

    [ObservableProperty]
    private bool bulkSelected;

    [ObservableProperty]
    private bool shopEnable;

    [ObservableProperty]
    private bool shopSelected;

    [ObservableProperty]
    private bool poePriceEnable;

    [ObservableProperty]
    private bool poePriceSelected;
}

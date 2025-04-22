using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class TabViewModel(bool useBulk) : ViewModelBase
{
    [ObservableProperty]
    private bool quickEnable;

    [ObservableProperty]
    private bool quickSelected = !useBulk;

    [ObservableProperty]
    private bool detailEnable;

    [ObservableProperty]
    private bool detailSelected;

    [ObservableProperty]
    private bool bulkEnable = useBulk;

    [ObservableProperty]
    private bool bulkSelected = useBulk;

    [ObservableProperty]
    private bool shopEnable = useBulk;

    [ObservableProperty]
    private bool shopSelected;

    [ObservableProperty]
    private bool poePriceEnable;

    [ObservableProperty]
    private bool poePriceSelected;
}

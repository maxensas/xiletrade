using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class CheckComboViewModel : ViewModelBase
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string toolTip = string.Empty;

    [ObservableProperty]
    private string none = string.Empty;
}

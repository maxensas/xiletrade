using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class CheckComboViewModel : ViewModelBase
{
    [ObservableProperty]
    private string text = Resources.Resources.Main036_None;

    [ObservableProperty]
    private string toolTip = null;

    [ObservableProperty]
    private string none = Resources.Resources.Main036_None;
}

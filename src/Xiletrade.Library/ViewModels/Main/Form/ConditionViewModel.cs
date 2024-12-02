using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class ConditionViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool freePrefix;

    [ObservableProperty]
    private bool freeSuffix;

    [ObservableProperty]
    private bool socketColors;

    [ObservableProperty]
    private string freePrefixText = string.Empty;

    [ObservableProperty]
    private string freeSuffixText = string.Empty;

    [ObservableProperty]
    private string socketColorsText = string.Empty;

    [ObservableProperty]
    private string freePrefixToolTip = string.Empty;

    [ObservableProperty]
    private string freeSuffixToolTip = string.Empty;

    [ObservableProperty]
    private string socketColorsToolTip = string.Empty;
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class RewardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string tip = string.Empty;

    [ObservableProperty]
    private string fgColor = string.Empty;
}

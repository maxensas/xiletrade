using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class SocketViewModel : ViewModelBase
{
    [ObservableProperty]
    private string socketMin = string.Empty;

    [ObservableProperty]
    private string socketMax = string.Empty;

    [ObservableProperty]
    private string linkMin = string.Empty;

    [ObservableProperty]
    private string linkMax = string.Empty;

    [ObservableProperty]
    private string redColor = string.Empty;

    [ObservableProperty]
    private string greenColor = string.Empty;

    [ObservableProperty]
    private string blueColor = string.Empty;

    [ObservableProperty]
    private string whiteColor = string.Empty;

    [ObservableProperty]
    private bool selected;
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class SocketViewModel : ViewModelBase
{
    [ObservableProperty]
    private string socketMin;

    [ObservableProperty]
    private string socketMax;

    [ObservableProperty]
    private string linkMin;

    [ObservableProperty]
    private string linkMax;

    [ObservableProperty]
    private string redColor;

    [ObservableProperty]
    private string greenColor;

    [ObservableProperty]
    private string blueColor;

    [ObservableProperty]
    private string whiteColor;

    [ObservableProperty]
    private bool selected;
}

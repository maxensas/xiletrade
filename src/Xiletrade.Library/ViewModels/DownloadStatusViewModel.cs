using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public partial class DownloadStatusViewModel : ObservableObject
{
    [ObservableProperty]
    private int downloadProgress = 0;

    [ObservableProperty]
    private bool isIndeterminate;

    [ObservableProperty]
    private bool downloadStarted;
}

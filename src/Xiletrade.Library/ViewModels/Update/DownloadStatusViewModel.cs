using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Update;

public partial class DownloadStatusViewModel : ViewModelBase
{
    [ObservableProperty]
    private int downloadProgress = 0;

    [ObservableProperty]
    private bool isIndeterminate;

    [ObservableProperty]
    private bool downloadStarted;
}

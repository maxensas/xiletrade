using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Live;

/// <summary>
/// State shared by the toasts of the live searches : pinned toasts and live searches running.
/// </summary>
public sealed partial class LiveToastStateViewModel : ViewModelBase
{
    /// <summary>The maximum of pinned toasts is reached, until one is closed : the pin of the toasts shows it.</summary>
    [ObservableProperty]
    private bool isPinFull;

    /// <summary>Several live searches are running : the toasts show the number of their live search.</summary>
    [ObservableProperty]
    private bool hasSeveralLives;
}

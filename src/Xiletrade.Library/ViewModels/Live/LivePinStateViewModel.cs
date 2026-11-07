using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Live;

/// <summary>
/// Pinned toasts of the live searches, shared by their view models : the pin of the toasts shows when no more toast can be pinned.
/// </summary>
public sealed partial class LivePinStateViewModel : ViewModelBase
{
    /// <summary>The maximum of pinned toasts is reached, until one is closed.</summary>
    [ObservableProperty]
    private bool isFull;
}

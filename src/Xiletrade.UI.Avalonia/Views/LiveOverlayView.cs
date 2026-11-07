using Xiletrade.Library.ViewModels.Live;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

/// <summary>
/// Buttons of the live searches over the game : not available with Avalonia yet, never shown.
/// </summary>
/// <remarks>Registered so that the application starts, its content comes with the Avalonia version of the live search.</remarks>
public sealed class LiveOverlayView : ViewBase, ILiveOverlayView
{
    public LiveOverlayView(LiveOverlayViewModel vm)
    {
        DataContext = vm;
    }
}

using System;
using System.Windows;
using System.Windows.Interop;
using Xiletrade.Library.Shared.Interop.Windows;
using Xiletrade.Library.ViewModels.Live;
using Xiletrade.Library.Views;
using Xiletrade.UI.WPF.Util;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Buttons of the live searches (pause, stop) over the game, glued to the bottom right corner of the primary screen.
/// </summary>
/// <remarks>
/// Created hidden at startup, displayed while a live search is running (visibility bound to its state).
/// Tool window clicked without being activated : not listed by Alt+Tab, the game keeps the focus.
/// The toasts are stacked above it.
/// </remarks>
public partial class LiveOverlayView : ViewBase, ILiveOverlayView
{
    public LiveOverlayView(LiveOverlayViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        SourceInitialized += OnSourceInitialized;
        SizeChanged += (_, _) => UpdatePosition();
        IsVisibleChanged += (_, _) => UpdatePosition();
    }

    // before the window is visible : not listed by Alt+Tab, not activated by a click
    private void OnSourceInitialized(object sender, EventArgs e)
        => Native.AddExtendedStyle(new WindowInteropHelper(this).Handle, Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE);

    // bottom right corner of the primary screen, the toasts are stacked above the buttons while they are displayed
    private void UpdatePosition()
    {
        Left = SystemParameters.PrimaryScreenWidth - ActualWidth;
        Top = SystemParameters.PrimaryScreenHeight - ActualHeight;
        NotificationHost.ReservedBottom = IsVisible ? ActualHeight : 0;
    }
}

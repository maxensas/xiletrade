using System;
using System.Windows.Interop;
using System.Windows.Media;
using Xiletrade.Library.Shared.Interop.Windows;
using Xiletrade.Library.ViewModels.Live;
using Xiletrade.Library.Views;
using Xiletrade.UI.WPF.Util;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Buttons of the live searches (pause, stop) over the game, glued to the bottom right corner of the screen of the game.
/// </summary>
/// <remarks>
/// Created hidden at startup, displayed while a live search is running and the game or Xiletrade has the focus
/// (visibility bound to their state).
/// Tool window clicked without being activated : not listed by Alt+Tab, the game keeps the focus.
/// Scaled with the screen (same proportions whatever its size). The toasts are stacked above it.
/// </remarks>
public partial class LiveOverlayView : ViewBase, ILiveOverlayView
{
    private double _scale = 1;

    public LiveOverlayView(LiveOverlayViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        SourceInitialized += OnSourceInitialized;
        SizeChanged += (_, _) => UpdatePosition();
        IsVisibleChanged += (_, _) => OnVisibleChanged();
    }

    // before the window is visible : not listed by Alt+Tab, not activated by a click
    private void OnSourceInitialized(object sender, EventArgs e)
        => Native.AddExtendedStyle(new WindowInteropHelper(this).Handle, Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE);

    // displayed again over the game : set again, WPF puts it at the top of the topmost windows
    private void OnVisibleChanged()
    {
        if (IsVisible)
        {
            Topmost = false;
            Topmost = true;
        }
        UpdatePosition();
    }

    // bottom right corner of the screen of the game, the toasts are stacked above the buttons while they are displayed
    private void UpdatePosition()
    {
        if (!IsVisible)
        {
            NotificationHost.ReservedBottom = 0;
            return;
        }

        var screen = GameScreen.Get();
        if (screen.Scale != _scale)
        {
            // new size : placed again by SizeChanged
            _scale = screen.Scale;
            var transform = new ScaleTransform(_scale, _scale);
            transform.Freeze();
            Root.LayoutTransform = transform;
        }
        Left = screen.Bounds.Right - ActualWidth;
        Top = screen.Bounds.Bottom - ActualHeight;
        NotificationHost.ReservedBottom = ActualHeight;
    }
}

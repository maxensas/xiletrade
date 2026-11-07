using System;
using System.Windows;
using System.Windows.Interop;
using Xiletrade.Library.ViewModels.Live;
using Xiletrade.Library.Views;
using Xiletrade.UI.WPF.Util;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Buttons of the live searches (pause, stop) over the game, glued to the bottom right corner of the primary screen.
/// </summary>
/// <remarks>
/// Created hidden at startup, displayed while a live search is running (visibility bound to its state).
/// Clicked without being activated : the game keeps the focus. The toasts are stacked above it.
/// </remarks>
public partial class LiveOverlayView : ViewBase, ILiveOverlayView
{
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;

    public LiveOverlayView(LiveOverlayViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        SourceInitialized += OnSourceInitialized;
        SizeChanged += (_, _) => UpdatePosition();
        IsVisibleChanged += (_, _) => UpdatePosition();
    }

    // a click does not activate the window (no native call : message of the window answered by WPF)
    private void OnSourceInitialized(object sender, EventArgs e)
        => HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(WndProc);

    private static nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg is WM_MOUSEACTIVATE)
        {
            handled = true;
            return MA_NOACTIVATE;
        }
        return 0;
    }

    // bottom right corner of the primary screen, the toasts are stacked above the buttons while they are displayed
    private void UpdatePosition()
    {
        Left = SystemParameters.PrimaryScreenWidth - ActualWidth;
        Top = SystemParameters.PrimaryScreenHeight - ActualHeight;
        NotificationHost.ReservedBottom = IsVisible ? ActualHeight : 0;
    }
}

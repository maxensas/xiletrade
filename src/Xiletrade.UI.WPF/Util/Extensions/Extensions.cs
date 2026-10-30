using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.UI.WPF.Util.Extensions;

public static class Extensions
{
    public static Window CenterView(this Window view, double scale = 1)
    {
        var handle = new WindowInteropHelper(view).Handle;

        // work area (pixels) of the monitor of the window, the primary one if the window has no handle yet
        var info = new Native.MONITORINFO { cbSize = Marshal.SizeOf<Native.MONITORINFO>() };
        if (!Native.GetMonitorInfo(Native.MonitorFromWindow(handle, Native.MONITOR_DEFAULTTONEAREST), ref info))
        {
            return view;
        }
        var area = info.rcWork;
        var dpi = VisualTreeHelper.GetDpi(view);

        // WPF DIPs
        var widthDips = (area.Right - area.Left) * scale / dpi.DpiScaleX;
        var heightDips = (area.Bottom - area.Top) * scale / dpi.DpiScaleY;
        var leftDips = area.Left * scale / dpi.DpiScaleX;
        var topDips = area.Top * scale / dpi.DpiScaleY;

        // BAD VALUES: view.Width & view.Height
        var width = view.Width * scale;
        var height = view.Height * scale;

        view.Left = (leftDips + (widthDips - width) / 2) ;
        view.Top = (topDips + (heightDips - height) / 2) ;

        return view;
    }
}

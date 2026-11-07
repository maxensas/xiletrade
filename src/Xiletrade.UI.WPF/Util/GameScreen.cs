using System;
using System.Runtime.InteropServices;
using System.Windows;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.UI.WPF.Util;

/// <summary>
/// Screen displaying the game (Path of Exile 1 or 2) : the toasts and the buttons of the live searches are displayed on it,
/// the primary screen is used when the game is not running.
/// </summary>
/// <remarks>
/// The application is system DPI aware : the pixels of every screen are converted with the DPI of the primary screen.
/// </remarks>
internal static class GameScreen
{
    // height (DIPs) of the screen on which the toasts and the live buttons have been designed (2560 x 1440)
    private const double ReferenceHeight = 1440;
    private const double MinScale = 0.6;
    private const double MaxScale = 2;

    internal static ScreenArea Get()
    {
        var game = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
        var monitor = Native.MonitorFromWindow(game, game is 0 ? Native.MONITOR_DEFAULTTOPRIMARY : Native.MONITOR_DEFAULTTONEAREST);
        if (!TryGetInfo(monitor, out var info)
            || !TryGetInfo(Native.MonitorFromWindow(0, Native.MONITOR_DEFAULTTOPRIMARY), out var primary))
        {
            return Create(new(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight), SystemParameters.WorkArea);
        }

        // pixels for one DIP : primary screen in pixels and in DIPs
        var pixelsPerDip = (primary.rcMonitor.Bottom - primary.rcMonitor.Top) / SystemParameters.PrimaryScreenHeight;
        return Create(ToDips(info.rcMonitor, pixelsPerDip), ToDips(info.rcWork, pixelsPerDip));
    }

    private static ScreenArea Create(Rect bounds, Rect workArea)
        => new(bounds, workArea, Math.Clamp(bounds.Height / ReferenceHeight, MinScale, MaxScale));

    private static Rect ToDips(Native.RECT rect, double pixelsPerDip)
        => new(rect.Left / pixelsPerDip, rect.Top / pixelsPerDip,
            (rect.Right - rect.Left) / pixelsPerDip, (rect.Bottom - rect.Top) / pixelsPerDip);

    private static bool TryGetInfo(nint monitor, out Native.MONITORINFO info)
    {
        info = new Native.MONITORINFO { cbSize = Marshal.SizeOf<Native.MONITORINFO>() };
        return monitor is not 0 && Native.GetMonitorInfo(monitor, ref info);
    }
}

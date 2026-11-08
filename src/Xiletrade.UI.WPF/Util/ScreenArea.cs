using System.Windows;

namespace Xiletrade.UI.WPF.Util;

/// <summary>Screen in WPF units (DIPs) : whole screen, work area (without the taskbar) and scale of the content displayed on it.</summary>
/// <param name="Scale">Height of the screen compared to the reference one (1440) : the content keeps the same proportions.</param>
internal readonly record struct ScreenArea(Rect Bounds, Rect WorkArea, double Scale);

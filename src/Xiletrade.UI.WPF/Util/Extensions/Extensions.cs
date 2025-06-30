using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Xiletrade.UI.WPF.Util.Extensions;

public static class Extensions
{
    //WIP
    public static Window CenterView(this Window view, double scale = 1)
    {
        var handle = new WindowInteropHelper(view).Handle;
        var area = System.Windows.Forms.Screen.FromHandle(handle).WorkingArea;
        var dpi = VisualTreeHelper.GetDpi(view);

        // WPF DIPs
        var widthDips = area.Width * scale / dpi.DpiScaleX;
        var heightDips = area.Height * scale / dpi.DpiScaleY;
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

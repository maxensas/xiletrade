using System;
using System.Windows;
using System.Windows.Input;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Views;

/// <summary>
/// Logique d'interaction pour WhisperWindow.xaml
/// </summary>
public partial class WhisperListView : ViewBase
{
    public WhisperListView()
    {
        InitializeComponent();
        Name = Strings.WindowName.Whisper;
        Loaded += Window_Loaded;
        MouseLeftButtonDown += Window_DragWindow;
        this.Show();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var scaleFactor = GetDisplayScaleFactor(Strings.WindowName.Whisper);

        this.Left = (System.Windows.Forms.Cursor.Position.X / scaleFactor) - (this.Width / 2);
        this.Top = (System.Windows.Forms.Cursor.Position.Y / scaleFactor) - (this.Height / 2);
    }

    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private static float GetDisplayScaleFactor(string windowName)
    {
        try
        {
            IntPtr windowHandle = Native.FindWindow(null, windowName);
            return Native.GetDpiForWindow(windowHandle) / 96f;
        }
        catch
        {
            // or fallback to gdi solutions above
            return 1;
        }
        /*float dpix, dpiy;
        using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            dpix = graphics.DpiX;
            dpiy = graphics.DpiY;
        }*/
        /*
         * [dllimport("gdi32.dll")]
            static extern int getdevicecaps(intptr hdc, int nindex);
            public enum devicecap
            {
                vertres = 10,
                desktopvertres = 117,

                // http://pinvoke.net/default.aspx/gdi32/getdevicecaps.html
            }  


            private float getscalingfactor()
            {
                graphics g = graphics.fromhwnd(intptr.zero);
                intptr desktop = g.gethdc();
                int logicalscreenheight = getdevicecaps(desktop, (int)devicecap.vertres);
                int physicalscreenheight = getdevicecaps(desktop, (int)devicecap.desktopvertres); 

                float screenscalingfactor = (float)physicalscreenheight / (float)logicalscreenheight;

                return screenscalingfactor; // 1.25 = 125%
            }
         */
    }
}

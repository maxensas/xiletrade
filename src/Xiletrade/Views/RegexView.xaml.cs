using System;
using System.Windows;
using System.Windows.Input;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Views;

/// <summary>
/// Logique d'interaction pour RegexView.xaml
/// </summary>
public partial class RegexView : ViewBase
{
    public RegexView(object vm)
    {
        InitializeComponent();
        Name = Strings.WindowName.Regex;
        DataContext = vm;
        Loaded += Window_Loaded;
        MouseLeftButtonDown += Window_DragWindow;
        this.Show();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var scaleFactor = GetDisplayScaleFactor(Strings.WindowName.Regex);

        this.Left = (System.Windows.Forms.Cursor.Position.X / scaleFactor) - (this.Width / 2);
        this.Top = (System.Windows.Forms.Cursor.Position.Y / scaleFactor) - (this.Height / 2);
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
    }

    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
}

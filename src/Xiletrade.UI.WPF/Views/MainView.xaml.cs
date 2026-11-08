using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop.Windows;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Xiletrade Form : MainWindow.xaml
/// </summary>
public partial class MainView : ViewBase, IMainView
{
    // time given to WPF to render the transparent frame before hiding the window
    private const int HIDE_DELAY_MS = 100;

    private readonly DispatcherTimer _hideTimer = new() { Interval = TimeSpan.FromMilliseconds(HIDE_DELAY_MS) };

    public MainView(object vm)
    {
        InitializeComponent();
        _hideTimer.Tick += HideTimer_Tick;
        DataContext = vm;
        Application.Current.MainWindow = this;
        Closing += Window_Closing;
        MouseLeftButtonDown += Window_DragMove;
    }

    // events
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Keyboard.ClearFocus();

        // WPF stops rendering a hidden window, which displays its last frame (previous item) again on Show :
        // the window is made transparent and hidden once this transparent frame is rendered.
        Opacity = 0;
        _hideTimer.Stop();
        _hideTimer.Start();
    }

    private void HideTimer_Tick(object sender, EventArgs e)
    {
        _hideTimer.Stop();
        Visibility = Visibility.Hidden;
        Library.Shared.Common.CollectGarbage();
    }

    /// <summary>Displays the window content again, also when shown before the end of the hiding delay.</summary>
    internal void Reveal()
    {
        Show();
        ShowActivated = false;
        _hideTimer.Stop();
        Opacity = 1;
    }

    private void Window_DragMove(object sender, MouseButtonEventArgs e)
    {
        var mainHwnd = App.Services.GetRequiredService<IViewManager>().MainHandle;
        if (!Native.GetForegroundWindow().Equals(mainHwnd))
        {
            return;
        }
        try
        {
            this.DragMove();
        }
        catch (Exception)
        {
            Debug.WriteLine("Exception raised with Window.DragMove");
        }
    }
}

using System;
using System.Windows;
using System.Windows.Input;
using Xiletrade.Library.Shared.Interop;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services;
using System.Diagnostics;

namespace Xiletrade.Views;

/// <summary>
/// Xiletrade Form : MainWindow.xaml
/// </summary>
public partial class MainView : ViewBase
{
    //internal static readonly DependencyProperty CommandParameterProperty = ButtonBase.CommandParameterProperty.AddOwner(typeof(CommandSlider));

    public MainView(object vm)
    {
        InitializeComponent();
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
        this.Visibility = Visibility.Hidden;
        GC.Collect();
    }

    private void Window_DragMove(object sender, MouseButtonEventArgs e)
    {
        var mainHwnd = App.ServiceProvider.GetRequiredService<XiletradeService>().MainHwnd;
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
#if DEBUG
            Trace.WriteLine("Exception with Window.DragMove : ");
#endif
        }
    }
}

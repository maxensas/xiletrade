using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.UI.WPF.Views;

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
        Library.Shared.Common.CollectGarbage();
    }

    private void Window_DragMove(object sender, MouseButtonEventArgs e)
    {
        var mainHwnd = App.Services.GetRequiredService<XiletradeService>().MainHwnd;

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

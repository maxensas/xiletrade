using System;
using System.Windows.Input;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Logique d'interaction pour ConfigWindow.xaml
/// </summary>
public partial class ConfigView : ViewBase, IConfigView
{
    private readonly IDisposable _scope;

    public ConfigView(object vm, IDisposable scope = null)
    {
        _scope = scope;
        DataContext = vm;
        InitializeComponent();
        Name = Strings.WindowName.Config;
        MouseLeftButtonDown += Window_DragWindow;
        Closed += ConfigView_Closed;
    }
    
    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void ConfigView_Closed(object sender, EventArgs e)
    {
        Closed -= ConfigView_Closed;
        MouseLeftButtonDown -= Window_DragWindow;
        _scope?.Dispose();
    }
}

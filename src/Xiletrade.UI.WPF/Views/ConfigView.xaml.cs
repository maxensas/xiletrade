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
        Closed += OnClosed;
    }
    
    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void OnClosed(object sender, EventArgs e)
    {
        Content = null;
        DataContext = null;
        MouseLeftButtonDown -= Window_DragWindow;
        Closed -= OnClosed;
        _scope?.Dispose();
    }
}

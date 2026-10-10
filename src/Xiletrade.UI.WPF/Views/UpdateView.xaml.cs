using System;
using System.Windows.Input;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Logique d'interaction pour UpdateView.xaml
/// </summary>
public partial class UpdateView : ViewBase
{
    public UpdateView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
        MouseLeftButtonDown += Window_DragWindow;
        Closed += OnClosed;
    }

    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void OnClosed(object sender, EventArgs e)
    {
        MouseLeftButtonDown -= Window_DragWindow;
        Closed -= OnClosed;
        Content = null;
        DataContext = null;
    }
}

using System;
using System.Windows;
using System.Windows.Input;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Logique d'interaction pour RegexView.xaml
/// </summary>
public partial class RegexView : ViewBase, IRegexView
{
    public RegexView(object vm)
    {
        InitializeComponent();
        Name = Strings.WindowName.Regex;
        DataContext = vm;
        Loaded += Window_Loaded;
        MouseLeftButtonDown += Window_DragWindow;
        Closed += OnClosed;
    }

    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
        this.Top = SystemParameters.PrimaryScreenHeight / 6;
    }

    private void OnClosed(object sender, EventArgs e)
    {
        Loaded -= Window_Loaded;
        MouseLeftButtonDown -= Window_DragWindow;
        Closed -= OnClosed;
        Content = null;
        DataContext = null;
    }
}

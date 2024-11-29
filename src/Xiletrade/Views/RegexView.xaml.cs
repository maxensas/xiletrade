using System.Windows;
using System.Windows.Input;
using Xiletrade.Library.Shared;

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

    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
        this.Top = SystemParameters.PrimaryScreenHeight / 6;
    }
}

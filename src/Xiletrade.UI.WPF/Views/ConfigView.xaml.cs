using System.Windows.Input;
using Xiletrade.Library.Shared;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Logique d'interaction pour ConfigWindow.xaml
/// </summary>
public partial class ConfigView : ViewBase
{
    public ConfigView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
        Name = Strings.WindowName.Config;
        MouseLeftButtonDown += Window_DragWindow;
    }
    
    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
}

using System.Windows.Input;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Logique d'interaction pour StartWindow.xaml
/// </summary>
public partial class StartView : ViewBase
{
    public StartView()
    {
        InitializeComponent();
        MouseLeftButtonDown += Window_DragWindow;
    }

    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
}

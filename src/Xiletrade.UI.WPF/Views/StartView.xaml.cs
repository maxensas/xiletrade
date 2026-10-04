using System.Windows.Input;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Logique d'interaction pour StartWindow.xaml
/// </summary>
public partial class StartView : ViewBase, IStartView
{
    public StartView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
        MouseLeftButtonDown += Window_DragWindow;
    }

    private void Window_DragWindow(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
}

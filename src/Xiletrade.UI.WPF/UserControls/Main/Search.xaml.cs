using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xiletrade.UI.WPF.UserControls.Main;

/// <summary>
/// Logique d'interaction pour Search.xaml
/// </summary>
public partial class Search : UserControl
{
    public Search()
    {
        InitializeComponent();
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ListBox && !e.Handled)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }
}

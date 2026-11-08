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
        PreviewMouseLeftButtonDown += ClearFocusOnClickOutside;
    }

    // a click outside the field having the focus removes it (stat fields, search field, min / max) :
    // the event is not handled, the control clicked or the move of the window still get it
    private void ClearFocusOnClickOutside(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.FocusedElement is TextBox textBox && !textBox.IsMouseOver)
        {
            Keyboard.ClearFocus();
        }
    }
}

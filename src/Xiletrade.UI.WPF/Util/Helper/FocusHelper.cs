using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Xiletrade.UI.WPF.Util.Helper;

/// <summary>Helper class used by XAML view : gives the keyboard focus to a control from its view model.</summary>
public static class FocusHelper
{
    /// <summary>Set to true : the control takes the keyboard focus, the caret of a text box goes to the end.</summary>
    public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
        "IsFocused", typeof(bool), typeof(FocusHelper), new PropertyMetadata(false, OnIsFocusedChanged));

    public static bool GetIsFocused(DependencyObject element) => (bool)element.GetValue(IsFocusedProperty);

    public static void SetIsFocused(DependencyObject element, bool value) => element.SetValue(IsFocusedProperty, value);

    private static void OnIsFocusedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true || sender is not UIElement element)
        {
            return;
        }
        // once the control is displayed
        element.Dispatcher.BeginInvoke(() =>
        {
            element.Focus();
            Keyboard.Focus(element);
            if (element is TextBox textBox)
            {
                textBox.CaretIndex = textBox.Text.Length;
            }
        }, DispatcherPriority.Input);
    }
}

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xiletrade.UI.WPF.UserControls.Main;

/// <summary>
/// Logique d'interaction pour ModLine.xaml
/// </summary>
public partial class ModList : UserControl
{
    public ModList()
    {
        InitializeComponent();
    }

    // Increment or decrement with the mouse wheel. Manage decimals by holding down the Ctrl or Shift key.
    private void DecimalTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (decimal.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
            {
                decimal step = 1.0m;
                var modifiers = Keyboard.Modifiers;

                if (modifiers is ModifierKeys.Control)
                    step = 0.1m;
                else if (modifiers is ModifierKeys.Shift)
                    step = 0.01m;

                value += e.Delta > 0 ? step : -step;

                int precision = step switch
                {
                    1.0m => 0,
                    0.1m => 1,
                    0.01m => 2,
                    _ => 2
                };
                value = Math.Round(value, precision);

                string formatted = value % 1 is 0
                    ? ((int)value).ToString(CultureInfo.InvariantCulture)
                    : value.ToString($"F{precision}", CultureInfo.InvariantCulture);

                textBox.Text = formatted;

                e.Handled = true;
            }
        }
    }

    // Inverse direction and refresh template.
    private void InverseSlider_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Slider slider)
        {
            slider.IsDirectionReversed = !slider.IsDirectionReversed;

            var template = slider.Template;
            slider.Template = null;
            slider.ApplyTemplate();
            slider.Template = template;
            slider.ApplyTemplate();

            //slider.InvalidateVisual();
            //slider.UpdateLayout();
        }
    }
}

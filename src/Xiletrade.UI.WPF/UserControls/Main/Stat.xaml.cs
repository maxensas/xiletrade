using System.Windows.Controls;

namespace Xiletrade.UI.WPF.UserControls.Main;

/// <summary>
/// Logique d'interaction pour Stat.xaml
/// </summary>
public partial class Stat : UserControl
{
    public Stat()
    {
        InitializeComponent();
    }

    // Inverse direction and refresh template.
    private void InverseSlider_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Slider slider)
        {
            slider.IsDirectionReversed = !slider.IsDirectionReversed;

            var template = slider.Template;
            slider.Template = null;
            slider.ApplyTemplate(); // optionnel mais sûr
            slider.Template = template;
            slider.ApplyTemplate();

            //slider.InvalidateVisual();
            //slider.UpdateLayout();
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using System.Linq;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public class ViewBase : Window, IViewBase
{
    public ViewBase() : base()
    {

    }

    public void Center(double scale) => this.Center(scale);

    public bool? ShowDialog()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.Handled) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        // ignorer les clics provenant d'un contrôle interactif (hors fenêtre elle-même)
        if (e.Source is Visual v && v.GetSelfAndVisualAncestors()
            .Any(a => a is TemplatedControl and not Window))
            return;

        BeginMoveDrag(e);
    }
}
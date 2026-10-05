using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using System.Linq;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class EditorView : ViewBase, IEditorView
{
    public EditorView()
    {
        InitializeComponent();
    }

    public EditorView(object vm) : this()
    {
        DataContext = vm;
        PointerPressed += (s, e) =>
        {
            if (e.Handled) return;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

            // ignore clicks originating from an interactive control (outside the window itself)
            if (e.Source is Visual v && v.GetSelfAndVisualAncestors()
                .Any(a => a is TemplatedControl and not Window))
                return;

            BeginMoveDrag(e);
        };
    }
}
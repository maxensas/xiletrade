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
    }
}
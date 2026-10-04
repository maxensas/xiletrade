using Xiletrade.Library.Shared;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.WPF.Views;

/// <summary>
/// Logique d'interaction pour EditorWindow.xaml
/// </summary>
public partial class EditorView : ViewBase, IEditorView
{
    public EditorView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
        Name = Strings.WindowName.Editor;
    }
}

using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.Shared;

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

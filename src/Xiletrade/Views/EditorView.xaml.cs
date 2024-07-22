using Xiletrade.Library.Shared;

namespace Xiletrade.Views;

/// <summary>
/// Logique d'interaction pour EditorWindow.xaml
/// </summary>
public partial class EditorView : ViewBase
{
    public EditorView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
        Name = Strings.WindowName.Editor;
    }
}

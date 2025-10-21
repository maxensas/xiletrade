namespace Xiletrade.UI.Avalonia.Views;

public partial class EditorView : ViewBase
{
    public EditorView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
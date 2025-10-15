namespace Xiletrade.UI.Avalonia.Views;

public partial class WhisperListView : ViewBase
{
    public WhisperListView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
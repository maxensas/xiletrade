namespace Xiletrade.UI.Avalonia.Views;

public partial class WhisperListView : ViewBase
{
    public WhisperListView()
    {
        InitializeComponent();
    }

    public WhisperListView(object vm) : this()
    {
        DataContext = vm;
    }
}
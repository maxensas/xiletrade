namespace Xiletrade.UI.Avalonia.Views;

public partial class StartView : ViewBase
{
    public StartView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
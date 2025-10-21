namespace Xiletrade.UI.Avalonia.Views;

public partial class PopView : ViewBase
{
    public PopView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
namespace Xiletrade.UI.Avalonia.Views;

public partial class PopView : ViewBase
{
    public PopView()
    {
        InitializeComponent();
    }

    public PopView(object vm) : this()
    {
        DataContext = vm;
    }
}
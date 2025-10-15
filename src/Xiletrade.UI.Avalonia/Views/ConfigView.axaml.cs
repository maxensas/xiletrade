namespace Xiletrade.UI.Avalonia.Views;

public partial class ConfigView : ViewBase
{
    public ConfigView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
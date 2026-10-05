namespace Xiletrade.UI.Avalonia.Views;

public partial class UpdateView : ViewBase
{
    public UpdateView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
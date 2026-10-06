namespace Xiletrade.UI.Avalonia.Views;

public partial class UpdateView : ViewBase
{
    public UpdateView()
    {
        InitializeComponent();
    }

    public UpdateView(object vm) : this()
    {
        DataContext = vm;
    }
}
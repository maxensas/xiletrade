using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class StartView : ViewBase, IStartView
{
    public StartView()
    {
        InitializeComponent();
    }

    public StartView(object vm) : this()
    {
        DataContext = vm;
    }
}
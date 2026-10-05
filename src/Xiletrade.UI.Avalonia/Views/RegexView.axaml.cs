using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class RegexView : ViewBase, IRegexView
{
    public RegexView()
    {
        InitializeComponent();
    }

    public RegexView(object vm) : this()
    {
        DataContext = vm;
    }
}
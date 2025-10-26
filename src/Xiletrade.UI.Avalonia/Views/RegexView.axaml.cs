using Avalonia.Input;

namespace Xiletrade.UI.Avalonia.Views;

public partial class RegexView : ViewBase
{
    public RegexView()
    {
        InitializeComponent();
    }

    public RegexView(object vm) : this()
    {
        DataContext = vm;
        PointerPressed += Window_PointerPressed;
    }

    private void Window_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}
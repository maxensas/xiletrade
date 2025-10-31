using Avalonia.Input;

namespace Xiletrade.UI.Avalonia.Views;

public partial class StartView : ViewBase
{
    public StartView()
    {
        InitializeComponent();
    }

    public StartView(object vm) : this()
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
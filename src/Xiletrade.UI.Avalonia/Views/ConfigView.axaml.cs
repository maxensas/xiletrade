using Avalonia.Input;
using System;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class ConfigView : ViewBase, IConfigView
{
    private readonly IDisposable _scope;

    public ConfigView()
    {
        InitializeComponent();
    }

    public ConfigView(object vm, IDisposable scope = null) : this()
    {
        _scope = scope;
        DataContext = vm;
        PointerPressed += Window_PointerPressed;
        Closed += ConfigView_Closed;
    }

    private void Window_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void ConfigView_Closed(object sender, EventArgs e)
    {
        Closed -= ConfigView_Closed;
        PointerPressed -= Window_PointerPressed;
        _scope?.Dispose();
    }
}
using System;

namespace Xiletrade.UI.Avalonia.Views;

public partial class PopView : ViewBase
{
    public PopView()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public PopView(object vm) : this()
    {
        DataContext = vm;
    }

    private void OnClosed(object sender, EventArgs e)
    {
        Closed -= OnClosed;
        Content = null;
        DataContext = null;
    }
}
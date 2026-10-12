using System;

namespace Xiletrade.UI.Avalonia.Views;

public partial class UpdateView : ViewBase
{
    public UpdateView()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public UpdateView(object vm) : this()
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
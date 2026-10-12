using System;

namespace Xiletrade.UI.Avalonia.Views;

public partial class WhisperListView : ViewBase
{
    public WhisperListView()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public WhisperListView(object vm) : this()
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
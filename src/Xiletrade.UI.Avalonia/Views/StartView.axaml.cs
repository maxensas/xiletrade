using System;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class StartView : ViewBase, IStartView
{
    public StartView()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public StartView(object vm) : this()
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
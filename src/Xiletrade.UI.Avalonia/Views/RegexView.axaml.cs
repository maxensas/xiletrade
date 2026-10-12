using System;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class RegexView : ViewBase, IRegexView
{
    public RegexView()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public RegexView(object vm) : this()
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
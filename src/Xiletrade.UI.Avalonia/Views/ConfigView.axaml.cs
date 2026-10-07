using System;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class ConfigView : ViewBase, IConfigView
{
    private readonly IDisposable _scope;

    public ConfigView()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public ConfigView(object vm, IDisposable scope = null) : this()
    {
        _scope = scope;
        DataContext = vm;
    }

    private void OnClosed(object sender, EventArgs e)
    {
        Closed -= OnClosed;
        _scope?.Dispose();
        Content = null;
        DataContext = null;
    }
}
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
        Closed += ConfigView_Closed;
    }

    private void ConfigView_Closed(object sender, EventArgs e)
    {
        Closed -= ConfigView_Closed;
        _scope?.Dispose();
    }
}
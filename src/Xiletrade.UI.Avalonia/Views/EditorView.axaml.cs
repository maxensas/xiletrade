using System;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class EditorView : ViewBase, IEditorView
{
    public EditorView()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public EditorView(object vm) : this()
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
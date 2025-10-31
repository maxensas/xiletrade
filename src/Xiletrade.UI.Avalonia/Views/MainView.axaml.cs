using Avalonia.Input;
using System;

namespace Xiletrade.UI.Avalonia.Views;

public partial class MainView : ViewBase
{
    public MainView()
    {
        InitializeComponent();
    }

    public MainView(object vm) : this()
    {
        DataContext = vm;
        //Application.Current.MainWindow = this;
        Closing += Window_Closing;
        Loaded += MainView_Loaded;
        PointerPressed += Window_PointerPressed;
    }

    private void MainView_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsVisible = false;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        //Keyboard.ClearFocus();
        IsVisible = false;
        GC.Collect();
    }

    private void Window_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}
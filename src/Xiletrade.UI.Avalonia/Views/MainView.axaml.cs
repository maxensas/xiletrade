using System;

namespace Xiletrade.UI.Avalonia.Views;

public partial class MainView : ViewBase
{
    public MainView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
        //Application.Current.MainWindow = this;
        Closing += Window_Closing;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        //Keyboard.ClearFocus();
        //this.Visibility = Visibility.Hidden;
        IsVisible = false;
        GC.Collect();
    }
}
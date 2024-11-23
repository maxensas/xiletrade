using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace Xiletrade.Updater.Views;

public partial class MainWindow : Window
{
    /// <remarks>
    /// Does not use reflection to allow trimming for AOT publishing.
    /// </remarks>
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    public static readonly RoutedEvent<RoutedEventArgs> LaunchRoutedEvent =
    RoutedEvent.Register<MainWindow, RoutedEventArgs>(nameof(LaunchRouted), RoutingStrategies.Direct);
    public event EventHandler<RoutedEventArgs>? LaunchRouted
    {
        add => AddHandler(LaunchRoutedEvent, value);
        remove => RemoveHandler(LaunchRoutedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> CloseRoutedEvent =
    RoutedEvent.Register<MainWindow, RoutedEventArgs>(nameof(CloseRouted), RoutingStrategies.Direct);
    public event EventHandler<RoutedEventArgs>? CloseRouted
    {
        add => AddHandler(CloseRoutedEvent, value);
        remove => RemoveHandler(CloseRoutedEvent, value);
    }
    
    public void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        RoutedEventArgs routedEventArgs = new(routedEvent: LaunchRoutedEvent, source: sender);
        //(sender as Window)?.RaiseEvent(routedEventArgs);
        MainGrid.RaiseEvent(routedEventArgs);
        e.Handled = true;
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        RoutedEventArgs routedEventArgs = new(routedEvent: CloseRoutedEvent, source: sender);
        //(sender as Window)?.RaiseEvent(routedEventArgs);
        MainGrid.RaiseEvent(routedEventArgs);
    }
}
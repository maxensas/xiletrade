using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Notification.Core;
using System;

namespace Xiletrade.UI.Avalonia.Views;

public partial class MainView : ViewBase
{
    private static IServiceProvider _serviceProvider;

    public MainView()
    {
        InitializeComponent();
    }

    public MainView(IServiceProvider serviceProvider,object vm) : this()
    {
        _serviceProvider = serviceProvider;
        DataContext = vm;
        //Application.Current.MainWindow = this;
        Closing += Window_Closing;
        Loaded += MainView_Loaded;
        PointerPressed += Window_PointerPressed;
    }

    private void MainView_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        //IsVisible = false;
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

    // TEMP : ONLY FOR UI TESTS
    private void OnTestNotif(object sender, RoutedEventArgs e)
    {
        //_ = _serviceProvider.GetRequiredService<DataUpdaterService>().UpdateAsync();
        var notif = new NotificationRequest() { 
            Title = "Xiletrade : " + Library.Resources.Resources.Main192_DownloadOk, 
            Message = Library.Resources.Resources.Main190_FiltersOk, 
            Type = NotificationType.Success, 
            ShowCloseButton = false };
        _serviceProvider.GetRequiredService<INotificationService>().Show(notif);
    }

    private async void OnTestMessage(object sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("Title", "Hello from Avalonia!", ButtonEnum.Ok);

        await box.ShowAsync();
    }
}
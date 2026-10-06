using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Notification.Core;
using System;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.Views;

public partial class MainView : ViewBase, IMainView
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
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        //Keyboard.ClearFocus();
        IsVisible = false;
        GC.Collect();
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
}
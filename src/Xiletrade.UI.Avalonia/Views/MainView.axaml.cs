using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
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
        var title = "Xiletrade : " + Library.Resources.Resources.Main192_DownloadOk;
        var mess = new ToastNotification(title, Library.Resources.Resources.Main190_FiltersOk, NotificationStatus.Success);
        _serviceProvider.GetRequiredService<INotificationService>().Show(mess);
    }
}
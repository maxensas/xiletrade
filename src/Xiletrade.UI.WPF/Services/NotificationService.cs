using FontAwesome5;
using Microsoft.Extensions.DependencyInjection;
using Notification.Wpf;
using System;
using System.Windows.Media;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.UI.WPF.Services;

public sealed class NotificationService : INotificationService // will evolve
{
    private static IServiceProvider _serviceProvider;

    private static NotificationManager Manager { get; set; } = new();
    private static int DisplayTime { get; set; } = 10; // in secodns

    private static SvgAwesome IconOk { get; set; } = new() { Icon = EFontAwesomeIcon.Solid_Check, Height = 20, Foreground = new SolidColorBrush(Colors.LimeGreen) };
    private static SvgAwesome IconKo { get; set; } = new() { Icon = EFontAwesomeIcon.Solid_Times, Height = 20, Foreground = new SolidColorBrush(Colors.Red) };
    private static SvgAwesome IconError { get; set; } = new() { Icon = EFontAwesomeIcon.Solid_ExclamationTriangle, Height = 20, Foreground = new SolidColorBrush(Colors.Orange) };

    public NotificationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Send(string title, string message, Notify type)
    {
        NotificationContent clickContent = new()
        {
            Title = title,
            Message = message,
            RowsCount = 1,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF252525")),
            Icon = type is Notify.Ok ? IconOk : type is Notify.Ko ? IconKo : IconError
        };

        var action = new Action(() => { Manager.Show(clickContent, expirationTime: TimeSpan.FromSeconds(DisplayTime), ShowXbtn: false); });
        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(action);
    }
    /*
    public static void SendWithAction(string title, string message, Notification type)
    {
        var action = new Action(() =>
        {
            NotificationContent clickContent = new()
            {
                Title = title,
                Message = message,
                RowsCount = 1,
                Icon = type is Notification.Ok ? IconOk : type is Notification.Ko ? IconKo : IconError,
                RightButtonAction = SomeAction,
                RightButtonContent = "Launch action",
                TrimType = NotificationTextTrimType.Attach
            };

            Manager.Show(clickContent, expirationTime: TimeSpan.FromSeconds(DisplayTime), ShowXbtn: false);
        });
        Common.DelegateActionToUiThread(action);
    }
    */
    /*
    public static void SomeAction()
    {
        //not used
    }
    */
}

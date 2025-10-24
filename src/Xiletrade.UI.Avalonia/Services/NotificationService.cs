using Message.Avalonia;
using Message.Avalonia.Models;
using System;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.UI.Avalonia.Services;

public class NotificationService : INotificationService
{
    private static TimeSpan DisplayTime => TimeSpan.FromSeconds(10);

    public void Send(string title, string message, Notify type)
    {
        var options = new MessageOptions
        {
            Duration = DisplayTime,
            Title = title
        };
        switch (type)
        {
            case Notify.Ok:
                MessageManager.Default.ShowSuccessMessage(message, options);
                break;
            case Notify.Ko:
                MessageManager.Default.ShowErrorMessage(message, options);
                break;
            case Notify.Error:
                MessageManager.Default.ShowWarningMessage(message, options);
                break;
            default:
                MessageManager.Default.ShowInformationMessage(message, options);
                break;
        }
    }
}

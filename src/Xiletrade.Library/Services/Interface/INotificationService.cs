using System;
using System.Collections.Generic;
using Xiletrade.Library.Models.Application.Notification;

namespace Xiletrade.Library.Services.Interface;

public interface INotificationService
{
    IReadOnlyList<ToastNotification> Current { get; }
    event EventHandler<ToastNotification> NotificationAdded;
    event EventHandler<string> NotificationRemoved;
    void Show(ToastNotification notification);
    bool Dismiss(string id);
    void DismissAll();
}

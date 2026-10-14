using System;

namespace Xiletrade.Library.Models.Application.Notification;

public interface INotificationHost : IDisposable
{
    void OnAdded(object sender, ToastNotification notification);
    void OnRemoved(object sender, string id);
}
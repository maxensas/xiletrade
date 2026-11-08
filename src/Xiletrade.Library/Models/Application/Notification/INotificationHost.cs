using System;

namespace Xiletrade.Library.Models.Application.Notification;

public interface INotificationHost : IDisposable
{
    void OnAdded(object sender, ToastNotification notification);
    /// <param name="fade">False : closed at once (replaced by a newer toast for instance).</param>
    void OnRemoved(string id, bool fade);
}
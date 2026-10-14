using System;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Application.Notification;

public sealed record ToastNotification
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = "";
    public string Message { get; init; } = "";
    public NotificationStatus Status { get; init; } = NotificationStatus.Information;
    public TimeSpan? Duration { get; init; } = TimeSpan.FromSeconds(5);
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public ToastNotification() 
    { 

    }

    public ToastNotification(string title, string message, 
        NotificationStatus status = NotificationStatus.Information, TimeSpan? duration = null)
    {
        Title = title;
        Message = message;
        Status = status;
        Duration = duration ?? TimeSpan.FromSeconds(5);
    }
}

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

    /// <summary>
    /// View model displayed by its template instead of the title and the message, which stay as a text version
    /// (UI without template). Such a toast is closed by its own buttons, not by a click.
    /// </summary>
    public object Content { get; init; }

    /// <summary>A sound is played when the toast is displayed.</summary>
    public bool PlaySound { get; init; }

    /// <summary>Pinned by the user : golden border (its duration and placement are set by its sender).</summary>
    public bool IsPinned { get; init; }

    /// <summary>Place of the toast on the screen : bottom right by default.</summary>
    public NotificationPlacement Placement { get; init; } = NotificationPlacement.BottomRight;

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

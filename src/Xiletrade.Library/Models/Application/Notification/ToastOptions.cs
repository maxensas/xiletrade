using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Application.Notification;

public sealed record ToastOptions
{
    /// <summary>
    /// Fade duration in milliseconds
    /// </summary>
    public int FadeDuration { get; init; } = 180;
    public double Width { get; init; } = 360;
    public double Margin { get; init; } = 16;
    public double Gap { get; init; } = 10;
    public double TitleFontSize { get; init; } = 14;
    public double MessageFontSize { get; init; } = 13;
    /// <summary>
    /// Return hex code color
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public string GetStatusColor(NotificationStatus status) => status switch
    {
        NotificationStatus.Success => "#013E01",
        NotificationStatus.Warning => "#965F14",
        NotificationStatus.Error => "#820014",
        _ => "#2D4664"
    };
}

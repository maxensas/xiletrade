namespace Xiletrade.Library.Shared.Enum;

/// <summary>Place of a toast on the primary screen, the toasts of a same placement are stacked.</summary>
public enum NotificationPlacement
{
    /// <summary>Bottom right of the work area, the newest at the bottom : the stack grows upwards.</summary>
    BottomRight,
    /// <summary>Top right of the work area, the newest at the top : the stack grows downwards.</summary>
    TopRight,
    /// <summary>Right, from a quarter of the screen height starting from the bottom, the newest at the bottom : the stack grows upwards.</summary>
    LowerQuarterRight
}

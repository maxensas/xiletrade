namespace Xiletrade.Library.Shared.Enum;

/// <summary>Reason why a live search ended.</summary>
public enum LiveSearchEnd
{
    /// <summary>Stopped by the user, or by a new live search.</summary>
    Stopped,
    /// <summary>Connection refused by the server (session missing or expired).</summary>
    Refused,
    /// <summary>Connection lost and not restored.</summary>
    ConnectionLost
}

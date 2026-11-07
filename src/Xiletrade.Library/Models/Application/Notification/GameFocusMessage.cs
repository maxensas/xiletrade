namespace Xiletrade.Library.Models.Application.Notification;

/// <summary>
/// The game or a window of Xiletrade has the focus (as the shortcuts, see the input service) :
/// the toasts and the buttons of the live searches are displayed, hidden otherwise.
/// </summary>
public sealed record GameFocusMessage(bool HasFocus);

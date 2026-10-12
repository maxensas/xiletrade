using System;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Service responsible for launching features when a registered hotkey is pressed.
/// </summary>
public interface IShortcutDispatcher
{
    Action<int, nint> HandleMessage {  get; }
}

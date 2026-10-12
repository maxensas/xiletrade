namespace Xiletrade.Library.Models.Application.Keyboard;

/// <summary>
/// Platform-independent keyboard input.
/// </summary>
public readonly record struct KeyboardInput(Keys Key, Keys Modifiers, bool IsDown, bool IsRepeat = false);
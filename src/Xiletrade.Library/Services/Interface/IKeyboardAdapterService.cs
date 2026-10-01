using System;
using Xiletrade.Library.Models.Application.Hotkey.Converter;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides platform-specific keyboard input conversion.
/// </summary>
public interface IKeyboardAdapterService
{
    IKeysConverter Converter { get; }

    string GetKeyPressed(EventArgs e);
    int GetModifierCode(string textMod);
    string GetModifierText(int modifier);
    void ClearFocus();
}

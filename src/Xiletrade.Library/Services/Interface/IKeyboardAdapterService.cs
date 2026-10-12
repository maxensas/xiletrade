using Xiletrade.Library.Models.Application.Keyboard;
using Xiletrade.Library.Models.Application.Keyboard.Converter;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides keyboard input conversion without platform or framework dependency
/// </summary>
public interface IKeyboardAdapterService
{
    IKeysConverter Converter { get; }

    string GetKeyPressed(KeyboardInput input);
    int GetModifierCode(string textMod);
    string GetModifierText(int modifier);
}

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xiletrade.Library.Models.Application.Keyboard;
using Xiletrade.Library.Models.Application.Keyboard.Converter;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

public sealed class KeyboardAdapterService : IKeyboardAdapterService
{
    // Bitmask convention for modifier codes (identical to RegisterHotKey, but no API is called here)
    private const int MOD_NONE = 0x0;
    private const int MOD_ALT = 0x1;
    private const int MOD_CONTROL = 0x2;
    private const int MOD_SHIFT = 0x4;

    private static readonly HashSet<Keys> ModifierKeys =
    [
        Keys.ShiftKey, Keys.ControlKey, Keys.Menu,
        Keys.LShiftKey, Keys.RShiftKey, Keys.LControlKey, Keys.RControlKey,
        Keys.LMenu, Keys.RMenu, Keys.LWin, Keys.RWin
    ];

    public IKeysConverter Converter { get; private set; }

    public KeyboardAdapterService(ILogger<KeyboardAdapterService> logger)
    {
        Converter = new KeysConverter();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public string GetKeyPressed(KeyboardInput input)
    {
        Keys key = input.Key & Keys.KeyCode;

        if (!input.IsDown || key == Keys.None || ModifierKeys.Contains(key))
        {
            return string.Empty;
        }

        Keys keys = key | (input.Modifiers & Keys.Modifiers);

        string text = Converter.ConvertToInvariantString(keys) ?? string.Empty;
        return VerifyHotKey(text) ? text : string.Empty;
    }

    public int GetModifierCode(string modifier)
    {
        if (string.IsNullOrWhiteSpace(modifier))
        {
            return MOD_NONE;
        }

        return TryConvert(modifier.Trim().TrimEnd('+'), out Keys keys) ? ToModifierCode(keys) : MOD_NONE;
    }

    public string GetModifierText(int modifier)
    {
        Keys mods = ToKeysModifiers(modifier);
        if (mods is Keys.None)
        {
            return string.Empty;
        }

        // Formatting delegated to the converter (language-independent), with a dummy key removed afterwards
        string text = Converter.ConvertToInvariantString(mods | Keys.A) ?? string.Empty;
        int idx = text.LastIndexOf('+');
        return idx < 0 ? string.Empty : text[..(idx + 1)];
    }

    private bool VerifyHotKey(string hotKeyText)
    {
        if (string.IsNullOrWhiteSpace(hotKeyText) || hotKeyText.EndsWith('+'))
        {
            return false;
        }

        return TryConvert(hotKeyText, out Keys keys) && (keys & Keys.KeyCode) != Keys.None;
    }

    private bool TryConvert(string text, out Keys keys)
    {
        keys = Keys.None;
        try
        {
            if (Converter.ConvertFromInvariantString(text) is Keys k)
            {
                keys = k;
                return true;
            }
        }
        catch // unhandled converter exceptions
        {
        }
        return false;
    }

    private static int ToModifierCode(Keys keys)
    {
        int mod = MOD_NONE;
        if (keys.HasFlag(Keys.Control)) mod |= MOD_CONTROL;
        if (keys.HasFlag(Keys.Alt)) mod |= MOD_ALT;
        if (keys.HasFlag(Keys.Shift)) mod |= MOD_SHIFT;
        return mod;
    }

    private static Keys ToKeysModifiers(int modifier)
    {
        Keys mods = Keys.None;
        if ((modifier & MOD_CONTROL) != 0) mods |= Keys.Control;
        if ((modifier & MOD_ALT) != 0) mods |= Keys.Alt;
        if ((modifier & MOD_SHIFT) != 0) mods |= Keys.Shift;
        return mods;
    }
}
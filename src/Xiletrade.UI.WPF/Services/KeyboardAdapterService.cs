using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xiletrade.Library.Models.Application.Keyboard;
using Xiletrade.Library.Models.Application.Keyboard.Converter;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Services;

/// <summary>
/// Service used to access System.Windows.Input
/// </summary>
/// <remarks>
/// Deprecated
/// </remarks>
public sealed class KeyboardAdapterService : IKeyboardAdapterService
{
    public IKeysConverter Converter { get; private set; }

    public KeyboardAdapterService(ILogger<KeyboardAdapterService> logger)
    {
        Converter = new KeysConverter();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void ClearFocus() => Keyboard.ClearFocus();

    public string GetKeyPressed(EventArgs e)
    {
        string keyPressed = string.Empty;
        if (e is KeyEventArgs keyArg)
        {
            List<Key> modKeyList = new()
            {
                Key.LeftShift, Key.RightShift, Key.LeftCtrl, Key.RightCtrl,
                Key.LWin, Key.RWin, Key.LeftAlt, Key.RightAlt
            };

            var key = keyArg.Key switch
            {
                Key.System => keyArg.SystemKey,
                Key.ImeProcessed => keyArg.ImeProcessedKey,
                Key.DeadCharProcessed => keyArg.DeadCharProcessedKey,
                _ => keyArg.Key,
            };

            bool isModKey = modKeyList.Contains(key);

            if (keyArg.IsDown && !modKeyList.ToArray().Contains(key))
            {
                var modifiers = new List<ModifierKeys>();
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && !isModKey)
                {
                    modifiers.Add(ModifierKeys.Control);
                }

                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) && !isModKey)
                {
                    modifiers.Add(ModifierKeys.Alt);
                }

                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !isModKey)
                {
                    modifiers.Add(ModifierKeys.Shift);
                }

                string modifStr = System.ComponentModel.TypeDescriptor.GetConverter(typeof(ModifierKeys)).ConvertToString(Keyboard.Modifiers);
                string hotKey = modifiers.Count is 0 ? string.Format("{0}", key)
                    : string.Format("{0}+{1}", modifStr, key);

                if (VerifyHotKey(hotKey))
                {
                    if (hotKey.Length is 2 && hotKey.StartsWith('D')) // D0 to D9
                    {
                        hotKey = hotKey.Replace("D", string.Empty);
                    }
                    keyPressed = hotKey;
                }
            }
            keyArg.Handled = true;
        }
        return keyPressed;
    }

    private static readonly int MOD_NONE = 0x0;    // No modifier
    private static readonly int MOD_ALT = 0x1;     // If bit 0 is set, Alt is pressed
    private static readonly int MOD_CONTROL = 0x2; // If bit 1 is set, Ctrl is pressed
    private static readonly int MOD_SHIFT = 0x4;   // If bit 2 is set, Shift is pressed 

    //private static readonly int MOD_WIN = 0x8;   // If bit 3 is set, Win is pressed

    public int GetModifierCode(string modifier)
    {
        static bool GetMod(string text, ModifierKeys modkey)
        {
            var mkc = System.ComponentModel.TypeDescriptor.GetConverter(typeof(ModifierKeys));
            return text.ToLowerInvariant().Contains(mkc.ConvertToString(modkey).ToLowerInvariant(), StringComparison.Ordinal);
        }

        int mod = MOD_NONE;
        if (GetMod(modifier, ModifierKeys.Control))
        {
            mod |= MOD_CONTROL;
        }
        if (GetMod(modifier, ModifierKeys.Alt))
        {
            mod |= MOD_ALT;
        }
        if (GetMod(modifier, ModifierKeys.Shift))
        {
            mod |= MOD_SHIFT;
        }
        return mod;
    }

    public string GetModifierText(int modifier)
    {
        string returnVal = string.Empty;
        var modifiers = Enum.Parse<ModifierKeys>(modifier.ToString());

        if (modifiers.HasFlag(ModifierKeys.Control) || modifiers.HasFlag(ModifierKeys.Alt) || modifiers.HasFlag(ModifierKeys.Shift))
        {
            returnVal += System.ComponentModel.TypeDescriptor.GetConverter(typeof(ModifierKeys)).ConvertToString(modifiers) + "+";
        }

        return returnVal;
    }

    private bool VerifyHotKey(string hotKeyText)
    {
        if (hotKeyText.EndsWith('+')) // cannot set '+' as hotkey : ok for OemPlus & NumpadPlus
        {
            return false;
        }
        try
        {
            var returnKey = (int)Converter.ConvertFromInvariantString(hotKeyText);
            return true;
        }
        catch // exception not used
        {
            return false;
        }
    }

    public string GetKeyPressed(KeyboardInput input)
    {
        throw new NotImplementedException();
    }
}

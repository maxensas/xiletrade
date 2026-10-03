using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Xiletrade.Library.Models.Application.Keyboard;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class KeyEventArgsToKeyboardInputConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not KeyEventArgs e)
        {
            return null;
        }

        var key = e.Key switch
        {
            Key.System => e.SystemKey,
            Key.ImeProcessed => e.ImeProcessedKey,
            Key.DeadCharProcessed => e.DeadCharProcessedKey,
            _ => e.Key,
        };

        // Keys has the same values ​​as the VK_* codes.
        var keys = (Keys)KeyInterop.VirtualKeyFromKey(key);

        return new KeyboardInput(keys, ToKeysModifiers(Keyboard.Modifiers), e.IsDown, e.IsRepeat);
    }

    private static Keys ToKeysModifiers(ModifierKeys modifiers)
    {
        Keys result = Keys.None;
        if (modifiers.HasFlag(ModifierKeys.Control)) result |= Keys.Control;
        if (modifiers.HasFlag(ModifierKeys.Alt)) result |= Keys.Alt;
        if (modifiers.HasFlag(ModifierKeys.Shift)) result |= Keys.Shift;
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
using Avalonia.Input;
using System.Collections.Generic;

namespace Xiletrade.UI.Avalonia.Util;

public static class Extension
{
    private static readonly Dictionary<KeyModifiers, string> _map = new()
    {
        [KeyModifiers.None] = "",
        [KeyModifiers.Control] = "Control",
        [KeyModifiers.Shift] = "Shift",
        [KeyModifiers.Alt] = "Alt",
        [KeyModifiers.Meta] = "Windows"
    };

    public static string ToReadableString(this KeyModifiers modifiers)
    {
        if (modifiers == KeyModifiers.None)
            return "";

        var parts = new List<string>();
        foreach (var kvp in _map)
        {
            if (kvp.Key != KeyModifiers.None && (modifiers & kvp.Key) != 0)
                parts.Add(kvp.Value);
        }

        return string.Join(", ", parts);
    }
}


namespace Xiletrade.Library.Services.Linux.Helper;

/// <summary>
/// Translates the Windows virtual-key codes stored in the configuration (Keys enum)
/// <br/>and the RegisterHotKey modifier bitmask into their X11 equivalents.
/// </summary>
internal static class X11Keys
{
    // Modifier masks of X.h : same values as in the X11 interop class, repeated here so that this helper
    // stays free of any platform-specific type and can be used (and tested) on every platform.
    private const uint ShiftMask = 1u << 0;
    private const uint ControlMask = 1u << 2;
    private const uint Mod1Mask = 1u << 3; // Alt
    private const uint Mod4Mask = 1u << 6; // Super / Windows

    // Windows modifier bitmask (same convention as KeyboardAdapterService / RegisterHotKey)
    internal const int WinModAlt = 0x1;
    internal const int WinModControl = 0x2;
    internal const int WinModShift = 0x4;
    internal const int WinModWin = 0x8;

    // Windows virtual-key codes needed to send keys to the game
    internal const ushort VkBack = 0x08;
    internal const ushort VkReturn = 0x0D;
    internal const ushort VkMenu = 0x12; // ALT
    internal const ushort VkHome = 0x24;
    internal const ushort VkDelete = 0x2E;
    internal const ushort VkC = 0x43;
    internal const ushort VkF = 0x46;
    internal const ushort VkV = 0x56;
    internal const ushort VkX = 0x58;
    internal const ushort VkRShift = 0xA1;
    internal const ushort VkLControl = 0xA2;

    // Bits of the X11 modifier state compared when an event is received:
    // CapsLock (Lock) and NumLock (Mod2) are ignored.
    internal const uint RelevantStateMask = 0x1 | 0x4 | 0x8 | 0x40; // Shift | Control | Mod1 | Mod4

    /// <summary>
    /// Converts a RegisterHotKey modifier bitmask into an X11 modifier mask.
    /// </summary>
    internal static uint ToX11Modifiers(int windowsModifiers)
    {
        uint mask = 0;
        if ((windowsModifiers & WinModAlt) != 0) mask |= Mod1Mask;
        if ((windowsModifiers & WinModControl) != 0) mask |= ControlMask;
        if ((windowsModifiers & WinModShift) != 0) mask |= ShiftMask;
        if ((windowsModifiers & WinModWin) != 0) mask |= Mod4Mask;
        return mask;
    }

    /// <summary>
    /// Gets the X11 keysym for a Windows virtual-key code.
    /// </summary>
    /// <returns>False when the key has no known equivalent.</returns>
    internal static bool TryGetKeysym(int virtualKey, out nuint keysym)
    {
        keysym = 0;

        // 0-9 : same as ASCII
        if (virtualKey is >= 0x30 and <= 0x39)
        {
            keysym = (nuint)virtualKey;
            return true;
        }
        // A-Z : keysyms are lowercase letters
        if (virtualKey is >= 0x41 and <= 0x5A)
        {
            keysym = (nuint)(virtualKey + 0x20);
            return true;
        }
        // F1-F24
        if (virtualKey is >= 0x70 and <= 0x87)
        {
            keysym = (nuint)(0xFFBE + (virtualKey - 0x70));
            return true;
        }
        // Numpad 0-9
        if (virtualKey is >= 0x60 and <= 0x69)
        {
            keysym = (nuint)(0xFFB0 + (virtualKey - 0x60));
            return true;
        }

        uint value = virtualKey switch
        {
            0x08 => 0xFF08u, // Back      -> BackSpace
            0x09 => 0xFF09u, // Tab
            0x0C => 0xFF0Bu, // Clear
            0x0D => 0xFF0Du, // Return
            0x10 => 0xFFE1u, // ShiftKey  -> Shift_L
            0x11 => 0xFFE3u, // ControlKey-> Control_L
            0x12 => 0xFFE9u, // Menu      -> Alt_L
            0x13 => 0xFF13u, // Pause
            0x14 => 0xFFE5u, // Capital   -> Caps_Lock
            0x1B => 0xFF1Bu, // Escape
            0x20 => 0x0020u, // Space
            0x21 => 0xFF55u, // Prior     -> Page_Up
            0x22 => 0xFF56u, // Next      -> Page_Down
            0x23 => 0xFF57u, // End
            0x24 => 0xFF50u, // Home
            0x25 => 0xFF51u, // Left
            0x26 => 0xFF52u, // Up
            0x27 => 0xFF53u, // Right
            0x28 => 0xFF54u, // Down
            0x2C => 0xFF61u, // Snapshot  -> Print
            0x2D => 0xFF63u, // Insert
            0x2E => 0xFFFFu, // Delete
            0x5B => 0xFFEBu, // LWin      -> Super_L
            0x5C => 0xFFECu, // RWin      -> Super_R
            0x5D => 0xFF67u, // Apps      -> Menu
            0x6A => 0xFFAAu, // Multiply  -> KP_Multiply
            0x6B => 0xFFABu, // Add       -> KP_Add
            0x6C => 0xFFACu, // Separator -> KP_Separator
            0x6D => 0xFFADu, // Subtract  -> KP_Subtract
            0x6E => 0xFFAEu, // Decimal   -> KP_Decimal
            0x6F => 0xFFAFu, // Divide    -> KP_Divide
            0x90 => 0xFF7Fu, // NumLock
            0x91 => 0xFF14u, // Scroll    -> Scroll_Lock
            0xA0 => 0xFFE1u, // LShiftKey -> Shift_L
            0xA1 => 0xFFE2u, // RShiftKey -> Shift_R
            0xA2 => 0xFFE3u, // LControl  -> Control_L
            0xA3 => 0xFFE4u, // RControl  -> Control_R
            0xA4 => 0xFFE9u, // LMenu     -> Alt_L
            0xA5 => 0xFFEAu, // RMenu     -> Alt_R
            0xAE => 0x1008FF11u, // VolumeDown -> XF86AudioLowerVolume
            0xAF => 0x1008FF13u, // VolumeUp   -> XF86AudioRaiseVolume
            0xAD => 0x1008FF12u, // VolumeMute -> XF86AudioMute
            0xB0 => 0x1008FF17u, // MediaNextTrack -> XF86AudioNext
            0xB1 => 0x1008FF16u, // MediaPreviousTrack -> XF86AudioPrev
            0xB2 => 0x1008FF15u, // MediaStop -> XF86AudioStop
            0xB3 => 0x1008FF14u, // MediaPlayPause -> XF86AudioPlay
            0xBA => 0x003Bu, // OemSemicolon   ;
            0xBB => 0x003Du, // Oemplus        =
            0xBC => 0x002Cu, // Oemcomma       ,
            0xBD => 0x002Du, // OemMinus       -
            0xBE => 0x002Eu, // OemPeriod      .
            0xBF => 0x002Fu, // OemQuestion    /
            0xC0 => 0x0060u, // Oem3           `
            0xDB => 0x005Bu, // OemOpenBrackets [
            0xDC => 0x005Cu, // OemPipe        \
            0xDD => 0x005Du, // OemCloseBrackets ]
            0xDE => 0x0027u, // Oem7           '
            0xE2 => 0x003Cu, // Oem102         <
            _ => 0u
        };

        keysym = value;
        return value != 0;
    }

    /// <summary>
    /// Keysyms of the modifier keys that can be physically held while a shortcut is triggered
    /// (Shift, Control, Meta, Alt, Super, Hyper and ISO_Level3_Shift). Lock keys are excluded.
    /// </summary>
    internal static readonly nuint[] ModifierKeysyms =
    [
        0xFFE1, 0xFFE2, // Shift_L, Shift_R
        0xFFE3, 0xFFE4, // Control_L, Control_R
        0xFFE7, 0xFFE8, // Meta_L, Meta_R
        0xFFE9, 0xFFEA, // Alt_L, Alt_R
        0xFFEB, 0xFFEC, // Super_L, Super_R
        0xFFED, 0xFFEE, // Hyper_L, Hyper_R
        0xFE03          // ISO_Level3_Shift (AltGr)
    ];
}

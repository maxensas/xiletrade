using FluentAssertions;
using Xiletrade.Library.Services.Linux.Helper;

namespace Xiletrade.Test.CI.Linux;

public class X11KeysTests
{
    [Theory]
    [InlineData(0x30, 0x30u)]   // 0
    [InlineData(0x39, 0x39u)]   // 9
    [InlineData(0x41, 0x61u)]   // A -> a
    [InlineData(0x5A, 0x7Au)]   // Z -> z
    [InlineData(0x70, 0xFFBEu)] // F1
    [InlineData(0x7B, 0xFFC9u)] // F12
    [InlineData(0x87, 0xFFD5u)] // F24
    [InlineData(0x60, 0xFFB0u)] // NumPad0
    [InlineData(0x69, 0xFFB9u)] // NumPad9
    [InlineData(0x08, 0xFF08u)] // Back -> BackSpace
    [InlineData(0x0D, 0xFF0Du)] // Return
    [InlineData(0x1B, 0xFF1Bu)] // Escape
    [InlineData(0x20, 0x20u)]   // Space
    [InlineData(0x24, 0xFF50u)] // Home
    [InlineData(0x2E, 0xFFFFu)] // Delete
    [InlineData(0xA1, 0xFFE2u)] // RShiftKey -> Shift_R
    [InlineData(0xA2, 0xFFE3u)] // LControlKey -> Control_L
    [InlineData(0xBA, 0x3Bu)]   // OemSemicolon
    [InlineData(0xC0, 0x60u)]   // Oem3 -> grave
    public void _01_Known_VirtualKeys_Should_Be_Translated(int virtualKey, uint expected)
    {
        X11Keys.TryGetKeysym(virtualKey, out var keysym).Should().BeTrue();
        ((uint)keysym).Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x07)]
    [InlineData(0xFF)]
    [InlineData(-1)]
    public void _02_Unknown_VirtualKeys_Should_Be_Rejected(int virtualKey)
    {
        X11Keys.TryGetKeysym(virtualKey, out var keysym).Should().BeFalse();
        ((uint)keysym).Should().Be(0u);
    }

    [Fact]
    public void _03_Keys_Used_By_The_Game_Actions_Should_Be_Translated()
    {
        ushort[] used =
        [
            X11Keys.VkBack, X11Keys.VkReturn, X11Keys.VkMenu, X11Keys.VkHome, X11Keys.VkDelete,
            X11Keys.VkC, X11Keys.VkF, X11Keys.VkV, X11Keys.VkX, X11Keys.VkRShift, X11Keys.VkLControl
        ];
        foreach (var vk in used)
        {
            X11Keys.TryGetKeysym(vk, out _).Should().BeTrue($"0x{vk:X2} is sent to the game");
        }
    }

    [Theory]
    [InlineData(0, 0u)]
    [InlineData(X11Keys.WinModAlt, 1u << 3)]
    [InlineData(X11Keys.WinModControl, 1u << 2)]
    [InlineData(X11Keys.WinModShift, 1u << 0)]
    [InlineData(X11Keys.WinModWin, 1u << 6)]
    [InlineData(X11Keys.WinModControl | X11Keys.WinModShift, (1u << 2) | (1u << 0))]
    [InlineData(X11Keys.WinModAlt | X11Keys.WinModControl | X11Keys.WinModShift | X11Keys.WinModWin,
        (1u << 3) | (1u << 2) | (1u << 0) | (1u << 6))]
    public void _04_Windows_Modifiers_Should_Be_Translated(int windowsModifiers, uint expected)
    {
        X11Keys.ToX11Modifiers(windowsModifiers).Should().Be(expected);
    }

    [Fact]
    public void _05_Lock_Keys_Should_Be_Ignored_When_Matching_A_Shortcut()
    {
        const uint capsLock = 1u << 1;
        const uint numLock = 1u << 4;
        const uint control = 1u << 2;

        ((control | capsLock | numLock) & X11Keys.RelevantStateMask).Should().Be(control);
    }

    [Fact]
    public void _06_Modifier_Keysyms_Should_Not_Contain_Lock_Keys()
    {
        uint[] lockKeys = [0xFFE5, 0xFFE6, 0xFF7F]; // Caps_Lock, Shift_Lock, Num_Lock
        foreach (var lockKey in lockKeys)
        {
            X11Keys.ModifierKeysyms.Contains((nuint)lockKey).Should().BeFalse();
        }
        X11Keys.ModifierKeysyms.Distinct().Count().Should().Be(X11Keys.ModifierKeysyms.Length);
    }
}

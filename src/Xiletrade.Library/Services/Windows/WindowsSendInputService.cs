using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services.Windows;

public sealed class WindowsSendInputService : ISendInputService
{
    private static IServiceProvider _serviceProvider;

    public WindowsSendInputService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PasteClipboard()
    {
        // Ctrl+V, and Enter
        SendModifiedKey(Native.VK_CONTROL, Native.VK_V);
        SendKey(Native.VK_RETURN);
    }

    public void CleanChatAndPasteClipboard()
    {
        string chatKey = GetChatKey();
        SendUnicodeText(chatKey);
        SendKey(Native.VK_HOME);
        SendKey(Native.VK_DELETE);
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        string chatKey = GetChatKey();
        SendModifiedKey(Native.VK_CONTROL, (ushort)chatKey[0]);
        PasteClipboard();
    }

    public void CopyItemDetailAdvanced()
    {
        // Ctrl + Alt + C
        SendModifiedKeys([Native.VK_CONTROL, Native.VK_MENU], Native.VK_C);
    }

    public void CopyItemDetail()
    {
        SendModifiedKey(Native.VK_CONTROL, Native.VK_C);
    }

    public void CutLastWhisperToClipboard()
    {
        string chatKey = GetChatKey();
        SendModifiedKey(Native.VK_CONTROL, (ushort)chatKey[0]);
        SendKey(Native.VK_HOME);
        SendModifiedKey(Native.VK_CONTROL, Native.VK_X);
    }

    public void StartMouseWheelCapture() => Mouse.Hook.Start();

    public void StopMouseWheelCapture() => Mouse.Hook.Stop();

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        SendModifiedKey(Native.VK_CONTROL, Native.VK_F);
        SendKey(Native.VK_DELETE);
        PasteClipboard();
    }

    private static string GetChatKey() 
        => _serviceProvider.GetRequiredService<HotKeyService>().ChatKey;

    // -------- Unicode Text Input (multi-lang) --------
    public static void SendUnicodeText(string text)
    {
        foreach (char c in text)
        {
            SendUnicodeChar(c);
        }
    }

    private static void SendUnicodeChar(char character)
    {
        var inputs = new Native.INPUT[2];

        // Key down
        inputs[0].type = Native.INPUT_KEYBOARD;
        inputs[0].U.ki = new Native.KEYBDINPUT
        {
            wVk = 0,
            wScan = character,
            dwFlags = Native.KEYEVENTF_UNICODE,
            time = 0,
            dwExtraInfo = IntPtr.Zero
        };

        // Key up
        inputs[1].type = Native.INPUT_KEYBOARD;
        inputs[1].U.ki = new Native.KEYBDINPUT
        {
            wVk = 0,
            wScan = character,
            dwFlags = Native.KEYEVENTF_UNICODE | Native.KEYEVENTF_KEYUP,
            time = 0,
            dwExtraInfo = IntPtr.Zero
        };

        Native.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Native.INPUT)));
    }

    // -------- Standard Key Input --------

    private static void SendKey(ushort vk)
    {
        SendKeyDown(vk);
        SendKeyUp(vk);
    }

    private static void SendKeyDown(ushort vk)
    {
        var input = new Native.INPUT
        {
            type = Native.INPUT_KEYBOARD,
            U = new Native.InputUnion
            {
                ki = new Native.KEYBDINPUT
                {
                    wVk = vk,
                    wScan = 0,
                    dwFlags = 0,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Native.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(Native.INPUT)));
    }

    private static void SendKeyUp(ushort vk)
    {
        var input = new Native.INPUT
        {
            type = Native.INPUT_KEYBOARD,
            U = new Native.InputUnion
            {
                ki = new Native.KEYBDINPUT
                {
                    wVk = vk,
                    wScan = 0,
                    dwFlags = Native.KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Native.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(Native.INPUT)));
    }

    private void SendModifiedKey(ushort modifier, ushort vk)
    {
        SendKeyDown(modifier);
        SendKey(vk);
        SendKeyUp(modifier);
    }

    private void SendModifiedKeys(ushort[] modifiers, ushort vk)
    {
        foreach (var mod in modifiers)
            SendKeyDown(mod);

        SendKey(vk);

        foreach (var mod in modifiers.Reverse())
            SendKeyUp(mod);
    }
}

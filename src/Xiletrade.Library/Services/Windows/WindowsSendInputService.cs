using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services.Windows;

public sealed class WindowsSendInputService : ISendInputService
{
    private static IServiceProvider _serviceProvider;
    private static bool IsPoe2 => _serviceProvider.GetRequiredService<DataManagerService>()
        .Config.Options.GameVersion is 1;

    public WindowsSendInputService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PasteClipboard()
    {
        SendModifiedKey(Native.VK_RCONTROL, Native.VK_V, delay: true);
        SendKey(Native.VK_RETURN);
    }

    public void CleanChatAndPasteClipboard()
    {
        SendModifiedKeys([Native.VK_RCONTROL, Native.VK_RSHIFT], GetChatKeyCode());
        SendKey(Native.VK_BACK);
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        SendModifiedKey(Native.VK_RCONTROL, GetChatKeyCode());
        PasteClipboard();
    }

    public void CopyItemDetailAdvanced()
    {
        SendModifiedKeys([Native.VK_RCONTROL, Native.VK_MENU], Native.VK_C, delay: true);
        if (IsPoe2)
        {
            EnsureAltClosingWindow();
        }
    }

    public void CopyItemDetail()
    {
        SendModifiedKey(Native.VK_RCONTROL, Native.VK_C);
    }

    public void CutLastWhisperToClipboard()
    {
        SendModifiedKey(Native.VK_RCONTROL, GetChatKeyCode());
        SendModifiedKey(Native.VK_RSHIFT, Native.VK_HOME);
        SendModifiedKey(Native.VK_RCONTROL, Native.VK_X, delay: true);
    }

    public void StartMouseWheelCapture() => Input.MouseHook.Start();

    public void StopMouseWheelCapture() => Input.MouseHook.Stop();

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        SendModifiedKey(Native.VK_RCONTROL, Native.VK_F);
        SendKey(Native.VK_DELETE);
        PasteClipboard();
    }

    private static ushort GetChatKeyCode()
        => _serviceProvider.GetRequiredService<HotKeyService>().ChatKeyCode;

    // -------- Standard Key Input --------
    private static void SendKey(ushort vk, bool delay = false)
    {
        SendKeyDown(vk, delay);
        SendKeyUp(vk, delay);
    }

    private static void SendModifiedKey(ushort modifier, ushort vk, bool delay = false)
    {
        SendKeyDown(modifier, delay);
        SendKey(vk, delay);
        SendKeyUp(modifier, delay);
    }

    private static void SendModifiedKeys(ushort[] modifiers, ushort vk, bool delay = false)
    {
        foreach (var mod in modifiers)
        {
            SendKeyDown(mod, delay);
        }
        SendKey(vk);
        foreach (var mod in modifiers.Reverse())
        {
            SendKeyUp(mod, delay);
        }
    }

    private static void SendKeyUp(ushort vk, bool delay = false)
    {
        Input.Send.SendKeyUp(vk);
        if (delay)
        {
            Thread.Sleep(10);
        }
    }

    private static void SendKeyDown(ushort vk, bool delay = false)
    {
        Input.Send.SendKeyDown(vk);
        if (delay)
        {
            Thread.Sleep(10);
        }
    }

    // Ensures that the POE2 alternative description window will not remain open
    private static void EnsureAltClosingWindow()
    {
        Thread.Sleep(10);
        SendKeyUp(Native.VK_MENU);
    }

    private static void SendUnicodeText(ReadOnlySpan<char> text)
    {
        foreach (char c in text)
        {
            Input.Send.SendUnicodeChar(c);
        }
    }
}

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
    private static bool FastInputs => _serviceProvider.GetRequiredService<DataManagerService>()
        .Config.Options.FastInputs;

    private static int InputDelay => FastInputs ? 10 : 20;
    private static int ClipboardDelay => FastInputs ? 1 : 20;

    public WindowsSendInputService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PasteClipboard()
    {
        SendModifiedKey(Input.VK_RCONTROL, Input.VK_V, delay: true);
        SendKey(Input.VK_RETURN, delay: !FastInputs);
    }

    public void CleanChatAndPasteClipboard()
    {
        SendModifiedKeys([Input.VK_RCONTROL, Input.VK_RSHIFT], GetChatKeyCode());
        SendKey(Input.VK_BACK, delay: !FastInputs);
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        SendModifiedKey(Input.VK_RCONTROL, GetChatKeyCode());
        PasteClipboard();
    }

    public void CopyItemDetailAdvanced()
    {
        SendModifiedKeys([Input.VK_RCONTROL, Input.VK_MENU], Input.VK_C, delay: true);
        if (IsPoe2)
        {
            EnsureAltClosingWindow();
        }
        Thread.Sleep(ClipboardDelay);
    }

    public void CopyItemDetail()
    {
        SendModifiedKey(Input.VK_RCONTROL, Input.VK_C, delay: true);
        if (IsPoe2)
        {
            EnsureAltClosingWindow();
        }
        Thread.Sleep(ClipboardDelay);
    }

    public void CutLastWhisperToClipboard()
    {
        SendModifiedKey(Input.VK_RCONTROL, GetChatKeyCode(), delay: !FastInputs);
        SendModifiedKey(Input.VK_RSHIFT, Input.VK_HOME, delay: !FastInputs);
        SendModifiedKey(Input.VK_RCONTROL, Input.VK_X, delay: true);
    }

    public void StartMouseWheelCapture() => Input.MouseHook.Start();

    public void StopMouseWheelCapture() => Input.MouseHook.Stop();

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        SendModifiedKey(Input.VK_RCONTROL, Input.VK_F, delay: !FastInputs);
        SendKey(Input.VK_DELETE, delay: !FastInputs);
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
            Thread.Sleep(InputDelay);
        }
    }

    private static void SendKeyDown(ushort vk, bool delay = false)
    {
        Input.Send.SendKeyDown(vk);
        if (delay)
        {
            Thread.Sleep(InputDelay);
        }
    }

    // Ensures that the POE2 alternative description window will not remain open
    private static void EnsureAltClosingWindow()
    {
        Thread.Sleep(InputDelay);
        SendKeyUp(Input.VK_MENU);
    }

    private static void SendUnicodeText(ReadOnlySpan<char> text)
    {
        foreach (char c in text)
        {
            Input.Send.SendUnicodeChar(c);
        }
    }
}

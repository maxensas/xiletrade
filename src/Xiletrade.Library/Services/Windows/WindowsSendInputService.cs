using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services.Windows;

public sealed class WindowsSendInputService : ISendInputService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataManagerService _dm;

    private HotKeyService HotKey => _serviceProvider.GetRequiredService<HotKeyService>();

    private bool IsPoe2 => _dm.Config.Options.GameVersion is 1;
    private bool FastInputs => _dm.Config.Options.FastInputs;

    private int InputDelay => FastInputs ? 10 : 20;
    private int ClipboardDelay => FastInputs ? 1 : 20;

    private static ushort ControlKey => Input.VK_LCONTROL; // old: Input.VK_RCONTROL

    public WindowsSendInputService(IServiceProvider serviceProvider, DataManagerService dm)
    {
        _serviceProvider = serviceProvider;
        _dm = dm;
    }

    public void PasteClipboard()
    {
        SendModifiedKey(ControlKey, Input.VK_V, delay: true);
        SendKey(Input.VK_RETURN, delay: !FastInputs);
    }

    public void CleanChatAndPasteClipboard()
    {
        SendModifiedKeys([ControlKey, Input.VK_RSHIFT], GetChatKeyCode());
        SendKey(Input.VK_BACK, delay: !FastInputs);
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        SendModifiedKey(ControlKey, GetChatKeyCode());
        PasteClipboard();
    }

    // not used anymore
    public void CopyItemDetailAdvanced()
    {
        SendModifiedKeys([ControlKey, Input.VK_MENU], Input.VK_C, delay: true);
        if (IsPoe2)
        {
            EnsureAltClosingWindow();
        }
        Thread.Sleep(ClipboardDelay);
    }

    public void CopyItemDetail() => CopyItemDetailWithDelay();

    private void CopyItemDetailWithDelay()
    {
        SendModifiedKey(ControlKey, Input.VK_C, delay: true);
        Thread.Sleep(ClipboardDelay);
    }

    private void CopyItemDetailWithoutDelay()
    {
        SendModifiedKey(ControlKey, Input.VK_C);
    }

    public void CutLastWhisperToClipboard()
    {
        SendModifiedKey(ControlKey, GetChatKeyCode(), delay: !FastInputs);
        SendModifiedKey(Input.VK_RSHIFT, Input.VK_HOME, delay: !FastInputs);
        SendModifiedKey(ControlKey, Input.VK_X, delay: true);
    }

    public void StartMouseWheelCapture() => Input.MouseHook.Start();

    public void StopMouseWheelCapture() => Input.MouseHook.Stop();

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        SendModifiedKey(ControlKey, Input.VK_F, delay: !FastInputs);
        SendKey(Input.VK_DELETE, delay: !FastInputs);
        PasteClipboard();
    }

    private ushort GetChatKeyCode() => HotKey.ChatKeyCode;

    // -------- Standard Key Input --------
    private void SendKey(ushort vk, bool delay = false)
    {
        SendKeyDown(vk, delay);
        SendKeyUp(vk, delay);
    }

    private void SendModifiedKey(ushort modifier, ushort vk, bool delay = false)
    {
        SendKeyDown(modifier, delay);
        SendKey(vk, delay);
        SendKeyUp(modifier, delay);
    }

    private void SendModifiedKeys(ushort[] modifiers, ushort vk, bool delay = false)
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

    private void SendKeyUp(ushort vk, bool delay = false)
    {
        Input.Send.SendKeyUp(vk);
        if (delay)
        {
            Thread.Sleep(InputDelay);
        }
    }

    private void SendKeyDown(ushort vk, bool delay = false)
    {
        Input.Send.SendKeyDown(vk);
        if (delay)
        {
            Thread.Sleep(InputDelay);
        }
    }

    // Ensures that the POE2 alternative description window will not remain open
    private void EnsureAltClosingWindow()
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

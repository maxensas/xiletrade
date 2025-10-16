using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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
        try
        {
            SendModifiedKey(Native.VK_CONTROL, Native.VK_V);
        }
        catch (Exception) // clipboard access after SetClipoard
        {
            //SendUnicodeText(_serviceProvider.GetRequiredService<ClipboardService>().GetClipboard());
        }
        SendKey(Native.VK_RETURN);
    }

    public void CleanChatAndPasteClipboard()
    {
        SendModifiedKeys([Native.VK_CONTROL, Native.VK_RSHIFT], GetChatKeyCode());
        SendKey(Native.VK_BACK);
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        SendModifiedKey(Native.VK_CONTROL, GetChatKeyCode());
        PasteClipboard();
    }

    public void CopyItemDetailAdvanced()
    {
        SendModifiedKeys([Native.VK_CONTROL, Native.VK_MENU], Native.VK_C);
        AvoidAltWindow();
    }

    public void CopyItemDetail()
    {
        SendModifiedKey(Native.VK_CONTROL, Native.VK_C);
    }

    public void CutLastWhisperToClipboard()
    {
        SendModifiedKey(Native.VK_CONTROL, GetChatKeyCode());
        SendModifiedKey(Native.VK_RSHIFT, Native.VK_HOME);
        SendModifiedKey(Native.VK_CONTROL, Native.VK_X);
    }

    public void StartMouseWheelCapture() => Input.MouseHook.Start();

    public void StopMouseWheelCapture() => Input.MouseHook.Stop();

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        SendModifiedKey(Native.VK_CONTROL, Native.VK_F);
        SendKey(Native.VK_DELETE);
        PasteClipboard();
    }

    private static ushort GetChatKeyCode()
        => _serviceProvider.GetRequiredService<HotKeyService>().ChatKeyCode;

    // -------- Standard Key Input --------
    private static void SendKey(ushort vk)
    {
        SendKeyDown(vk);
        SendKeyUp(vk);
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
        {
            SendKeyDown(mod);
        }
        SendKey(vk);
        foreach (var mod in modifiers.Reverse())
        {
            SendKeyUp(mod);
        }
    }

    private static void SendKeyUp(ushort vk)
    {
        Input.Send.SendKeyUp(vk);
        System.Threading.Thread.Sleep(5);
    }

    private static void SendKeyDown(ushort vk)
    {
        Input.Send.SendKeyDown(vk);
        System.Threading.Thread.Sleep(5);
    }

    // Ensures that the POE2 alternative description window will not remain open
    private static void AvoidAltWindow()
    {
        System.Threading.Thread.Sleep(50);
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

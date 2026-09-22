using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services.Windows;

public sealed class WindowsSendInputService : ISendInputService
{
    private readonly DataManagerService _dm;

    public WindowsSendInputService(ILogger<WindowsSendInputService> logger, DataManagerService dm)
    {
        _dm = dm;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private bool FastInputs => _dm.Config.Options.FastInputs;

    private int InputDelay => FastInputs ? 10 : 20;
    private int ClipboardDelay => FastInputs ? 1 : 20;

    private static ushort ControlKey => Input.VK_LCONTROL; // old: Input.VK_RCONTROL

    public (string Key, ushort Code) ChatKey { get; set; } = (string.Empty, 0);

    public void PasteClipboard()
    {
        SendModifiedKey(ControlKey, Input.VK_V, delay: true);
        SendKey(Input.VK_RETURN, delay: !FastInputs);
    }

    public void CleanChatAndPasteClipboard()
    {
        SendModifiedKeys([ControlKey, Input.VK_RSHIFT], ChatKey.Code);
        SendKey(Input.VK_BACK, delay: !FastInputs);
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        SendModifiedKey(ControlKey, ChatKey.Code);
        PasteClipboard();
    }

    // not used anymore
    public void CopyItemDetailAdvanced()
    {
        SendModifiedKeys([ControlKey, Input.VK_MENU], Input.VK_C, delay: true);
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
        SendModifiedKey(ControlKey, ChatKey.Code, delay: !FastInputs);
        SendModifiedKey(Input.VK_RSHIFT, Input.VK_HOME, delay: !FastInputs);
        SendModifiedKey(ControlKey, Input.VK_X, delay: true);
    }

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        SendModifiedKey(ControlKey, Input.VK_F, delay: !FastInputs);
        SendKey(Input.VK_DELETE, delay: !FastInputs);
        PasteClipboard();
    }

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
}

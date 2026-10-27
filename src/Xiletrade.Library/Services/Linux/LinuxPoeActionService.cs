using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;
using Xiletrade.Library.Models.Application.Adapter;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux.Helper;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Provides clipboard operations for interacting with Path of Exile.
/// </summary>
/// <remarks>
/// Keys are sent to the focused window with the XTest extension of the X server (X11 and XWayland),
/// where the game runs through Wine / Proton. Same sequences as the Windows implementation.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxPoeActionService : IPoeActionService
{
    private const int CopyPollDelayMs = 20;

    private readonly DataManagerService _dm;
    private readonly LinuxDesktopService _desktop;
    private readonly LinuxClipboardAdapter _clipboard = new();

    public LinuxPoeActionService(ILogger<LinuxPoeActionService> logger, DataManagerService dm,
        LinuxDesktopService desktop)
    {
        _dm = dm;
        _desktop = desktop;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private bool FastInputs => _dm.Config.Options.FastInputs;

    private int InputDelay => FastInputs ? 10 : 20;
    private int ClipboardDelay => FastInputs ? 1 : 20;

    // The game takes some time to publish the copied text to the X selection.
    private int CopyTimeout => FastInputs ? 100 : 250;

    private static ushort ControlKey => X11Keys.VkLControl;

    public (string Key, ushort Code) ChatKey { get; set; } = (string.Empty, 0);

    public void PasteClipboard()
    {
        BeginKeys();
        Paste();
    }

    public void CleanChatAndPasteClipboard()
    {
        BeginKeys();
        SendModifiedKeys([ControlKey, X11Keys.VkRShift], ChatKey.Code);
        SendKey(X11Keys.VkBack, delay: !FastInputs);
        Paste();
    }

    public void ReplyLastWhisper()
    {
        BeginKeys();
        SendModifiedKey(ControlKey, ChatKey.Code);
        Paste();
    }

    // not used anymore
    public void CopyItemDetailAdvanced()
    {
        BeginKeys();
        SendModifiedKeys([ControlKey, X11Keys.VkMenu], X11Keys.VkC, delay: true);
        Thread.Sleep(ClipboardDelay);
    }

    public void CopyItemDetail()
    {
        BeginKeys();
        SendModifiedKey(ControlKey, X11Keys.VkC, delay: true);
        Thread.Sleep(ClipboardDelay);
        WaitForCopiedText();
    }

    public void CutLastWhisperToClipboard()
    {
        BeginKeys();
        SendModifiedKey(ControlKey, ChatKey.Code, delay: !FastInputs);
        SendModifiedKey(X11Keys.VkRShift, X11Keys.VkHome, delay: !FastInputs);
        SendModifiedKey(ControlKey, X11Keys.VkX, delay: true);
        WaitForCopiedText();
    }

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        BeginKeys();
        SendModifiedKey(ControlKey, X11Keys.VkF, delay: !FastInputs);
        SendKey(X11Keys.VkDelete, delay: !FastInputs);
        Paste();
    }

    // -------- Sequences --------

    private void Paste()
    {
        SendModifiedKey(ControlKey, X11Keys.VkV, delay: true);
        SendKey(X11Keys.VkReturn, delay: !FastInputs);
    }

    /// <summary>
    /// The modifiers held by the user (keys of the shortcut) must not alter the keys sent to the game.
    /// </summary>
    private void BeginKeys() => _desktop.ReleaseHeldModifiers();

    /// <summary>
    /// Waits until the game has put text in the clipboard, or for a short time if it does not.
    /// </summary>
    private void WaitForCopiedText()
    {
        var watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < CopyTimeout)
        {
            if (_clipboard.ContainsTextData())
            {
                return;
            }
            Thread.Sleep(CopyPollDelayMs);
        }
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
        foreach (var modifier in modifiers)
        {
            SendKeyDown(modifier, delay);
        }
        SendKey(vk);
        for (int i = modifiers.Length - 1; i >= 0; i--)
        {
            SendKeyUp(modifiers[i], delay);
        }
    }

    private void SendKeyUp(ushort vk, bool delay = false)
    {
        _desktop.SendKey(vk, press: false);
        if (delay)
        {
            Thread.Sleep(InputDelay);
        }
    }

    private void SendKeyDown(ushort vk, bool delay = false)
    {
        _desktop.SendKey(vk, press: true);
        if (delay)
        {
            Thread.Sleep(InputDelay);
        }
    }
}

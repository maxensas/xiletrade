using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.Library.Services.Windows;

/// <summary> Service used to interact with clipboard and PoE message whispering.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsClipboardService(IPoeActionService poe, IClipboardAdapterService clipboard, 
    IMessageAdapterService message, IViewManager view) : IClipboardService
{
    private readonly IPoeActionService _poe = poe;
    private readonly IClipboardAdapterService _clipboard = clipboard;
    private readonly IMessageAdapterService _message = message;
    private readonly IViewManager _view = view;

    private bool _sendingWhisper;

    public void Clear() => _clipboard.Clear();

    public void SetClipboard(string data) => _clipboard.SetClipboard(data);

    public string GetClipboard(bool clear = false) => _clipboard.GetClipboard(clear);

    public bool ContainsUnicodeTextData() => _clipboard.ContainsUnicodeTextData();

    public bool ContainsTextData() => _clipboard.ContainsTextData();

    public bool ContainsAnyTextData()
    {
        return ContainsUnicodeTextData() || ContainsTextData();
    }

    public void SendClipboardCommand(string command)
    {
        try
        {
            Clear();
            SetClipboard(command);
            _poe.CleanChatAndPasteClipboard();
            Clear();
        }
        catch (COMException ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error022_XClipboard + command, MessageStatus.Error);
        }
    }

    public void SendClipboardCommandLastWhisper(string command)
    {
        Clear();
        _poe.CutLastWhisperToClipboard();
        string clip = GetClipboard();
        if (clip?.Length > 1 && clip.StartsWith('@') && clip.Contain(' '))
        {
            string charName = clip.Split(' ')[0].Replace("@", string.Empty);
            SetClipboard(command + " " + charName);
            _poe.PasteClipboard();
            Clear();
        }
    }

    public void SendWhisperMessage(ReadOnlySpan<char> message)
    {
        if (_sendingWhisper)
        {
            return;
        }
        _sendingWhisper = true;
        
        try
        {
            string tradechat;
            bool IsMesssage = message.Length > 0;

            nint origHwnd = IsMesssage ? _view.MainHandle : origHwnd = Native.GetForegroundWindow();
            nint findHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
            bool isPoeWindow = findHwnd.ToInt32() > 0 && findHwnd.ToInt32() != origHwnd.ToInt32();
            if (!isPoeWindow)
            {
                return;
            }

            if (IsMesssage)
            {
                tradechat = message.ToString();
            }
            else
            {
                if (!ContainsUnicodeTextData())
                {
                    return;
                }
                tradechat = GetClipboard();
            }
            if (tradechat is null || !tradechat.StartsWith('@')
                || !Strings.Collection.dicWantToBuy.Keys.Any(item => tradechat.Contain(item)))
            {
                tradechat = null;
                return;
            }

            if (!tradechat.Contain(Strings.Info))
            {
                Clear();
                SetClipboard(tradechat + Strings.Info);
            }

            if (ContainsUnicodeTextData() || ContainsTextData())
            {
                if (Native.SwitchWindow(findHwnd))
                {
                    _poe.CleanChatAndPasteClipboard();
                }
                Clear();
                Native.SwitchWindow(origHwnd);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error016_XSendWhisper, MessageStatus.Error);
        }
        finally
        {
            _sendingWhisper = false;
        }
    }

    public void SendRegex(string message)
    {
        try
        {
            if (message is null)
            {
                return;
            }
            nint origHwnd = Native.GetForegroundWindow();

            nint findPoeHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
            bool poeLaunched = findPoeHwnd.ToInt32() > 0;

            if (poeLaunched && findPoeHwnd.ToInt32() != origHwnd.ToInt32())
            {
                SetClipboard(message);

                if (ContainsUnicodeTextData() || ContainsTextData())
                {
                    if (Native.SwitchWindow(findPoeHwnd))
                    {
                        _poe.CleanPoeSearchBarAndPasteClipboard();
                    }
                    Clear();
                    Native.SwitchWindow(origHwnd);
                }
            }
        }
        catch (ExternalException ex)
        {
            if (!ex.Message.Contain("0x800401D0")) // CLIPBRD_E_CANT_OPEN // System.Runtime.InteropServices.COMException
            {
                _message.Show(ex.GetFormated(), Resources.Resources.Error023_XClipboardRegex, MessageStatus.Error);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error014_XSendRegex, MessageStatus.Error);
        }
    }
}

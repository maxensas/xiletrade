using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary> Service used to interact with clipboard and PoE message whispering.</summary>
public sealed class ClipboardService
{
    private readonly XiletradeService _xiletrade;
    private readonly ISendInputService _input;
    private readonly IClipboardAdapterService _clipboard;
    private readonly IMessageAdapterService _message;

    private bool _sendingWhisper;

    public ClipboardService(XiletradeService xiletrade, ISendInputService input, 
        IClipboardAdapterService clipboard, IMessageAdapterService message)
    {
        _xiletrade = xiletrade;
        _input = input;
        _clipboard = clipboard;
        _message = message;
    }

    internal void Clear() => _clipboard.Clear();

    internal void SetClipboard(string data) => _clipboard.SetClipboard(data);

    internal string GetClipboard(bool clear = false) => _clipboard.GetClipboard(clear);

    internal bool ContainsUnicodeTextData() => _clipboard.ContainsUnicodeTextData();

    internal bool ContainsTextData() => _clipboard.ContainsTextData();

    internal bool ContainsAnyTextData()
    {
        return ContainsUnicodeTextData() || ContainsTextData();
    }

    internal void SendClipboardCommand(string command)
    {
        try
        {
            Clear();
            SetClipboard(command);
            _input.CleanChatAndPasteClipboard();
            Clear();
        }
        catch (COMException ex)
        {
            _message.Show(ex.GetFormated(), "Clipboard access error : setting " + command, MessageStatus.Error);
        }
    }

    internal void SendClipboardCommandLastWhisper(string command)
    {
        Clear();
        _input.CutLastWhisperToClipboard();
        string clip = GetClipboard();
        if (clip?.Length > 1 && clip.StartsWith('@') && clip.Contain(' '))
        {
            string charName = clip.Split(' ')[0].Replace("@", string.Empty);
            SetClipboard(command + " " + charName);
            _input.PasteClipboard();
            Clear();
        }
    }

    internal void SendWhisperMessage(ReadOnlySpan<char> message)
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

            nint origHwnd = IsMesssage ? _xiletrade.MainHwnd
                : origHwnd = Native.GetForegroundWindow();
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
                || !Strings.dicWantToBuy.Keys.Any(item => tradechat.Contain(item)))
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
                    _input.CleanChatAndPasteClipboard();
                }
                Clear();
                Native.SwitchWindow(origHwnd);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Send whisper message error", MessageStatus.Error);
        }
        finally
        {
            _sendingWhisper = false;
        }
    }

    internal void SendRegex(string message)
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
                        _input.CleanPoeSearchBarAndPasteClipboard();
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
                _message.Show(ex.GetFormated(), "Clipboard access error (regex)", MessageStatus.Error);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Send regex error", MessageStatus.Error);
        }
    }
}

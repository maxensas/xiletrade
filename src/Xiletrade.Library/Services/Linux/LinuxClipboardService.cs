using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using Xiletrade.Library.Models.Application.Adapter;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Linux;

/// <summary> Service used to interact with clipboard and PoE message whispering.</summary>
/// <remarks>
/// Same behavior as the Windows service. The game window is found and focused through X11.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxClipboardService : IClipboardService
{
    private readonly IPoeActionService _poe;
    private readonly IMessageAdapterService _message;
    private readonly IViewManager _view;
    private readonly LinuxClipboardAdapter _clipboard = new();
    private readonly LinuxDesktopService _desktop;

    // The game reads the clipboard when it handles Ctrl+V, not when the key is sent : the owner of the
    // selection must stay available until then (X11 clipboard data is transferred on demand).
    private const int PasteSettleDelayMs = 50;

    private bool _sendingWhisper;

    public LinuxClipboardService(IPoeActionService poe, IMessageAdapterService message, IViewManager view,
        LinuxDesktopService desktop)
    {
        _poe = poe;
        _message = message;
        _view = view;
        _desktop = desktop;

        if (_clipboard.MissingToolHint is { } missingTool)
        {
            _message.Show($"The clipboard cannot be used : the '{missingTool}' package is not installed.\n"
                + "Install it with your package manager (for instance : sudo apt install " + missingTool + ").",
                "Xiletrade", MessageStatus.Warning);
        }
    }

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
            ClearAfterPaste();
        }
        catch (InvalidOperationException ex)
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
            ClearAfterPaste();
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
            bool isMessage = message.Length > 0;

            nuint origWindow = isMessage ? (nuint)_view.MainHandle : _desktop.GetActiveWindow();
            nuint poeWindow = _desktop.FindPoeWindow();
            bool isPoeWindow = poeWindow is not 0 && poeWindow != origWindow;
            if (!isPoeWindow)
            {
                return;
            }

            if (isMessage)
            {
                tradechat = message.ToString();
            }
            else
            {
                // one read instead of "contains" then "get" : each one runs a tool on Linux
                tradechat = GetClipboard();
                if (string.IsNullOrEmpty(tradechat))
                {
                    return;
                }
            }
            if (tradechat is null || !tradechat.StartsWith('@')
                || !Strings.Collection.dicWantToBuy.Keys.Any(item => tradechat.Contain(item)))
            {
                return;
            }

            if (!tradechat.Contain(Strings.Info))
            {
                Clear();
                SetClipboard(tradechat + Strings.Info);
            }

            if (ContainsUnicodeTextData() || ContainsTextData())
            {
                if (_desktop.SwitchWindow(poeWindow))
                {
                    _poe.CleanChatAndPasteClipboard();
                    Thread.Sleep(PasteSettleDelayMs);
                }
                Clear();
                _desktop.SwitchWindow(origWindow);
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
            nuint origWindow = _desktop.GetActiveWindow();
            nuint poeWindow = _desktop.FindPoeWindow();

            if (poeWindow is not 0 && poeWindow != origWindow)
            {
                SetClipboard(message);

                if (ContainsUnicodeTextData() || ContainsTextData())
                {
                    if (_desktop.SwitchWindow(poeWindow))
                    {
                        _poe.CleanPoeSearchBarAndPasteClipboard();
                        Thread.Sleep(PasteSettleDelayMs);
                    }
                    Clear();
                    _desktop.SwitchWindow(origWindow);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error023_XClipboardRegex, MessageStatus.Error);
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error014_XSendRegex, MessageStatus.Error);
        }
    }

    private void ClearAfterPaste()
    {
        Thread.Sleep(PasteSettleDelayMs);
        Clear();
    }
}

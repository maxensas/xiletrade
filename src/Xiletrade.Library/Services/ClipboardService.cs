﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary> Service used to interact with clipboard and PoE message whispering.</summary>
public sealed class ClipboardService
{
    private static IServiceProvider _serviceProvider;
    private static ISendInputService _sendInputService;
    private static bool _sendingWhisper;

    public ClipboardService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _sendInputService = serviceProvider.GetRequiredService<ISendInputService>();
    }

    internal void Clear() => _serviceProvider.GetRequiredService<IClipboardAdapterService>().Clear();

    internal void SetClipboard(object data) => _serviceProvider.GetRequiredService<IClipboardAdapterService>().SetClipboard(data);

    internal string GetClipboard(bool clear = false)
    {
        return _serviceProvider.GetRequiredService<IClipboardAdapterService>().GetClipboard(clear);
    }

    internal bool ContainsUnicodeTextData()
    {
        return _serviceProvider.GetRequiredService<IClipboardAdapterService>().ContainsUnicodeTextData();
    }

    internal bool ContainsTextData()
    {
        return _serviceProvider.GetRequiredService<IClipboardAdapterService>().ContainsTextData();
    }

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
            _sendInputService.CleanChatAndPasteClipboard();
            Clear();
        }
        catch (COMException ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Clipboard access error : setting " + command, MessageStatus.Error);
        }
    }

    internal void SendClipboardCommandLastWhisper(string command)
    {
        Clear();
        _sendInputService.CutLastWhisperToClipboard();
        string clip = GetClipboard();
        if (clip?.Length > 1 && clip.StartsWith('@') && clip.Contain(' '))
        {
            string charName = clip.Split(' ')[0].Replace("@", string.Empty);
            SetClipboard(command + " " + charName);
            //Thread.Sleep(100);
            _sendInputService.PasteClipboard();
            Clear();
        }
    }

    internal void SendWhisperMessage(string message)
    {
        if (_sendingWhisper)
        {
            return;
        }
        _sendingWhisper = true;

        try
        {
            nint origHwnd;
            string tradechat;

            if (message is not null)
            {
                origHwnd = _serviceProvider.GetRequiredService<XiletradeService>().MainHwnd;
                tradechat = message;
            }
            else
            {
                if (!ContainsUnicodeTextData())
                {
                    return;
                }
                origHwnd = Native.GetForegroundWindow();
                tradechat = GetClipboard();
            }

            if (tradechat is null || !tradechat.StartsWith('@')
                || !Strings.dicWantToBuy.Keys.Any(item => tradechat.Contain(item)))
            {
                return;
            }

            nint findHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
            if (findHwnd.ToInt32() > 0 && findHwnd.ToInt32() != origHwnd.ToInt32()) // POE launched
            {
                if (!tradechat.Contain(Strings.Info))
                {
                    Clear();
                    SetClipboard(tradechat + Strings.Info);
                }
                Thread.Sleep(10);

                if (ContainsUnicodeTextData() || ContainsTextData())
                {
                    if (Native.SwitchWindow(findHwnd))
                    {
                        _sendInputService.CleanChatAndPasteClipboard();
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
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Clipboard access error (autowhisper)", MessageStatus.Error);
            }
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Send whisper message error", MessageStatus.Error);
        }
        finally
        {
            _sendingWhisper = false;
        }
        //timer.IsEnabled = true;
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
            //bool poeFocused = Native.GetForegroundWindow().Equals(findPoeHwnd);

            if (poeLaunched && findPoeHwnd.ToInt32() != origHwnd.ToInt32())
            {
                var msg = $"\"{message}\"";
                SetClipboard(msg);
                Thread.Sleep(10);

                if (ContainsUnicodeTextData() || ContainsTextData())
                {
                    if (Native.SwitchWindow(findPoeHwnd))
                    {
                        _sendInputService.CleanPoeSearchBarAndPasteClipboard();
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
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Clipboard access error (regex)", MessageStatus.Error);
            }
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Send regex error", MessageStatus.Error);
        }
    }
}

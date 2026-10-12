using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.UI.Avalonia.Services;

/// <summary>
/// Implementation using Package Reference "MessageBox.Avalonia"
/// </summary>
/// <param name="ui"></param>
public sealed class MessageAdapterService: IMessageAdapterService
{
    private readonly IUIService _ui;

    public MessageAdapterService(ILogger<MessageAdapterService> logger, IUIService ui)
    {
        _ui = ui;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void Show(string message, string caption, MessageStatus status)
    {
        _ui.Invoke(() => _ = CreateMessageBox(message, caption, status, false).ShowAsync());
    }

    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        return _ui.Invoke(() =>
        {
            var frame = new DispatcherFrame();
            var task = CreateMessageBox(message, caption, status, yesNo).ShowAsync();
            _ = task.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false), TaskScheduler.Default);

            Dispatcher.UIThread.PushFrame(frame);

            var result = task.GetAwaiter().GetResult();
            return result == ButtonResult.Yes || result == ButtonResult.Ok;
        });
    }

    public async Task<bool> ShowResultAsync(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        var result = await _ui.InvokeAsync(() => CreateMessageBox(message, caption, status, yesNo).ShowAsync());
        return IsPositive(result);
    }

    private static bool IsPositive(ButtonResult result) => result == ButtonResult.Yes || result == ButtonResult.Ok;

    private static Icon GetMessageBoxIcon(MessageStatus status)
    {
        return status switch
        {
            MessageStatus.Exclamation => Icon.Warning,
            MessageStatus.Information => Icon.Info,
            MessageStatus.Warning => Icon.Warning,
            _ => Icon.Error,
        };
    }

    private static IMsBox<ButtonResult> CreateMessageBox(string message, string caption, MessageStatus status, bool yesNo)
    {
        PlaySystemSound(status);

        return MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ButtonDefinitions = yesNo ? ButtonEnum.YesNo : ButtonEnum.Ok,
            ContentTitle = caption,
            ContentMessage = message,
            Icon = GetMessageBoxIcon(status),
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        });
    }

    private static void PlaySystemSound(MessageStatus status)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Message.MessageBeep(status);
            }
            else if (OperatingSystem.IsLinux())
            {
                var id = status switch
                {
                    MessageStatus.Information => "dialog-information",
                    MessageStatus.Exclamation or MessageStatus.Warning => "dialog-warning",
                    _ => "dialog-error"
                };
                Process.Start(new ProcessStartInfo("canberra-gtk-play", $"-i {id}")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                var sound = status == MessageStatus.Information ? "Glass" : "Basso";
                Process.Start(new ProcessStartInfo("afplay", $"/System/Library/Sounds/{sound}.aiff")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
        }
        catch
        {
            // Sound is optional: ignore any errors (missing tool, no audio device, etc.)
        }
    }

    
}

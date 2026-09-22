using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services.Windows;

public sealed class WindowsMessageAdapterService : IMessageAdapterService
{
    private readonly UIService _ui;

    public WindowsMessageAdapterService(ILogger<WindowsMessageAdapterService> logger, UIService ui)
    {
        _ui = ui;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void Show(string message, string caption, MessageStatus status)
    {
        var action = new Action(() =>
        {
            Message.MessageBox(IntPtr.Zero, message, caption, Message.MB_OK | Message.MB_TOPMOST | GetNativeIcon(status));
        });
        _ui.DelegateActionToUiThread(action);
    }

    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        var func = new Func<bool>(() =>
        {
            return Message.MessageBox(IntPtr.Zero, message, caption,
                    (yesNo ? Message.MB_YESNO : Message.MB_OK) | Message.MB_TOPMOST | GetNativeIcon(status))
                    is Message.IDYES or Message.IDOK;
        });
        return _ui.DelegateFuncToUiThread(func);
    }

    public Task<bool> ShowResultAsync(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        return Task.Run(() => ShowResult(message, caption, status, yesNo));
    }

    private static uint GetNativeIcon(MessageStatus status)
    {
        return status is MessageStatus.Exclamation ? Message.MB_ICONWARNING
            : status is MessageStatus.Information ? Message.MB_ICONINFORMATION
            : status is MessageStatus.Warning ? Message.MB_ICONWARNING
            : Message.MB_ICONERROR;
    }
}

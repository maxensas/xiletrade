using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xiletrade.UI.WPF.Services;

public sealed class SendInputService : ISendInputService
{
    private static IServiceProvider _serviceProvider;

    public SendInputService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PasteClipboard()
    {
        System.Windows.Forms.SendKeys.SendWait("^{V}{ENTER}");
    }

    public void CleanChatAndPasteClipboard()
    {
        System.Windows.Forms.SendKeys.SendWait(GetChatKey() + "+{HOME}{DELETE}");
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        System.Windows.Forms.SendKeys.SendWait("^" + GetChatKey());
        PasteClipboard();
    }

    public void CopyItemDetailAdvanced()
    {
        System.Windows.Forms.SendKeys.SendWait("^%{c}");
    }

    public void CopyItemDetail()
    {
        System.Windows.Forms.SendKeys.SendWait("^{c}");
    }

    public void CutLastWhisperToClipboard()
    {
        System.Windows.Forms.SendKeys.SendWait("^" + GetChatKey());
        System.Windows.Forms.SendKeys.SendWait("+{HOME}^{X}");
    }

    private static string GetChatKey()
    {
        return _serviceProvider.GetRequiredService<HotKeyService>().ChatKey;
    }

    public void StartMouseWheelCapture()
    {
        Library.Shared.Interop.Input.MouseHook.Start();
    }

    public void StopMouseWheelCapture()
    {
        Library.Shared.Interop.Input.MouseHook.Stop();
    }

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        System.Windows.Forms.SendKeys.SendWait("^{F}{DELETE}");
        PasteClipboard();
    }
}

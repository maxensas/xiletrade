using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services;

namespace Xiletrade.UI.WPF.Services;

// not used
public sealed class SendInputService(HotKeyService hotkey) : ISendInputService
{
    private readonly HotKeyService _hotkey = hotkey;

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

    private string GetChatKey() => _hotkey.ChatKey;

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

using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class ReplyLastFeature(ClipboardService clipboard, ISendInputService sendInput,
    ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        clipboard.Clear();
        clipboard.SetClipboard(_shortcut.Value);
        //Thread.Sleep(100);
        sendInput.ReplyLastWhisper();
        clipboard.Clear();
    }
}

using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Application.Feature;

internal class ReplyLastFeature(IClipboardService clipboard, IPoeActionService poe,
    ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        clipboard.Clear();
        clipboard.SetClipboard(_shortcut.Value);
        //Thread.Sleep(100);
        poe.ReplyLastWhisper();
        clipboard.Clear();
    }
}

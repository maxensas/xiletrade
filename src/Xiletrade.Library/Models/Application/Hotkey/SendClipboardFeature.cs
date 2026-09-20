using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class SendClipboardFeature(ClipboardService clipboard, ConfigShortcut shortcut, 
    string stringValue) : BaseFeature(shortcut, stringValue)
{
    internal override void Launch()
    {
        if (_stringValue is not null)
        {
            if (_stringValue is Strings.Chat.invite or Strings.Chat.tradewith or Strings.Chat.whois)
            {
                clipboard.SendClipboardCommandLastWhisper(_stringValue);
                return;
            }
            clipboard.SendClipboardCommand(_stringValue);
        }
    }
}

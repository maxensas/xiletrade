using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Shared;
namespace Xiletrade.Library.Models.Feature;

internal class SendClipboardFeature(IServiceProvider service, ConfigShortcut shortcut, string stringValue) 
    : BaseFeature(service, shortcut, stringValue)
{
    internal override void Launch()
    {
        if (StringValue is not null)
        {
            if (StringValue is Strings.Chat.invite or Strings.Chat.tradewith or Strings.Chat.whois)
            {
                ClipboardHelper.SendClipboardCommandLastWhisper(StringValue);
                return;
            }
            ClipboardHelper.SendClipboardCommand(StringValue);
        }
    }
}

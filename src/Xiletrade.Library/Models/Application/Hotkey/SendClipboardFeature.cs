using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
namespace Xiletrade.Library.Models.Application.Hotkey;

internal class SendClipboardFeature(IServiceProvider service, ConfigShortcut shortcut, string stringValue) 
    : BaseFeature(service, shortcut, stringValue)
{
    internal override void Launch()
    {
        if (StringValue is not null)
        {
            var clipService = ServiceProvider.GetRequiredService<ClipboardService>();
            if (StringValue is Strings.Chat.invite or Strings.Chat.tradewith or Strings.Chat.whois)
            {
                clipService.SendClipboardCommandLastWhisper(StringValue);
                return;
            }
            clipService.SendClipboardCommand(StringValue);
        }
    }
}

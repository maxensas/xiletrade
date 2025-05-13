using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Feature;

internal class ReplyLastFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var clipService = ServiceProvider.GetRequiredService<ClipboardService>();

        clipService.Clear();
        clipService.SetClipboard(Shortcut.Value);
        //Thread.Sleep(100);
        ServiceProvider.GetRequiredService<ISendInputService>().ReplyLastWhisper();
        clipService.Clear();
    }
}

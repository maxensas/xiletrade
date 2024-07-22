using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Feature;

internal class ReplyLastFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        ClipboardHelper.Clear();
        ClipboardHelper.SetClipboard(Shortcut.Value);
        //Thread.Sleep(100);
        ServiceProvider.GetRequiredService<ISendInputService>().ReplyLastWhisper();
        ClipboardHelper.Clear();
    }
}

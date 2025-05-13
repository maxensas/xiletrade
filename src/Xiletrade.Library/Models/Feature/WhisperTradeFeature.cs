using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Feature;

internal class WhisperTradeFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        ServiceProvider.GetRequiredService<ClipboardService>().SendWhisperMessage(null);
    }
}

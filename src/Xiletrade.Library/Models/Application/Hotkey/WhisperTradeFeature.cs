using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class WhisperTradeFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        ServiceProvider.GetRequiredService<ClipboardService>().SendWhisperMessage([]);
    }
}

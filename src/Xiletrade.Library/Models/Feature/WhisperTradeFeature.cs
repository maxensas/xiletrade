using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Feature;

internal class WhisperTradeFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        ClipboardHelper.SendWhisperMessage(null);
    }
}

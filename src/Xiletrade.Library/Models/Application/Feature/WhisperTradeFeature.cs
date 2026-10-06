using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Application.Feature;

internal class WhisperTradeFeature(IClipboardService clipboard,
    ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch() => clipboard.SendWhisperMessage([]);
}

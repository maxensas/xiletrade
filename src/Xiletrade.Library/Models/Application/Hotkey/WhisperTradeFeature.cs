using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class WhisperTradeFeature(ClipboardService clipboard,
    ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch() => clipboard.SendWhisperMessage([]);
}

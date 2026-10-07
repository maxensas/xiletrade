using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Application.Feature;

internal sealed class CloseFeature(IViewManager view, ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch() => view.CloseOpenedView(_shortcut);
}
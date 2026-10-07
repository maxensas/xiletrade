using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Application.Feature;

internal class OpenConfigFeature(IViewManager view, ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch() => view.ShowConfigView();
}

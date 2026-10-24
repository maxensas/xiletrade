using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Application.Feature;

internal class OpenBulkFeature(IViewManager view, 
    MainViewModel vm, ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        vm.InitViewModels(useCustomOrBulk: true);
        view.ShowMainView();
    }
}

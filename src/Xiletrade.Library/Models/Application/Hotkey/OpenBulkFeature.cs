using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class OpenBulkFeature(INavigationService navigation, INetService net, 
    MainViewModel vm, ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        if (net.TradeCooldown is not null && net.TradeCooldown.IsEnabled)
        {
            navigation.ShowMainView();
            return;
        }
        vm.InitViewModels(useCustomOrBulk: true);
        navigation.ShowMainView();
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Feature;

internal class OpenBulkFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var vm = ServiceProvider.GetRequiredService<MainViewModel>();
        var service = ServiceProvider.GetRequiredService<PoeApiService>();
        if (service.IsCooldownEnabled)
        {
            ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
            return;
        }
        vm.InitViewModels(true);
        ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
    }
}

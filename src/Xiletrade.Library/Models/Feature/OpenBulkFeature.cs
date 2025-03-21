﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Feature;

internal class OpenBulkFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var vm = ServiceProvider.GetRequiredService<MainViewModel>();
        if (vm.Logic.Task.Price.CoolDown.IsEnabled)
        {
            ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
            return;
        }
        vm.InitViewModel(true);
        ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
    }
}

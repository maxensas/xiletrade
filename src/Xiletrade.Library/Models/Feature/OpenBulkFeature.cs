using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels;

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
        //var vm = Application.Current.MainWindow.DataContext as MainViewModel;

        vm.Logic.ResetViewModel();
        vm.Form.Tab.BulkEnable = true;
        vm.Form.Tab.BulkSelected = true;
        vm.Form.Tab.ShopEnable = true;
        vm.Form.Tab.ShopSelected = false;

        vm.Form.Visible.Wiki = false;
        vm.Form.Visible.BtnPoeDb = false;
        vm.Form.ItemName = string.Empty;
        vm.Form.ItemBaseType = Resources.Resources.Main032_cbTotalExchange;
        vm.Form.BaseTypeFontSize = 16;

        ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
    }
}

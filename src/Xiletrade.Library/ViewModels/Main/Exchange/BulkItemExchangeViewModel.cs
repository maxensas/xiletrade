using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class BulkItemExchangeViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private BulkViewModel bulk;

    [ObservableProperty]
    private ShopViewModel shop;

    public BulkItemExchangeViewModel(IServiceProvider serviceProvider, bool useCustomOrBulk)
    {
        _serviceProvider = serviceProvider;

        bulk = new(serviceProvider); // mandatory (auto select currency item on price check)
        if (useCustomOrBulk)
        {
            shop = new(serviceProvider);
        }
    }

    [RelayCommand]
    internal void Change(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            ExchangeViewModel exVm = @string.StartWith("get") ? Bulk.Get :
                @string.StartWith("pay") ? Bulk.Pay :
                @string.StartWith("shop") ? Shop.Exchange : null;
            if (exVm is null)
            {
                return;
            }

            if (exVm.CategoryIndex > 0 && exVm.CurrencyIndex > 0)
            {
                string tier = null;
                if (exVm.TierIndex > 0)
                {
                    tier = exVm.Tier[exVm.TierIndex].ToLowerInvariant().Replace("t", string.Empty);
                }
                var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                exVm.Image = Common.GetCurrencyImageUri(dm, exVm.Currency[exVm.CurrencyIndex], tier);
            }
            if (exVm.CurrencyIndex is 0)
            {
                exVm.Image = null;
            }
        }
    }
}

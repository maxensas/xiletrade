using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class BulkViewModel(IServiceProvider serviceProvider) : ViewModelBase
{
    [ObservableProperty]
    private string args;

    [ObservableProperty]
    private string currency;

    [ObservableProperty]
    private string tier;

    [ObservableProperty]
    private string stock = "1";

    [ObservableProperty]
    private ExchangeViewModel get = new(serviceProvider);

    [ObservableProperty]
    private ExchangeViewModel pay = new(serviceProvider);

    [RelayCommand]
    private void InvertBulk(object commandParameter)
    {
        int idxCategory = Get.CategoryIndex;
        int idxCurrency = Get.CurrencyIndex;
        int idxTier = Get.TierIndex;

        Get.CategoryIndex = Pay.CategoryIndex;
        if (Get.CategoryIndex > 0)
        {
            if (Pay.TierIndex >= 0)
            {
                Get.TierIndex = Pay.TierIndex;
            }
            Get.CurrencyIndex = Pay.CurrencyIndex;
        }

        Pay.CategoryIndex = idxCategory;
        if (idxCategory > 0)
        {
            if (idxTier >= 0)
            {
                Pay.TierIndex = idxTier;
            }
            Pay.CurrencyIndex = idxCurrency;
        }
    }
}

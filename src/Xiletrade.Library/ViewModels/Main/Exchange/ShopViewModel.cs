using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ShopViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string stock;

    [ObservableProperty]
    private ExchangeViewModel exchange;

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> getList;

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> payList;

    public ShopViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        stock = "1";
        exchange = new(serviceProvider);
        getList = new();
        payList = new();
    }

    [RelayCommand]
    private void RemoveGetList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            GetList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in GetList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    [RelayCommand]
    private void RemovePayList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            PayList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in PayList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    [RelayCommand]
    private void SelectShopIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            _serviceProvider.GetRequiredService<MainViewModel>()
                .Result.SelectedIndex.Shop = idx;
        }
    }

    [RelayCommand]
    private void ResetShopLists(object commandParameter)
    {
        PayList.Clear();
        GetList.Clear();
    }

    [RelayCommand]
    private void InvertShopLists(object commandParameter)
    {
        var tempList = PayList;
        PayList = GetList;
        GetList = tempList;
    }

    [RelayCommand]
    private void AddShopList(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            var shopList = @string.Contain("get") ? GetList :
                @string.Contain("pay") ? PayList : null;
            if (shopList is null)
            {
                return;
            }
            if (Exchange.CategoryIndex > 0 && Exchange.CurrencyIndex > 0)
            {
                string currency = Exchange.Currency[Exchange.CurrencyIndex];
                bool addItem = true;
                foreach (var item in shopList)
                {
                    if (item.Content == currency)
                    {
                        addItem = false;
                        break;
                    }
                }
                if (addItem)
                {
                    shopList.Add(new(shopList.Count, currency, _serviceProvider.GetRequiredService<MainViewModel>()
                        .Form.GetExchangeCurrencyTag(ExchangeType.Shop), Strings.Color.Azure));
                }
            }
        }
    }
}

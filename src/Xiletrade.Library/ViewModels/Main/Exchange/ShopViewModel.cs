using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ShopViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IMessageAdapterService _message;
    private readonly DataManagerService _dm;
    private readonly MainViewModel _vm;

    [ObservableProperty]
    private string stock;

    [ObservableProperty]
    private ExchangeViewModel exchange;

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> getList;

    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> payList;

    public ShopViewModel(DataManagerService dm, MainViewModel vm, 
        INavigationService navigation, IMessageAdapterService message)
    {
        _dm = dm;
        _vm = vm;
        _navigation = navigation;
        _message = message;

        stock = "1";
        exchange = new(_dm, _navigation, _vm);
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
            _vm.Result.SelectedIndex.Shop = idx;
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
                    shopList.Add(new(shopList.Count, currency, _vm.Form.ItemExchange
                        .GetCurrencyTag(ExchangeType.Shop), Strings.Color.Azure));
                }
            }
        }
    }

    [RelayCommand]
    private void ShowShopWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = _vm.Result.ShopList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = _vm.Result.ShopOffers[idx];
            _navigation.ShowWhisperView(data);
        }
    }

    [RelayCommand]
    private void RefreshShop(object commandParameter)
    {
        try
        {
            _navigation.ClearKeyboardFocus();
            _vm.Result.InitData();

            if (!_vm.Form.Tab.ShopSelected)
            {
                return;
            }

            if (GetList.Count > 0 && PayList.Count > 0)
            {
                if (!int.TryParse(Stock, out int minimumStock))
                {
                    minimumStock = 1;
                    Stock = "1";
                }
                _vm.Result.UpdateResultWithPoeApi(minimumStock);
                return;
            }

            _vm.Result.Shop.RightString = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
            _vm.Result.Shop.LeftString = string.Empty;
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Refreshing search error", MessageStatus.Error);
        }
    }

    internal Task OpenTask(string market, string league)
    {
        var curGetList = from list in GetList select list.ToolTip;
        var curPayList = from list in PayList select list.ToolTip;
        if (curGetList.Any() && curPayList.Any())
        {
            bool isInteger = int.TryParse(Stock, out int minimumStock);
            if (!isInteger)
            {
                minimumStock = 1;
                Stock = "1";
            }

            Models.Poe.Contract.Exchange change = new();
            change.ExchangeData.Status.Option = market;
            change.ExchangeData.Have = [.. curPayList];
            change.ExchangeData.Want = [.. curGetList];
            change.ExchangeData.Minimum = minimumStock;
            //change.ExchangeData.Collapse = true;
            change.Engine = "new";

            string url = Strings.Url.Exchange + league + "/?q=" + Uri.EscapeDataString(_dm.Json.Serialize<Models.Poe.Contract.Exchange>(change));
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _message.Show(ex.GetFormated(), "Failed to open PoE search window.", MessageStatus.Error);
            }
        }
        return Task.CompletedTask;
    }
}

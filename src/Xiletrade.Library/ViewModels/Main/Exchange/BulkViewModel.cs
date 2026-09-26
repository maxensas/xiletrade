using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class BulkViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IMessageAdapterService _message;
    private readonly DataManagerService _dm;
    private readonly MainViewModel _vm;

    [ObservableProperty]
    private string args;

    [ObservableProperty]
    private string currency;

    [ObservableProperty]
    private string tier;

    [ObservableProperty]
    private string stock;

    [ObservableProperty]
    private ExchangeViewModel get;

    [ObservableProperty]
    private ExchangeViewModel pay;

    public BulkViewModel(DataManagerService dm, MainViewModel vm,
        INavigationService navigation, IMessageAdapterService message)
    {
        _dm = dm;
        _vm = vm;
        _navigation = navigation;
        _message = message;

        stock = "1";
        get = new(_dm, _navigation, _vm);
        pay = new(_dm, _navigation, _vm);
    }

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

    [RelayCommand]
    private void SelectBulkIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            _vm.Result.SelectedIndex.Bulk = idx;
        }
    }

    [RelayCommand]
    private void ShowBulkWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = _vm.Result.BulkList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = _vm.Result.BulkOffers[idx];
            _navigation.ShowWhisperView(data);
        }
    }

    [RelayCommand]
    private void RefreshBulk(object commandParameter)
    {
        try
        {
            _navigation.ClearKeyboardFocus();
            _vm.Result.InitData();

            if (!_vm.Form.Tab.BulkSelected)
            {
                return;
            }

            if (Pay.CurrencyIndex > 0 && Get.CurrencyIndex > 0)
            {
                if (!int.TryParse(Stock, out int minimumStock))
                {
                    minimumStock = 1;
                    Stock = "1";
                }
                Get.ImageLast = Get.Image;
                Pay.ImageLast = Pay.Image;

                _vm.Form.Visible.BulkLastSearch = true;
                _vm.Result.UpdateWithApiAsync(minimumStock);
                if (!_vm.Form.IsPoeTwo)
                {
                    UpdateBulkNinjaTask();
                }
                return;
            }

            _vm.Result.Bulk.RightString = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
            _vm.Result.Bulk.LeftString = string.Empty;
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Refreshing search error", MessageStatus.Error);
        }
    }

    internal Task OpenTask(string market, string league)
    {
        if (Pay.CurrencyIndex < 1 || Get.CurrencyIndex < 1)
        {
            return Task.CompletedTask;
        }

        string[] exchange = new string[2];
        if (Pay.CurrencyIndex > 0)
        {
            var tmpBase = _dm.Bases.FindBaseByName(Pay.Currency[Pay.CurrencyIndex]);
            if (tmpBase is null)
            {
                exchange[0] = _vm.Form.ItemExchange.GetCurrencyTag(ExchangeType.Pay);
            }
        }
        if (Get.CurrencyIndex > 0)
        {
            var tmpBase = _dm.Bases.FindBaseByName(Get.Currency[Get.CurrencyIndex]);
            if (tmpBase is null)
            {
                exchange[1] = _vm.Form.ItemExchange.GetCurrencyTag(ExchangeType.Get);
            }
        }
        if (exchange[0] is null && exchange[1] is null)
        {
            return Task.CompletedTask;
        }

        bool isInteger = int.TryParse(Stock, out int minimumStock);
        if (!isInteger)
        {
            minimumStock = 1;
            Stock = "1";
        }

        Models.Poe.Contract.Exchange change = new();
        change.ExchangeData.Status.Option = market;
        change.ExchangeData.Minimum = minimumStock;
        if (exchange[0] is not null)
        {
            change.ExchangeData.Have = [exchange[0]];
        }
        if (exchange[1] is not null)
        {
            change.ExchangeData.Want = [exchange[1]];
        }

        string url = Strings.Url.Exchange + league + "/?q="
            + Uri.EscapeDataString(_dm.Json.Serialize<Models.Poe.Contract.Exchange>(change));
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Failed to open PoE search window.", MessageStatus.Error);
        }
        return Task.CompletedTask;
    }

    internal Task UpdateBulkNinjaTask()
    {
        return Task.Run(async () =>
        {
            try
            {
                if (_vm.TaskManager.NinjaTask is not null)
                {
                    await _vm.TaskManager.NinjaTask;
                }

                string tipGet = Get.Currency[Get.CurrencyIndex];
                string tagGet = string.Empty;
                string tipPay = Pay.Currency[Pay.CurrencyIndex];
                string tagPay = string.Empty;

                if (_dm.Config.Options.Language is not 8 and not 9) // ! tw & ! cn
                {
                    string translatedGet = Common.TranslateCurrency(_dm, Get.Currency[Get.CurrencyIndex]);
                    if (translatedGet is Strings.ChaosOrb)
                    {
                        _vm.Result.Data.NinjaEq.ChaosGet = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Get.Tier.Count > 0)
                        {
                            tier = Get.Tier[Get.TierIndex].ToLowerInvariant();
                        }

                        _vm.Result.Data.NinjaEq.ChaosGet = await _vm.Ninja.GetChaosEqAsync(_vm.Form.League[_vm.Form.LeagueIndex], translatedGet, tier);
                    }

                    if (_vm.Result.Data.NinjaEq.ChaosGet > 0 && translatedGet is not Strings.ChaosOrb)
                    {
                        tipGet = "1 " + Get.Currency[Get.CurrencyIndex] + " = " + _vm.Result.Data.NinjaEq.ChaosGet.ToString() + " chaos";
                        tagGet = "ninja";
                    }

                    string translatedPay = Common.TranslateCurrency(_dm, Pay.Currency[Pay.CurrencyIndex]);
                    if (translatedPay is Strings.ChaosOrb)
                    {
                        _vm.Result.Data.NinjaEq.ChaosPay = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Pay.Tier.Count > 0)
                        {
                            tier = Pay.Tier[Pay.TierIndex].Replace("T", string.Empty);
                        }

                        _vm.Result.Data.NinjaEq.ChaosPay = 
                            await _vm.Ninja.GetChaosEqAsync(_vm.Form.League[_vm.Form.LeagueIndex], translatedPay, tier);
                    }

                    if (_vm.Result.Data.NinjaEq.ChaosPay > 0 && translatedPay is not Strings.ChaosOrb)
                    {
                        tipPay = "1 " + Pay.Currency[Pay.CurrencyIndex] + " = " + _vm.Result.Data.NinjaEq.ChaosPay.ToString() + " chaos";
                        tagPay = "ninja";
                    }
                }
                Get.ImageLastToolTip = tipGet;
                Get.ImageLastTag = tagGet;
                Pay.ImageLastToolTip = tipPay;
                Pay.ImageLastTag = tagPay;
            }
            catch (Exception ex)
            {
                _message.Show(ex.GetFormated(), "Exception encountered : getting chaos equivalent", MessageStatus.Error);
            }
        });
    }
}

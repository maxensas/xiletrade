using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
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
    private static IServiceProvider _serviceProvider;

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

    public BulkViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        stock = "1";
        get = new(serviceProvider);
        pay = new(serviceProvider);
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
            _serviceProvider.GetRequiredService<MainViewModel>()
                .Result.SelectedIndex.Bulk = idx;
        }
    }

    [RelayCommand]
    private void ShowBulkWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var vm = _serviceProvider.GetRequiredService<MainViewModel>();
            var item = vm.Result.BulkList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = vm.Result.BulkOffers[idx];
            _serviceProvider.GetRequiredService<INavigationService>().ShowWhisperView(data);
        }
    }

    internal Task OpenBulkSearchTask(string market, string league)
    {
        if (Pay.CurrencyIndex < 1 || Get.CurrencyIndex < 1)
        {
            return Task.CompletedTask;
        }
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var vm = _serviceProvider.GetRequiredService<MainViewModel>();

        string[] exchange = new string[2];
        if (Pay.CurrencyIndex > 0)
        {
            var tmpBase = dm.Bases.FindBaseByName(Pay.Currency[Pay.CurrencyIndex]);
            if (tmpBase is null)
            {
                exchange[0] = vm.Form.ItemExchange.GetExchangeCurrencyTag(ExchangeType.Pay);
            }
        }
        if (Get.CurrencyIndex > 0)
        {
            var tmpBase = dm.Bases.FindBaseByName(Get.Currency[Get.CurrencyIndex]);
            if (tmpBase is null)
            {
                exchange[1] = vm.Form.ItemExchange.GetExchangeCurrencyTag(ExchangeType.Get);
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

        string url = Strings.ExchangeUrl + league + "/?q="
            + Uri.EscapeDataString(dm.Json.Serialize<Models.Poe.Contract.Exchange>(change));
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Failed to open PoE search window.", MessageStatus.Error);
        }
        return Task.CompletedTask;
    }

    internal Task UpdateBulkNinjaTask()
    {
        return Task.Run(async () =>
        {
            try
            {
                var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                var vm = _serviceProvider.GetRequiredService<MainViewModel>();

                vm.TaskManager.NinjaTask?.Wait();

                string tipGet = Get.Currency[Get.CurrencyIndex];
                string tagGet = string.Empty;
                string tipPay = Pay.Currency[Pay.CurrencyIndex];
                string tagPay = string.Empty;

                if (dm.Config.Options.Language is not 8 and not 9) // ! tw & ! cn
                {
                    string translatedGet = Common.TranslateCurrency(dm, Get.Currency[Get.CurrencyIndex]);
                    if (translatedGet is Strings.ChaosOrb)
                    {
                        vm.Result.Data.NinjaEq.ChaosGet = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Get.Tier.Count > 0)
                        {
                            tier = Get.Tier[Get.TierIndex].ToLowerInvariant();
                        }

                        vm.Result.Data.NinjaEq.ChaosGet = await vm.Ninja.GetChaosEqAsync(vm.Form.League[vm.Form.LeagueIndex], translatedGet, tier);
                    }

                    if (vm.Result.Data.NinjaEq.ChaosGet > 0 && translatedGet is not Strings.ChaosOrb)
                    {
                        tipGet = "1 " + Get.Currency[Get.CurrencyIndex] + " = " + vm.Result.Data.NinjaEq.ChaosGet.ToString() + " chaos";
                        tagGet = "ninja";
                    }

                    string translatedPay = Common.TranslateCurrency(dm, Pay.Currency[Pay.CurrencyIndex]);
                    if (translatedPay is Strings.ChaosOrb)
                    {
                        vm.Result.Data.NinjaEq.ChaosPay = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Pay.Tier.Count > 0)
                        {
                            tier = Pay.Tier[Pay.TierIndex].Replace("T", string.Empty);
                        }

                        vm.Result.Data.NinjaEq.ChaosPay = await vm.Ninja.GetChaosEqAsync(vm.Form.League[vm.Form.LeagueIndex], translatedPay, tier);
                    }

                    if (vm.Result.Data.NinjaEq.ChaosPay > 0 && translatedPay is not Strings.ChaosOrb)
                    {
                        tipPay = "1 " + Pay.Currency[Pay.CurrencyIndex] + " = " + vm.Result.Data.NinjaEq.ChaosPay.ToString() + " chaos";
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
                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                ms.Show(ex.GetFormated(), "Exception encountered : getting chaos equivalent", MessageStatus.Error);
            }
        });
    }
}

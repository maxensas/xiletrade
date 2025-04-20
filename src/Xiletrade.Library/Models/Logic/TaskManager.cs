using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Logic;

/// <summary>
/// Class used by Xiletrade to handle and launch tasks/threads.
/// </summary>
// TODO : make it cleaner, maybe use 'AsyncAwaitBestPractices' nuget.
// https://learn.microsoft.com/fr-fr/dotnet/api/system.threading.tasks.task.factory?view=net-8.0
internal sealed class TaskManager
{
    private static IServiceProvider _serviceProvider;
    private static MainViewModel Vm { get; set; }

    private static Task PriceTask { get; set; } = null;
    private static Task NinjaTask { get; set; } = null;
    private static Task MainUpdaterTask { get; set; } = null;
    private static CancellationTokenSource PriceTS { get; set; } = null;
    private static CancellationTokenSource NinjaTS { get; set; } = null;
    private static CancellationTokenSource MainUpdaterTS { get; set; } = null;

    internal TaskManager(MainViewModel vm, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Vm = vm;
    }

    // internal methods
    /// <summary>
    /// Avoid price check spam, previous threads need to end properly
    /// </summary>
    internal void CancelPreviousTasks()
    {
        if (PriceTask is not null && !PriceTask.IsCompleted)
        {
            PriceTS?.Cancel();
        }
        if (NinjaTask is not null && !NinjaTask.IsCompleted)
        {
            NinjaTS?.Cancel();
        }
    }

    internal void RunMainUpdaterTask(string itemText, bool openWindow = true)
    {
        if (itemText is null || itemText.Length is 0)
        {
            return;
        }
        MainUpdaterTS?.Cancel();
        MainUpdaterTS = new();
        MainUpdaterTask = Task.Run(() =>
            Vm.RunMainUpdater(itemText, openWindow, MainUpdaterTS.Token), MainUpdaterTS.Token);
    }

    internal void RunNinjaTask(NinjaInfo nInfo, XiletradeItem xiletradeItem)
    {
        NinjaTS?.Cancel();
        NinjaTS = new();
        NinjaTask = Task.Run(() =>
            Vm.Ninja.Check(nInfo, xiletradeItem, NinjaTS.Token), NinjaTS.Token);
    }

    internal void RunPriceTask(PricingInfo pricingInfo)
    {
        PriceTS?.Cancel();
        PriceTS = new();
        PriceTask = Task.Run(() => 
            Vm.Result.UpdateWithApi(pricingInfo, PriceTS.Token), PriceTS.Token);
        GC.Collect();
    }

    internal void FetchResultTask()
    {
        Task.Run(async () =>
        {
            ResultBar result = null;
            try
            {
                // doing this or it raise InvalidOperationException (cannot access thread)
                string market = Vm.Form.Market[Vm.Form.MarketIndex];
                bool sameUser = Vm.Form.SameUser;

                result = await Task.Run(() => Vm.Result.FetchWithApi(20, market, sameUser,
                    PriceTS.Token), PriceTS.Token); // maxFetch is set to 20 by default !
            }
            catch (InvalidOperationException ex)
            {
                result = new(emptyLine: true);
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Error : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Invalid operation", MessageStatus.Error);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is HttpRequestException exception)
                {
                    result = new(exception, false);
                }
            }

            if (!PriceTS.Token.IsCancellationRequested)
            {
                Vm.Result.RefreshResultBar(false, result);
            }
        });
    }

    internal void OpenWikiTask()
    {
        Task.Run(() =>
        {
            try
            {
                MainUpdaterTask?.Wait();

                var poeWiki = new PoeWiki(Vm.CurrentItem, Vm.Form.Rarity.Item);
                Process.Start(new ProcessStartInfo { FileName = poeWiki.Link, UseShellExecute = true });
            }
            catch (Exception)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(Resources.Resources.Main124_WikiFail, "Redirection to wiki failed ", MessageStatus.Warning);
            }
        });
    }

    internal void OpenNinjaTask()
    {
        Task.Run(() =>
        {
            try
            {
                MainUpdaterTask?.Wait();

                string influences = Vm.Form.GetInfluenceSate("/");
                if (influences.Length is 0) influences = Resources.Resources.Main036_None;

                var nInfo = new NinjaInfo(Vm.Form.League[Vm.Form.LeagueIndex]
                    , Vm.Form.Rarity.Item
                    , Vm.Form.Panel.Common.ItemLevel.Min.Trim()
                    , Vm.Form.Panel.Common.Quality.Min.Trim()
                    , Vm.Form.Panel.AlternateGemIndex
                    , Vm.Form.Panel.SynthesisBlight
                    , Vm.Form.Panel.BlighRavaged
                    , Vm.Form.Panel.Scourged
                    , influences);

                string url = Strings.UrlPoeNinja + Vm.Ninja.GetLink(nInfo, Vm.Form.GetXiletradeItem());
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(Resources.Resources.Main125_NinjaFail, "Redirection to ninja failed", MessageStatus.Warning);
            }
        });
    }

    internal void OpenPoedbTask()
    {
        Task.Run(() =>
        {
            try
            {
                MainUpdaterTask?.Wait();

                var poeDb = new PoeDb(Vm.CurrentItem);
                Process.Start(new ProcessStartInfo { FileName = poeDb.Link, UseShellExecute = true });
            }
            catch (Exception)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(Resources.Resources.Main201_PoedbFail, "Redirection to poedb failed ", MessageStatus.Warning);
            }
        });
    }

    internal void UpdateBulkNinjaTask()
    {
        var task = Task.Run(() =>
        {
            try
            {
                NinjaTask?.Wait();

                string tipGet = Vm.Form.Bulk.Get.Currency[Vm.Form.Bulk.Get.CurrencyIndex];
                string tagGet = string.Empty;
                string tipPay = Vm.Form.Bulk.Pay.Currency[Vm.Form.Bulk.Pay.CurrencyIndex];
                string tagPay = string.Empty;

                if (DataManager.Config.Options.Language is not 8 and not 9) // ! tw & ! cn
                {
                    string translatedGet = Common.TranslateCurrency(Vm.Form.Bulk.Get.Currency[Vm.Form.Bulk.Get.CurrencyIndex]);
                    if (translatedGet is Strings.ChaosOrb)
                    {
                        Vm.Result.Data.NinjaEq.ChaosGet = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Vm.Form.Bulk.Get.Tier.Count > 0)
                        {
                            tier = Vm.Form.Bulk.Get.Tier[Vm.Form.Bulk.Get.TierIndex].ToLowerInvariant();
                        }

                        Vm.Result.Data.NinjaEq.ChaosGet = Vm.Ninja.GetChaosEq(Vm.Form.League[Vm.Form.LeagueIndex], translatedGet, tier);
                    }

                    if (Vm.Result.Data.NinjaEq.ChaosGet > 0 && translatedGet is not Strings.ChaosOrb)
                    {
                        tipGet = "1 " + Vm.Form.Bulk.Get.Currency[Vm.Form.Bulk.Get.CurrencyIndex] + " = " + Vm.Result.Data.NinjaEq.ChaosGet.ToString() + " chaos";
                        tagGet = "ninja";
                    }

                    string translatedPay = Common.TranslateCurrency(Vm.Form.Bulk.Pay.Currency[Vm.Form.Bulk.Pay.CurrencyIndex]);
                    if (translatedPay is Strings.ChaosOrb)
                    {
                        Vm.Result.Data.NinjaEq.ChaosPay = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Vm.Form.Bulk.Pay.Tier.Count > 0)
                        {
                            tier = Vm.Form.Bulk.Pay.Tier[Vm.Form.Bulk.Pay.TierIndex].Replace("T", string.Empty);
                        }

                        Vm.Result.Data.NinjaEq.ChaosPay = Vm.Ninja.GetChaosEq(Vm.Form.League[Vm.Form.LeagueIndex], translatedPay, tier);
                    }

                    if (Vm.Result.Data.NinjaEq.ChaosPay > 0 && translatedPay is not Strings.ChaosOrb)
                    {
                        tipPay = "1 " + Vm.Form.Bulk.Pay.Currency[Vm.Form.Bulk.Pay.CurrencyIndex] + " = " + Vm.Result.Data.NinjaEq.ChaosPay.ToString() + " chaos";
                        tagPay = "ninja";
                    }
                }
                Vm.Form.Bulk.Get.ImageLastToolTip = tipGet;
                Vm.Form.Bulk.Get.ImageLastTag = tagGet;
                Vm.Form.Bulk.Pay.ImageLastToolTip = tipPay;
                Vm.Form.Bulk.Pay.ImageLastTag = tagPay;
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Exception encountered : getting chaos equivalent", MessageStatus.Error);
            }
        });
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;

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
    private static Task MainWindowUpdaterTask { get; set; } = null;
    private static CancellationTokenSource PriceTS { get; set; } = null;
    private static CancellationTokenSource NinjaTS { get; set; } = null;
    private static CancellationTokenSource MainWindowUpdaterTS { get; set; } = null;
    private static string ClipboardText { get; set; } = string.Empty;

    internal MainPricing Price { get; private set; }

    internal TaskManager(MainViewModel vm, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Vm = vm;
        Price = new(vm, _serviceProvider);
    }

    // internal methods
    /// <summary>
    /// Avoid price check spam, previous threads need to end properly
    /// </summary>
    internal void HandlePriceCheckSpam()
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

    internal void UpdateMainViewModel(string itemText, bool openWindow = true)
    {
        if (itemText is null || itemText.Length == 0)
        {
            return;
        }
        MainWindowUpdaterTS?.Cancel();
        MainWindowUpdaterTS = new();
        MainWindowUpdaterTask = Task.Run(() =>
        {
            static void CheckToken()
            {
                if (MainWindowUpdaterTS.IsCancellationRequested)
                {
                    throw new OperationCanceledException("MainWindowUpdaterTask was canceled");
                }
            }
            try
            {
                StringBuilder sbItemText = new(itemText);
                // some "\r" are missing while copying directly from the game, not from website copy
                sbItemText.Replace(Strings.CRLF, Strings.LF).Replace(Strings.LF, Strings.CRLF).Replace("()", string.Empty);
                string[] clipData = sbItemText.ToString().Trim().Split(new string[] { Strings.ItemInfoDelimiter }, StringSplitOptions.None);

                bool isPoeItem = clipData.Length > 1 &&
                clipData[0].StartsWith(Resources.Resources.General126_ItemClassPrefix, StringComparison.Ordinal);

                if (isPoeItem)
                {
                    ClipboardText = itemText;
                    if (clipData[^1].Contains("~b/o", StringComparison.Ordinal)
                    || clipData[^1].Contains("~price", StringComparison.Ordinal))
                    {
                        clipData = clipData.Where((source, index) => index != clipData.Length - 1).ToArray(); // clipDataWhitoutPrice
                    }
                    CheckToken();
                    Vm.Logic.ResetViewModel();
                    CheckToken();

                    Vm.Logic.CurrentItem = Vm.Logic.FillViewModel(clipData);
                    CheckToken();

                    if (openWindow)
                    {
                        Vm.Form.PriceTime = Price.Watch.StopAndGetTimeString();
                        UpdateItemPrices(Vm.Logic.GetItemFromViewModel(), 0);
                        _serviceProvider.GetRequiredService<INavigationService>().ShowMainView();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //nothing
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Item parsing error : method UpdateMainViewModel", MessageStatus.Error);
            }
        });
    }

    internal void UpdateItemPrices(XiletradeItem itemOptions, int minimumStock)
    {
        try
        {
            string league = Vm.Form.League[Vm.Form.LeagueIndex];
            int maxFetch = 0;
            List<string>[] entity = new List<string>[2];

            Vm.Form.FetchDetailIsEnabled = false;
            for (int i = 0; i < 5; i++)
            {
                Price.Buffer.StatsFetchDetail[i] = 0;
            }
            for (int i = 0; i < 5; i++)
            {
                Price.Buffer.StatsFetchBulk[i] = 0;
            }

            if (Vm.Form.Tab.QuickSelected || Vm.Form.Tab.DetailSelected)
            {
                Vm.Result.Quick.Price = Vm.Result.Detail.Price = Resources.Resources.Main006_PriceCheck; // "Price checking..."
                Vm.Result.Quick.PriceBis = Vm.Result.Detail.PriceBis = string.Empty;
                Vm.Result.DetailList.Clear();

                Vm.Result.Quick.Total = string.Empty;
                Vm.Result.Detail.Total = Resources.Resources.Main005_PriceResearch; //"Research ..."
                maxFetch = (int)DataManager.Config.Options.SearchFetchDetail;

                // !StringsTable.Culture[DataManager.Config.Options.Language].Equals("zh-TW") && !StringsTable.Culture[DataManager.Config.Options.Language].Equals("zh-CN")
                if (DataManager.Config.Options.Language is not 8 and not 9)
                {
                    string rarity = Vm.Form.Rarity.Item;
                    string influences = string.Empty;
                    foreach (KeyValuePair<string, bool> inf in Vm.Logic.GetInfluenceSate())
                    {
                        if (inf.Value)
                        {
                            if (influences.Length > 0) influences += "/";
                            influences += inf.Key;
                        }
                    }
                    if (influences.Length is 0) influences = Resources.Resources.Main036_None;


                    string name = Vm.Logic.CurrentItem.NameEn;
                    string type = Vm.Logic.CurrentItem.TypeEn;
                    string itemInherit = Vm.Logic.CurrentItem.Inherits[0].ToLowerInvariant();

                    string lvlMin = Vm.Form.Panel.Common.ItemLevel.Min.Trim();
                    string qualMin = Vm.Form.Panel.Common.Quality.Min.Trim();
                    int altIdx = Vm.Form.Panel.AlternateGemIndex;
                    bool sb = Vm.Form.Panel.SynthesisBlight;
                    bool ravaged = Vm.Form.Panel.BlighRavaged;
                    bool scourgedMap = Vm.Form.Panel.Scourged;

                    NinjaTS?.Cancel();
                    NinjaTS = new();
                    NinjaTask = Task.Run(() =>
                        Addons.CheckNinja(Vm, league, rarity, influences, lvlMin, qualMin, altIdx, sb, ravaged, scourgedMap, itemOptions, NinjaTS.Token), NinjaTS.Token);
                }
            }
            else if (Vm.Form.Tab.BulkSelected)
            {
                Vm.Result.Bulk.Price = Resources.Resources.Main003_PriceFetching; // "Fetching data..."
                Vm.Result.Bulk.PriceBis = string.Empty;
                Vm.Result.BulkList.Clear();
                Vm.Result.BulkOffers.Clear();
                Vm.Result.Bulk.Total = Resources.Resources.Main005_PriceResearch; //"Research ..."

                if (Vm.Form.Bulk.Pay.CurrencyIndex > 0 && Vm.Form.Bulk.Get.CurrencyIndex > 0)
                {
                    entity[0] = new() { Vm.Logic.GetExchangeCurrencyTag(Exchange.Pay) };
                    entity[1] = new() { Vm.Logic.GetExchangeCurrencyTag(Exchange.Get) };
                    maxFetch = (int)DataManager.Config.Options.SearchFetchBulk;
                }
            }
            else if (Vm.Form.Tab.ShopSelected)
            {
                Vm.Result.Shop.Price = Resources.Resources.Main003_PriceFetching; // "Fetching data..."
                Vm.Result.Shop.PriceBis = string.Empty;
                Vm.Result.ShopList.Clear();
                Vm.Result.ShopOffers.Clear();
                Vm.Result.Shop.Total = Resources.Resources.Main005_PriceResearch; //"Research ..."

                var curGetList = from list in Vm.Form.Shop.GetList select list.ToolTip;
                if (!curGetList.Any())
                {
                    return;
                }
                entity[1] = curGetList.ToList();
                var curPayList = from list in Vm.Form.Shop.PayList select list.ToolTip;
                if (!curPayList.Any())
                {
                    return;
                }
                entity[0] = curPayList.ToList();
            }
            bool hideSameUser = Vm.SameUser;

            if (entity[0] is null)
            {
                entity[0] = new() { Json.GetSerialized(itemOptions, Vm.Logic.CurrentItem, true, Vm.Form.Market[Vm.Form.MarketIndex]) };
                string test = entity[0][0];
            }
            PriceTS?.Cancel();
            PriceTS = new();
            PriceTask = Task.Run(() => Price.UpdateVmWithApi(entity, league, Vm.Form.Market[Vm.Form.MarketIndex], minimumStock, maxFetch, hideSameUser, PriceTS.Token)
                , PriceTS.Token);
            GC.Collect();
        }
        catch (Exception ex)
        {
            throw new Exception("Exception encountered : method PriceUpdate", ex);
        }
    }

    internal async Task FetchDetailResults()
    {
        string[] result = new string[2];
        try
        {
            // doing this or it raise InvalidOperationException (cannot access thread)
            string market = Vm.Form.Market[Vm.Form.MarketIndex];
            bool sameUser = Vm.SameUser;

            result = await Task.Run(() => Price.FillDetailVm(20, market, sameUser,
                PriceTS.Token), PriceTS.Token); // maxFetch is set to 20 by default !
        }
        catch (InvalidOperationException ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Invalid operation", MessageStatus.Error);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is HttpRequestException exception)
            {
                string[] mess = exception.Message.Split(':');
                result[0] = "The request encountered" + Strings.LF + "an exception. [C]";
                result[1] = mess.Length > 1 ? "ERROR : Code " + mess[1].Trim() : exception.Message;
            }
        }

        if (!PriceTS.Token.IsCancellationRequested)
        {
            Vm.Logic.RefreshViewModelStatus(false, result);
        }
    }

    internal async Task OpenSearch(string sEntity, string league)
    {
        string sResult = await Task.Run(() =>
        {
            string result = string.Empty;
            try
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                result = service.SendHTTP(sEntity, Strings.TradeApi[DataManager.Config.Options.Language] + league, Client.Trade).Result;
                if (result.Length > 0)
                {
                    ResultData resultData = Json.Deserialize<ResultData>(result);// voir
                    string url = Strings.TradeUrl[DataManager.Config.Options.Language] + league + "/" + resultData.ID;
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is HttpRequestException exception)
                {
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show("Cannot open search in browser : \n" + exception.Message, "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                }
            }

            return result;
        });
        return;
    }

    internal void OpenWiki()
    {
        var task = Task.Run(() =>
        {
            try
            {
                MainWindowUpdaterTask?.Wait();

                string rarity = Vm.Form.Rarity.Item;
                string url = Addons.GetPoeWikiLink(rarity);
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(Resources.Resources.Main124_WikiFail, "Redirection to wiki failed ", MessageStatus.Warning);
            }
        });
    }

    internal void OpenNinja()
    {
        var task = Task.Run(() =>
        {
            try
            {
                MainWindowUpdaterTask?.Wait();

                string influences = string.Empty;
                foreach (KeyValuePair<string, bool> inf in Vm.Logic.GetInfluenceSate())
                {
                    if (inf.Value)
                    {
                        if (influences.Length > 0) influences += "/";
                        influences += inf.Key;
                    }
                }
                if (influences.Length == 0) influences = Resources.Resources.Main036_None;

                string url = Strings.UrlPoeNinja + Addons.GetNinjaLink(Vm.Form.League[Vm.Form.LeagueIndex], Vm.Form.Rarity.Item, influences,
                    Vm.Form.Panel.Common.ItemLevel.Min.Trim(), Vm.Form.Panel.Common.Quality.Min.Trim(), Vm.Form.Panel.AlternateGemIndex,
                    Vm.Form.Panel.SynthesisBlight, Vm.Form.Panel.BlighRavaged, Vm.Form.Panel.Scourged, Vm.Logic.GetItemFromViewModel());
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(Resources.Resources.Main125_NinjaFail, "Redirection to ninja failed", MessageStatus.Warning);
            }
        });
    }

    internal void OpenPoeDb()
    {
        var task = Task.Run(() =>
        {
            try
            {
                MainWindowUpdaterTask?.Wait();

                string url = Addons.GetPoeDbLink();
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(Resources.Resources.Main201_PoedbFail, "Redirection to poedb failed ", MessageStatus.Warning);
            }
        });
    }

    internal void UpdateNinjaChaosEq()
    {
        // unid task & no cancel token yet
        // TODO : add better task management
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
                        Price.Buffer.NinjaChaosEqGet = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Vm.Form.Bulk.Get.Tier.Count > 0)
                        {
                            tier = Vm.Form.Bulk.Get.Tier[Vm.Form.Bulk.Get.TierIndex].ToLowerInvariant();
                        }

                        Price.Buffer.NinjaChaosEqGet = Addons.GetNinjaChaosEq(Vm.Form.League[Vm.Form.LeagueIndex], translatedGet, tier);
                    }

                    if (Price.Buffer.NinjaChaosEqGet > 0 && translatedGet is not Strings.ChaosOrb)
                    {
                        tipGet = "1 " + Vm.Form.Bulk.Get.Currency[Vm.Form.Bulk.Get.CurrencyIndex] + " = " + Price.Buffer.NinjaChaosEqGet.ToString() + " chaos";
                        tagGet = "ninja";
                    }

                    string translatedPay = Common.TranslateCurrency(Vm.Form.Bulk.Pay.Currency[Vm.Form.Bulk.Pay.CurrencyIndex]);
                    if (translatedPay is Strings.ChaosOrb)
                    {
                        Price.Buffer.NinjaChaosEqPay = 1;
                    }
                    else
                    {
                        string tier = null;
                        if (Vm.Form.Bulk.Pay.Tier.Count > 0)
                        {
                            tier = Vm.Form.Bulk.Pay.Tier[Vm.Form.Bulk.Pay.TierIndex].Replace("T", string.Empty);
                        }

                        Price.Buffer.NinjaChaosEqPay = Addons.GetNinjaChaosEq(Vm.Form.League[Vm.Form.LeagueIndex], translatedPay, tier);
                    }

                    if (Price.Buffer.NinjaChaosEqPay > 0 && translatedPay is not Strings.ChaosOrb)
                    {
                        tipPay = "1 " + Vm.Form.Bulk.Pay.Currency[Vm.Form.Bulk.Pay.CurrencyIndex] + " = " + Price.Buffer.NinjaChaosEqPay.ToString() + " chaos";
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

    internal void UpdatePoePricesTab()
    {
        var poePrices = Task.Run(() =>
        {
            string errorMsg = string.Empty;
            List<Tuple<string, string>> lines = new();
            try
            {
                Vm.Result.PoepricesList.Clear();
                Vm.Result.PoepricesList.Add(new ListItemViewModel { Content = "Waiting response from poeprices.info ..." });

                var service = _serviceProvider.GetRequiredService<NetService>();
                string result = service.SendHTTP(null, Strings.ApiPoePrice + DataManager.Config.Options.League + "&i=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(ClipboardText)), Client.PoePrice).Result;
                if (result is null || result.Length == 0)
                {
                    errorMsg = "Http request error : www.poeprices.info cannot respond, please try again later.";
                    return;
                }
                PoePrices jsonData = Json.Deserialize<PoePrices>(result);
                if (jsonData is null)
                {
                    errorMsg = "Json deserialize error : difference between Xiletrade and poeprices json format.";
                    return;
                }
                if (jsonData.Error != 0)
                {
                    errorMsg = "Issue with Poeprices.info, error received: " + jsonData.ErrorMsg;
                    return;
                }

                lines.Add(new Tuple<string, string>("Result from poeprices.info website :", string.Empty));
                //liPoePriceInfo.Items.Add(new ListBoxItem { Content = "Results from poeprices.info (Machine Learning Prediction)", Foreground = System.Windows.Media.Brushes.LimeGreen });
                // FontStyle="Italic"
                //double score = (double)(jsonData.PredConfidenceScore is double ? jsonData.PredConfidenceScore : 0);
                _ = double.TryParse(jsonData.PredConfidenceScore.ToString(), out double score);
                lines.Add(new Tuple<string, string>("Confidence score : " + string.Format("{0:0.00}", score) + "%", score >= 90 ? Strings.Color.LimeGreen : Strings.Color.Red));

                if (jsonData.Min != 0.0)
                    lines.Add(new Tuple<string, string>("Min price : " + string.Format("{0:0.0}", jsonData.Min) + " " + jsonData.Currency, Strings.Color.LimeGreen));
                if (jsonData.Max != 0.0)
                    lines.Add(new Tuple<string, string>("Max price : " + string.Format("{0:0.0}", jsonData.Max) + " " + jsonData.Currency, Strings.Color.LimeGreen));

                if (jsonData.PredExplantion is not null && jsonData.PredExplantion.Length > 0)
                {
                    lines.Add(new Tuple<string, string>("Weight:    Mod: ", Strings.Color.LightGray));
                    foreach (Array items in jsonData.PredExplantion)
                    {
                        lines.Add(new Tuple<string, string>("  " + string.Format("{0:0.00}", items.GetValue(1)) + "       " + items.GetValue(0), Strings.Color.LightGray));
                    }
                }
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                if (ex.InnerException is HttpRequestException exception)
                {
                    service.Show(ex.Message, "Poeprices error code : " + exception.StatusCode, MessageStatus.Information);
                    return;
                }
                service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "UTF8 Deserialize error", MessageStatus.Error);
            }
            finally
            {
                if (errorMsg.Length > 0)
                {
                    lines.Add(new Tuple<string, string>(errorMsg, Strings.Color.Red));
                }

                Vm.Result.PoepricesList.Clear();
                foreach (Tuple<string, string> line in lines)
                {
                    Vm.Result.PoepricesList.Add(new ListItemViewModel { Content = line.Item1, FgColor = line.Item2 });
                }
            }
        });
    }
}

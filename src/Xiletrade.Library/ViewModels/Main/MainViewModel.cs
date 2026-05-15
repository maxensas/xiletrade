using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Models.Application.Diagnostic;
using Xiletrade.Library.Models.CoE.Domain;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Poe.Contract.Two;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Models.Wiki.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Command;
using Xiletrade.Library.ViewModels.Main.Form;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class MainViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private FormViewModel form;

    [ObservableProperty]
    private ResultViewModel result;

    [ObservableProperty]
    private NinjaViewModel ninja;

    [ObservableProperty]
    private string notifyName;

    [ObservableProperty]
    private bool showMinMax;

    [ObservableProperty]
    private double viewScale;

    [ObservableProperty]
    private bool authenticated;

    [ObservableProperty]
    private bool authentication;

    internal string ClipboardText { get; set; } = string.Empty;
    public List<MouseGestureCom> GestureList { get; private set; } = new();

    //viewmodels split
    public MainCommand Commands { get; private set; }
    public TrayMenuCommand TrayCommands { get; private set; }

    //models
    internal ItemData Item { get; private set; }
    internal StopWatch StopWatch { get; } = new();
    internal TaskManager TaskManager { get; } = new();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        TrayCommands = new(_serviceProvider);
        Commands = new(this, _serviceProvider);
        NotifyName = "Xiletrade " + Common.GetFileVersion();
    }

    //internal methods
    internal void InitViewModels(bool useBulk = false)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        ViewScale = dm.Config.Options.Scale;

        Result = new(_serviceProvider);
        Ninja = new(_serviceProvider);
        if (useBulk)
        {
            Form = new(_serviceProvider, useBulk);
        }
    }

    // clear item data and lists on closing main view.
    internal void ClearContentViewModels()
    {
        Item = null;
        Form.ClearLists();
        Result.ClearLists();

        // To release string caches from vm
        if (Form.CustomSearch is not null)
        {
            Form.CustomSearch.Search = null;
            Form.CustomSearch.Stat = null;
        }
    }

    internal Task OpenUrlTask(string url, UrlType type)
    {
        return Task.Run(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception)
            {
                var message = type is UrlType.PoeDb ? Resources.Resources.Main201_PoedbFail
                : type is UrlType.PoeWiki ? Resources.Resources.Main124_WikiFail
                : type is UrlType.Ninja ? Resources.Resources.Main125_NinjaFail
                : type is UrlType.CraftOfExile ? "Fail CoE"
                : string.Empty;
                var caption = type is UrlType.PoeDb ? "Redirection to poedb failed "
                : type is UrlType.PoeWiki ? "Redirection to wiki failed "
                : type is UrlType.Ninja ? "Redirection to ninja failed "
                : type is UrlType.CraftOfExile ? "Redirection to Craft of Exile failed "
                : string.Empty;

                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                ms.Show(message, caption, MessageStatus.Warning);
            }
        });
    }

    internal async Task RunMainUpdaterTaskAsync(string fonction)
    {
        try
        {
            await TaskManager.CancelPreviousTasksAsync().ConfigureAwait(false);
            
            var token = TaskManager.GetMainUpdaterToken(initCts: true);

            bool openWikiOnly = fonction is Strings.Feature.wiki;
            bool openNinjaOnly = fonction is Strings.Feature.ninja;
            bool openCoeOnly = fonction is Strings.Feature.coe;
            bool openWindow = !openWikiOnly && !openNinjaOnly && !openCoeOnly;

            TaskManager.MainUpdaterTask = Task.Run(() =>
            {
                try
                {
#if DEBUG
                    var logger = _serviceProvider.GetRequiredService<ILogger<MainViewModel>>();
                    logger.LogInformation("Starting Main Updater Task.");
#endif
                    var infoDesc = new InfoDescription(ClipboardText);
                    if (!infoDesc.IsPoeItem)
                        return;

                    var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                    Item = new ItemData(dm, infoDesc);
                    Form = new(_serviceProvider, Item, infoDesc, ShowMinMax)
                    {
                        FillTime = StopWatch.StopAndGetTimeString()
                    };
                    if (Form.Tab.BulkEnable) // TOFIX : Select currency in 'Pay' section
                    {
                        _ = Form.SelectExchangeCurrency("pay/equals",
                            Item.Type, Item.Flag.Map ? Item.Options.MapTier : string.Empty);
                    }
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    logger.LogInformation("Main view model updated.");
#endif
                    if (openWindow)
                    {
                        _serviceProvider.GetRequiredService<INavigationService>().ShowMainView();
                        UpdatePrices(minimumStock: 0);
                        return;
                    }

                    if (openWikiOnly)
                    {
                        var poeWiki = new PoeWiki(dm, Item);
                        _ = OpenUrlTask(poeWiki.Link, UrlType.PoeWiki);
                        return;
                    }
                    if (openNinjaOnly)
                    {
                        _ = OpenUrlTask(Ninja.FullUrl, UrlType.Ninja);
                        return;
                    }
                    if (openCoeOnly)
                    {
                        var coe = new CraftOfExile(ClipboardText);
                        _ = OpenUrlTask(coe.Link, UrlType.CraftOfExile);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Task canceled: ignore
                }
                catch (Exception ex)
                {
                    var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    ms.Show(ex.GetFormated(), "Item parsing error : method UpdateMainViewModel", MessageStatus.Error);
                }
            }, token);
        }
        catch (Exception ex)
        {
            // Log cancel/initialization errors (this doesn't happen often, but better to be safe than sorry)
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Anti-spam task error", MessageStatus.Warning);
        }
    }

    internal void UpdatePrices(int minimumStock)
    {
        try
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();

            int maxFetch = 0;
            var entity = new List<string>[2];

            Form.FetchDetailIsEnabled = false;

            if (Form.Tab.QuickSelected || Form.Tab.DetailSelected)
            {
                Result.Detail.Total = Resources.Resources.Main005_PriceResearch;
                Result.Quick.RightString = Result.Detail.RightString = Resources.Resources.Main006_PriceCheck;
                Result.Quick.LeftString = Result.Detail.LeftString = string.Empty;
                Result.Quick.Total = string.Empty;
                Result.DetailList.Clear();

                maxFetch = (int)dm.Config.Options.SearchFetchDetail;

                if (dm.Config.Options.Gateway is not 8 and not 9)
                {
                    TaskManager.NinjaTask = Ninja.TryUpdatePriceTask();
                }
            }
            else if (Form.Tab.BulkSelected)
            {
                Result.Bulk.RightString = Resources.Resources.Main003_PriceFetching;
                Result.Bulk.Total = Resources.Resources.Main005_PriceResearch;
                Result.Bulk.LeftString = string.Empty;
                Result.BulkList.Clear();
                Result.BulkOffers.Clear();

                if (Form.Bulk.Pay.CurrencyIndex > 0 && Form.Bulk.Get.CurrencyIndex > 0)
                {
                    entity[0] = new() { Form.GetExchangeCurrencyTag(ExchangeType.Pay) };
                    entity[1] = new() { Form.GetExchangeCurrencyTag(ExchangeType.Get) };
                    maxFetch = (int)dm.Config.Options.SearchFetchBulk;
                }
            }
            else if (Form.Tab.ShopSelected)
            {
                Result.Shop.RightString = Resources.Resources.Main003_PriceFetching;
                Result.Shop.Total = Resources.Resources.Main005_PriceResearch;
                Result.Shop.LeftString = string.Empty;
                Result.ShopList.Clear();
                Result.ShopOffers.Clear();

                var curGetList = from list in Form.Shop.GetList select list.ToolTip;
                var curPayList = from list in Form.Shop.PayList select list.ToolTip;
                if (!curGetList.Any() || !curPayList.Any())
                {
                    return;
                }
                entity[0] = curPayList.ToList();
                entity[1] = curGetList.ToList();
            }

            if (entity[0] is null)
            {
                try
                {
                    entity[0] = new() { GetSerialized(Form.Market[Form.MarketIndex], useSaleType: true) };
                }
                catch (Exception ex)
                {
                    var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    ms.Show(ex.GetFormated(), "JSON serialization error", MessageStatus.Error);
                }
            }
            // conditions needed to price check using GGG APIs
            if (Form.Tab.BulkSelected || Form.Tab.ShopSelected 
                || (Item is not null && !Item.State.ExchangeCurrency))
            {
                var priceInfo = new PricingInfo(entity, Form.League[Form.LeagueIndex]
                , Form.Market[Form.MarketIndex], minimumStock, maxFetch, Form.SameUser, Form.Tab.BulkSelected);
                Result.UpdateWithApi(priceInfo);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Exception encountered : method UpdateItemPrices", ex);
        }
    }

    internal void LaunchCustomSearch()
    {
        if (!Form.Tab.CustomSearchSelected ||
            (string.IsNullOrEmpty(Form.CustomSearch.Search.SearchQuery) && Form.CustomSearch.UnidUniquesIndex is 0))
        {
            return;
        }

        try
        {
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();

            Result.InitData();
            Result.DetailList.Clear();

            var json = GetSerialized(Form.Market[Form.MarketIndex], customSearch: true);
            var maxFetch = (int)dm.Config.Options.SearchFetchDetail;

            var priceInfo = new PricingInfo([new() { json }, null], Form.League[Form.LeagueIndex]
                , Form.Market[Form.MarketIndex], minimumStock: 1, maxFetch
                , Form.SameUser, Form.Tab.BulkSelected);

            Result.UpdateWithApi(priceInfo);
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Custom search error", MessageStatus.Error);
        }
    }

    internal string GetSerialized(string market, bool useSaleType = false, bool customSearch = false)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var isPoe2 = dm.Config.Options.GameVersion is 1;
        var xItem = Form.GetXiletradeItem(customSearch);

        if (!customSearch)
        {
            if (isPoe2)
            {
                var jsonDataTwo = new JsonDataTwoFactory(dm).Create(xItem, Item, useSaleType, market);
                return dm.Json.Serialize<JsonDataTwo>(jsonDataTwo);
            }
            var jsonData = new JsonDataFactory(dm).Create(xItem, Item, useSaleType, market);
            return dm.Json.Serialize<JsonData>(jsonData);
        }
        
        var search = Form.CustomSearch.Search.SearchQuery;
        var unid = Form.CustomSearch.UnidUniquesIndex > 0 ?
            Form.CustomSearch.UnidUniques[Form.CustomSearch.UnidUniquesIndex] : null;

        if (isPoe2)
        {
            var jsonDataTwo = new JsonDataTwoFactory(dm).Create(xItem, unid, market, search);
            return dm.Json.Serialize<JsonDataTwo>(jsonDataTwo);
        }
        var json = new JsonDataFactory(dm).Create(xItem, unid, market, search);
        return dm.Json.Serialize<JsonData>(json);
    }
}

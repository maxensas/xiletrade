using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Xiletrade.Library.Interactions;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Diagnostic;
using Xiletrade.Library.Models.CoE.Domain;
using Xiletrade.Library.Models.DB.Domain;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Interface;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Models.Wiki.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Form;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<MainViewModel> _logger;
    private readonly DataManagerService _dm;
    private readonly INetService _net;
    private readonly IMessageAdapterService _message;

    private INavigationService Navigation => _sp.GetRequiredService<INavigationService>();
    
    private ResultViewModel GetNewResult => _sp.GetRequiredService<ResultViewModel>();
    private NinjaViewModel GetNewNinja => _sp.GetRequiredService<NinjaViewModel>();

    private FormViewModel GetNewForm(bool useCustomOrBulk) =>
        _sp.CreateInstance<FormViewModel>(useCustomOrBulk);

    private FormViewModel GetNewForm(ItemData item, InfoDescription infoDesc, bool showMinMax) =>
        _sp.CreateInstance<FormViewModel>(item, infoDesc, showMinMax);

    [ObservableProperty]
    private FormViewModel form;

    [ObservableProperty]
    private ResultViewModel result;

    [ObservableProperty]
    private NinjaViewModel ninja;

    [ObservableProperty]
    private bool showMinMax;

    [ObservableProperty]
    private double viewScale;

    internal string ClipboardText { get; set; } = string.Empty;
    public List<MouseGestureCom> GestureList { get; private set; } = new();

    //models
    internal ItemData Item { get; private set; }
    internal StopWatch StopWatch { get; } = new();
    internal TaskManager TaskManager { get; } = new();

    public MainViewModel(IServiceProvider sp, ILogger<MainViewModel> logger, 
        DataManagerService dm, INetService net, IMessageAdapterService message)
    {
        _sp = sp;
        _logger = logger;
        _dm = dm;
        _net = net;
        _message = message;
    }

    [RelayCommand]
    private void ViewLoaded(object commandParameter) => Navigation.SetMainHandle(commandParameter);

    [RelayCommand]
    private void ViewDeactivated(object commandParameter)
    {
        if (Form is not null && !Form.Tab.CustomSearchSelected
            && !Form.Tab.BulkSelected && !Form.Tab.ShopSelected
            && _dm.Config.Options.Autoclose)
        {
            Navigation.CloseMainView();
        }
    }

    [RelayCommand]
    private void ViewMinimized(object commandParameter) => Form.Minimized = !Form.Minimized;

    [RelayCommand]
    private void AutoClose(object commandParameter) => _dm.Config.Options.Autoclose = Form.AutoClose;

    [RelayCommand]
    private void CloseView(object commandParameter)
    {
        Navigation.CloseMainView();
        ClearContentViewModels();
    }

    [RelayCommand]
    private void UpdateOpacity(object commandParameter)
    {
        if (_dm.Config is not null)
        {
            _dm.Config.Options.Opacity = Form.Opacity;
        }
    }

    [RelayCommand]
    private void ExpanderCollapse(object commandParameter)
        => _dm.SaveConfiguration(_dm.Json.Serialize<ConfigData>(_dm.Config));

    [RelayCommand]
    private void OpenWiki(object commandParameter) 
        => OpenUrlTask(new PoeWiki(_dm, Item).Link, UrlType.PoeWiki);

    [RelayCommand]
    private void OpenPoeDb(object commandParameter) 
        => OpenUrlTask(new PoeDb(_dm, Item).Link, UrlType.PoeDb);

    [RelayCommand]
    private void OpenNinja(object commandParameter) 
        => OpenUrlTask(Ninja.FullUrl, UrlType.Ninja);

    [RelayCommand]
    private void OpenCraftOfExile(object commandParameter) 
        => OpenUrlTask(new CraftOfExile(ClipboardText).Link, UrlType.CraftOfExile);

    [RelayCommand]
    private void OpenXiletradeChangelog(object commandParameter) 
        => OpenUrlTask(Strings.UrlChangelog, UrlType.Xiletrade);

    [RelayCommand]
    private async Task OpenSearch(object commandParameter)
    {
        string market = Form.Market[Form.MarketIndex];
        string league = Form.League[Form.LeagueIndex];

        if (Form.Tab.BulkSelected)
        {
            await Form.ItemExchange.Bulk.OpenBulkSearchTask(market, league);
            return;
        }
        if (Form.Tab.ShopSelected)
        {
            await Form.ItemExchange.Shop.OpenShopSearchTask(market, league);
            return;
        }
        var priceCheck = Form.Tab.QuickSelected || Form.Tab.DetailSelected;
        if (priceCheck || Form.Tab.CustomSearchSelected)
        {
            var sEntity = GetSerialized(market, customSearch: !priceCheck);
            if (!string.IsNullOrEmpty(sEntity))
            {
                await OpenSearchTask(sEntity, league);
            }
        }
    }

    private async Task OpenSearchTask(string sEntity, string league)
    {
        try
        {
            var result = await _net.SendHTTP(sEntity, Strings.TradeApi + league, Client.Trade);
            if (result.Length > 0)
            {
                var resultData = _dm.Json.Deserialize<ResultData>(result);
                string url = Strings.TradeUrl + league + "/" + resultData.Id;
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is HttpRequestException exception)
            {
                _message.Show("Cannot open search in browser : \n" + exception.Message, "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
            }
        }
    }

    //internal methods
    internal void InitViewModels(bool useCustomOrBulk = false)
    {
        ViewScale = _dm.Config.Options.Scale;

        Result = GetNewResult;
        Ninja = GetNewNinja;
        if (useCustomOrBulk)
        {
            Form = GetNewForm(useCustomOrBulk);
        }
    }

    // clear item data and lists on closing main view.
    internal void ClearContentViewModels()
    {
        Item = null;
        Form?.ClearLists();
        Result?.ClearLists();

        // To release string caches from vm
        if (Form?.CustomSearch is not null)
        {
            Form.CustomSearch.Search = null;
            Form.CustomSearch.Stat = null;
        }
    }

    internal Task OpenUrlTask(string url, UrlType type)
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

            _message.Show(message, caption, MessageStatus.Warning);
        }
        return Task.CompletedTask;
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
                    _logger.LogInformation("Starting Main Updater Task.");
#endif
                    var infoDesc = new InfoDescription(ClipboardText);
                    if (!infoDesc.IsPoeItem)
                        return;

                    Item = new ItemData(_dm, infoDesc);
                    Form = GetNewForm(Item, infoDesc, ShowMinMax);
                    Form.FillTime = StopWatch.StopAndGetTimeString();
                    
                    if (Form.Tab.BulkEnable) // TOFIX : Select currency in 'Pay' section
                    {
                        _ = Form.SelectExchangeCurrency("pay/equals",
                            Item.Type, Item.Flag.Map.IsMap ? Item.Options.MapTier : string.Empty);
                    }
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    _logger.LogInformation("Main view model updated.");
#endif
                    if (openWindow)
                    {
                        Navigation.ShowMainView();
                        if (_dm.Config.Options.Gateway is not 8 and not 9)
                        {
                            TaskManager.NinjaTask = Ninja.TryUpdateNinjaTask();
                        }
                        Result.UpdateResultWithPoeApi(minimumStock: 0);
                        return;
                    }

                    if (openWikiOnly)
                    {
                        var poeWiki = new PoeWiki(_dm, Item);
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
                    _message.Show(ex.GetFormated(), "Item parsing error : method UpdateMainViewModel", MessageStatus.Error);
                }
            }, token);
        }
        catch (Exception ex)
        {
            // Log cancel/initialization errors (this doesn't happen often, but better to be safe than sorry)
            _message.Show(ex.GetFormated(), "Anti-spam task error", MessageStatus.Warning);
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
            Navigation.ClearKeyboardFocus();

            Result.InitData();
            Result.DetailList.Clear();

            var json = GetSerialized(Form.Market[Form.MarketIndex], customSearch: true);
            var maxFetch = (int)_dm.Config.Options.SearchFetchDetail;

            var priceInfo = new PricingInfo([new() { json }, null], Form.League[Form.LeagueIndex]
                , Form.Market[Form.MarketIndex], minimumStock: 1, maxFetch
                , Form.SameUser, Form.Tab.BulkSelected);

            Result.UpdateWithPoeApi(priceInfo);
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Custom search error", MessageStatus.Error);
        }
    }

    internal string GetSerialized(string market, bool useSaleType = false, bool customSearch = false)
    {
        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        var xItem = new XiletradeItem(_dm, Form, customSearch);

        try
        {
            IJsonDataFactory factory = isPoe2 ? new JsonDataTwoFactory(_dm) : new JsonDataFactory(_dm);
            if (customSearch)
            {
                var search = Form.CustomSearch.Search.SearchQuery;
                var unid = Form.CustomSearch.UnidUniquesIndex > 0
                    ? Form.CustomSearch.UnidUniques[Form.CustomSearch.UnidUniquesIndex] : null;

                return factory.CreateAndSerialize(xItem, unid, market, search);
            }
            return factory.CreateAndSerialize(xItem, Item, useSaleType, market);
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "JSON serialization error", MessageStatus.Error);
        }
        return null;
    }
}

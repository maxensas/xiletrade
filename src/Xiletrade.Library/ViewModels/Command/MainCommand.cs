using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Xiletrade.Library.Models.CoE.Domain;
using Xiletrade.Library.Models.DB.Domain;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Wiki.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.ViewModels.Command;

public sealed partial class MainCommand : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    private readonly MainViewModel _vm;
    private readonly DataManagerService _dm;

    public MainCommand(MainViewModel vm, IServiceProvider serviceProvider)
    {
        _vm = vm;
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
    }

    [RelayCommand]
    private async Task OpenSearch(object commandParameter)
    {
        string market = _vm.Form.Market[_vm.Form.MarketIndex];
        string league = _vm.Form.League[_vm.Form.LeagueIndex];

        if (_vm.Form.Tab.BulkSelected)
        {
            await _vm.Form.ItemExchange.Bulk.OpenBulkSearchTask(market, league);
            return;
        }
        if (_vm.Form.Tab.ShopSelected)
        {
            await _vm.Form.ItemExchange.Shop.OpenShopSearchTask(market, league);
            return;
        }
        var priceCheck = _vm.Form.Tab.QuickSelected || _vm.Form.Tab.DetailSelected;
        if (priceCheck || _vm.Form.Tab.CustomSearchSelected)
        {
            var sEntity = _vm.GetSerialized(market, customSearch: !priceCheck);
            if (!string.IsNullOrEmpty(sEntity))
            {
                await OpenSearchTask(sEntity, league);
            }
        }
    }

    [RelayCommand]
    private static async Task TravelToHideout(object commandParameter)
    {
        if (commandParameter is null)
        {
            return;
        }

        if (commandParameter is SaleInfo saleInfo)
        {
            if (saleInfo.HideoutToken is null || saleInfo.HideoutToken.Length is 0)
            {
                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                ms.Show("Cannot travel to hideout : " + "\n\nYour POESESSID is missing or expired !" +
                    "\n\nFor advanced users : You can manually update your POESESSID " +
                    "under settings by using the developer manager in the authentication section.",
                    "This feature requires authentication", MessageStatus.Exclamation);
                return;
            }
            
            try
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                var sEntity = $"{{\"token\":\"{saleInfo.HideoutToken}\"}}";
                //var urlRef = Strings.TradeUrl + _vm.Form.League[_vm.Form.LeagueIndex] + "/" + _vm.Result.Data.ResultData.Id;
                var result = await service.SendHTTP(sEntity, Strings.WhisperApi, Client.Trade, isXml: true);
                if (result.Length > 0)
                {
                    //{"success":true}
                }
            }
            catch (Exception ex)
            {
                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                if (ex is HttpRequestException exception)
                {
                    if (exception.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        return; // can be normal
                    }
                    if (exception.StatusCode is System.Net.HttpStatusCode.BadRequest)
                    {
                        ms.Show("Cannot travel to hideout :" + "\n\nIs your account correctly connected ?" 
                        , "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                        return;
                    }
                    if (exception.StatusCode is System.Net.HttpStatusCode.Forbidden)
                    {
                        ms.Show("Cannot travel to hideout.", "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                        return;
                    }
                    ms.Show("Cannot travel to hideout : " + "\n\nYour POESESSID is probably missing or expired !" +
                        "\n\nYou can manually update it under settings by using the developer manager in the authentication section.",
                        "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
                    
                }
                ms.Show("Cannot travel to hideout :\n\n" + 
                    string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), 
                    "Unknown error encountered", MessageStatus.Error);
            }
        }
    }

    private async Task OpenSearchTask(string sEntity, string league)
    {
        try
        {
            var service = _serviceProvider.GetRequiredService<NetService>();
            var result = await service.SendHTTP(sEntity, Strings.TradeApi + league, Client.Trade);
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
                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                ms.Show("Cannot open search in browser : \n" + exception.Message, "ERROR Code : " + exception.StatusCode, MessageStatus.Error);
            }
        }
    }

    [RelayCommand]
    private void OpenNinja(object commandParameter) => _vm.OpenUrlTask(_vm.Ninja.FullUrl, UrlType.Ninja);

    [RelayCommand]
    private void OpenWiki(object commandParameter)
    {
        var poeWiki = new PoeWiki(_dm, _vm.Item);
        _vm.OpenUrlTask(poeWiki.Link, UrlType.PoeWiki);
    }

    [RelayCommand]
    private void OpenPoeDb(object commandParameter)
    {
        var poeDb = new PoeDb(_dm, _vm.Item);
        _vm.OpenUrlTask(poeDb.Link, UrlType.PoeDb);
    }

    [RelayCommand]
    private void OpenCraftOfExile(object commandParameter)
    {
        var coe = new CraftOfExile(_vm.ClipboardText);
        _vm.OpenUrlTask(coe.Link, UrlType.CraftOfExile);
    }

    [RelayCommand]
    private void OpenXiletradeChangelog(object commandParameter)
    {
        _vm.OpenUrlTask(Strings.UrlChangelog, UrlType.Xiletrade);
    }

    [RelayCommand]
    private static void OpenDonateUrl(object commandParameter)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = Strings.UrlPaypalDonate, UseShellExecute = true });
        }
        catch (Exception)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(Resources.Resources.Main126_PaypalFail, "Redirection to paypal failed ", MessageStatus.Warning);
        }
    }

    [RelayCommand]
    private void RefreshSearch(object commandParameter)
    {
        try
        {
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
            _vm.Result.InitData();
            if (_vm.Form.Tab.QuickSelected || _vm.Form.Tab.DetailSelected)
            {
                _vm.UpdateResultWithPoeApi(minimumStock: 1);
                return;
            }
            if (_vm.Form.Tab.BulkSelected)
            {
                if (_vm.Form.ItemExchange.Bulk.Pay.CurrencyIndex > 0 && _vm.Form.ItemExchange.Bulk.Get.CurrencyIndex > 0)
                {
                    if (!int.TryParse(_vm.Form.ItemExchange.Bulk.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        _vm.Form.ItemExchange.Bulk.Stock = "1";
                    }
                    _vm.Form.ItemExchange.Bulk.Get.ImageLast = _vm.Form.ItemExchange.Bulk.Get.Image;
                    _vm.Form.ItemExchange.Bulk.Pay.ImageLast = _vm.Form.ItemExchange.Bulk.Pay.Image;
                    _vm.Form.Visible.BulkLastSearch = true;

                    _vm.UpdateResultWithPoeApi(minimumStock);
                    if (!_vm.Form.IsPoeTwo)
                    {
                        _vm.Form.ItemExchange.Bulk.UpdateBulkNinjaTask();
                    }
                    return;
                }

                _vm.Result.Bulk.RightString = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                _vm.Result.Bulk.LeftString = string.Empty;
                return;
            }
            if (_vm.Form.Tab.ShopSelected)
            {
                if (_vm.Form.ItemExchange.Shop.GetList.Count > 0 && _vm.Form.ItemExchange.Shop.PayList.Count > 0)
                {
                    if (!int.TryParse(_vm.Form.ItemExchange.Shop.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        _vm.Form.ItemExchange.Shop.Stock = "1";
                    }
                    _vm.UpdateResultWithPoeApi(minimumStock);
                    return;
                }

                _vm.Result.Shop.RightString = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                _vm.Result.Shop.LeftString = string.Empty;
            }
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Refreshing search error", MessageStatus.Error);
        }
    }

    [RelayCommand]
    private async Task Fetch(object commandParameter)
    {
        _vm.Form.FetchDetailIsEnabled = false;
        _vm.Result.Detail.Total = "Fetching new results...";
        var market = _vm.Form.Market[_vm.Form.MarketIndex];
        var sameUser = _vm.Form.SameUser;
        var token = _vm.TaskManager.GetPriceToken();

        ResultBar result = null;
        try
        {
            result = await Task.Run(() => _vm.Result.FetchWithApi(20, market, sameUser, token), token); // maxFetch is set to 20 by default !
        }
        catch (InvalidOperationException ex)
        {
            result = new(emptyLine: true);
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Invalid operation", MessageStatus.Error);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is HttpRequestException exception)
            {
                result = new(exception, false);
            }
        }
        _vm.Result.RefreshResultBar(false, result);
    }

    [RelayCommand]
    private void SetModCurrent(object commandParameter) => _vm.Form.SetModCurrent(_vm.Item);

    [RelayCommand]
    private void SetModTier(object commandParameter) => _vm.Form.SetModTier(_vm.Item);

    [RelayCommand]
    public void CheckCondition(object commandParameter) 
        => _vm.Form.CheckComboCondition = new(_vm.Form.Condition);

    [RelayCommand]
    public void CheckInfluence(object commandParameter) 
        => _vm.Form.CheckComboInfluence = new(_vm.Form.Influence);
}

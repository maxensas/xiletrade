using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class BulkItemExchangeViewModel : ViewModelBase
{
    private readonly DataManagerService _dm;
    private readonly INavigationService _navigation;
    private readonly IMessageAdapterService _message;
    private readonly MainViewModel _vm;

    [ObservableProperty]
    private BulkViewModel bulk;

    [ObservableProperty]
    private ShopViewModel shop;

    public BulkItemExchangeViewModel(DataManagerService dm, MainViewModel vm, 
        INavigationService navigation, IMessageAdapterService message,
        bool useCustomOrBulk)
    {
        _dm = dm;
        _vm = vm;
        _navigation = navigation;
        _message = message;

        bulk = new(_dm, _vm, _navigation, _message); // mandatory (auto select currency item on price check)
        if (useCustomOrBulk)
        {
            shop = new(_dm, _vm, _navigation, _message);
        }
    }

    [RelayCommand]
    private void Change(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            ExchangeViewModel exVm = @string.StartWith("get") ? Bulk.Get :
                @string.StartWith("pay") ? Bulk.Pay :
                @string.StartWith("shop") ? Shop.Exchange : null;
            if (exVm is null)
            {
                return;
            }

            if (exVm.CategoryIndex > 0 && exVm.CurrencyIndex > 0)
            {
                string tier = null;
                if (exVm.TierIndex > 0)
                {
                    tier = exVm.Tier[exVm.TierIndex].ToLowerInvariant().Replace("t", string.Empty);
                }
                exVm.Image = Common.GetCurrencyImageUri(_dm, exVm.Currency[exVm.CurrencyIndex], tier);
            }
            if (exVm.CurrencyIndex is 0)
            {
                exVm.Image = null;
            }
        }
    }

    internal string GetCurrencyTag(ExchangeType exchange) // get: true, pay: false
    {
        var exVm = exchange is ExchangeType.Get ? Bulk.Get :
            exchange is ExchangeType.Pay ? Bulk.Pay :
            exchange is ExchangeType.Shop ? Shop.Exchange : null;
        if (exVm is null)
        {
            return string.Empty;
        }

        string category = exVm.Category.Count > 0 && exVm.CategoryIndex > -1 ?
                exVm.Category[exVm.CategoryIndex] : string.Empty;
        string currency = exVm.Currency.Count > 0 && exVm.CurrencyIndex > -1 ?
            exVm.Currency[exVm.CurrencyIndex] : string.Empty;
        string tier = exVm.Tier.Count > 0 && exVm.TierIndex > -1 ?
            exVm.Tier[exVm.TierIndex] : string.Empty;

        var mapKind = string.Empty;
        if (category is Strings.Maps)
        {
            mapKind = tier.Replace("T", string.Empty);
            mapKind = mapKind is Strings.Blight or Strings.Ravaged ?
                Strings.CurrencyTypePoe1.MapsBlighted : Strings.CurrencyTypePoe1.Maps;
        }
        var res = _dm.Currencies.FindEntryByTypeAndPossibleMapKind(currency, mapKind);
        if (res is not null)
        {
            return res.Id;
        }
        return null;
    }

    internal async Task SelectCurrency(string args, string currency, string tier = null)
    {
        var arg = args.Split('/');
        //bool search = false;
        if (arg[0] is not "pay" and not "get" and not "shop")
        {
            return;
        }

        CurrencyEntrie entry;
        string curClass;
        if (arg.Length > 1 && arg[1] is "contains")
        {
            var curKeys = currency.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            (entry, curClass) = _dm.Currencies.FindEntryAndGroupIdByTypeOrId(curKeys);
        }
        else
        {
            (entry, curClass) = _dm.Currencies.FindEntryAndGroupIdByType(currency, image: false);
        }

        if (entry is null || curClass.Length is 0)
        {
            return;
        }

        string selectedTier = string.Empty;
        string selectedCategory = Strings.GetBulkCategory(curClass, entry.Id);

        if (selectedCategory.Length is 0)
        {
            return;
        }

        if (selectedCategory == Resources.Resources.Main055_Divination)
        {
            var tmpDiv = _dm.DivTiers.FindDivTierByTag(entry.Id);
            selectedTier = tmpDiv != null ? "T" + tmpDiv.Tier : Resources.Resources.Main016_TierNothing;
        }
        if (selectedCategory == Resources.Resources.Main056_Maps
            || selectedCategory == Resources.Resources.Main179_UniqueMaps
            || selectedCategory == Resources.Resources.Main217_BlightedMaps)
        {
            if (tier?.Length > 0)
            {
                selectedTier = "T" + tier;
            }
            else
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(entry.Text);
                if (match.Count is 1)
                {
                    selectedTier = "T" + match[0].Value.ToString();
                }
            }
        }
        var isTier = selectedTier.Length > 0;

        var bulk = arg[0] is "pay" ? Bulk.Pay
            : arg[0] is "get" ? Bulk.Get
            : arg[0] is "shop" ? Shop.Exchange
            : null;

        int idxCat = bulk.Category.IndexOf(selectedCategory);
        if (idxCat > -1)
        {
            bulk.CategoryIndex = idxCat;
        }

        if (isTier)
        {
            int idxTier = bulk.Tier.IndexOf(selectedTier);
            if (idxTier > -1 && selectedTier.Length > 0)
            {
                bulk.TierIndex = idxTier;
            }
        }

        // TO FIX, tier selection for maps/divcard not working properly
        int watchdog = 0;
        // 2 seconds max
        while (bulk.Currency.Count is 0 && watchdog < 10)
        {
            bulk.CategoryIndex = -1;
            await Task.Delay(100);
            bulk.CategoryIndex = idxCat;
            await Task.Delay(100);
            watchdog++;
        }

        int idxCur = bulk.Currency.IndexOf(entry.Text);
        if (idxCur > -1)
        {
            bulk.CurrencyIndex = idxCur;
        }
        /*
        if (!search)
        {
            IsSelectionEnabled = false;
            Command.MainCommand.SelectBulk("pay");
        }
        int idxCur = bulk.Currency.IndexOf(selectedCurrency);
        if (idxCur > -1)
        {
            bulk.CurrencyIndex = idxCur;
        }
        if (!search)
        {
            Command.MainCommand.Change("pay");
            IsSelectionEnabled = true;
        }
        */
    }
}

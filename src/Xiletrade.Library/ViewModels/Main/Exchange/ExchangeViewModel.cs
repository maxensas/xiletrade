using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ExchangeViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private AsyncObservableCollection<string> category;

    [ObservableProperty]
    private AsyncObservableCollection<string> currency = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> tier = new();

    [ObservableProperty]
    private int categoryIndex = 0;

    [ObservableProperty]
    private int currencyIndex = 0;

    [ObservableProperty]
    private int tierIndex;

    [ObservableProperty]
    private Uri image = null;

    [ObservableProperty]
    private Uri imageLast;

    [ObservableProperty]
    private string imageLastToolTip;

    [ObservableProperty]
    private string imageLastTag;

    [ObservableProperty]
    private bool tierVisible;

    [ObservableProperty]
    private bool currencyVisible;

    [ObservableProperty]
    private string search = string.Empty;

    public ExchangeViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var dm = serviceProvider.GetRequiredService<DataManagerService>();
        var isPoe2 = dm.Config.Options.GameVersion is 1;
        if (isPoe2)
        {
            category = new()
            {
                Resources.Resources.Main043_Choose, Resources.Resources.Main044_MainCur, Resources.Resources.Main045_OtherCur, 
                Resources.Resources.Main046_MapFrag, Resources.Resources.General132_Rune, Resources.Resources.Main054_Essences, 
                Resources.Resources.Main235_AbyssalBones, Resources.Resources.General069_Ultimatum, Resources.Resources.Main049_Catalysts, 
                Resources.Resources.Main186_Expedition, Resources.Resources.ItemClass_omen, Resources.Resources.Main236_Delirium,
                Resources.Resources.ItemClass_talismans, Resources.Resources.ItemClass_sanctumRelic, Resources.Resources.ItemClass_vaultKeys, 
                Resources.Resources.Main237_UncutGems, Resources.Resources.Main238_LineageGems, Resources.Resources.ItemClass_maps
            };
            return;
        }
        category = new()
        { 
            Resources.Resources.Main043_Choose, Resources.Resources.Main044_MainCur, Resources.Resources.Main207_ExoticCurrency, 
            Resources.Resources.Main045_OtherCur, Resources.Resources.Main046_MapFrag, Resources.Resources.Main047_Stones,
            Resources.Resources.General132_Rune, Resources.Resources.ItemClass_allflame,
            Resources.Resources.Main198_ScoutingReports, Resources.Resources.Main186_Expedition, 
            Resources.Resources.Main048_Delirium, Resources.Resources.Main049_Catalysts, Resources.Resources.Main050_Oils, 
            Resources.Resources.Main051_Incubators, Resources.Resources.Main052_Scarabs, Resources.Resources.Main053_Fossils,
            Resources.Resources.Main054_Essences, Resources.Resources.Main211_AncestorCurrency, Resources.Resources.Main212_Sanctum,
            Resources.Resources.Main055_Divination, Resources.Resources.Main056_Maps, Resources.Resources.Main179_UniqueMaps, 
            Resources.Resources.Main216_BossMaps, Resources.Resources.Main217_BlightedMaps, Resources.Resources.Main219_Beasts, 
            Resources.Resources.Main218_Heist
        };
    }

    partial void OnCurrencyIndexChanged(int value)
    {
        if (value < 0)
        {
            return;
        }
        if (CategoryIndex > 0 && CurrencyIndex > 0)
        {
            string tier = null;
            if (TierIndex > 0)
            {
                tier = Tier[TierIndex].ToLowerInvariant().Replace("t", string.Empty);
            }
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            Image = Common.GetCurrencyImageUri(dm, Currency[CurrencyIndex], tier);
        }
        if (CurrencyIndex is 0)
        {
            Image = null;
        }
    }

    partial void OnCategoryIndexChanged(int value)
    {
        if (value < 0)
        {
            return;
        }

        var dm = _serviceProvider.GetRequiredService<DataManagerService>();

        Image = null;

        if (CategoryIndex > 0)
        {
            if (dm.Config.Options.GameVersion is 0)
            {
                bool isMap = Category[CategoryIndex] == Resources.Resources.Main056_Maps
                        || Category[CategoryIndex] == Resources.Resources.Main179_UniqueMaps
                        || Category[CategoryIndex] == Resources.Resources.Main217_BlightedMaps;
                bool isDiv = Category[CategoryIndex] == Resources.Resources.Main055_Divination;

                TierVisible = isDiv || isMap;
                if (isDiv || isMap)
                {
                    Tier = isDiv ? new(Strings.BulkStrings.DivinationCardTier) : new(Strings.BulkStrings.MapTierPoe1);
                    TierIndex = 0;
                }
            }

            AsyncObservableCollection<string> listBulk = new();
            listBulk.Add(Strings.BulkStrings.Delimiter);
            CurrencyIndex = 0;

            string selValue = Category[CategoryIndex];
            string searchKind = GetSearchKind(selValue);

            if (searchKind.Length > 0)
            {
                var exchangeTier = string.Empty;
                if (TierVisible && Tier.Count > 0 && TierIndex >= 0)
                {
                    exchangeTier = Tier[TierIndex];
                }
                var isDelve = searchKind is Strings.Delve;
                var list = dm.Currencies.GetCurrenciesList(dm.DivTiers,
                    searchKind, selValue, exchangeTier, isDelve);
                foreach (var str in list)
                {
                    listBulk.Add(str);
                }
            }
            Currency = listBulk;
            CurrencyVisible = true;
        }
        else
        {
            TierVisible = false;
            CurrencyVisible = false;
        }
    }

    private static string GetSearchKind(string selValue)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        if (dm.Config.Options.GameVersion is 1)
        {
            return (selValue == Resources.Resources.Main044_MainCur
            || selValue == Resources.Resources.Main045_OtherCur) ? Strings.CurrencyTypePoe2.Currency :
            selValue == Resources.Resources.Main046_MapFrag ? Strings.CurrencyTypePoe2.Fragments :
            selValue == Resources.Resources.General132_Rune ? Strings.CurrencyTypePoe2.Runes :
            selValue == Resources.Resources.Main054_Essences ? Strings.CurrencyTypePoe2.Essences :
            selValue == Resources.Resources.ItemClass_sanctumRelic ? Strings.CurrencyTypePoe2.Relics :
            selValue == Resources.Resources.General069_Ultimatum ? Strings.CurrencyTypePoe2.Ultimatum :
            selValue == Resources.Resources.Main049_Catalysts ? Strings.CurrencyTypePoe2.Breach :
            selValue == Resources.Resources.Main186_Expedition ? Strings.CurrencyTypePoe2.Expedition :
            selValue == Resources.Resources.ItemClass_omen ? Strings.CurrencyTypePoe2.Ritual :
            selValue == Resources.Resources.Main236_Delirium ? Strings.CurrencyTypePoe2.Delirium :
            selValue == Resources.Resources.ItemClass_maps ? Strings.CurrencyTypePoe2.Waystones :
            selValue == Resources.Resources.ItemClass_talismans ? Strings.CurrencyTypePoe2.Talismans :
            selValue == Resources.Resources.ItemClass_vaultKeys ? Strings.CurrencyTypePoe2.VaultKeys :
            selValue == Resources.Resources.Main235_AbyssalBones ? Strings.CurrencyTypePoe2.Abyss :
            selValue == Resources.Resources.Main237_UncutGems ? Strings.CurrencyTypePoe2.UncutGems :
            selValue == Resources.Resources.Main238_LineageGems ? Strings.CurrencyTypePoe2.LineageSupportGems :
            string.Empty;
        }

        return (selValue == Resources.Resources.Main044_MainCur
            || selValue == Resources.Resources.Main207_ExoticCurrency
            || selValue == Resources.Resources.Main045_OtherCur) ? Strings.CurrencyTypePoe1.Currency :
            (selValue == Resources.Resources.Main046_MapFrag
            || selValue == Resources.Resources.Main047_Stones
            || selValue == Resources.Resources.Main052_Scarabs) ? Strings.CurrencyTypePoe1.Fragments :
            selValue == Resources.Resources.Main186_Expedition ? Strings.CurrencyTypePoe1.Expedition :
            selValue == Resources.Resources.Main048_Delirium ? Strings.CurrencyTypePoe1.DeliriumOrbs :
            selValue == Resources.Resources.Main049_Catalysts ? Strings.CurrencyTypePoe1.Catalysts :
            selValue == Resources.Resources.Main050_Oils ? Strings.CurrencyTypePoe1.Oils :
            selValue == Resources.Resources.Main051_Incubators ? Strings.CurrencyTypePoe1.Incubators :
            selValue == Resources.Resources.Main053_Fossils ? Strings.Delve :
            selValue == Resources.Resources.Main054_Essences ? Strings.CurrencyTypePoe1.Essences :
            selValue == Resources.Resources.Main211_AncestorCurrency ? Strings.CurrencyTypePoe1.Ancestor :
            selValue == Resources.Resources.Main212_Sanctum ? Strings.CurrencyTypePoe1.Sanctum :
            selValue == Resources.Resources.Main198_ScoutingReports ? Strings.CurrencyTypePoe1.ScoutingReport :
            selValue == Resources.Resources.Main055_Divination ? Strings.CurrencyTypePoe1.Cards :
            selValue == Resources.Resources.Main200_SentinelCurrency ? Strings.CurrencyTypePoe1.Sentinel :
            selValue == Resources.Resources.Main056_Maps ? Strings.CurrencyTypePoe1.Maps :
            selValue == Resources.Resources.Main179_UniqueMaps ? Strings.CurrencyTypePoe1.MapsUnique :
            selValue == Resources.Resources.Main216_BossMaps ? Strings.CurrencyTypePoe1.MapsSpecial :
            selValue == Resources.Resources.Main217_BlightedMaps ? Strings.CurrencyTypePoe1.MapsBlighted :
            selValue == Resources.Resources.Main218_Heist ? Strings.CurrencyTypePoe1.Heist :
            selValue == Resources.Resources.Main219_Beasts ? Strings.CurrencyTypePoe1.Beasts :
            selValue == Resources.Resources.General132_Rune ? Strings.CurrencyTypePoe1.Runegrafts :
            selValue == Resources.Resources.ItemClass_allflame ? Strings.CurrencyTypePoe1.AllflameEmbers :
            string.Empty;
    }
}

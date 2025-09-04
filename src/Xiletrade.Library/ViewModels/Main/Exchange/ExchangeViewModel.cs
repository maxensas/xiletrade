using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ExchangeViewModel : ViewModelBase
{
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
                Resources.Resources.Main229_Talismans, Resources.Resources.ItemClass_sanctumRelic, Resources.Resources.Main230_VaultKeys, 
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
}

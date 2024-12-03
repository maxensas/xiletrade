using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels.Main.Exchange;

public sealed partial class ExchangeViewModel : ViewModelBase
{
    [ObservableProperty]
    private AsyncObservableCollection<string> category = new(){ Resources.Resources.Main043_Choose, Resources.Resources.Main044_MainCur, Resources.Resources.Main207_ExoticCurrency, Resources.Resources.Main045_OtherCur, //Resources.Resources.Main149_Shards,
        Resources.Resources.Main046_MapFrag, Resources.Resources.Main047_Stones, Resources.Resources.Main198_ScoutingReports, Resources.Resources.Main208_MemoryLine, Resources.Resources.Main186_Expedition, Resources.Resources.Main048_Delirium, Resources.Resources.Main049_Catalysts,
        Resources.Resources.Main050_Oils, Resources.Resources.Main051_Incubators, Resources.Resources.Main052_Scarabs, Resources.Resources.Main053_Fossils,
        Resources.Resources.Main054_Essences, Resources.Resources.Main211_AncestorCurrency, Resources.Resources.Main212_Sanctum, //Resources.Resources.Main213_Crucible,
        Resources.Resources.Main055_Divination, Resources.Resources.Main056_Maps, Resources.Resources.Main179_UniqueMaps, Resources.Resources.Main216_BossMaps, Resources.Resources.Main217_BlightedMaps,
        Resources.Resources.Main219_Beasts, Resources.Resources.Main218_Heist, Resources.Resources.General132_Rune
        };

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
}

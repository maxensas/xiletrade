using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Models.Application.Configuration.Domain;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class GeneralViewModel : ViewModelBase
{
    private readonly DataManagerService _dm;
    
    [ObservableProperty]
    private int leagueIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> league = new();

    [ObservableProperty]
    private int languageIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> gateway = new();

    [ObservableProperty]
    private int gatewayIndex;

    [ObservableProperty]
    private AsyncObservableCollection<Language> language = new();

    [ObservableProperty]
    private int gameIndex;

    [ObservableProperty]
    private int searchDayLimit;

    [ObservableProperty]
    private int maxFetch;

    [ObservableProperty]
    private int timeoutRequest;

    [ObservableProperty]
    private double viewScale;

    [ObservableProperty]
    private double opacityLevel;

    [ObservableProperty]
    private bool autoCloseMain;

    [ObservableProperty]
    private bool devMode;

    [ObservableProperty]
    private bool startupMessage;

    [ObservableProperty]
    private bool autoUpdate;

    [ObservableProperty]
    private bool autoFilter;

    [ObservableProperty]
    private bool autoWhisper;

    [ObservableProperty]
    private bool checkMinTier;

    [ObservableProperty]
    private bool checkMinPercentage;

    [ObservableProperty]
    private bool checkModLevel;

    [ObservableProperty]
    private int modLevel;

    [ObservableProperty]
    private bool regroupResults;

    [ObservableProperty]
    private bool byBaseType;

    [ObservableProperty]
    private bool checkPseudoAffix;

    [ObservableProperty]
    private bool checkCorrupted;

    [ObservableProperty]
    private bool checkGlobalEs;

    [ObservableProperty]
    private bool checkTotalDps;

    [ObservableProperty]
    private bool checkTotalArmourStats;

    [ObservableProperty]
    private bool checkTotalLife;

    [ObservableProperty]
    private bool checkTotalResists;

    [ObservableProperty]
    private bool checkTotalAttributes;

    [ObservableProperty]
    private bool checkExplicitsUniques;

    [ObservableProperty]
    private bool checkExplicitsNonUniques;

    [ObservableProperty]
    private bool checkCrafted;

    [ObservableProperty]
    private bool checkImplicits;

    [ObservableProperty]
    private bool checkCorruptions;

    [ObservableProperty]
    private bool checkEnchants;

    [ObservableProperty]
    private bool btnUpdateEnable;

    [ObservableProperty]
    private bool ctrlWheel;

    [ObservableProperty]
    private bool asyncMarketDefault;

    [ObservableProperty]
    private bool fastInputs;

    private GeneralViewModel()
    {
        language = new()
        {
            new(Lang.English, "English"),
            new(Lang.Korean, "한국어"),
            new(Lang.French, "Français"),
            new(Lang.Spanish, "Castellano"),
            new(Lang.German, "Deutsch"),
            new(Lang.Portuguese, "Português"),
            new(Lang.Russian, "Русский"),
            new(Lang.Thai, "ภาษาไทย"),
            new(Lang.Taiwanese, "正體中文"),
            new(Lang.Chinese, "简体中文"),
            new(Lang.Japanese, "日本語")
        };

        gateway = new()
        {
            "EN", "KR", "FR", "ES", "DE",
            "BR", "RU", "TH", "TW", "CN",
            "JP"
        };
    }

    internal GeneralViewModel(DataManagerService dm, ConfigOption options, bool initIndexCollections = true) : this()
    {
        _dm = dm;

        league = GetLeague();
        leagueIndex = GetLeagueIndex(league);

        viewScale = options.Scale;
        opacityLevel = options.Opacity;
        autoCloseMain = options.Autoclose;

        if (initIndexCollections)
        {
            languageIndex = options.Language;
            gatewayIndex = options.Gateway;
            gameIndex = options.GameVersion;
        }

        searchDayLimit = options.SearchBeforeDay;
        maxFetch = options.SearchFetchDetail;
        timeoutRequest = options.TimeoutTradeApi;

        btnUpdateEnable = true;

        startupMessage = options.DisableStartupMessage;
        regroupResults = options.HideSameOccurs;
        checkCorrupted = options.AutoSelectCorrupt;
        checkPseudoAffix = options.AutoSelectPseudo;
        byBaseType = options.SearchByType;
        autoUpdate = options.CheckUpdates;
        autoFilter = options.CheckFilters;
        checkTotalLife = options.AutoSelectLife;
        checkGlobalEs = options.AutoSelectGlobalEs;
        checkTotalResists = options.AutoSelectRes;
        checkTotalAttributes = options.AutoSelectAttr;
        checkTotalArmourStats = options.AutoSelectArEsEva;
        checkTotalDps = options.AutoSelectDps;
        checkMinTier = options.AutoSelectMinTierValue;
        checkMinPercentage = options.AutoSelectMinPercentValue;
        checkModLevel = options.AutoUnSelectBelowModLevel;
        modLevel = Math.Clamp(options.ModLevel, 1, 100);
        checkExplicitsUniques = options.AutoCheckUniques;
        checkExplicitsNonUniques = options.AutoCheckNonUniques;
        checkImplicits = options.AutoCheckImplicits;
        checkEnchants = options.AutoCheckEnchants;
        checkCrafted = options.AutoCheckCrafted;
        checkCorruptions = options.AutoCheckCorruptions;
        devMode = options.DevMode;
        autoWhisper = options.Autopaste;
        ctrlWheel = options.CtrlWheel;
        asyncMarketDefault = options.AsyncMarketDefault;
        fastInputs = options.FastInputs;
    }

    internal void InitLeagueList()
    {
        League = GetLeague();
        LeagueIndex = GetLeagueIndex(League);
    }

    //private
    private AsyncObservableCollection<string> GetLeague()
    {
        var league = new AsyncObservableCollection<string>();
        if (_dm.League.Result.Length < 2)
        {
            return League is null ? league : League;
        }
        foreach (LeagueResult res in _dm.League.Result)
        {
            league.Add(res.Id);
        }
        return league;
    }

    private int GetLeagueIndex(AsyncObservableCollection<string> league)
    {
        int leagueIdx = league.IndexOf(_dm.Config.Options.League);
        return leagueIdx is -1 ? 0 : leagueIdx;
    }
}

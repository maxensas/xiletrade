using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed class GeneralViewModel : BaseViewModel
{
    private int leagueIndex;
    private AsyncObservableCollection<string> league = new();
    private int languageIndex;
    private AsyncObservableCollection<string> language = new();
    private int searchDayLimitIndex;
    private AsyncObservableCollection<string> searchDayLimit = new();
    private int maxFetchIndex;
    private AsyncObservableCollection<string> maxFetch = new();
    private int maxWaitRequestIndex;
    private AsyncObservableCollection<string> maxWaitRequest = new();
    private bool devMode;
    private bool startupMessage;
    private bool autoUpdate;
    private bool autoFilter;
    private bool autoWhisper;
    private bool checkMinTier;
    private bool regroupResults;
    private bool byBaseType;
    private bool checkPseudoAffix;
    private bool checkCorrupted;
    private bool checkGlobalEs;
    private bool checkTotalDps;
    private bool checkTotalArmourStats;
    private bool checkTotalLife;
    private bool checkTotalResists;
    private bool checkExplicitsUniques;
    private bool checkExplicitsNonUniques;
    private bool checkCrafted;
    private bool checkImplicits;
    private bool checkCorruptions;
    private bool checkEnchants;

    private string btnUpdateText = string.Empty;
    private bool btnUpdateEnable;

    public int LeagueIndex { get => leagueIndex; set => SetProperty(ref leagueIndex, value); }
    public AsyncObservableCollection<string> League { get => league; set => SetProperty(ref league, value); }
    public int LanguageIndex { get => languageIndex; set => SetProperty(ref languageIndex, value); }
    public AsyncObservableCollection<string> Language { get => language; set => SetProperty(ref language, value); }
    public int SearchDayLimitIndex { get => searchDayLimitIndex; set => SetProperty(ref searchDayLimitIndex, value); }
    public AsyncObservableCollection<string> SearchDayLimit { get => searchDayLimit; set => SetProperty(ref searchDayLimit, value); }
    public int MaxFetchIndex { get => maxFetchIndex; set => SetProperty(ref maxFetchIndex, value); }
    public AsyncObservableCollection<string> MaxFetch { get => maxFetch; set => SetProperty(ref maxFetch, value); }
    public int MaxWaitRequestIndex { get => maxWaitRequestIndex; set => SetProperty(ref maxWaitRequestIndex, value); }
    public AsyncObservableCollection<string> MaxWaitRequest { get => maxWaitRequest; set => SetProperty(ref maxWaitRequest, value); }
    public bool DevMode { get => devMode; set => SetProperty(ref devMode, value); }
    public bool StartupMessage { get => startupMessage; set => SetProperty(ref startupMessage, value); }
    public bool AutoUpdate { get => autoUpdate; set => SetProperty(ref autoUpdate, value); }
    public bool AutoFilter { get => autoFilter; set => SetProperty(ref autoFilter, value); }
    public bool AutoWhisper { get => autoWhisper; set => SetProperty(ref autoWhisper, value); }
    public bool CheckMinTier { get => checkMinTier; set => SetProperty(ref checkMinTier, value); }
    public bool RegroupResults { get => regroupResults; set => SetProperty(ref regroupResults, value); }
    public bool ByBaseType { get => byBaseType; set => SetProperty(ref byBaseType, value); }
    public bool CheckPseudoAffix { get => checkPseudoAffix; set => SetProperty(ref checkPseudoAffix, value); }
    public bool CheckCorrupted { get => checkCorrupted; set => SetProperty(ref checkCorrupted, value); }
    public bool CheckGlobalEs { get => checkGlobalEs; set => SetProperty(ref checkGlobalEs, value); }
    public bool CheckTotalDps { get => checkTotalDps; set => SetProperty(ref checkTotalDps, value); }
    public bool CheckTotalArmourStats { get => checkTotalArmourStats; set => SetProperty(ref checkTotalArmourStats, value); }
    public bool CheckTotalLife { get => checkTotalLife; set => SetProperty(ref checkTotalLife, value); }
    public bool CheckTotalResists { get => checkTotalResists; set => SetProperty(ref checkTotalResists, value); }
    public bool CheckExplicitsUniques { get => checkExplicitsUniques; set => SetProperty(ref checkExplicitsUniques, value); }
    public bool CheckExplicitsNonUniques { get => checkExplicitsNonUniques; set => SetProperty(ref checkExplicitsNonUniques, value); }
    public bool CheckCrafted { get => checkCrafted; set => SetProperty(ref checkCrafted, value); }
    public bool CheckImplicits { get => checkImplicits; set => SetProperty(ref checkImplicits, value); }
    public bool CheckCorruptions { get => checkCorruptions; set => SetProperty(ref checkCorruptions, value); }
    public bool CheckEnchants { get => checkEnchants; set => SetProperty(ref checkEnchants, value); }

    public string BtnUpdateText { get => btnUpdateText; set => SetProperty(ref btnUpdateText, value); }
    public bool BtnUpdateEnable { get => btnUpdateEnable; set => SetProperty(ref btnUpdateEnable, value); }

    public GeneralViewModel()
    {
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed partial class GeneralViewModel : ViewModelBase
{
    [ObservableProperty]
    private int leagueIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> league = new();

    [ObservableProperty]
    private int languageIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> language = new();

    [ObservableProperty]
    private int searchDayLimitIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> searchDayLimit = new();

    [ObservableProperty]
    private int maxFetchIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> maxFetch = new();

    [ObservableProperty]
    private int maxWaitRequestIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> maxWaitRequest = new();

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
    private string btnUpdateText = string.Empty;

    [ObservableProperty]
    private bool btnUpdateEnable;

    [ObservableProperty]
    private bool ctrlWheel;
}

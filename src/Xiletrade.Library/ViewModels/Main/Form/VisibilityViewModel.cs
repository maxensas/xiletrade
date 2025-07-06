using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class VisibilityViewModel(bool iSpoe1English, bool useBulk) : ViewModelBase
{
    [ObservableProperty]
    private bool corrupted;

    [ObservableProperty]
    private bool btnPoeDb = !useBulk;

    [ObservableProperty]
    private bool btnDust;

    [ObservableProperty]
    private bool influences;

    [ObservableProperty]
    private bool conditions;

    [ObservableProperty]
    private bool panelForm;

    [ObservableProperty]
    private bool panelStat;

    [ObservableProperty]
    private bool totalLife;

    [ObservableProperty]
    private bool totalRes;

    [ObservableProperty]
    private bool totalEs;

    [ObservableProperty]
    private bool damage;

    [ObservableProperty]
    private bool defense;

    [ObservableProperty]
    private bool alternateGem;

    [ObservableProperty]
    private bool quality;

    [ObservableProperty]
    private bool sockets;

    [ObservableProperty]
    private bool runeSockets;

    [ObservableProperty]
    private bool byBase;

    [ObservableProperty]
    private bool rarity;

    [ObservableProperty]
    private bool checkAll;

    [ObservableProperty]
    private bool facetor;

    [ObservableProperty]
    private bool modSet;

    [ObservableProperty]
    private bool modCurrent;

    [ObservableProperty]
    private bool modPercent;

    [ObservableProperty]
    private bool detail;

    [ObservableProperty]
    private bool headerMod;

    [ObservableProperty]
    private bool reward;

    [ObservableProperty]
    private bool synthesisBlight;

    [ObservableProperty]
    private bool blightRavaged;

    [ObservableProperty]
    private bool hiddablePanel;

    [ObservableProperty]
    private bool armour;

    [ObservableProperty]
    private bool energy;

    [ObservableProperty]
    private bool evasion;

    [ObservableProperty]
    private bool ward;

    [ObservableProperty]
    private bool poeprices = iSpoe1English;

    [ObservableProperty]
    private bool wiki = !useBulk;

    [ObservableProperty]
    private bool ninja;

    [ObservableProperty]
    private bool bulkLastSearch;

    [ObservableProperty]
    private bool sanctumFields;

    [ObservableProperty]
    private bool mapStats;
}

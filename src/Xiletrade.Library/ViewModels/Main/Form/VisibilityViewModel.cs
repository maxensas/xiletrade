using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class VisibilityViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool corrupted = true;

    [ObservableProperty]
    private bool btnPoeDb = true;

    [ObservableProperty]
    private bool influences;

    [ObservableProperty]
    private bool conditions;

    [ObservableProperty]
    private bool panelForm = true;

    [ObservableProperty]
    private bool panelStat = true;

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
    private bool quality = true;

    [ObservableProperty]
    private bool sockets;

    [ObservableProperty]
    private bool runeSockets;

    [ObservableProperty]
    private bool byBase = true;

    [ObservableProperty]
    private bool rarity = true;

    [ObservableProperty]
    private bool checkAll = true;

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
    private bool headerMod = true;

    [ObservableProperty]
    private bool reward = false;

    [ObservableProperty]
    private bool synthesisBlight;

    [ObservableProperty]
    private bool blightRavaged;

    [ObservableProperty]
    private bool scourged;

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
    private bool poeprices = true;

    [ObservableProperty]
    private bool wiki = true;

    [ObservableProperty]
    private bool ninja;

    [ObservableProperty]
    private bool bulkLastSearch;

    [ObservableProperty]
    private bool sanctumFields;

    [ObservableProperty]
    private bool mapStats;
}

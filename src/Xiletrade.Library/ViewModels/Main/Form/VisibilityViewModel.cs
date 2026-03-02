using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class VisibilityViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private bool corrupted;

    [ObservableProperty]
    private bool btnPoeDb;

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
    private bool totalAttr;

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
    private bool poeprices;

    [ObservableProperty]
    private bool wiki;

    [ObservableProperty]
    private bool ninja;

    [ObservableProperty]
    private bool bulkLastSearch;

    [ObservableProperty]
    private bool sanctumFields;

    [ObservableProperty]
    private bool mapStats;

    public VisibilityViewModel(IServiceProvider serviceProvider, bool useBulk)
    {
        _serviceProvider = serviceProvider;
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var iSpoe1English = dm.Config.Options.Language is 0 && dm.Config.Options.GameVersion is 0;

        wiki = !useBulk;
        btnPoeDb = !useBulk;
        poeprices = iSpoe1English;
    }
}

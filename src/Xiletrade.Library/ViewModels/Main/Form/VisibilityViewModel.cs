using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class VisibilityViewModel : ViewModelBase
{
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

    internal VisibilityViewModel() // Custom search
    {
        rarity = true;
        corrupted = true;
    }

    internal VisibilityViewModel(DataManagerService dm, ItemData item, bool useDust)
    {
        var flag = item.Flag;
        var iSpoe1English = dm.Config.Options.Language is 0 && dm.Config.Options.GameVersion is 0;

        wiki = true;
        btnPoeDb = true;
        poeprices = iSpoe1English;
        btnDust = useDust;
        if (flag.SanctumResearch)
        {
            bool isTome = dm.Bases.FindBaseByNameEn(Strings.Unique.ForbiddenTome)?.Name == item.Type;
            if (!isTome)
            {
                sanctumFields = true;
            }
        }

        var visibilityCond = flag.Unidentified || flag.MapFragment
            || flag.Invitation || flag.CapturedBeast || flag.Chronicle || flag.Map
            || flag.Gems || flag.Currency || flag.Divcard || flag.Incubator;
        if (flag.Unique || visibilityCond)
        {
            btnPoeDb = false;
        }
        totalRes = item.Stats.Resistance && !flag.Map && !flag.Flask;
        totalLife = item.Stats.Life;
        totalEs = item.Stats.EnergyShield && !flag.ArmourPiece;
        totalAttr = item.Stats.Attribute;

        runeSockets = item.IsPoe2 && flag.ItemSocketable;
        sockets = !item.IsPoe2 && flag.ItemSocketable;
        influences = !item.IsPoe2 && (flag.ItemSocketable || flag.Jewellery);

        conditions = !item.IsPoe2 && !flag.Currency && !item.State.ExchangeCurrency
            && !flag.CapturedBeast && !flag.Map && !flag.MiscMapItems && !flag.Gems;
        facetor = flag.Facetor;
        modSet = !item.State.ExchangeCurrency && !flag.Gems && !flag.Chronicle
            && !flag.CapturedBeast && !flag.Ultimatum && !flag.MapValdo;
        var areaItem = flag.Chronicle || flag.Ultimatum || flag.MirroredTablet
            || flag.SanctumResearch || flag.TrialCoins || flag.Logbook;
        byBase = !item.State.ExchangeCurrency && !item.State.ConquerorMap
            && !flag.Waystones && !flag.Gems && !areaItem;

        rarity = !item.State.ExchangeCurrency && !flag.Gems && !areaItem;
        checkAll = !item.State.ExchangeCurrency || flag.Imbued;
        quality = !item.State.ExchangeCurrency && !flag.Waystones && !areaItem;
        corrupted = !item.State.ExchangeCurrency && !areaItem;
        panelStat = !item.State.ExchangeCurrency;
        panelForm = !item.State.ExchangeCurrency
            || flag.UncutGem || flag.Wombgift || flag.UltimatumPoe2 || flag.TrialCoins;

        if (flag.MapBlight || flag.MapBlightRavaged)
        {
            synthesisBlight = true;
            blightRavaged = true;
            hiddablePanel = true;
        }
        mapStats = flag.Map || flag.Waystones;
        reward = !item.IsPoe2 && (flag.Ultimatum || flag.MapValdo);
        detail = flag.ShowDetail;
        headerMod = !flag.ShowDetail;
        damage = flag.Weapon && !flag.Unidentified;
        defense = flag.ArmourPiece && !flag.Unidentified;
        if (flag.ArmourPiece && !flag.Unidentified)
        {
            if (item.Options.Ward.Length > 0)
            {
                ward = true;
            }
            else
            {
                armour = true;
                energy = true;
                evasion = true;
            }
        }
    }
}

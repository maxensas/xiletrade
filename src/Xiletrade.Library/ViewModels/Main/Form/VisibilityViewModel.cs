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
        if (flag.Area.SanctumResearch)
        {
            bool isTome = dm.Bases.FindBaseByNameEn(Strings.Unique.ForbiddenTome)?.Name == item.Type;
            if (!isTome)
            {
                sanctumFields = true;
            }
        }

        var visibilityCond = flag.Tag.Unidentified || flag.Map.Fragment
            || flag.Invitation || flag.Tag.CapturedBeast || flag.Area.Chronicle || flag.Map.IsMap
            || flag.Gem.IsGem || flag.Currency || flag.Divcard || flag.Incubator;
        if (flag.Rarity.Unique || visibilityCond)
        {
            btnPoeDb = false;
        }
        totalRes = item.Stats.Resistance && !flag.Map.IsMap && !flag.Slot.Flask;
        totalLife = item.Stats.Life;
        totalEs = item.Stats.EnergyShield && !flag.Armour.IsArmour;
        totalAttr = item.Stats.Attribute;

        runeSockets = item.IsPoe2 && flag.Socket.CanSocket;
        sockets = !item.IsPoe2 && flag.Socket.CanSocket;
        influences = !item.IsPoe2 && (flag.Socket.CanSocket || flag.Jewellery.IsJewellery);

        conditions = !item.IsPoe2 && !flag.Currency && !item.State.ExchangeCurrency
            && !flag.Tag.CapturedBeast && !flag.Map.IsMap && !flag.Map.Misc && !flag.Gem.IsGem;
        facetor = flag.Facetor;
        modSet = !item.State.ExchangeCurrency && !flag.Gem.IsGem && !flag.Area.Chronicle
            && !flag.Tag.CapturedBeast && !flag.Area.Ultimatum && !flag.Map.Valdo;
        byBase = !item.State.ExchangeCurrency && !item.State.ConquerorMap
            && !flag.Waystones && !flag.Gem.IsGem && !flag.Map.Blight && !flag.Map.BlightRavaged && !flag.Area.IsArea;

        rarity = !item.State.ExchangeCurrency && !flag.Gem.IsGem && !flag.Area.IsArea;
        checkAll = !item.State.ExchangeCurrency || flag.Tag.Imbued;
        quality = !item.State.ExchangeCurrency && !flag.Waystones && !flag.Area.IsArea;
        corrupted = !item.State.ExchangeCurrency && !flag.Area.IsArea;
        panelStat = !item.State.ExchangeCurrency && !flag.Facetor && !flag.Tag.CapturedBeast 
            && !flag.Corpses && !flag.Area.SanctumResearch && !flag.Area.Chronicle;
        panelForm = !item.State.ExchangeCurrency
            || flag.Gem.Uncut || flag.Wombgift || flag.UltimatumPoe2 || flag.Area.TrialCoins;

        if (flag.Map.Blight || flag.Map.BlightRavaged)
        {
            synthesisBlight = true;
            blightRavaged = true;
            hiddablePanel = true;
        }
        mapStats = flag.Map.IsMap || flag.Waystones;
        reward = !item.IsPoe2 && (flag.Area.Ultimatum || flag.Map.Valdo || flag.Contracts);
        detail = item.Rule.ShowDetail;
        headerMod = !item.Rule.ShowDetail;
        damage = flag.Weapon.IsWeapon && !flag.Tag.Unidentified;
        defense = flag.Armour.IsArmour && !flag.Tag.Unidentified;
        if (flag.Armour.IsArmour && !flag.Tag.Unidentified)
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

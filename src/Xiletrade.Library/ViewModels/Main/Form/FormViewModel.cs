using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Exchange;
using Xiletrade.Library.ViewModels.Main.Form.Panel;
using Xiletrade.Library.ViewModels.Main.Form.Search;

namespace Xiletrade.Library.ViewModels.Main.Form;

[ViewModelCreation(ViewModelCreation.Factory)]
public sealed partial class FormViewModel : ViewModelBase
{
    private readonly IViewManager _view;
    private readonly IMessageAdapterService _message;
    private readonly DataManagerService _dm;
    private readonly MainViewModel _vm;

    [ObservableProperty]
    private string itemName = string.Empty;

    [ObservableProperty]
    private string itemNameColor = string.Empty;

    [ObservableProperty]
    private string itemBaseType;

    [ObservableProperty]
    private string itemBaseTypeColor;

    [ObservableProperty]
    private double baseTypeFontSize;

    [ObservableProperty]
    private string dps = string.Empty;

    [ObservableProperty]
    private string dpsTip = string.Empty;

    [ObservableProperty]
    private string dustValue = string.Empty;

    [ObservableProperty]
    private string rarityBox;

    [ObservableProperty]
    private bool byBase;

    [ObservableProperty]
    private bool allCheck;

    [ObservableProperty]
    private string detail = string.Empty;

    [ObservableProperty]
    private PanelViewModel panel;

    [ObservableProperty]
    private AsyncObservableCollection<ModLineViewModel> modList;

    [ObservableProperty]
    private AsyncObservableCollection<string> fractured = GetNewOptions();

    [ObservableProperty]
    private int fracturedIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> split = GetNewOptions();

    [ObservableProperty]
    private int splitIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> mirrored = GetNewOptions();

    [ObservableProperty]
    private int mirroredIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> identified = GetNewOptions();

    [ObservableProperty]
    private int identifiedIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> corruption = GetNewOptions();

    [ObservableProperty]
    private int corruptedIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> doubleCorruption = GetNewOptions();

    [ObservableProperty]
    private int doubleCorruptedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> crafted = GetNewOptions();

    [ObservableProperty]
    private int craftedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> mutated = GetNewOptions();

    [ObservableProperty]
    private int mutatedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> desecrated = GetNewOptions();

    [ObservableProperty]
    private int desecratedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> veiled = GetNewOptions();

    [ObservableProperty]
    private int veiledIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> sanctified = GetNewOptions();

    [ObservableProperty]
    private int sanctifiedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> market;

    [ObservableProperty]
    private int marketIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> league;

    [ObservableProperty]
    private int leagueIndex;

    [ObservableProperty]
    private AsyncObservableCollection<UniqueItem> unique;

    [ObservableProperty]
    private int uniqueIndex;

    [ObservableProperty]
    private InfluenceViewModel influence;

    [ObservableProperty]
    private ConditionViewModel condition;

    [ObservableProperty]
    private TabViewModel tab;

    [ObservableProperty]
    private VisibilityViewModel visible;

    [ObservableProperty]
    private BulkItemExchangeViewModel itemExchange;

    [ObservableProperty]
    private CustomSearchViewModel customSearch;

    [ObservableProperty]
    private RarityViewModel rarity;

    [ObservableProperty]
    private double opacity;

    [ObservableProperty]
    private string fillTime = string.Empty;

    [ObservableProperty]
    private CheckComboViewModel checkComboInfluence;

    [ObservableProperty]
    private CheckComboViewModel checkComboCondition;

    [ObservableProperty]
    private bool freeze;

    [ObservableProperty]
    private string rateText = string.Empty;

    [ObservableProperty]
    private bool minimized;

    [ObservableProperty]
    private bool fetchDetailIsEnabled;

    [ObservableProperty]
    private bool sameUser;

    /// <summary>
    /// Buyout price option (index of the combobox) : 0 = all currencies.
    /// PoE 1 : 1 = chaos or divine, 2 = divine. PoE 2 : 1 = exalted, 2 = exalted or divine, 3 = chaos.
    /// </summary>
    [ObservableProperty]
    private int buyoutPriceIndex;

    [ObservableProperty]
    private bool autoClose;

    [ObservableProperty]
    private bool isPoeTwo;

    [ObservableProperty]
    private bool isSelectionEnabled = true;

    [ObservableProperty]
    private bool unidentifiedUnique;

    // buyout price changed by the user : the search is done again, as with a click on the result bar
    partial void OnBuyoutPriceIndexChanged(int value)
    {
        if (value < 0 || Freeze)
        {
            return;
        }
        RefreshSearch(null);
    }

    partial void OnUniqueIndexChanged(int value)
    {
        if (value < 0 || !(Tab.QuickSelected || Tab.DetailSelected))
        {
            return;
        }
        _view.ClearFocus();

        _vm.Result.InitData();
        if (_dm.Config.Options.Gateway is not 8 and not 9)
        {
            _vm.TaskManager.NinjaTask = _vm.Ninja.TryUpdateNinjaTask();
        }
        _vm.Result.UpdateAsync(minimumStock: 0);
    }

    /// <param name="live">Live searches of the custom search.</param>
    public FormViewModel(DataManagerService dm, MainViewModel vm, IViewManager view, IMessageAdapterService message,
        LiveSearchService live, bool useCustomOrBulk)
    {
        _dm = dm;
        _vm = vm;
        _view = view;
        _message = message;

        // title of the tab selected (see TabViewModel) : custom search first
        itemBaseType = useCustomOrBulk ? Resources.Resources.Main247_CustomSearch : string.Empty;
        itemBaseTypeColor = useCustomOrBulk ? Strings.Color.Moccasin : string.Empty;
        baseTypeFontSize = useCustomOrBulk ? 16 : 12; // FontSize cannot be equal to 0
        itemExchange = new(_dm, _vm, _view, _message, useCustomOrBulk);
        tab = new(this, useCustomOrBulk);
        if (useCustomOrBulk)
        {
            visible = new();
            rarity = new();
            customSearch = new(_dm, _vm, _view, _message, live);
        }

        isPoeTwo = _dm.Config.Options.GameVersion is 1;

        market = new() { Strings.Status.Available, Strings.Status.Online, Strings.Status.Securable, Strings.Word.any };
        marketIndex = _dm.Config.Options.AsyncMarketDefault ? 2 : 0;

        autoClose = _dm.Config.Options.Autoclose;
        sameUser = _dm.Config.Options.HideSameOccurs;
        opacity = _dm.Config.Options.Opacity;
        leagueIndex = _dm.GetDefaultLeagueIndex();
        league = _dm.GetLeagueAsyncCollection();
    }

    public FormViewModel(DataManagerService dm, MainViewModel vm, IViewManager view, IMessageAdapterService message,
        LiveSearchService live, ItemData item, InfoDescription infoDesc, bool showMinMax) : this(dm, vm, view, message, live, useCustomOrBulk: false)
    {
        var flag = item.Flag;
        if (item.ModList?.Count > 0)
        {
            var modListVm = new AsyncObservableCollection<ModLineViewModel>();
            foreach (var mod in item.ModList)
            {
                modListVm.Add(new(_dm, item, mod, showMinMax));
            }
            modList = modListVm;
        }
        if (item.Rule.ShowDetail)
        {
            detail = item.GetDetails(infoDesc);
        }
        if (flag.Weapon.IsWeapon && !flag.Tag.Unidentified)
        {
            dps = item.Damage.TotalString;
            dpsTip = item.Damage.Tip;
        }
        if (flag.Tag.Unidentified && flag.Rarity.Unique)
        {
            unique = GetUniqueList(item);
            if (unique.Count > 0)
            {
                unidentifiedUnique = true;
            }
        }

        identifiedIndex = flag.Tag.Unidentified ? 1 : 0;
        fracturedIndex = flag.Jewel.Cluster && !flag.Tag.Fractured && !flag.Tag.Corrupted ? 1 : 0;
        corruptedIndex = !flag.Tag.Corrupted && (flag.Gem.IsGem || (!flag.Rarity.Unique
            && (flag.Map.IsMap || flag.Waystones || flag.Invitation || flag.Area.Logbook))) ? 1
            : flag.Tag.Corrupted && _dm.Config.Options.AutoSelectCorrupt ? 2
            : (flag.Rarity.Normal || (isPoeTwo && flag.Rarity.Unique)) ? 1 : 0;
        doubleCorruptedIndex = flag.Tag.TwiceCorrupted && _dm.Config.Options.AutoSelectCorrupt ? 2 : 0; // to refine

        var poe2SkillWeapon = item.IsPoe2 && (flag.Weapon.Wand || flag.Weapon.IsStave || flag.Weapon.Sceptre);
        byBase = item.State.SpecialBase || _dm.Config.Options.SearchByType || item.Rule.ByBase || poe2SkillWeapon;
        itemName = item.Name;
        itemBaseType = item.Type;

        itemNameColor = flag.Rarity.Magic ? Strings.Color.DeepSkyBlue :
            flag.Rarity.Rare ? Strings.Color.Gold :
            flag.Tag.FoilVariant ? Strings.Color.Green :
            flag.Rarity.Unique ? Strings.Color.Peru : string.Empty;

        itemBaseTypeColor = flag.Gem.IsGem ? Strings.Color.Teal :
            item.State.ExchangeCurrency || flag.Tag.CapturedBeast ? Strings.Color.Moccasin : string.Empty;

        var minMax = MinMaxModel.GetMinMax(_dm, item);
        dustValue = GetDustValue(_dm, item, minMax);
        var showDust = dustValue.Length > 0;

        panel = new(_dm, item, minMax);
        visible = new(_dm, item, showDust);
        rarity = new(item);
        tab = new(this, item);
        condition = new(item, panel.Sockets);
        influence = new(item.Flag.Tag);
        checkComboCondition = new(condition);
        checkComboInfluence = new(influence);
    }

    [RelayCommand]
    private void CheckAllMods(object commandParameter) // quick view
    {
        if (ModList is null || ModList.Count is 0)
        {
            return;
        }
        foreach (var mod in ModList)
        {
            mod.Selected = AllCheck;
        }
    }

    [RelayCommand]
    private void ShowMinMaxMods(object commandParameter) // quick view
    {
        if (ModList is null || ModList.Count is 0)
        {
            return;
        }
        foreach (var mod in ModList)
        {
            if (!mod.IsSlidable || mod.PreferMinMax == _vm.ShowMinMax)
            {
                continue;
            }
            if (_vm.ShowMinMax)
            {
                // reversed slider (right click) : the value goes to max
                if (mod.IsSlideReversed)
                {
                    mod.Max = mod.SlideValue.ToStr();
                    mod.Min = string.Empty;
                }
                else
                {
                    mod.Min = mod.SlideValue.ToStr();
                }
            }
            else
            {
                var value = mod.IsSlideReversed ? mod.Max : mod.Min;
                if (value.Length > 0)
                {
                    mod.SlideValue = value.ToDoubleDefault();
                }
                if (mod.IsSlideReversed)
                {
                    mod.Max = mod.DefaultMax; // reversed slider : the max field is sent as min (see XiletradeItem)
                }
            }
            mod.PreferMinMax = _vm.ShowMinMax;
        }
    }

    [RelayCommand]
    private void ClearFocus(object commandParameter) => _view.ClearFocus();

    [RelayCommand]
    private void SetModCurrent(object commandParameter) // quick view
    {
        if (_vm.Item is not null)
        {
            UpdateStats(_vm.Item);
        }

        if (ModList is null || ModList.Count <= 0)
        {
            return;
        }

        Panel.TierSelection = false;

        List<bool> sameText = new();
        bool remove = true;

        foreach (var mod in ModList)
        {
            sameText.Add((mod.IsDefaultMax ? mod.Max : mod.Min) == mod.Current);
            ResetMax(mod);
            if (mod.IsDefaultMax || mod.Max.Length is 0)
            {
                SetModValue(mod, mod.Current);
            }
            mod.SlideValue = mod.Current.ToDoubleDefault();
        }

        foreach (bool same in sameText) remove &= same;
        if (!remove)
        {
            return;
        }
        foreach (var mod in ModList)
        {
            if (mod.IsDefaultMax)
            {
                mod.Max = string.Empty;
                continue;
            }
            if (mod.Min.Length > 0)
            {
                mod.Min = string.Empty;
            }
        }
    }

    [RelayCommand]
    private void SetModTier(object commandParameter) // quick view
    {
        if (_vm.Item is not null)
        {
            UpdateStats(_vm.Item, useTier: true);
        }

        if (ModList is null || ModList.Count <= 0)
        {
            return;
        }

        Panel.TierSelection = true;

        foreach (var mod in ModList)
        {
            if (mod.TierTip.Count <= 0)
            {
                continue;
            }
            ResetMax(mod);
            var value = GetTierValue(mod.TierTip[0].Text) ?? mod.Current;
            SetModValue(mod, value);
            mod.SlideValue = value.ToDoubleEmptyField();
        }
    }

    // before tier and real values : the slider direction and the max value (min/max display) are reset to default
    private static void ResetMax(ModLineViewModel mod)
    {
        mod.IsSlideReversed = false;
        if (mod.PreferMinMax)
        {
            mod.Max = mod.DefaultMax;
        }
    }

    // first value of the tier : number, range "a-b" or negative range "-a-b" / "-a--b", null otherwise
    private static string GetTierValue(string tier)
    {
        if (Double.TryParse(tier, out double val))
        {
            return val.ToStr();
        }
        var span = tier.AsSpan();
        var count = span.Count('-');
        if (count is < 1 or > 3)
        {
            return null;
        }
        var first = span.IndexOf('-');
        if (count is 1 || first > 0)
        {
            return span[..first].ToString();
        }
        // negative value : leading '-' kept up to the next separator
        var negative = span[..(span[1..].IndexOf('-') + 1)];
        return negative.Length > 1 && !negative.Contains('+') ? negative.ToString() : null;
    }

    // tier and real values : max value for the mods filled by default with a max value, min value otherwise
    private static void SetModValue(ModLineViewModel mod, string value)
    {
        if (mod.IsDefaultMax)
        {
            mod.Max = value;
            return;
        }
        mod.Min = value;
    }

    [RelayCommand]
    public void CheckCondition(object commandParameter) => CheckComboCondition = new(Condition);

    [RelayCommand]
    public void CheckInfluence(object commandParameter) => CheckComboInfluence = new(Influence);

    [RelayCommand]
    private void RefreshSearch(object commandParameter) // quick and detail views
    {
        try
        {
            _view.ClearFocus();
            _vm.Result.InitData();

            if (Tab.QuickSelected || Tab.DetailSelected)
            {
                _vm.Result.UpdateAsync(minimumStock: 1);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error009_XSearchRefresh, MessageStatus.Error);
        }
    }

    internal void ClearLists()
    {
        ModList?.Clear();
        Panel?.StatList?.Clear();
        CustomSearch?.MinMaxList?.Clear();
    }

    internal void UpdateMarket(bool useBulk)
    {
        Market = useBulk ? new() { Strings.Status.Online, Strings.Word.any }
            : new() { Strings.Status.Available, Strings.Status.Online, Strings.Status.Securable, Strings.Word.any };
        MarketIndex = !useBulk && _dm.Config.Options.AsyncMarketDefault ? 2 : 0;
    }

    internal string GetUniqueName()
    {
        var check = UnidentifiedUnique && UniqueIndex > -1
            && UniqueIndex < Unique.Count && Unique[UniqueIndex].Name?.Length > 0;
        return check ? Unique[UniqueIndex].Name : string.Empty;
    }

    private static string GetDustValue(DataManagerService dm, ItemData item, Dictionary<StatPanel, MinMaxModel> minMax)
    {
        var flag = item.Flag;
        if (!item.IsPoe2 && flag.Rarity.Unique && !flag.Tag.Unidentified && !flag.Map.IsMap
            && dm.DustLevel.FindDustByName(item.NameEn) is var dust && dust is not null)
        {
            var level = minMax[StatPanel.CommonItemLevel];
            var qual = minMax[StatPanel.CommonQuality];

            var ilvl = Math.Clamp(level.Min.ToDoubleDefault(), 65, 84);
            var valQual = qual.Min.ToDoubleDefault();
            double qualMultiplier = 1;
            if (valQual > 0)
            {
                qualMultiplier += valQual * 1 / 50;
            }
            var multiplier = (20 - (84 - ilvl)) * qualMultiplier;
            var calc = Math.Truncate(dust.DustVal * 125 * multiplier);
            return calc.FormatWithSuffix();
        }
        return string.Empty;
    }

    private void UpdateStats(ItemData item, bool useTier = false)
    {
        if (item?.Stats is null)
            return;

        foreach (var kvp in item.Stats.Map)
        {
            var value = useTier ? kvp.Value.tier : kvp.Value.current;
            var stat = Panel.StatList?.FirstOrDefault(x => x.Id == kvp.Key);
            if (stat is not null && stat.SlideValue > 0 && value > 0)
            {
                stat.SlideValue = value;
            }
        }
    }

    private AsyncObservableCollection<UniqueItem> GetUniqueList(ItemData item)
    {
        var uniqueList = _dm.Items.SelectMany(cat => cat.Entries)
                .Where(x => x.Type == item.Type && x.Text is not null).Select(x => new UniqueItem(x.Name, x.Text));
        var unique = new AsyncObservableCollection<UniqueItem>();
        foreach (var uniqueItem in uniqueList)
        {
            unique.Add(uniqueItem);
        }
        return unique;
    }

    private static AsyncObservableCollection<string> GetNewOptions() =>
        new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };
}

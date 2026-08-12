using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
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

public sealed partial class FormViewModel(bool useBulk) : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly DataManagerService _dm;

    [ObservableProperty]
    private string itemName = string.Empty;

    [ObservableProperty]
    private string itemNameColor = string.Empty;

    [ObservableProperty]
    private string itemBaseType = useBulk ? Resources.Resources.Main247_CustomSearch + " / "
        + Resources.Resources.Main032_cbTotalExchange : string.Empty;

    [ObservableProperty]
    private string itemBaseTypeColor = useBulk ? Strings.Color.Moccasin : string.Empty;

    [ObservableProperty]
    private double baseTypeFontSize = useBulk ? 16 : 12; // FontSize cannot be equal to 0

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
    private AsyncObservableCollection<string> fractured = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int fracturedIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> split = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int splitIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> mirrored = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int mirroredIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> identified = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int identifiedIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> corruption = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int corruptedIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> doubleCorruption = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int doubleCorruptedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> crafted = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int craftedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> mutated = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int mutatedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> desecrated = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int desecratedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> veiled = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int veiledIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> sanctified = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

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
    private BulkViewModel bulk;

    [ObservableProperty]
    private ShopViewModel shop;

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

    [ObservableProperty]
    private bool chaosDiv;

    [ObservableProperty]
    private bool exalt;

    [ObservableProperty]
    private bool chaos;

    [ObservableProperty]
    private bool autoClose;

    [ObservableProperty]
    private bool isPoeTwo;

    [ObservableProperty]
    private bool isSelectionEnabled = true;

    [ObservableProperty]
    private bool unidentifiedUnique;

    partial void OnUniqueIndexChanged(int value)
    {
        if (value < 0 || !(Tab.QuickSelected || Tab.DetailSelected))
        {
            return;
        }
        _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        vm.Result.InitData();
        if (_dm.Config.Options.Gateway is not 8 and not 9)
        {
            vm.TaskManager.NinjaTask = vm.Ninja.TryUpdateNinjaTask();
        }
        vm.UpdateResultWithPoeApi(minimumStock: 0);
    }

    public FormViewModel(IServiceProvider serviceProvider, bool useCustomOrBulk) : this(useCustomOrBulk)
    {
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        
        bulk = new(_serviceProvider); // mandatory (auto select currency item on price check)
        if (useCustomOrBulk)
        {
            visible = new();
            rarity = new();
            tab = new(this);
            shop = new(_serviceProvider);
            customSearch = new(_serviceProvider);
        }

        isPoeTwo = _dm.Config.Options.GameVersion is 1;

        market = new() { Strings.Status.Available, Strings.Status.Online, Strings.Status.Securable, Strings.any };
        marketIndex = _dm.Config.Options.AsyncMarketDefault ? 2 : 0;

        autoClose = _dm.Config.Options.Autoclose;
        sameUser = _dm.Config.Options.HideSameOccurs;
        opacity = _dm.Config.Options.Opacity;
        leagueIndex = _dm.GetDefaultLeagueIndex();
        league = _dm.GetLeagueAsyncCollection();
    }

    internal FormViewModel(IServiceProvider serviceProvider, ItemData item, InfoDescription infoDesc, bool showMinMax) : this(serviceProvider, useCustomOrBulk: false)
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
        if (flag.ShowDetail)
        {
            detail = item.GetDetails(infoDesc);
        }
        if (flag.Weapon && !flag.Unidentified)
        {
            dps = item.Damage.TotalString;
            dpsTip = item.Damage.Tip;
        }
        if (flag.Unidentified && flag.Unique)
        {
            unique = GetUniqueList(item);
            if (unique.Count > 0)
            {
                unidentifiedUnique = true;
            }
        }

        identifiedIndex = flag.Unidentified ? 1 : 0;
        fracturedIndex = flag.Cluster && !flag.Fractured && !flag.Corrupted ? 1 : 0;
        corruptedIndex = !flag.Corrupted && (flag.Gems || (!flag.Unique
            && (flag.Map || flag.Waystones || flag.Invitation || flag.Logbook))) ? 1
            : flag.Corrupted && _dm.Config.Options.AutoSelectCorrupt ? 2
            : (flag.Normal || (isPoeTwo && flag.Unique)) ? 1 : 0;
        doubleCorruptedIndex = flag.TwiceCorrupted && _dm.Config.Options.AutoSelectCorrupt ? 2 : 0; // to refine

        var poe2SkillWeapon = item.IsPoe2 && (flag.Wand || flag.Stave || flag.Sceptre);
        byBase = item.State.SpecialBase || _dm.Config.Options.SearchByType || flag.ByBase || poe2SkillWeapon;
        itemName = item.Name;
        itemBaseType = item.Type;

        itemNameColor = flag.Magic ? Strings.Color.DeepSkyBlue :
            flag.Rare ? Strings.Color.Gold :
            flag.FoilVariant ? Strings.Color.Green :
            flag.Unique ? Strings.Color.Peru : string.Empty;

        itemBaseTypeColor = flag.Gems ? Strings.Color.Teal :
            item.State.ExchangeCurrency || flag.CapturedBeast ? Strings.Color.Moccasin : string.Empty;

        var minMax = item.GetMinMax();
        dustValue = GetDustValue(_dm, item, minMax);
        var showDust = dustValue.Length > 0;

        panel = new(_dm, item, minMax);
        visible = new(_dm, item, showDust);
        rarity = new(item);
        tab = new(this, item);
        condition = new(item, panel.Sockets);
        influence = new(item.Flag);
        checkComboCondition = new(condition);
        checkComboInfluence = new(influence);
    }

    internal void ClearLists()
    {
        ModList?.Clear();
        Panel?.StatList?.Clear();
        CustomSearch?.MinMaxList?.Clear();
    }

    internal void UpdateMarket(bool useBulk)
    {
        Market = useBulk ? new() { Strings.Status.Online, Strings.any }
            : new() { Strings.Status.Available, Strings.Status.Online, Strings.Status.Securable, Strings.any };
        MarketIndex = !useBulk && _dm.Config.Options.AsyncMarketDefault ? 2 : 0;
    }

    internal void SetModCurrent(ItemData item, bool clear = true)
    {
        if (item is not null)
        {
            UpdateStats(item);
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
            sameText.Add(mod.Min == mod.Current);
            if (!mod.PreferMinMax && mod.Max.Length is 0)
            {
                mod.Min = mod.Current;
            }
            mod.SlideValue = mod.Current.ToDoubleDefault();
        }

        foreach (bool same in sameText) remove &= same;
        if (!remove || !clear)
        {
            return;
        }
        foreach (var mod in ModList)
        {
            if (mod.Min.Length > 0)
            {
                mod.Min = string.Empty;
            }
        }
    }

    internal void SetModTier(ItemData item)
    {
        if (item is not null)
        {
            UpdateStats(item, useTier: true);
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
            if (Double.TryParse(mod.TierTip[0].Text, out double val))
            {
                mod.Min = val.ToStr();
                mod.SlideValue = val;
                continue;
            }
            string[] range = mod.TierTip[0].Text.Split("-");
            if (range.Length is 2)
            {
                mod.Min = range[0];
                mod.SlideValue = range[0].ToDoubleEmptyField();
                continue;
            }
            if (range.Length is 3 or 4)
            {
                if (range[0].Length > 0)
                {
                    mod.Min = range[0];
                    mod.SlideValue = range[0].ToDoubleEmptyField();
                    continue;
                }
                if (range[1].Length > 0 && !range[1].Contain('+'))
                {
                    mod.Min = "-" + range[1];
                    mod.SlideValue = - range[1].ToDoubleEmptyField();
                    continue;
                }
            }
            mod.Min = mod.Current;
            mod.SlideValue = mod.Current.ToDoubleEmptyField();
        }
    }

    internal XiletradeItem GetXiletradeItem(bool customSearch = false)
    {
        var listPanel = customSearch ? CustomSearch.MinMaxList : Panel.StatList;

        var rarity = Rarity is null ? string.Empty : 
            Rarity.Index >= 0 && Rarity.Index < Rarity.ComboBox.Count ?
            Rarity.ComboBox[Rarity.Index] : Rarity.Item;
        var noSockets = Panel?.Sockets is null;
        var panel = Panel is not null;
        var influence = Influence is not null;

        var item = new XiletradeItem()
        {
            InfShaper = influence && Influence.Shaper,
            InfElder = influence && Influence.Elder,
            InfCrusader = influence && Influence.Crusader,
            InfRedeemer = influence && Influence.Redeemer,
            InfHunter = influence && Influence.Hunter,
            InfWarlord = influence && Influence.Warlord,

            //itemOption.Corrupt = (byte)cbCorrupt.SelectedIndex;
            Corrupted = GetOption(CorruptedIndex),
            TwiceCorrupted = GetOption(DoubleCorruptedIndex),
            Identified = GetOption(IdentifiedIndex),
            Mirrored = GetOption(MirroredIndex),
            Fractured = GetOption(FracturedIndex),
            Split = GetOption(SplitIndex),
            Crafted = GetOption(CraftedIndex),
            Mutated = GetOption(MutatedIndex),
            Desecrated = GetOption(DesecratedIndex),
            Veiled = GetOption(VeiledIndex),
            Sanctified = GetOption(SanctifiedIndex),
            SynthesisBlight = panel && Panel.SynthesisBlight,
            BlightRavaged = panel && Panel.BlighRavaged,
            ByType = ByBase != true,

            RewardType = Panel?.Reward?.Tip.Length > 0 ? Panel.Reward.Tip : null,
            Reward = Panel?.Reward?.Text.Length > 0 ? Panel.Reward.Text : null,
            ChaosDivOnly = ChaosDiv,
            ExaltOnly = Exalt,
            ChaosOnly = Chaos,
            Rarity = rarity,
            UniqueName = GetUniqueName(),

            PriceMin = 0, // not used

            SocketColors = Condition is not null && Condition.SocketColors,

            SocketRed = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.RedColor.ToDoubleEmptyField(),
            SocketGreen = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.GreenColor.ToDoubleEmptyField(),
            SocketBlue = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.BlueColor.ToDoubleEmptyField(),
            SocketWhite = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.WhiteColor.ToDoubleEmptyField()
        };
        item.FacetorExp.Min = !panel ? ModFilter.EMPTYFIELD : Panel.FacetorMin.ToDoubleEmptyField();
        item.FacetorExp.Max = !panel ? ModFilter.EMPTYFIELD : Panel.FacetorMax.ToDoubleEmptyField();
        item.FacetorExp.Enable = (item.FacetorExp.Min?.IsNotEmpty() ?? item.FacetorExp.Max?.IsNotEmpty()) ?? false;

        // add item filters
        if (ModList?.Count > 0)
        {
            int modLimit = 1;
            foreach (var mod in ModList)
            {
                var itemFilter = new ItemFilter();
                if (mod.Affix.Count is 0)
                {
                    continue;
                }

                double minValue = mod.PreferMinMax ? mod.Min.ToDoubleEmptyField()
                        : !mod.IsSlideReversed ? mod.SlideValue : mod.Max.ToDoubleEmptyField();
                double maxValue = mod.PreferMinMax ? mod.Max.ToDoubleEmptyField()
                    : mod.IsSlideReversed ? mod.SlideValue : mod.Max.ToDoubleEmptyField();

                itemFilter.Text = mod.Mod.Trim();
                itemFilter.Type = mod.Affix[mod.AffixIndex].Type;
                itemFilter.Disabled = mod.Selected != true;
                itemFilter.Min = minValue;
                itemFilter.Max = maxValue;

                itemFilter.Id = mod.Affix[mod.AffixIndex].ID;
                if (mod.OptionVisible)
                {
                    itemFilter.Option = mod.OptionID[mod.OptionIndex];
                    itemFilter.Min = ModFilter.EMPTYFIELD;
                }
                item.ItemFilters.Add(itemFilter);
                if (modLimit >= ItemData.NB_MAX_MODS)
                {
                    break;
                }
                modLimit++;
            }
        }

        var stats = new[]
        {
            (StatPanel.CommonItemLevel, item.Lvl),
            (StatPanel.CommonQuality, item.Quality),
            (StatPanel.CommonSocket, item.Socket),
            (StatPanel.CommonLink, item.Link),
            (StatPanel.CommonSocketRune, item.RuneSockets),
            (StatPanel.CommonSocketGem, item.GemSockets),
            (StatPanel.CommonRequiresLevel, item.ReqLevel),
            (StatPanel.CommonMemoryStrand, item.MemoryStrand),
            (StatPanel.DamageElemental, item.DpsElem),
            (StatPanel.DamagePhysical, item.DpsPhys),
            (StatPanel.DamageTotal, item.DpsTotal),
            (StatPanel.DefenseArmour, item.Armour),
            (StatPanel.DefenseEnergy, item.Energy),
            (StatPanel.DefenseEvasion, item.Evasion),
            (StatPanel.DefenseWard, item.Ward),
            (StatPanel.DefenseRunicWard, item.Ward),
            (StatPanel.MapPackSize, item.MapPack),
            (StatPanel.MapQuantity, item.MapIiq),
            (StatPanel.MapRarity, item.MapIir),
            (StatPanel.GoldFound, item.GoldFound),
            (StatPanel.DeadSulphur, item.DeadSulphur),
            (StatPanel.SanctumAureus, item.Aureus),
            (StatPanel.SanctumInspiration, item.Inspiration),
            (StatPanel.SanctumMaxResolve, item.MaxResolve),
            (StatPanel.SanctumResolve, item.Resolve),
            (StatPanel.WaystoneRarity, item.ItemRarity),
            (StatPanel.WaystoneMonsterRarity, item.MonsterRarity),
            (StatPanel.WaystoneMonsterEffectiveness, item.Effectiveness),
            (StatPanel.WaystonePackSize, item.PackSize),
            (StatPanel.WaystoneDrop, item.WaystoneDrop),
            (StatPanel.WaystoneRevives, item.Revives)
        };

        foreach ((var stat, var option) in stats)
        {
            var vm = listPanel?.FirstOrDefault(x => x.Id == stat);
            if (vm is not null)
            {
                option.Enable = vm.Selected;
                option.Min = vm.ItemMin;
                option.Max = vm.ItemMax;
            }
        }

        var pseudos = new[]
        {
            (StatPanel.TotalElemResistance, Strings.Stat.Pseudo.TotalElemResistance),
            (StatPanel.TotalLife, Strings.Stat.Pseudo.TotalLife),
            (StatPanel.TotalAttribute, Strings.Stat.Pseudo.TotalAttribute),
            (StatPanel.TotalGlobalEs, Strings.Stat.Pseudo.TotalEs),
            (StatPanel.MapMoreScarab, Strings.Stat.Pseudo.MoreScarab),
            (StatPanel.MapMoreCurrency, Strings.Stat.Pseudo.MoreCurrency),
            (StatPanel.MapMoreDivCard, Strings.Stat.Pseudo.MoreDivCard)
            //(StatPanel.MapMoreMap, "pseudo.pseudo_map_more_map_drops")
        };

        foreach ((var stat, var pseudo) in pseudos)
        {
            var vm = listPanel?.FirstOrDefault(x => x.Id == stat);

            if (vm is null || !vm.Selected)
                continue;

            var filter = vm.SlideValue is not ModFilter.EMPTYFIELD
                ? new ItemFilter(_dm.Filter, pseudo, vm.SlideValue, vm.Max)
                : new ItemFilter(_dm.Filter, pseudo, vm.Min, vm.Max);

            if (!string.IsNullOrEmpty(filter.Id))
                item.ItemFilters.Add(filter);
        }

        if (Condition is not null)
        {
            if (Condition.FreePrefix)
            {
                var filter = new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.EmmptyPrefix, 1, ModFilter.EMPTYFIELD);
                if (filter.Id.Length > 0)
                {
                    item.ItemFilters.Add(filter);
                }
            }

            if (Condition.FreeSuffix)
            {
                var filter = new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.EmptySuffix, 1, ModFilter.EMPTYFIELD);
                if (filter.Id.Length > 0)
                {
                    item.ItemFilters.Add(filter);
                }
            }
        }

        List<string> listInfluence = new();

        if (item.InfShaper)
        {
            listInfluence.Add(Strings.Stat.Influence.Shaper);
        }
        if (item.InfElder)
        {
            listInfluence.Add(Strings.Stat.Influence.Elder);
        }
        if (item.InfCrusader)
        {
            listInfluence.Add(Strings.Stat.Influence.Crusader);
        }
        if (item.InfRedeemer)
        {
            listInfluence.Add(Strings.Stat.Influence.Redeemer);
        }
        if (item.InfHunter)
        {
            listInfluence.Add(Strings.Stat.Influence.Hunter);
        }
        if (item.InfWarlord)
        {
            listInfluence.Add(Strings.Stat.Influence.Warlord);
        }

        if (listInfluence.Count > 0)
        {
            foreach (var influ in listInfluence)
            {
                var filter = new ItemFilter(_dm.Filter, "pseudo." + influ, ModFilter.EMPTYFIELD, ModFilter.EMPTYFIELD);
                if (filter.Id.Length > 0)
                {
                    item.ItemFilters.Add(filter);
                }
            }
        }

        return item;
    }

    internal string GetExchangeCurrencyTag(ExchangeType exchange) // get: true, pay: false
    {
        var exVm = exchange is ExchangeType.Get ? Bulk.Get :
            exchange is ExchangeType.Pay ? Bulk.Pay :
            exchange is ExchangeType.Shop ? Shop.Exchange : null;
        if (exVm is null)
        {
            return string.Empty;
        }

        string category = exVm.Category.Count > 0 && exVm.CategoryIndex > -1 ?
                exVm.Category[exVm.CategoryIndex] : string.Empty;
        string currency = exVm.Currency.Count > 0 && exVm.CurrencyIndex > -1 ?
            exVm.Currency[exVm.CurrencyIndex] : string.Empty;
        string tier = exVm.Tier.Count > 0 && exVm.TierIndex > -1 ?
            exVm.Tier[exVm.TierIndex] : string.Empty;

        var mapKind = string.Empty;
        if (category is Strings.Maps)
        {
            mapKind = tier.Replace("T", string.Empty);
            mapKind = mapKind is Strings.Blight or Strings.Ravaged ?
                Strings.CurrencyTypePoe1.MapsBlighted : Strings.CurrencyTypePoe1.Maps;
        }
        var res = _dm.Currencies.FindEntryByTypeAndPossibleMapKind(currency, mapKind);
        if (res is not null)
        {
            return res.Id;
        }
        return null;
    }

    internal async Task SelectExchangeCurrency(string args, string currency, string tier = null)
    {
        var arg = args.Split('/');
        //bool search = false;
        if (arg[0] is not "pay" and not "get" and not "shop")
        {
            return;
        }

        CurrencyEntrie entry;
        string curClass;
        if (arg.Length > 1 && arg[1] is "contains")
        {
            var curKeys = currency.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            (entry, curClass) = _dm.Currencies.FindEntryAndGroupIdByTypeOrId(curKeys);
        }
        else
        {
            (entry, curClass) = _dm.Currencies.FindEntryAndGroupIdByType(currency, image: false);
        }

        if (entry is null || curClass.Length is 0)
        {
            return;
        }

        string selectedTier = string.Empty;
        string selectedCategory = Strings.GetBulkCategory(curClass, entry.Id);

        if (selectedCategory.Length is 0)
        {
            return;
        }

        if (selectedCategory == Resources.Resources.Main055_Divination)
        {
            var tmpDiv = _dm.DivTiers.FindDivTierByTag(entry.Id);
            selectedTier = tmpDiv != null ? "T" + tmpDiv.Tier : Resources.Resources.Main016_TierNothing;
        }
        if (selectedCategory == Resources.Resources.Main056_Maps
            || selectedCategory == Resources.Resources.Main179_UniqueMaps
            || selectedCategory == Resources.Resources.Main217_BlightedMaps)
        {
            if (tier?.Length > 0)
            {
                selectedTier = "T" + tier;
            }
            else
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(entry.Text);
                if (match.Count is 1)
                {
                    selectedTier = "T" + match[0].Value.ToString();
                }
            }
        }
        var isTier = selectedTier.Length > 0;

        var bulk = arg[0] is "pay" ? Bulk.Pay
            : arg[0] is "get" ? Bulk.Get
            : arg[0] is "shop" ? Shop.Exchange
            : null;

        int idxCat = bulk.Category.IndexOf(selectedCategory);
        if (idxCat > -1)
        {
            bulk.CategoryIndex = idxCat;
        }
        
        if (isTier)
        {
            int idxTier = bulk.Tier.IndexOf(selectedTier);
            if (idxTier > -1 && selectedTier.Length > 0)
            {
                bulk.TierIndex = idxTier;
            }
        }

        // TO FIX, tier selection for maps/divcard not working properly
        int watchdog = 0;
        // 2 seconds max
        while (bulk.Currency.Count is 0 && watchdog < 10)
        {
            bulk.CategoryIndex = -1;
            await Task.Delay(100);
            bulk.CategoryIndex = idxCat;
            await Task.Delay(100);
            watchdog++;
        }

        int idxCur = bulk.Currency.IndexOf(entry.Text);
        if (idxCur > -1)
        {
            bulk.CurrencyIndex = idxCur;
        }
        /*
        if (!search)
        {
            IsSelectionEnabled = false;
            Command.MainCommand.SelectBulk("pay");
        }
        int idxCur = bulk.Currency.IndexOf(selectedCurrency);
        if (idxCur > -1)
        {
            bulk.CurrencyIndex = idxCur;
        }
        if (!search)
        {
            Command.MainCommand.Change("pay");
            IsSelectionEnabled = true;
        }
        */
    }

    //private
    private static string GetDustValue(DataManagerService dm, ItemData item, Dictionary<StatPanel, MinMaxModel> minMax)
    {
        var flag = item.Flag;
        if (!item.IsPoe2 && flag.Unique && !flag.Unidentified && !flag.Map
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

    private static DefaultOption GetOption(int index)
    {
        return index switch
        {
            1 => DefaultOption.False,
            2 => DefaultOption.True,
            _ => DefaultOption.Any,
        };
    }

    private string GetUniqueName()
    {
        var check = UnidentifiedUnique && UniqueIndex > -1
            && UniqueIndex < Unique.Count && Unique[UniqueIndex].Name?.Length > 0;
        return check ? Unique[UniqueIndex].Name : string.Empty;
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
}

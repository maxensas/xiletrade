using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Exchange;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class FormViewModel(bool useBulk) : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private static bool _showMinMax;
    private readonly DataManagerService _dm;

    /// <summary>Maximum number of mods to display.</summary>
    private const int NB_MAX_MODS = 30;

    [ObservableProperty]
    private string itemName = string.Empty;

    [ObservableProperty]
    private string itemNameColor = string.Empty;

    [ObservableProperty]
    private string itemBaseType = useBulk ? Resources.Resources.Main032_cbTotalExchange : string.Empty;

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
    private int corruptedIndex = 0;

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
    private AsyncObservableCollection<ModLineViewModel> modList = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> corruption = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private AsyncObservableCollection<string> alternate = new() { Resources.Resources.General005_Any, Resources.Resources.General001_Anomalous, Resources.Resources.General002_Divergent,
        Resources.Resources.General003_Phantasmal }; // obsolete

    [ObservableProperty]
    private AsyncObservableCollection<string> market;

    [ObservableProperty]
    private int marketIndex;

    [ObservableProperty]
    private AsyncObservableCollection<string> league;

    [ObservableProperty]
    private int leagueIndex;

    [ObservableProperty]
    private InfluenceViewModel influence = new();

    [ObservableProperty]
    private ConditionViewModel condition = new();

    [ObservableProperty]
    private TabViewModel tab = new(useBulk);

    [ObservableProperty]
    private VisibilityViewModel visible;

    [ObservableProperty]
    private BulkViewModel bulk;

    [ObservableProperty]
    private ShopViewModel shop;

    [ObservableProperty]
    private RarityViewModel rarity = new();

    [ObservableProperty]
    private double opacity;

    [ObservableProperty]
    private string fillTime = string.Empty;

    [ObservableProperty]
    private CheckComboViewModel checkComboInfluence = new();

    [ObservableProperty]
    private CheckComboViewModel checkComboCondition = new();

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

    public FormViewModel(IServiceProvider serviceProvider, bool useBulk) : this(useBulk)
    {
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var iSpoe1English = _dm.Config.Options.Language is 0 && _dm.Config.Options.GameVersion is 0;
        _showMinMax = _serviceProvider.GetRequiredService<MainViewModel>().ShowMinMax;

        bulk = new(_serviceProvider);
        shop = new(_serviceProvider);
        panel = new(_serviceProvider);
        visible = new(iSpoe1English, useBulk);

        isPoeTwo = _dm.Config.Options.GameVersion is 1;

        market = !isPoeTwo || useBulk ? new() { Strings.Status.Online, Strings.any }
            : new() { Strings.Status.Available, Strings.Status.Online, Strings.Status.Securable, Strings.any };
        marketIndex = !useBulk && isPoeTwo && _dm.Config.Options.AsyncMarketDefault ? 2 : 0;

        autoClose = _dm.Config.Options.Autoclose;
        sameUser = _dm.Config.Options.HideSameOccurs;
        opacity = _dm.Config.Options.Opacity;
        leagueIndex = _dm.GetDefaultLeagueIndex();
        league = _dm.GetLeagueAsyncCollection();
    }

    internal void SetModCurrent(bool clear = true)
    {
        if (ModList.Count <= 0)
        {
            return;
        }
        List<bool> sameText = new();
        bool remove = true;

        foreach (var mod in ModList)
        {
            sameText.Add(mod.Min == mod.Current);
            mod.Min = mod.Current;
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

    internal void SetModTier()
    {
        if (ModList.Count <= 0)
        {
            return;
        }
        foreach (var mod in ModList)
        {
            if (mod.TierTip.Count <= 0)
            {
                continue;
            }
            if (Double.TryParse(mod.TierTip[0].Text, out double val))
            {
                mod.Min = val.ToString("G", CultureInfo.InvariantCulture);
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

    internal XiletradeItem GetXiletradeItem()
    {
        var listPanel = Panel.Row.FirstRow.AsEnumerable()
            .Concat(Panel.Row.SecondRow.AsEnumerable())
            .Concat(Panel.Row.ThirdRow.AsEnumerable())
            .Concat(Panel.Row.FourthRow.AsEnumerable());

        var item = new XiletradeItem()
        {
            InfShaper = Influence.Shaper,
            InfElder = Influence.Elder,
            InfCrusader = Influence.Crusader,
            InfRedeemer = Influence.Redeemer,
            InfHunter = Influence.Hunter,
            InfWarlord = Influence.Warlord,

            //itemOption.Corrupt = (byte)cbCorrupt.SelectedIndex;
            Corrupted = CorruptedIndex switch
            {
                1 => "false",
                2 => "true",
                _ => Strings.any,
            },
            SynthesisBlight = Panel.SynthesisBlight,
            BlightRavaged = Panel.BlighRavaged,
            ByType = ByBase != true,

            RewardType = Panel.Reward.Tip.Length > 0 ? Panel.Reward.Tip : null,
            Reward = Panel.Reward.Text.Length > 0 ? Panel.Reward.Text : null,
            ChaosDivOnly = ChaosDiv,
            ExaltOnly = Exalt,
            ChaosOnly = Chaos,
            Rarity = Rarity.Index >= 0 && Rarity.Index < Rarity.ComboBox.Count ?
                Rarity.ComboBox[Rarity.Index] : Rarity.Item,

            PriceMin = 0, // not used

            SocketColors = Condition.SocketColors,

            SocketRed = Panel.Sockets.RedColor.ToDoubleEmptyField(),
            SocketGreen = Panel.Sockets.GreenColor.ToDoubleEmptyField(),
            SocketBlue = Panel.Sockets.BlueColor.ToDoubleEmptyField(),
            SocketWhite = Panel.Sockets.WhiteColor.ToDoubleEmptyField(),
            FacetorExpMin = Panel.FacetorMin.ToDoubleEmptyField(),
            FacetorExpMax = Panel.FacetorMax.ToDoubleEmptyField(),
        };

        // add item filters
        if (ModList.Count > 0)
        {
            int modLimit = 1;
            foreach (var mod in ModList)
            {
                var itemFilter = new ItemFilter();
                if (mod.Affix.Count > 0)
                {
                    double minValue = mod.PreferMinMax ? mod.Min.ToDoubleEmptyField() 
                        : !mod.IsSlideReversed ? mod.SlideValue : mod.Max.ToDoubleEmptyField();
                    double maxValue = mod.PreferMinMax ? mod.Max.ToDoubleEmptyField() 
                        : mod.IsSlideReversed ? mod.SlideValue : mod.Max.ToDoubleEmptyField();

                    itemFilter.Text = mod.Mod.Trim();
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
                    if (modLimit >= NB_MAX_MODS)
                    {
                        break;
                    }
                    modLimit++;
                }
            }
        }

        var search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonItemLevel);
        if (search is not null)
        {
            item.ChkLv = search.Selected;
            item.LvMin = search.ItemMin;
            item.LvMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonQuality);
        if (search is not null)
        {
            item.ChkQuality = search.Selected;
            item.QualityMin = search.ItemMin;
            item.QualityMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonSocket);
        if (search is not null)
        {
            item.ChkSocket = search.Selected;
            item.SocketMin = search.ItemMin;
            item.SocketMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonLink);
        if (search is not null)
        {
            item.ChkLink = search.Selected;
            item.LinkMin = search.ItemMin;
            item.LinkMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonSocketRune);
        if (search is not null)
        {
            item.ChkRuneSockets = search.Selected;
            item.RuneSocketsMin = search.ItemMin;
            item.RuneSocketsMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonSocketGem);
        if (search is not null)
        {
            item.ChkGemSockets = search.Selected;
            item.GemSocketsMin = search.ItemMin;
            item.GemSocketsMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonRequiresLevel);
        if (search is not null)
        {
            item.ChkReqLevel = search.Selected;
            item.ReqLevelMin = search.ItemMin;
            item.ReqLevelMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.CommonMemoryStrand);
        if (search is not null)
        {
            item.ChkMemoryStrand = search.Selected;
            item.MemoryStrandMin = search.ItemMin;
            item.MemoryStrandMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.DamageElemental);
        if (search is not null)
        {
            item.ChkDpsElem = search.Selected;
            item.DpsElemMin = search.ItemMin;
            item.DpsElemMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.DamagePhysical);
        if (search is not null)
        {
            item.ChkDpsPhys = search.Selected;
            item.DpsPhysMin = search.ItemMin;
            item.DpsPhysMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.DamageTotal);
        if (search is not null)
        {
            item.ChkDpsTotal = search.Selected;
            item.DpsTotalMin = search.ItemMin;
            item.DpsTotalMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.DefenseArmour);
        if (search is not null)
        {
            item.ChkArmour = search.Selected;
            item.ArmourMin = search.ItemMin;
            item.ArmourMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.DefenseEnergy);
        if (search is not null)
        {
            item.ChkEnergy = search.Selected;
            item.EnergyMin = search.ItemMin;
            item.EnergyMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.DefenseEvasion);
        if (search is not null)
        {
            item.ChkEvasion = search.Selected;
            item.EvasionMin = search.ItemMin;
            item.EvasionMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.DefenseWard);
        if (search is not null)
        {
            item.ChkWard = search.Selected;
            item.WardMin = search.ItemMin;
            item.WardMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.MapPackSize);
        if (search is not null)
        {
            item.ChkMapPack = search.Selected;
            item.MapPackSizeMin = search.ItemMin;
            item.MapPackSizeMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.MapQuantity);
        if (search is not null)
        {
            item.ChkMapIiq = search.Selected;
            item.MapItemQuantityMin = search.ItemMin;
            item.MapItemQuantityMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.MapRarity);
        if (search is not null)
        {
            item.ChkMapIir = search.Selected;
            item.MapItemRarityMin = search.ItemMin;
            item.MapItemRarityMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.SanctumAureus);
        if (search is not null)
        {
            item.ChkAureus = search.Selected;
            item.AureusMin = search.ItemMin;
            item.AureusMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.SanctumInspiration);
        if (search is not null)
        {
            item.ChkInspiration = search.Selected;
            item.InspirationMin = search.ItemMin;
            item.InspirationMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.SanctumMaxResolve);
        if (search is not null)
        {
            item.ChkMaxResolve = search.Selected;
            item.MaxResolveMin = search.ItemMin;
            item.MaxResolveMax = search.ItemMax;
        }
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.SanctumResolve);
        if (search is not null)
        {
            item.ChkResolve = search.Selected;
            item.ResolveMin = search.ItemMin;
            item.ResolveMax = search.ItemMax;
        }

        //pseudo
        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.TotalResistance);
        if (search is not null && search.Selected)
        {
            var useSlide = search.SlideValue is not ModFilter.EMPTYFIELD;
            var filter = useSlide ? 
                new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.TotalResistance, 
                search.SlideValue, search.Max)
                : new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.TotalResistance, 
                search.Min, search.Max);
            if (filter.Id.Length > 0)
            {
                item.ItemFilters.Add(filter);
            }
        }

        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.TotalLife);
        if (search is not null && search.Selected)
        {
            var useSlide = search.SlideValue is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.TotalLife,
                search.SlideValue, search.Max)
                : new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.TotalLife,
                search.Min, search.Max);
            if (filter.Id.Length > 0)
            {
                item.ItemFilters.Add(filter);
            }
        }

        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.TotalGlobalEs);
        if (search is not null && search.Selected)
        {
            var useSlide = search.SlideValue is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.TotalEs,
                search.SlideValue, search.Max)
                : new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.TotalEs,
                search.Min, search.Max);
            if (filter.Id.Length > 0)
            {
                item.ItemFilters.Add(filter);
            }
        }

        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.MapMoreScarab);
        if (search is not null && search.Selected)
        {
            var useSlide = search.SlideValue is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.MoreScarab,
                search.SlideValue, search.Max)
                : new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.MoreScarab,
                search.Min, search.Max);
            if (filter.Id.Length > 0)
            {
                item.ItemFilters.Add(filter);
            }
        }

        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.MapMoreCurrency);
        if (search is not null && search.Selected)
        {
            var useSlide = search.SlideValue is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.MoreCurrency,
                search.SlideValue, search.Max)
                : new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.MoreCurrency,
                search.Min, search.Max);
            if (filter.Id.Length > 0)
            {
                item.ItemFilters.Add(filter);
            }
        }

        search = listPanel.FirstOrDefault(x => x.Id is StatPanel.MapMoreDivCard);
        if (search is not null && search.Selected)
        {
            var useSlide = search.SlideValue is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.MoreDivCard,
                search.SlideValue, search.Max)
                : new ItemFilter(_dm.Filter, Strings.Stat.Pseudo.MoreDivCard,
                search.Min, search.Max);
            if (filter.Id.Length > 0)
            {
                item.ItemFilters.Add(filter);
            }
        }
        /*
        if (Panel.Map.MoreMap.Selected) // always false, not in view intentionally
        {
            var useSlide = Panel.Map.MoreMap.MinSlide is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter("pseudo.pseudo_map_more_map_drops",
                Panel.Map.MoreMap.MinSlide,
                Panel.Map.MoreMap.Max)
                : new ItemFilter("pseudo.pseudo_map_more_map_drops", 
                Panel.Map.MoreMap.Min, 
                Panel.Map.MoreMap.Max);
            if (filter.Id.Length > 0) // More Maps: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }
        */
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

        List<string> listInfluence = new();

        if (Influence.Shaper)
        {
            listInfluence.Add(Strings.Stat.Influence.Shaper);
        }
        if (Influence.Elder)
        {
            listInfluence.Add(Strings.Stat.Influence.Elder);
        }
        if (Influence.Crusader)
        {
            listInfluence.Add(Strings.Stat.Influence.Crusader);
        }
        if (Influence.Redeemer)
        {
            listInfluence.Add(Strings.Stat.Influence.Redeemer);
        }
        if (Influence.Hunter)
        {
            listInfluence.Add(Strings.Stat.Influence.Hunter);
        }
        if (Influence.Warlord)
        {
            listInfluence.Add(Strings.Stat.Influence.Warlord);
        }

        if (listInfluence.Count > 0)
        {
            foreach (string influence in listInfluence)
            {
                var filter = new ItemFilter(_dm.Filter, "pseudo." + influence, ModFilter.EMPTYFIELD, ModFilter.EMPTYFIELD);
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

        foreach (var resultDat in _dm.Currencies)
        {
            bool runLoop = true;

            if (category is Strings.Maps)
            {
                string mapKind = tier.Replace("T", string.Empty);
                mapKind = mapKind is Strings.Blight or Strings.Ravaged ?
                    Strings.CurrencyTypePoe1.MapsBlighted : Strings.CurrencyTypePoe1.Maps;
                if (resultDat.Id != mapKind)
                {
                    runLoop = false;
                }
            }

            if (runLoop)
            {
                foreach (var entrieDat in resultDat.Entries)
                {
                    if (entrieDat.Text == currency)
                    {
                        return entrieDat.Id;
                    }
                }
            }
        }
        return null;
    }

    internal ItemData FillModList(string[] clipData)
    {
        bool isPoe2 = _dm.Config.Options.GameVersion is 1;
        var item = new ItemData(_dm, clipData);

        if (!item.Flag.ShowDetail || item.Flag.Gems || item.Flag.SanctumResearch || item.Flag.TrialCoins
            || item.Flag.AllflameEmber || item.Flag.Corpses || item.Flag.UncutGem)
        {
            for (int i = 1; i < clipData.Length; i++)
            {
                var data = clipData[i].Trim().Split(Strings.CRLF, StringSplitOptions.None);
                var sameReward = data.Where(x => x.StartWith(Resources.Resources.General098_DeliriumReward));
                if (sameReward.Any())
                {
                    data = data.Distinct().ToArray();
                }

                if (item.Flag.SanctumResearch && i == clipData.Length - 1) // at the last loop
                {
                    var sanctumMods = item.GetSanctumMods();
                    if (sanctumMods.Length > 0)
                    {
                        Array.Resize(ref data, data.Length + sanctumMods.Length);
                        Array.Copy(sanctumMods, 0, data, data.Length - sanctumMods.Length, sanctumMods.Length);
                    }
                }

                var lSubMods = GetModsFromData(data, item);
                var flaskHeaderMods = (item.Flag.Flask || (item.Flag.Charm && isPoe2)) && i is 1;
                if (lSubMods.Any() && !flaskHeaderMods)
                {
                    foreach (var submod in lSubMods)
                    {
                        ModList.Add(submod);
                    }
                }
            }
        }

        return item;
    }

    internal async Task SelectExchangeCurrency(string args, string currency, string tier = null)
    {
        var arg = args.Split('/');
        //bool search = false;
        if (arg[0] is not "pay" and not "get" and not "shop")
        {
            return;
        }
        IEnumerable<(string, string, string Text)> cur;
        if (arg.Length > 1 && arg[1] is "contains")
        {
            var curKeys = currency.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            cur = _dm.Currencies
                .SelectMany(result => result.Entries, (result, entrie) => new { result.Id, Entrie = entrie })
                .Where(x => x.Entrie.Id is not Strings.sep &&
                            curKeys.All(key =>
                                x.Entrie.Text.ToLowerInvariant().Contain(key) ||
                                x.Entrie.Id.ToLowerInvariant().Contain(key)))
                .Select(x => (x.Id, x.Entrie.Id, x.Entrie.Text));
        }
        else
        {
            cur = _dm.Currencies
                .SelectMany(result => result.Entries, (result, entrie) => new { result.Id, Entrie = entrie })
                .Where(x => x.Entrie.Id is not Strings.sep && x.Entrie.Text == currency)
                .Select(x => (x.Id, x.Entrie.Id, x.Entrie.Text));
        }

        if (!cur.Any())
        {
            return;
        }

        string curClass = cur.First().Item1;
        string curId = cur.First().Item2;
        string curText = cur.First().Text;

        string selectedCurrency = string.Empty, selectedTier = string.Empty;
        string selectedCategory = Strings.GetBulkCategory(curClass, curId);

        if (selectedCategory.Length is 0)
        {
            return;
        }

        selectedCurrency = curText;

        if (selectedCategory == Resources.Resources.Main055_Divination)
        {
            var tmpDiv = _dm.DivTiers.FirstOrDefault(x => x.Tag == curId);
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
                var match = RegexUtil.DecimalNoPlusPattern().Matches(curText);
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

        int idxCur = bulk.Currency.IndexOf(selectedCurrency);
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

    internal bool UpdateModList(ItemData item)
    {
        bool isSocketUnmodifiable = false;
        var opt = _dm.Config.Options;
        foreach (var modLine in ModList)
        {
            var firstAffix = modLine.Affix[0];
            if (firstAffix is null) continue;

            var affix = modLine.Affix[modLine.AffixIndex];
            var affixName = modLine.Affix[modLine.AffixIndex]?.Name;
            var filter = modLine.ItemFilter;

            string englishMod = modLine.Mod;
            if (item.Lang is not Lang.English)
            {
                var enEntry = _dm.FilterEn.Result.SelectMany(result => result.Entries)
                    .FirstOrDefault(e => e.ID == firstAffix.ID);
                if (enEntry is not null)
                {
                    englishMod = enEntry.Text;
                }
            }
            bool condLife = opt.AutoSelectLife
                && !item.Flag.Unique && TotalStats.IsTotalStat(englishMod, Stat.Life)
                && !englishMod.ToLowerInvariant().Contain(Strings.Words.ToStrength);
            bool condEs = opt.AutoSelectGlobalEs //&& !item.IsPoe2
                && !item.Flag.Unique && TotalStats.IsTotalStat(englishMod, Stat.Es) && !item.Flag.ArmourPiece;
            bool condRes = opt.AutoSelectRes
                && !item.Flag.Unique && TotalStats.IsTotalStat(englishMod, Stat.Resist);

            bool implicitRegular = affixName == Resources.Resources.General013_Implicit;
            bool implicitCorrupt = affixName == Resources.Resources.General017_CorruptImp;
            bool implicitEnch = affixName == Resources.Resources.General011_Enchant;
            bool implicitScourge = affixName == Resources.Resources.General099_Scourge;

            if (implicitScourge) // Temporary
            {
                modLine.Selected = false;
                modLine.ItemFilter.Disabled = true;
            }

            if (implicitRegular || implicitCorrupt || implicitEnch)
            {
                bool condImpAuto = opt.AutoCheckImplicits && implicitRegular;
                bool condCorruptAuto = opt.AutoCheckCorruptions && implicitCorrupt;
                bool condEnchAuto = opt.AutoCheckEnchants && implicitEnch;

                bool specialImp = false;
                if (affix is not null)
                {
                    specialImp = Strings.Stat.lSpecialImplicits.Contains(affix.ID)
                        || ((item.Flag.Amulets || item.Flag.Rings) 
                        && Strings.Stat.lMagnitudeImplicits.Contains(affix.ID));
                }

                if ((condImpAuto || condCorruptAuto || condEnchAuto)
                    && !condLife && !condEs && !condRes || specialImp || IsInfluenced(filter.Id))
                {
                    modLine.Selected = true;
                    modLine.ItemFilter.Disabled = false;
                }
                if (filter.Id is Strings.Stat.Option.MapOccupConq)
                {
                    item.IsConqMap = true;
                }
            }

            if (opt.AutoCheckUniques && item.Flag.Unique || opt.AutoCheckNonUniques && !item.Flag.Unique)
            {
                bool isLogbookRare = IsLogbookRareMod(filter.Id);
                bool isCrafted = filter.Id.Contain(Strings.Stat.Generic.Crafted);
                if (modLine.AffixIndex >= 0)
                {
                    isCrafted = isCrafted || affixName == Resources.Resources.General012_Crafted
                        && !opt.AutoCheckCrafted;
                }
                if (isCrafted || item.Flag.Logbook && !isLogbookRare)
                {
                    modLine.Selected = false;
                    modLine.ItemFilter.Disabled = true;
                }
                else if (!item.Flag.Invitation && !item.Flag.Map && !item.Flag.Waystones
                    && !isCrafted && !condLife && !condEs && !condRes)
                {
                    bool isChronicleRare = item.Flag.Chronicle && IsChronicleRoom(firstAffix.ID);
                    bool isTabletRare = item.Flag.MirroredTablet && IsTabletRoom(firstAffix.ID);
                    bool unselectPoe2Mod = item.IsPoe2 && ShouldUnselectPoe2Mods(item, firstAffix.ID);

                    if (!implicitRegular && !implicitCorrupt && !implicitEnch && !implicitScourge && !unselectPoe2Mod
                        && (!item.Flag.Chronicle && !item.Flag.Ultimatum && !item.Flag.MirroredTablet
                        || isChronicleRare || isTabletRare))
                    {
                        modLine.Selected = true;
                        modLine.ItemFilter.Disabled = false;
                    }
                    // temp: Maligaro fix until GGG add filter for shock duration
                    if (item.Flag.Unique && item.Flag.Belts && firstAffix.ID is Strings.Stat.StunOnYou)
                    {
                        modLine.Selected = false;
                    }
                }
            }

            UpdateDangerousAndRareMods(item, modLine, affix);

            if (modLine.Selected)
            {
                if (item.Flag.Unique)
                {
                    modLine.AffixCanBeEnabled = false;
                }
                else
                {
                    modLine.AffixEnable = true;
                }
            }
            if (!isSocketUnmodifiable)
            {
                isSocketUnmodifiable = firstAffix.ID.Contain(Strings.Stat.SocketsUnmodifiable);
            }
        }
        return isSocketUnmodifiable;
    }

    private void UpdateDangerousAndRareMods(ItemData item, ModLineViewModel modLine, AffixFilterEntrie affix)
    {
        var idStat = affix.ID.Split('.');
        if (idStat.Length is 2)
        {
            if (item.Flag.Map &&
                _dm.Config.DangerousMapMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
            {
                modLine.ModKind = Strings.ModKind.DangerousMod;
            }
            if (!item.Flag.Map &&
                _dm.Config.RareItemMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
            {
                modLine.ModKind = Strings.ModKind.RareMod;
            }
        }
    }

    private static bool IsInfluenced(string filterId)
    {
        return filterId is Strings.Stat.Option.MapOccupConq
            or Strings.Stat.Option.MapOccupElder
            or Strings.Stat.Option.AreaInflu
            or Strings.Stat.AreaInfluOrigin;
    }

    private static bool IsLogbookRareMod(string id)
    {
        return id.Contain(Strings.Stat.Generic.LogbookBoss)
            || id.Contain(Strings.Stat.Generic.LogbookArea)
            || id.Contain(Strings.Stat.Generic.LogbookTwice);
    }

    private static bool IsChronicleRoom(string id) =>
        id.Contain(Strings.Stat.Temple.Room01) // Apex of Atzoatl
        || id.Contain(Strings.Stat.Temple.Room11) // Doryani's Institute
        || id.Contain(Strings.Stat.Temple.Room15) // Apex of Ascension
        || id.Contain(Strings.Stat.Temple.Room17); // Locus of Corruption

    private static bool IsTabletRoom(string id) =>
        id.Contain(Strings.Stat.Lake.Tablet01) // Paradise
        || id.Contain(Strings.Stat.Lake.Tablet02) // Kalandra
        || id.Contain(Strings.Stat.Lake.Tablet03) // the Sun
        || id.Contain(Strings.Stat.Lake.Tablet04); // Angling

    private bool ShouldUnselectPoe2Mods(ItemData item, string id)
    {
        var opt = _dm.Config.Options;
        var idSplit = id.Split('.');
        if (idSplit.Length < 2) return false;

        return (opt.AutoSelectArEsEva && item.Flag.ArmourPiece && Strings.StatPoe2.lDefenceMods.Contains(idSplit[1]))
            || (opt.AutoSelectDps && item.Flag.Weapon && Strings.StatPoe2.lWeaponMods.Contains(idSplit[1]));
    }

    private AsyncObservableCollection<ModLineViewModel> GetModsFromData(string[] data, ItemData item)
    {
        var lMods = new AsyncObservableCollection<ModLineViewModel>();
        var modDesc = new ModDescription();

        for (int j = 0; j < data.Length; j++)
        {
            if (data[j].Trim().Length is 0)
            {
                continue;
            }

            var affix = new AffixFlag(data[j]);
            if (item.UpdateOption(affix.ParsedData, lMods.Count < NB_MAX_MODS))
            {
                continue;
            }

            bool impLogbook = item.Flag.Logbook && affix.Implicit;
            var desc = new ModDescription(affix.ParsedData, impLogbook);
            if (desc.IsParsed)
            {
                modDesc = desc;
                continue;
            }

            var nextMod = (j + 1 < data.Length) && data[j + 1].Length > 0 ?
                RegexUtil.DecimalPattern().Replace(data[j + 1], "#") : string.Empty;
            var modifier = new ItemModifier(_dm, item, affix.ParsedData, modDesc.Name, nextMod);
            var modFilter = new ModFilter(_dm, modifier, item);
            if (!modFilter.IsFetched)
            {
                continue;
            }

            var mod = new ModLineViewModel(_dm, item, modFilter, affix, modDesc, _showMinMax);

            if (!item.Flag.Unique && !item.Flag.Jewel)
            {
                item.Stats.Fill(_dm.FilterEn, modFilter, item, mod.Current);
            }

            UpdateModValue(item, mod);

            item.UpdateTotalIncPhys(modFilter, mod.ItemFilter.Min);

            lMods.Add(mod);
        }
        return MergeSamePrefixMods(lMods);
    }

    private void UpdateModValue(ItemData item, ModLineViewModel mod)
    {
        if (item.Flag.Unique && item.Flag.Belts && mod.CurrentSlide is not ModFilter.EMPTYFIELD
            && _dm.Words.FirstOrDefault(x => x.NameEn is Strings.Unique.StringOfServitude).Name == item.Name)
        {
            var tripledVal = mod.CurrentSlide * 3;
            mod.Current = mod.Min = tripledVal.ToString();
            mod.CurrentSlide = tripledVal;
        }
    }

    private static AsyncObservableCollection<ModLineViewModel> MergeSamePrefixMods(AsyncObservableCollection<ModLineViewModel> listMod)
    {
        if (listMod.Count <= 1)
        {
            return listMod;
        }

        var duplicatesIdList = listMod.Where(g => g.TierKind is Strings.TierKind.Prefix).GroupBy(t => t.ItemFilter.Id)
            .Where(g => g.Count() > 1).Select(g => g.Key);
        if (!duplicatesIdList.Any())
        {
            return listMod;
        }

        bool aborted = false;
        var groupedDuplicates = listMod.Where(g => g.TierKind is Strings.TierKind.Prefix)
            .GroupBy(t => t.ItemFilter.Id).Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.ToList());
        var mergedDupList = groupedDuplicates.Select(kvp =>
            {
                var modList = kvp.Value;
                var mod = modList[0];
                mod.Tier = string.Join("+", modList.Select(i => i.Tier).Distinct());
                var abort = mod.Max.Length > 0 || mod.Mod.Count(i => i is '#') is not 1;
                if (abort)
                {
                    aborted = true;
                }
                if (mod.CurrentSlide > 0 && !abort)
                {
                    mod.CurrentSlide = modList.Sum(i => i.Current.ToDoubleDefault());
                    mod.Current = mod.CurrentSlide.ToString();
                    mod.SlideValue = mod.CurrentSlide;
                    mod.Min = mod.Current;
                    if (mod.TierMin.IsNotEmpty() && mod.TierMax.IsNotEmpty() && mod.TierTip.Count > 1)
                    {
                        mod.TierMin = modList.Sum(i => i.TierMin);
                        mod.TierMax = modList.Sum(i => i.TierMax);
                        var range = Math.Truncate(mod.TierMin) + "-" + Math.Truncate(mod.TierMax);
                        mod.TierTip[0].Text = range;
                        mod.ModBis = mod.ModBisTooltip = mod.Mod.ReplaceFirst("#", "(" + range + ")");
                    }
                    else
                    {
                        mod.ModBis = mod.ModBisTooltip = mod.Mod.ReplaceFirst("#", mod.Min);
                    }
                }
                return mod;
            }).ToList();
        
        if (!aborted && mergedDupList.Count > 0 && mergedDupList.Count == duplicatesIdList.Count())
        {
            return new AsyncObservableCollection<ModLineViewModel>
                (mergedDupList.Concat(listMod.Where(i => !duplicatesIdList.Contains(i.ItemFilter.Id))));
        }

        return listMod;
    }
}

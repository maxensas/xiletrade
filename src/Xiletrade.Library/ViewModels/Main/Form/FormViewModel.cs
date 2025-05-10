using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main.Exchange;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class FormViewModel(bool useBulk) : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private static bool _showMinMax;

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
    private PanelViewModel panel = new();

    [ObservableProperty]
    private AsyncObservableCollection<ModLineViewModel> modList = new();

    [ObservableProperty]
    private AsyncObservableCollection<string> corruption = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private AsyncObservableCollection<string> alternate = new() { Resources.Resources.General005_Any, Resources.Resources.General001_Anomalous, Resources.Resources.General002_Divergent,
        Resources.Resources.General003_Phantasmal }; // obsolete

    [ObservableProperty]
    private AsyncObservableCollection<string> market = new() { "online", Strings.any };

    [ObservableProperty]
    private int marketIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> league = DataManager.GetLeagueAsyncCollection();

    [ObservableProperty]
    private int leagueIndex = DataManager.GetDefaultLeagueIndex();

    [ObservableProperty]
    private InfluenceViewModel influence = new();

    [ObservableProperty]
    private ConditionViewModel condition = new();

    [ObservableProperty]
    private TabViewModel tab = new(useBulk);

    [ObservableProperty]
    private VisibilityViewModel visible = new(useBulk);

    [ObservableProperty]
    private BulkViewModel bulk;

    [ObservableProperty]
    private ShopViewModel shop;

    [ObservableProperty]
    private RarityViewModel rarity = new();

    [ObservableProperty]
    private double opacity = DataManager.Config.Options.Opacity;

    [ObservableProperty]
    private string opacityText = DataManager.Config.Options.Opacity * 100 + "%";

    [ObservableProperty]
    private string fillTime = string.Empty;

    [ObservableProperty]
    private ExpanderViewModel expander = new();

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
    private bool sameUser = DataManager.Config.Options.HideSameOccurs;

    [ObservableProperty]
    private bool chaosDiv;

    [ObservableProperty]
    private bool exalt;

    [ObservableProperty]
    private bool autoClose = DataManager.Config.Options.Autoclose;

    [ObservableProperty]
    private bool isPoeTwo;

    [ObservableProperty]
    private bool isSelectionEnabled = true;

    public FormViewModel(IServiceProvider serviceProvider, bool useBulk) : this(useBulk)
    {
        _serviceProvider = serviceProvider;
        bulk = new(_serviceProvider);
        shop = new(_serviceProvider);
        isPoeTwo = _serviceProvider.GetRequiredService<XiletradeService>().IsPoe2;
        _showMinMax = _serviceProvider.GetRequiredService<MainViewModel>().ShowMinMax;
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
            mod.MinSlide = mod.Current.ToDoubleDefault();
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
                mod.MinSlide = val;
                continue;
            }
            string[] range = mod.TierTip[0].Text.Split("-");
            if (range.Length is 2)
            {
                mod.Min = range[0];
                mod.MinSlide = range[0].ToDoubleEmptyField();
                continue;
            }
            if (range.Length is 3 or 4)
            {
                if (range[0].Length > 0)
                {
                    mod.Min = range[0];
                    mod.MinSlide = range[0].ToDoubleEmptyField();
                    continue;
                }
                if (range[1].Length > 0 && !range[1].Contain('+'))
                {
                    mod.Min = "-" + range[1];
                    mod.MinSlide = - range[1].ToDoubleEmptyField();
                    continue;
                }
            }
            mod.Min = mod.Current;
            mod.MinSlide = mod.Current.ToDoubleEmptyField();
        }
    }

    internal XiletradeItem GetXiletradeItem()
    {
        var xiletradeItem = new XiletradeItem()
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
            ChkSocket = Panel.Common.Sockets.Selected,
            ChkQuality = Panel.Common.Quality.Selected,
            ChkLv = Panel.Common.ItemLevel.Selected,
            ByType = ByBase != true,
            ChkArmour = Panel.Defense.Armour.Selected,
            ChkEnergy = Panel.Defense.Energy.Selected,
            ChkEvasion = Panel.Defense.Evasion.Selected,
            ChkWard = Panel.Defense.Ward.Selected,
            ChkDpsTotal = Panel.Damage.Total.Selected,
            ChkDpsPhys = Panel.Damage.Physical.Selected,
            ChkDpsElem = Panel.Damage.Elemental.Selected,

            ChkResolve = Panel.Sanctum.Resolve.Selected,
            ChkMaxResolve = Panel.Sanctum.MaximumResolve.Selected,
            ChkInspiration = Panel.Sanctum.Inspiration.Selected,
            ChkAureus = Panel.Sanctum.Aureus.Selected,

            ChkMapIiq = Panel.Map.Quantity.Selected,
            ChkMapIir = Panel.Map.Rarity.Selected,
            ChkMapPack = Panel.Map.PackSize.Selected,
            ChkMapScarab = Panel.Map.MoreScarab.Selected,
            ChkMapCurrency = Panel.Map.MoreCurrency.Selected,
            ChkMapDivCard = Panel.Map.MoreDivCard.Selected,

            ChkRuneSockets = Panel.Common.RuneSockets.Selected,

            RewardType = Panel.Reward.Tip.Length > 0 ? Panel.Reward.Tip : null,
            Reward = Panel.Reward.Text.Length > 0 ? Panel.Reward.Text : null,
            ChaosDivOnly = ChaosDiv,
            ExaltOnly = Exalt,
            Rarity = Rarity.Index >= 0 && Rarity.Index < Rarity.ComboBox.Count ?
                Rarity.ComboBox[Rarity.Index] : Rarity.Item,

            PriceMin = 0, // not used

            SocketColors = Condition.SocketColors,

            SocketRed = Panel.Common.Sockets.RedColor.ToDoubleEmptyField(),
            SocketGreen = Panel.Common.Sockets.GreenColor.ToDoubleEmptyField(),
            SocketBlue = Panel.Common.Sockets.BlueColor.ToDoubleEmptyField(),
            SocketWhite = Panel.Common.Sockets.WhiteColor.ToDoubleEmptyField(),
            SocketMin = Panel.Common.Sockets.SocketMin.ToDoubleEmptyField(),
            SocketMax = Panel.Common.Sockets.SocketMax.ToDoubleEmptyField(),
            LinkMin = Panel.Common.Sockets.LinkMin.ToDoubleEmptyField(),
            LinkMax = Panel.Common.Sockets.LinkMax.ToDoubleEmptyField(),
            RuneSocketsMin = Panel.Common.RuneSockets.Min.ToDoubleEmptyField(),
            RuneSocketsMax = Panel.Common.RuneSockets.Max.ToDoubleEmptyField(),
            FacetorExpMin = Panel.FacetorMin.ToDoubleEmptyField(),
            FacetorExpMax = Panel.FacetorMax.ToDoubleEmptyField(),

            // using slider
            QualityMin = Panel.Common.Quality.MinSlide is ModFilter.EMPTYFIELD 
            ? Panel.Common.Quality.Min.ToDoubleEmptyField()
            : Panel.Common.Quality.MinSlide,
            QualityMax = Panel.Common.Quality.Max.ToDoubleEmptyField(),

            LvMin = Panel.Common.ItemLevel.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Common.ItemLevel.Min.ToDoubleEmptyField()
            : Panel.Common.ItemLevel.MinSlide,
            LvMax = Panel.Common.ItemLevel.Max.ToDoubleEmptyField(),

            ArmourMin = Panel.Defense.Armour.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Defense.Armour.Min.ToDoubleEmptyField()
            : Panel.Defense.Armour.MinSlide,
            ArmourMax = Panel.Defense.Armour.Max.ToDoubleEmptyField(),

            EnergyMin = Panel.Defense.Energy.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Defense.Energy.Min.ToDoubleEmptyField()
            : Panel.Defense.Energy.MinSlide,
            EnergyMax = Panel.Defense.Energy.Max.ToDoubleEmptyField(),

            EvasionMin = Panel.Defense.Evasion.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Defense.Evasion.Min.ToDoubleEmptyField()
            : Panel.Defense.Evasion.MinSlide,
            EvasionMax = Panel.Defense.Evasion.Max.ToDoubleEmptyField(),

            WardMin = Panel.Defense.Ward.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Defense.Evasion.Min.ToDoubleEmptyField()
            : Panel.Defense.Ward.MinSlide,
            WardMax = Panel.Defense.Ward.Max.ToDoubleEmptyField(),

            DpsTotalMin = Panel.Damage.Total.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Damage.Total.Min.ToDoubleEmptyField()
            : Panel.Damage.Total.MinSlide,
            DpsTotalMax = Panel.Damage.Total.Max.ToDoubleEmptyField(),

            DpsPhysMin = Panel.Damage.Physical.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Damage.Physical.Min.ToDoubleEmptyField()
            : Panel.Damage.Physical.MinSlide,
            DpsPhysMax = Panel.Damage.Physical.Max.ToDoubleEmptyField(),

            DpsElemMin = Panel.Damage.Elemental.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Damage.Elemental.Min.ToDoubleEmptyField()
            : Panel.Damage.Elemental.MinSlide,
            DpsElemMax = Panel.Damage.Elemental.Max.ToDoubleEmptyField(),

            ResolveMin = Panel.Sanctum.Resolve.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Sanctum.Resolve.Min.ToDoubleEmptyField()
            : Panel.Sanctum.Resolve.MinSlide,
            ResolveMax = Panel.Sanctum.Resolve.Max.ToDoubleEmptyField(),

            MaxResolveMin = Panel.Sanctum.MaximumResolve.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Sanctum.MaximumResolve.Min.ToDoubleEmptyField()
            : Panel.Sanctum.MaximumResolve.MinSlide,
            MaxResolveMax = Panel.Sanctum.MaximumResolve.Max.ToDoubleEmptyField(),

            InspirationMin = Panel.Sanctum.Inspiration.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Sanctum.Inspiration.Min.ToDoubleEmptyField()
            : Panel.Sanctum.Inspiration.MinSlide,
            InspirationMax = Panel.Sanctum.Inspiration.Max.ToDoubleEmptyField(),

            AureusMin = Panel.Sanctum.Aureus.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Sanctum.Aureus.Min.ToDoubleEmptyField()
            : Panel.Sanctum.Aureus.MinSlide,
            AureusMax = Panel.Sanctum.Aureus.Max.ToDoubleEmptyField(),

            MapItemQuantityMin = Panel.Map.Quantity.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Map.Quantity.Min.ToDoubleEmptyField()
            : Panel.Map.Quantity.MinSlide,
            MapItemQuantityMax = Panel.Map.Quantity.Max.ToDoubleEmptyField(),

            MapItemRarityMin = Panel.Map.Rarity.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Map.Rarity.Min.ToDoubleEmptyField()
            : Panel.Map.Rarity.MinSlide,
            MapItemRarityMax = Panel.Map.Rarity.Max.ToDoubleEmptyField(),

            MapPackSizeMin = Panel.Map.PackSize.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Map.PackSize.Min.ToDoubleEmptyField()
            : Panel.Map.PackSize.MinSlide,
            MapPackSizeMax = Panel.Map.PackSize.Max.ToDoubleEmptyField(),

            MapMoreScarabMin = Panel.Map.MoreScarab.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Map.MoreScarab.Min.ToDoubleEmptyField()
            : Panel.Map.MoreScarab.MinSlide,
            MapMoreScarabMax = Panel.Map.MoreScarab.Max.ToDoubleEmptyField(),

            MapMoreCurrencyMin = Panel.Map.MoreCurrency.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Map.MoreCurrency.Min.ToDoubleEmptyField()
            : Panel.Map.MoreCurrency.MinSlide,
            MapMoreCurrencyMax = Panel.Map.MoreCurrency.Max.ToDoubleEmptyField(),

            MapMoreDivCardMin = Panel.Map.MoreDivCard.MinSlide is ModFilter.EMPTYFIELD
            ? Panel.Map.MoreDivCard.Min.ToDoubleEmptyField()
            : Panel.Map.MoreDivCard.MinSlide,
            MapMoreDivCardMax = Panel.Map.MoreDivCard.Max.ToDoubleEmptyField()
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
                    double minValue = mod.PreferMinMax ? mod.Min.ToDoubleEmptyField() : mod.MinSlide;
                    double maxValue = mod.Max.ToDoubleEmptyField();

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
                    xiletradeItem.ItemFilters.Add(itemFilter);
                    if (modLimit >= NB_MAX_MODS)
                    {
                        break;
                    }
                    modLimit++;
                }
            }
        }

        if (Panel.Total.Resistance.Selected)
        {
            var useSlide = Panel.Total.Resistance.MinSlide is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter("pseudo.pseudo_total_resistance",
                Panel.Total.Resistance.MinSlide,
                Panel.Total.Resistance.Max)
                : new ItemFilter("pseudo.pseudo_total_resistance",
                Panel.Total.Resistance.Min, 
                Panel.Total.Resistance.Max);
            if (filter.Id.Length > 0) // +#% total Resistance
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Total.Life.Selected)
        {
            var useSlide = Panel.Total.Life.MinSlide is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter("pseudo.pseudo_total_life",
                Panel.Total.Life.MinSlide,
                Panel.Total.Life.Max)
                : new ItemFilter("pseudo.pseudo_total_life", 
                Panel.Total.Life.Min, 
                Panel.Total.Life.Max);
            if (filter.Id.Length > 0) // +# total maximum Life
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }
        if (Panel.Total.GlobalEs.Selected)
        {
            var useSlide = Panel.Total.GlobalEs.MinSlide is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter("pseudo.pseudo_total_energy_shield",
                Panel.Total.GlobalEs.MinSlide,
                Panel.Total.GlobalEs.Max)
                : new ItemFilter("pseudo.pseudo_total_energy_shield", 
                Panel.Total.GlobalEs.Min, 
                Panel.Total.GlobalEs.Max);
            if (filter.Id.Length > 0) // # to maximum Energy Shield
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreScarab.Selected)
        {
            var useSlide = Panel.Map.MoreScarab.MinSlide is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter("pseudo.pseudo_map_more_scarab_drops",
                Panel.Map.MoreScarab.MinSlide,
                Panel.Map.MoreScarab.Max)
                : new ItemFilter("pseudo.pseudo_map_more_scarab_drops", 
                Panel.Map.MoreScarab.Min, 
                Panel.Map.MoreScarab.Max);
            if (filter.Id.Length > 0) // More Scarabs: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreCurrency.Selected)
        {
            var useSlide = Panel.Map.MoreCurrency.MinSlide is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter("pseudo.pseudo_map_more_currency_drops",
                Panel.Map.MoreCurrency.MinSlide,
                Panel.Map.MoreCurrency.Max)
                : new ItemFilter("pseudo.pseudo_map_more_currency_drops", 
                Panel.Map.MoreCurrency.Min, 
                Panel.Map.MoreCurrency.Max);
            if (filter.Id.Length > 0) // More Currency: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreDivCard.Selected)
        {
            var useSlide = Panel.Map.MoreDivCard.MinSlide is not ModFilter.EMPTYFIELD;
            var filter = useSlide ?
                new ItemFilter("pseudo.pseudo_map_more_card_drops",
                Panel.Map.MoreDivCard.MinSlide,
                Panel.Map.MoreDivCard.Max)
                : new ItemFilter("pseudo.pseudo_map_more_card_drops", 
                Panel.Map.MoreDivCard.Min, 
                Panel.Map.MoreDivCard.Max);
            if (filter.Id.Length > 0) // More Divination Cards: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

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

        if (Condition.FreePrefix)
        {
            var filter = new ItemFilter("pseudo.pseudo_number_of_empty_prefix_mods", 1, ModFilter.EMPTYFIELD);
            if (filter.Id.Length > 0) // # Empty Prefix Modifiers
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Condition.FreeSuffix)
        {
            var filter = new ItemFilter("pseudo.pseudo_number_of_empty_suffix_mods", 1, ModFilter.EMPTYFIELD);
            if (filter.Id.Length > 0) // # Empty Suffix Modifiers
            {
                xiletradeItem.ItemFilters.Add(filter);
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
                var filter = new ItemFilter("pseudo." + influence, ModFilter.EMPTYFIELD, ModFilter.EMPTYFIELD);
                if (filter.Id.Length > 0)
                {
                    xiletradeItem.ItemFilters.Add(filter);
                }
            }
        }

        return xiletradeItem;
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

        foreach (var resultDat in DataManager.Currencies)
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
        var lang = (Lang)DataManager.Config.Options.Language;
        bool isPoe2 = DataManager.Config.Options.GameVersion is 1;
        var item = new ItemData(clipData, lang, isPoe2);

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

        Panel.Update(item);

        return item;
    }

    internal void SelectExchangeCurrency(string args, string currency, string tier = null)
    {
        var arg = args.Split('/');
        bool search = false;
        if (arg[0] is not "pay" and not "get" and not "shop")
        {
            return;
        }
        IEnumerable<(string, string, string Text)> cur;
        if (arg.Length > 1 && arg[1] is "contains") // contains requests to improve
        {
            search = true;
            var curKeys = currency.ToLowerInvariant().Split(' ');
            if (curKeys.Length >= 3)
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contain(curKeys[0])
                || Entrie.Id.ToLowerInvariant().Contain(curKeys[0]))
                && (Entrie.Text.ToLowerInvariant().Contain(curKeys[1])
                || Entrie.Id.ToLowerInvariant().Contain(curKeys[1]))
                && (Entrie.Text.ToLowerInvariant().Contain(curKeys[2])
                || Entrie.Id.ToLowerInvariant().Contain(curKeys[2]))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
            else if (curKeys.Length is 2)
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contain(curKeys[0])
                || Entrie.Id.ToLowerInvariant().Contain(curKeys[0]))
                && (Entrie.Text.ToLowerInvariant().Contain(curKeys[1])
                || Entrie.Id.ToLowerInvariant().Contain(curKeys[1]))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
            else
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contain(curKeys[0])
                || Entrie.Id.ToLowerInvariant().Contain(curKeys[0]))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
        }
        else
        {
            cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep && Entrie.Text == currency
                select (result.Id, Entrie.Id, Entrie.Text);
        }

        if (!cur.Any())
        {
            return;
        }

        string curClass = cur.First().Item1;
        string curId = cur.First().Item2;
        string curText = cur.First().Text;

        string selectedCurrency = string.Empty, selectedTier = string.Empty;
        string selectedCategory = Strings.GetCategory(curClass, curId);

        if (selectedCategory.Length is 0)
        {
            return;
        }

        selectedCurrency = curText;

        if (selectedCategory == Resources.Resources.Main055_Divination)
        {
            var tmpDiv = DataManager.DivTiers.FirstOrDefault(x => x.Tag == curId);
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
        System.Threading.Tasks.Task.Run(async () =>
        {
            int watchdog = 0;
            // 2 seconds max
            while (bulk.Currency.Count is 0 && watchdog < 10)
            {
                bulk.CategoryIndex = -1;
                await System.Threading.Tasks.Task.Delay(100);
                bulk.CategoryIndex = idxCat;
                await System.Threading.Tasks.Task.Delay(100);
                watchdog++;
            }

            int idxCur = bulk.Currency.IndexOf(selectedCurrency);
            if (idxCur > -1)
            {
                bulk.CurrencyIndex = idxCur;
            }
        });
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

    internal void UpdateModList(ItemData item)
    {
        for (int i = 0; i < ModList.Count; i++)
        {
            var filter = ModList[i].ItemFilter;

            string englishMod = ModList[i].Mod;
            if (item.Lang is not Lang.English)
            {
                var affix = ModList[i].Affix[0];
                if (affix is not null)
                {
                    var enResult =
                        from result in DataManager.FilterEn.Result
                        from Entrie in result.Entries
                        where Entrie.ID == affix.ID
                        select Entrie.Text;
                    if (enResult.Any())
                    {
                        englishMod = enResult.First();
                    }
                }
            }
            bool condLife = DataManager.Config.Options.AutoSelectLife && !item.IsPoe2
                && !item.Flag.Unique && TotalStats.IsTotalStat(englishMod, Stat.Life)
                && !englishMod.ToLowerInvariant().Contain("to strength");
            bool condEs = DataManager.Config.Options.AutoSelectGlobalEs && !item.IsPoe2
                && !item.Flag.Unique && TotalStats.IsTotalStat(englishMod, Stat.Es) && !item.Flag.ArmourPiece;
            bool condRes = DataManager.Config.Options.AutoSelectRes && !item.IsPoe2
                && !item.Flag.Unique && TotalStats.IsTotalStat(englishMod, Stat.Resist);
            bool implicitRegular = ModList[i].Affix[ModList[i].AffixIndex].Name == Resources.Resources.General013_Implicit;
            bool implicitCorrupt = ModList[i].Affix[ModList[i].AffixIndex].Name == Resources.Resources.General017_CorruptImp;
            bool implicitEnch = ModList[i].Affix[ModList[i].AffixIndex].Name == Resources.Resources.General011_Enchant;
            bool implicitScourge = ModList[i].Affix[ModList[i].AffixIndex].Name == Resources.Resources.General099_Scourge;

            if (implicitScourge) // Temporary
            {
                ModList[i].Selected = false;
                ModList[i].ItemFilter.Disabled = true;
            }

            if (implicitRegular || implicitCorrupt || implicitEnch)
            {
                bool condImpAuto = DataManager.Config.Options.AutoCheckImplicits && implicitRegular;
                bool condCorruptAuto = DataManager.Config.Options.AutoCheckCorruptions && implicitCorrupt;
                bool condEnchAuto = DataManager.Config.Options.AutoCheckEnchants && implicitEnch;

                bool specialImp = false;
                var affix = ModList[i].Affix[ModList[i].AffixIndex];
                if (affix is not null)
                {
                    specialImp = Strings.Stat.lSpecialImplicits.Contains(affix.ID);
                }

                if ((condImpAuto || condCorruptAuto || condEnchAuto) && !condLife && !condEs && !condRes || specialImp || filter.Id is Strings.Stat.MapOccupConq or Strings.Stat.MapOccupElder or Strings.Stat.AreaInflu)
                {
                    ModList[i].Selected = true;
                    ModList[i].ItemFilter.Disabled = false;
                }
                if (filter.Id is Strings.Stat.MapOccupConq)
                {
                    item.IsConqMap = true;
                }
            }

            if (DataManager.Config.Options.AutoCheckUniques && item.Flag.Unique 
                || DataManager.Config.Options.AutoCheckNonUniques && !item.Flag.Unique)
            {
                bool logbookRareMod = filter.Id.Contain(Strings.Stat.LogbookBoss)
                    || filter.Id.Contain(Strings.Stat.LogbookArea)
                    || filter.Id.Contain(Strings.Stat.LogbookTwice);
                bool craftedCond = filter.Id.Contain(Strings.Stat.Crafted);
                if (ModList[i].AffixIndex >= 0)
                {
                    craftedCond = craftedCond || ModList[i].Affix[ModList[i].AffixIndex].Name
                        == Resources.Resources.General012_Crafted && !DataManager.Config.Options.AutoCheckCrafted;
                }
                if (craftedCond || item.Flag.Logbook && !logbookRareMod)
                {
                    ModList[i].Selected = false;
                    ModList[i].ItemFilter.Disabled = true;
                }
                else if (!item.Flag.Invitation && !item.Flag.Map && !item.Flag.Waystones
                    && !craftedCond && !condLife && !condEs && !condRes)
                {
                    bool condChronicle = false, condMirroredTablet = false;
                    if (item.Flag.Chronicle)
                    {
                        var affix = ModList[i].Affix[0];
                        if (affix is not null)
                        {
                            condChronicle = affix.ID.Contain(Strings.Stat.Room01) // Apex of Atzoatl
                                || affix.ID.Contain(Strings.Stat.Room11) // Doryani's Institute
                                || affix.ID.Contain(Strings.Stat.Room15) // Apex of Ascension
                                || affix.ID.Contain(Strings.Stat.Room17); // Locus of Corruption
                        }
                    }
                    if (item.Flag.MirroredTablet)
                    {
                        var affix = ModList[i].Affix[0];
                        if (affix is not null)
                        {
                            condMirroredTablet = affix.ID.Contain(Strings.Stat.Tablet01) // Paradise
                                || affix.ID.Contain(Strings.Stat.Tablet02) // Kalandra
                                || affix.ID.Contain(Strings.Stat.Tablet03) // the Sun
                                || affix.ID.Contain(Strings.Stat.Tablet04); // Angling
                        }
                    }
                    var unselectPoe2Mod = item.IsPoe2 &&
                        ((DataManager.Config.Options.AutoSelectArEsEva && item.Flag.ArmourPiece)
                        || (DataManager.Config.Options.AutoSelectDps && item.Flag.Weapon));
                    if (unselectPoe2Mod)
                    {
                        var affix = ModList[i].Affix[0];
                        if (affix is not null)
                        {
                            var idSplit = affix.ID.Split('.');
                            if (idSplit.Length > 1)
                            {
                                unselectPoe2Mod = (DataManager.Config.Options.AutoSelectArEsEva && Strings.StatPoe2.lDefenceMods.Contains(idSplit[1]))
                                    || (DataManager.Config.Options.AutoSelectDps && Strings.StatPoe2.lWeaponMods.Contains(idSplit[1]));
                            }
                        }
                    }

                    //TOSIMPLIFY
                    if (!implicitRegular && !implicitCorrupt && !implicitEnch && !implicitScourge && !unselectPoe2Mod
                        && (!item.Flag.Chronicle && !item.Flag.Ultimatum && !item.Flag.MirroredTablet
                        || condChronicle || condMirroredTablet))
                    {
                        ModList[i].Selected = true;
                        ModList[i].ItemFilter.Disabled = false;
                    }
                }
            }

            var idStat = ModList[i].Affix[ModList[i].AffixIndex].ID.Split('.');
            if (idStat.Length is 2)
            {
                if (item.Flag.Map &&
                    DataManager.Config.DangerousMapMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                {
                    ModList[i].ModKind = Strings.ModKind.DangerousMod;
                }
                if (!item.Flag.Map &&
                    DataManager.Config.RareItemMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                {
                    ModList[i].ModKind = Strings.ModKind.RareMod;
                }
            }

            if (ModList[i].Selected)
            {
                if (item.Flag.Unique)
                {
                    ModList[i].AffixCanBeEnabled = false;
                }
                else
                {
                    ModList[i].AffixEnable = true;
                }
            }

            if (Panel.Common.Sockets.SocketMin is "6")
            {
                bool condColors = false;
                var affix = ModList[i].Affix[0];
                if (affix is not null)
                {
                    condColors = affix.ID.Contain(Strings.Stat.SocketsUnmodifiable);
                }
                if (condColors || Panel.Common.Sockets.WhiteColor is "6")
                {
                    Condition.SocketColors = true;
                    Panel.Common.Sockets.Selected = true;
                }
            }
        }
    }

    // private
    private static AsyncObservableCollection<ModLineViewModel> GetModsFromData(string[] data, ItemData item)
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
            var modifier = new ItemModifier(affix.ParsedData, nextMod, modDesc.Name, item);
            var modFilter = new ModFilter(modifier, item);
            if (!modFilter.IsFetched)
            {
                continue;
            }

            var mod = new ModLineViewModel(modFilter, affix, modDesc, _showMinMax);

            if (!item.Flag.Unique && !item.Flag.Jewel)
            {
                item.Stats.Fill(modFilter, mod.Current);
            }

            item.UpdateTotalIncPhys(modFilter, mod.ItemFilter.Min);

            lMods.Add(mod);
        }
        return lMods;
    }
}

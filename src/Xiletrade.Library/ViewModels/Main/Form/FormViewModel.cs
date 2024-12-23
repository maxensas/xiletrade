using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main.Exchange;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class FormViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string itemName;

    [ObservableProperty]
    private string itemNameColor = string.Empty;

    [ObservableProperty]
    private string itemBaseType = string.Empty;

    [ObservableProperty]
    private string itemBaseTypeColor = string.Empty;

    [ObservableProperty]
    private double baseTypeFontSize = 12; // FontSize cannot be equal to 0

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
    private AsyncObservableCollection<ModLineViewModel> modLine = new();

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
    private AsyncObservableCollection<string> league = new();

    [ObservableProperty]
    private int leagueIndex;

    [ObservableProperty]
    private InfluenceViewModel influence = new();

    [ObservableProperty]
    private ConditionViewModel condition = new();

    [ObservableProperty]
    private TabViewModel tab = new();

    [ObservableProperty]
    private VisibilityViewModel visible = new();

    [ObservableProperty]
    private BulkViewModel bulk;

    [ObservableProperty]
    private ShopViewModel shop;

    [ObservableProperty]
    private RarityViewModel rarity = new();

    [ObservableProperty]
    private double opacity = 1.0;

    [ObservableProperty]
    private string opacityText = string.Empty;

    [ObservableProperty]
    private string priceTime = string.Empty;

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
    private bool sameUser;

    [ObservableProperty]
    private bool chaosDiv;

    [ObservableProperty]
    private bool autoClose;

    [ObservableProperty]
    private bool isPoeTwo;

    public FormViewModel(IServiceProvider serviceProvider, bool useBulk = false)
    {
        _serviceProvider = serviceProvider;
        Bulk = new(_serviceProvider);
        Shop = new(_serviceProvider);
        // Init using data
        InitLeagues();
        Opacity = DataManager.Config.Options.Opacity;
        AutoClose = DataManager.Config.Options.Autoclose;
        CorruptedIndex = DataManager.Config.Options.AutoSelectCorrupt ? 2 : 0;
        SameUser = DataManager.Config.Options.HideSameOccurs;
        Visible.Poeprices = DataManager.Config.Options.Language is 0;

        OpacityText = Opacity * 100 + "%";

        if (useBulk)
        {
            ItemBaseType = Resources.Resources.Main032_cbTotalExchange;
            ItemBaseTypeColor = Strings.Color.Moccasin;
            Tab.BulkEnable = true;
            Tab.BulkSelected = true;
            Tab.ShopEnable = true;
            Tab.ShopSelected = false;
            Visible.Wiki = false;
            Visible.BtnPoeDb = false;
            ItemName = string.Empty;
            BaseTypeFontSize = 16;
        }
        IsPoeTwo = _serviceProvider.GetRequiredService<XiletradeService>().IsPoe2;
    }

    private void InitLeagues()
    {
        AsyncObservableCollection<string> listLeague = new();

        if (DataManager.League.Result.Length >= 2)
        {
            foreach (LeagueResult res in DataManager.League.Result)
            {
                listLeague.Add(res.Id);
            }
        }
        League = listLeague;
        int idx = listLeague.IndexOf(DataManager.Config.Options.League);
        LeagueIndex = idx > -1 ? idx : 0;
    }

    internal Dictionary<string, bool> GetInfluenceSate()
    {
        return new Dictionary<string, bool>() 
        {
            { Influence.ShaperText, Influence.Shaper },
            { Influence.ElderText, Influence.Elder },
            { Influence.CrusaderText, Influence.Crusader },
            { Influence.RedeemerText, Influence.Redeemer },
            { Influence.WarlordText, Influence.Warlord },
            { Influence.HunterText, Influence.Hunter }
        };
    }

    internal void SetModCurrent()
    {
        if (ModLine.Count <= 0)
        {
            return;
        }
        List<bool> sameText = new();
        bool remove = true;

        foreach (ModLineViewModel mod in ModLine)
        {
            sameText.Add(mod.Min == mod.Current);
            mod.Min = mod.Current;
        }

        foreach (bool same in sameText) remove &= same;
        if (!remove)
        {
            return;
        }
        foreach (ModLineViewModel mod in ModLine)
        {
            if (mod.Min.Length > 0)
            {
                mod.Min = string.Empty;
            }
        }
    }

    internal void SetModPercent()
    {
        if (ModLine.Count <= 0)
        {
            return;
        }
        foreach (ModLineViewModel mod in ModLine)
        {
            if (mod.Current.Length is 0)
            {
                continue;
            }
            mod.Min = (mod.Current.ToDoubleDefault() - (mod.Current.ToDoubleDefault() / 10)).ToString(CultureInfo.InvariantCulture);
        }
    }

    internal void SetModTier()
    {
        if (ModLine.Count <= 0)
        {
            return;
        }
        foreach (ModLineViewModel mod in ModLine)
        {
            if (mod.TierTip.Count <= 0)
            {
                continue;
            }
            if (Double.TryParse(mod.TierTip[0].Text, out double val))
            {
                mod.Min = val.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                continue;
            }
            string[] range = mod.TierTip[0].Text.Split("-");
            if (range.Length == 2)
            {
                mod.Min = range[0];
                continue;
            }
            if (range.Length is 3 or 4)
            {
                if (range[0].Length > 0)
                {
                    mod.Min = range[0];
                    continue;
                }
                if (range[1].Length > 0 && !range[1].Contains('+', StringComparison.Ordinal))
                {
                    mod.Min = "-" + range[1];
                    continue;
                }
            }
            mod.Min = mod.Current;
        }
    }

    internal XiletradeItem GetXiletradeItem()
    {
        XiletradeItem xiletradeItem = new()
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
            Scourged = Panel.Scourged,
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

            AlternateQuality = Panel.AlternateGemIndex switch
            {
                0 => null,
                1 => "1",
                2 => "2",
                3 => "3",
                _ => null,
            },
            RewardType = Panel.Reward.Tip.Length > 0 ? Panel.Reward.Tip : null,
            Reward = Panel.Reward.Text.Length > 0 ? Panel.Reward.Text : null,
            ChaosDivOnly = ChaosDiv,

            SocketColors = Condition.SocketColors,

            SocketRed = Panel.Common.Sockets.RedColor.ToDoubleEmptyField(),
            SocketGreen = Panel.Common.Sockets.GreenColor.ToDoubleEmptyField(),
            SocketBlue = Panel.Common.Sockets.BlueColor.ToDoubleEmptyField(),
            SocketWhite = Panel.Common.Sockets.WhiteColor.ToDoubleEmptyField(),
            SocketMin = Panel.Common.Sockets.SocketMin.ToDoubleEmptyField(),
            SocketMax = Panel.Common.Sockets.SocketMax.ToDoubleEmptyField(),
            LinkMin = Panel.Common.Sockets.LinkMin.ToDoubleEmptyField(),
            LinkMax = Panel.Common.Sockets.LinkMax.ToDoubleEmptyField(),
            QualityMin = Panel.Common.Quality.Min.ToDoubleEmptyField(),
            QualityMax = Panel.Common.Quality.Max.ToDoubleEmptyField(),
            LvMin = Panel.Common.ItemLevel.Min.ToDoubleEmptyField(),
            LvMax = Panel.Common.ItemLevel.Max.ToDoubleEmptyField(),
            ArmourMin = Panel.Defense.Armour.Min.ToDoubleEmptyField(),
            ArmourMax = Panel.Defense.Armour.Max.ToDoubleEmptyField(),
            EnergyMin = Panel.Defense.Energy.Min.ToDoubleEmptyField(),
            EnergyMax = Panel.Defense.Energy.Max.ToDoubleEmptyField(),
            EvasionMin = Panel.Defense.Evasion.Min.ToDoubleEmptyField(),
            EvasionMax = Panel.Defense.Evasion.Max.ToDoubleEmptyField(),
            WardMin = Panel.Defense.Ward.Min.ToDoubleEmptyField(),
            WardMax = Panel.Defense.Ward.Max.ToDoubleEmptyField(),
            DpsTotalMin = Panel.Damage.Total.Min.ToDoubleEmptyField(),
            DpsTotalMax = Panel.Damage.Total.Max.ToDoubleEmptyField(),
            DpsPhysMin = Panel.Damage.Physical.Min.ToDoubleEmptyField(),
            DpsPhysMax = Panel.Damage.Physical.Max.ToDoubleEmptyField(),
            DpsElemMin = Panel.Damage.Elemental.Min.ToDoubleEmptyField(),
            DpsElemMax = Panel.Damage.Elemental.Max.ToDoubleEmptyField(),
            FacetorExpMin = Panel.FacetorMin.ToDoubleEmptyField(),
            FacetorExpMax = Panel.FacetorMax.ToDoubleEmptyField(),

            ResolveMin = Panel.Sanctum.Resolve.Min.ToDoubleEmptyField(),
            ResolveMax = Panel.Sanctum.Resolve.Max.ToDoubleEmptyField(),
            MaxResolveMin = Panel.Sanctum.MaximumResolve.Min.ToDoubleEmptyField(),
            MaxResolveMax = Panel.Sanctum.MaximumResolve.Max.ToDoubleEmptyField(),
            InspirationMin = Panel.Sanctum.Inspiration.Min.ToDoubleEmptyField(),
            InspirationMax = Panel.Sanctum.Inspiration.Max.ToDoubleEmptyField(),
            AureusMin = Panel.Sanctum.Aureus.Min.ToDoubleEmptyField(),
            AureusMax = Panel.Sanctum.Aureus.Max.ToDoubleEmptyField(),
            MapItemQuantityMin = Panel.Map.Quantity.Min.ToDoubleEmptyField(),
            MapItemQuantityMax = Panel.Map.Quantity.Max.ToDoubleEmptyField(),
            MapItemRarityMin = Panel.Map.Rarity.Min.ToDoubleEmptyField(),
            MapItemRarityMax = Panel.Map.Rarity.Max.ToDoubleEmptyField(),
            MapPackSizeMin = Panel.Map.PackSize.Min.ToDoubleEmptyField(),
            MapPackSizeMax = Panel.Map.PackSize.Max.ToDoubleEmptyField(),
            MapMoreScarabMin = Panel.Map.MoreScarab.Min.ToDoubleEmptyField(),
            MapMoreScarabMax = Panel.Map.MoreScarab.Max.ToDoubleEmptyField(),
            MapMoreCurrencyMin = Panel.Map.MoreCurrency.Min.ToDoubleEmptyField(),
            MapMoreCurrencyMax = Panel.Map.MoreCurrency.Max.ToDoubleEmptyField(),
            MapMoreDivCardMin = Panel.Map.MoreDivCard.Min.ToDoubleEmptyField(),
            MapMoreDivCardMax = Panel.Map.MoreDivCard.Max.ToDoubleEmptyField(),
            Rarity = Rarity.Index >= 0 && Rarity.Index < Rarity.ComboBox.Count ?
                Rarity.ComboBox[Rarity.Index] : Rarity.Item,

            PriceMin = 0 // not used
        };

        // add item filters

        if (ModLine.Count > 0)
        {
            int modLimit = 1;
            foreach (ModLineViewModel mod in ModLine)
            {
                ItemFilter itemFilter = new();
                if (mod.Affix.Count > 0)
                {
                    double minValue = mod.Min.ToDoubleEmptyField();
                    double maxValue = mod.Max.ToDoubleEmptyField();

                    itemFilter.Text = mod.Mod.Trim();
                    itemFilter.Disabled = mod.Selected != true;
                    itemFilter.Min = minValue;
                    itemFilter.Max = maxValue;

                    itemFilter.Id = mod.Affix[mod.AffixIndex].ID;
                    if (mod.OptionVisible)
                    {
                        itemFilter.Option = mod.OptionID[mod.OptionIndex];
                        itemFilter.Min = Modifier.EMPTYFIELD;
                    }
                    xiletradeItem.ItemFilters.Add(itemFilter);
                    if (modLimit >= Modifier.NB_MAX_MODS)
                    {
                        break;
                    }
                    modLimit++;
                }
            }
        }

        if (Panel.Total.Resistance.Selected)
        {
            var filter = GetItemFilter("pseudo.pseudo_total_resistance", Panel.Total.Resistance.Min, Panel.Total.Resistance.Max);
            if (filter is not null) // +#% total Resistance
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Total.Life.Selected)
        {
            var filter = GetItemFilter("pseudo.pseudo_total_life", Panel.Total.Life.Min, Panel.Total.Life.Max);
            if (filter is not null) // +# total maximum Life
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }
        if (Panel.Total.GlobalEs.Selected)
        {
            var filter = GetItemFilter("pseudo.pseudo_total_energy_shield", Panel.Total.GlobalEs.Min, Panel.Total.GlobalEs.Max);
            if (filter is not null) // # to maximum Energy Shield
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Condition.FreePrefix)
        {
            var filter = GetItemFilter("pseudo.pseudo_number_of_empty_prefix_mods", 1, Modifier.EMPTYFIELD);
            if (filter is not null) // # Empty Prefix Modifiers
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Condition.FreeSuffix)
        {
            var filter = GetItemFilter("pseudo.pseudo_number_of_empty_suffix_mods", 1, Modifier.EMPTYFIELD);
            if (filter is not null) // # Empty Suffix Modifiers
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreScarab.Selected)
        {
            var filter = GetItemFilter("pseudo.pseudo_map_more_scarab_drops", Panel.Map.MoreScarab.Min, Panel.Map.MoreScarab.Max);
            if (filter is not null) // More Scarabs: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreCurrency.Selected)
        {
            var filter = GetItemFilter("pseudo.pseudo_map_more_currency_drops", Panel.Map.MoreCurrency.Min, Panel.Map.MoreCurrency.Max);
            if (filter is not null) // More Currency: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreDivCard.Selected)
        {
            var filter = GetItemFilter("pseudo.pseudo_map_more_card_drops", Panel.Map.MoreDivCard.Min, Panel.Map.MoreDivCard.Max);
            if (filter is not null) // More Divination Cards: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreMap.Selected) // always false, not in view intentionally
        {
            var filter = GetItemFilter("pseudo.pseudo_map_more_map_drops", Panel.Map.MoreMap.Min, Panel.Map.MoreMap.Max);
            if (filter is not null) // More Maps: #%
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
                var filter = GetItemFilter("pseudo." + influence, Modifier.EMPTYFIELD, Modifier.EMPTYFIELD);
                if (filter is not null)
                {
                    xiletradeItem.ItemFilters.Add(filter);
                }
            }
        }

        return xiletradeItem;
    }

    private static ItemFilter GetItemFilter(string stat, double min, double max)
    {
        ItemFilter itemFilter = new(stat);
        var entry = DataManager.Filter.Result[0].Entries.FirstOrDefault(x => x.ID == itemFilter.Id);
        if (entry is not null)
        {
            itemFilter.Disabled = false;
            itemFilter.Text = entry.Text;
            itemFilter.Min = min;
            itemFilter.Max = max;
            return itemFilter;
        }
        return null;
    }

    private static ItemFilter GetItemFilter(string stat, string strMin, string strMax)
    {
        return GetItemFilter(stat, strMin.ToDoubleEmptyField(), strMax.ToDoubleEmptyField());
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

        foreach (CurrencyResultData resultDat in DataManager.Currencies)
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
                foreach (CurrencyEntrie entrieDat in resultDat.Entries)
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
}

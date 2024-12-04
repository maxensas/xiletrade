using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
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
    private BulkViewModel bulk = new();

    [ObservableProperty]
    private ShopViewModel shop = new();

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

    public FormViewModel(bool useBulk = false)
    {
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
        Dictionary<string, bool> dicInflu = new()
        {
            { Influence.ShaperText, Influence.Shaper },
            { Influence.ElderText, Influence.Elder },
            { Influence.CrusaderText, Influence.Crusader },
            { Influence.RedeemerText, Influence.Redeemer },
            { Influence.WarlordText, Influence.Warlord },
            { Influence.HunterText, Influence.Hunter }
        };
        return dicInflu;
    }

    internal void SetModCurrent()
    {
        List<bool> sameText = new();
        bool remove = true;

        if (ModLine.Count > 0)
        {
            foreach (ModLineViewModel mod in ModLine)
            {
                sameText.Add(mod.Min == mod.Current);
                mod.Min = mod.Current;
            }
            foreach (bool same in sameText) remove &= same;
            if (remove)
            {
                foreach (ModLineViewModel mod in ModLine)
                {
                    if (mod.Min.Length > 0)
                    {
                        mod.Min = string.Empty;
                    }
                }
            }
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
            if (mod.TierTip.Count > 0)
            {
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

            SocketRed = Common.StrToDouble(Panel.Common.Sockets.RedColor, true),
            SocketGreen = Common.StrToDouble(Panel.Common.Sockets.GreenColor, true),
            SocketBlue = Common.StrToDouble(Panel.Common.Sockets.BlueColor, true),
            SocketWhite = Common.StrToDouble(Panel.Common.Sockets.WhiteColor, true),

            SocketMin = Common.StrToDouble(Panel.Common.Sockets.SocketMin, true),
            SocketMax = Common.StrToDouble(Panel.Common.Sockets.SocketMax, true),
            LinkMin = Common.StrToDouble(Panel.Common.Sockets.LinkMin, true),
            LinkMax = Common.StrToDouble(Panel.Common.Sockets.LinkMax, true),
            QualityMin = Common.StrToDouble(Panel.Common.Quality.Min, true),
            QualityMax = Common.StrToDouble(Panel.Common.Quality.Max, true),
            LvMin = Common.StrToDouble(Panel.Common.ItemLevel.Min, true),
            LvMax = Common.StrToDouble(Panel.Common.ItemLevel.Max, true),
            ArmourMin = Common.StrToDouble(Panel.Defense.Armour.Min, true),
            ArmourMax = Common.StrToDouble(Panel.Defense.Armour.Max, true),
            EnergyMin = Common.StrToDouble(Panel.Defense.Energy.Min, true),
            EnergyMax = Common.StrToDouble(Panel.Defense.Energy.Max, true),
            EvasionMin = Common.StrToDouble(Panel.Defense.Evasion.Min, true),
            EvasionMax = Common.StrToDouble(Panel.Defense.Evasion.Max, true),
            WardMin = Common.StrToDouble(Panel.Defense.Ward.Min, true),
            WardMax = Common.StrToDouble(Panel.Defense.Ward.Max, true),
            DpsTotalMin = Common.StrToDouble(Panel.Damage.Total.Min, true),
            DpsTotalMax = Common.StrToDouble(Panel.Damage.Total.Max, true),
            DpsPhysMin = Common.StrToDouble(Panel.Damage.Physical.Min, true),
            DpsPhysMax = Common.StrToDouble(Panel.Damage.Physical.Max, true),
            DpsElemMin = Common.StrToDouble(Panel.Damage.Elemental.Min, true),
            DpsElemMax = Common.StrToDouble(Panel.Damage.Elemental.Max, true),
            FacetorExpMin = Common.StrToDouble(Panel.FacetorMin, true),
            FacetorExpMax = Common.StrToDouble(Panel.FacetorMax, true),

            ResolveMin = Common.StrToDouble(Panel.Sanctum.Resolve.Min, true),
            ResolveMax = Common.StrToDouble(Panel.Sanctum.Resolve.Max, true),
            MaxResolveMin = Common.StrToDouble(Panel.Sanctum.MaximumResolve.Min, true),
            MaxResolveMax = Common.StrToDouble(Panel.Sanctum.MaximumResolve.Max, true),
            InspirationMin = Common.StrToDouble(Panel.Sanctum.Inspiration.Min, true),
            InspirationMax = Common.StrToDouble(Panel.Sanctum.Inspiration.Max, true),
            AureusMin = Common.StrToDouble(Panel.Sanctum.Aureus.Min, true),
            AureusMax = Common.StrToDouble(Panel.Sanctum.Aureus.Max, true),
            MapItemQuantityMin = Common.StrToDouble(Panel.Map.Quantity.Min, true),
            MapItemQuantityMax = Common.StrToDouble(Panel.Map.Quantity.Max, true),
            MapItemRarityMin = Common.StrToDouble(Panel.Map.Rarity.Min, true),
            MapItemRarityMax = Common.StrToDouble(Panel.Map.Rarity.Max, true),
            MapPackSizeMin = Common.StrToDouble(Panel.Map.PackSize.Min, true),
            MapPackSizeMax = Common.StrToDouble(Panel.Map.PackSize.Max, true),
            MapMoreScarabMin = Common.StrToDouble(Panel.Map.MoreScarab.Min, true),
            MapMoreScarabMax = Common.StrToDouble(Panel.Map.MoreScarab.Max, true),
            MapMoreCurrencyMin = Common.StrToDouble(Panel.Map.MoreCurrency.Min, true),
            MapMoreCurrencyMax = Common.StrToDouble(Panel.Map.MoreCurrency.Max, true),
            MapMoreDivCardMin = Common.StrToDouble(Panel.Map.MoreDivCard.Min, true),
            MapMoreDivCardMax = Common.StrToDouble(Panel.Map.MoreDivCard.Max, true),
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
                    double minValue = Common.StrToDouble(mod.Min, true);
                    double maxValue = Common.StrToDouble(mod.Max, true);

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
        return GetItemFilter(stat, Common.StrToDouble(strMin, true), Common.StrToDouble(strMax, true));
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
                    Strings.CurrencyType.MapsBlighted : Strings.CurrencyType.Maps;
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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main.Exchange;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class FormViewModel(bool useBulk) : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

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
    
    public FormViewModel(IServiceProvider serviceProvider, bool useBulk) : this(useBulk)
    {
        _serviceProvider = serviceProvider;
        bulk = new(_serviceProvider);
        shop = new(_serviceProvider);
        isPoeTwo = _serviceProvider.GetRequiredService<XiletradeService>().IsPoe2;
    }

    internal void SetModCurrent()
    {
        if (ModLine.Count <= 0)
        {
            return;
        }
        List<bool> sameText = new();
        bool remove = true;

        foreach (var mod in ModLine)
        {
            sameText.Add(mod.Min == mod.Current);
            mod.Min = mod.Current;
        }

        foreach (bool same in sameText) remove &= same;
        if (!remove)
        {
            return;
        }
        foreach (var mod in ModLine)
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
        foreach (var mod in ModLine)
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
        foreach (var mod in ModLine)
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
            if (range.Length is 2)
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
                if (range[1].Length > 0 && !range[1].Contain('+'))
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

            ChkRuneSockets = Panel.Common.RuneSockets.Selected,

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
            ExaltOnly = Exalt,

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
            RuneSocketsMin = Panel.Common.RuneSockets.Min.ToDoubleEmptyField(),
            RuneSocketsMax = Panel.Common.RuneSockets.Max.ToDoubleEmptyField(),
            Rarity = Rarity.Index >= 0 && Rarity.Index < Rarity.ComboBox.Count ?
                Rarity.ComboBox[Rarity.Index] : Rarity.Item,

            PriceMin = 0 // not used
        };

        // add item filters

        if (ModLine.Count > 0)
        {
            int modLimit = 1;
            foreach (var mod in ModLine)
            {
                var itemFilter = new ItemFilter();
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
            var filter = new ItemFilter("pseudo.pseudo_total_resistance", Panel.Total.Resistance.Min, Panel.Total.Resistance.Max);
            if (filter.Id.Length > 0) // +#% total Resistance
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Total.Life.Selected)
        {
            var filter = new ItemFilter("pseudo.pseudo_total_life", Panel.Total.Life.Min, Panel.Total.Life.Max);
            if (filter.Id.Length > 0) // +# total maximum Life
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }
        if (Panel.Total.GlobalEs.Selected)
        {
            var filter = new ItemFilter("pseudo.pseudo_total_energy_shield", Panel.Total.GlobalEs.Min, Panel.Total.GlobalEs.Max);
            if (filter.Id.Length > 0) // # to maximum Energy Shield
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Condition.FreePrefix)
        {
            var filter = new ItemFilter("pseudo.pseudo_number_of_empty_prefix_mods", 1, Modifier.EMPTYFIELD);
            if (filter.Id.Length > 0) // # Empty Prefix Modifiers
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Condition.FreeSuffix)
        {
            var filter = new ItemFilter("pseudo.pseudo_number_of_empty_suffix_mods", 1, Modifier.EMPTYFIELD);
            if (filter.Id.Length > 0) // # Empty Suffix Modifiers
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreScarab.Selected)
        {
            var filter = new ItemFilter("pseudo.pseudo_map_more_scarab_drops", Panel.Map.MoreScarab.Min, Panel.Map.MoreScarab.Max);
            if (filter.Id.Length > 0) // More Scarabs: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreCurrency.Selected)
        {
            var filter = new ItemFilter("pseudo.pseudo_map_more_currency_drops", Panel.Map.MoreCurrency.Min, Panel.Map.MoreCurrency.Max);
            if (filter.Id.Length > 0) // More Currency: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreDivCard.Selected)
        {
            var filter = new ItemFilter("pseudo.pseudo_map_more_card_drops", Panel.Map.MoreDivCard.Min, Panel.Map.MoreDivCard.Max);
            if (filter.Id.Length > 0) // More Divination Cards: #%
            {
                xiletradeItem.ItemFilters.Add(filter);
            }
        }

        if (Panel.Map.MoreMap.Selected) // always false, not in view intentionally
        {
            var filter = new ItemFilter("pseudo.pseudo_map_more_map_drops", Panel.Map.MoreMap.Min, Panel.Map.MoreMap.Max);
            if (filter.Id.Length > 0) // More Maps: #%
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
                var filter = new ItemFilter("pseudo." + influence, Modifier.EMPTYFIELD, Modifier.EMPTYFIELD);
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

    internal TotalStats FillModList(string[] clipData, ItemFlag itemIs, string itemName, string itemType, string itemClass, int idLang, out Dictionary<string, string> listOptions)
    {
        TotalStats totalStats = new();
        listOptions = GetNewListOption(); // itemType, itemIs.Gem

        if (!itemIs.ShowDetail || itemIs.Gem || itemIs.SanctumResearch || itemIs.AllflameEmber || itemIs.Corpses || itemIs.TrialCoins)
        {
            for (int i = 1; i < clipData.Length; i++)
            {
                var data = clipData[i].Trim().Split(Strings.CRLF, StringSplitOptions.None);
                var sameReward = data.Where(x => x.StartWith(Resources.Resources.General098_DeliriumReward));
                if (sameReward.Any())
                {
                    data = data.Distinct().ToArray();
                }

                if (itemIs.SanctumResearch && i == clipData.Length - 1) // at the last loop
                {
                    string[] sanctumMods = [.. GetSanctumMods(listOptions)];

                    if (sanctumMods.Length > 0)
                    {
                        Array.Resize(ref data, data.Length + sanctumMods.Length);
                        Array.Copy(sanctumMods, 0, data, data.Length - sanctumMods.Length, sanctumMods.Length);
                    }
                }

                var lSubMods = GetModsFromData(data, itemIs, itemName, itemType, itemClass, idLang, totalStats, listOptions);
                foreach (var submod in lSubMods)
                {
                    ModLine.Add(submod);
                }
            }
        }
        return totalStats;
    }

    // private
    private static AsyncObservableCollection<ModLineViewModel> GetModsFromData(string[] data, ItemFlag itemIs, string itemName, string itemType, string itemClass, int idLang, TotalStats totalStats, Dictionary<string, string> lOptions)
    {
        var lMods = new AsyncObservableCollection<ModLineViewModel>();
        var modDesc = new ModDescription();

        for (int j = 0; j < data.Length; j++)
        {
            if (data[j].Trim().Length is 0)
            {
                continue;
            }

            string unparsedData = data[j];
            var affix = new AffixFlag(data[j]);
            data[j] = affix.ParseAffix(data[j]);
            var splitData = data[j].Split(':', StringSplitOptions.TrimEntries);
            if (splitData[0].Contain(Resources.Resources.General110_FoilUnique))
            {
                splitData[0] = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
            }

            if (lOptions.TryGetValue(splitData[0], out string value))
            {
                if (value.Length is 0)
                {
                    lOptions[splitData[0]] = splitData.Length > 1 ? splitData[1] : Strings.TrueOption;
                    itemIs.ItemLevel = lOptions[Resources.Resources.General032_ItemLv].Length > 0
                        || lOptions[Resources.Resources.General143_WaystoneTier].Length > 0;
                    itemIs.AreaLevel = lOptions[Resources.Resources.General067_AreaLevel].Length > 0;
                    itemIs.Weapon = lOptions[Resources.Resources.General058_PhysicalDamage].Length > 0
                        || lOptions[Resources.Resources.General059_ElementalDamage].Length > 0 || itemIs.Wand; // to update 
                }
            }
            else
            {
                if (itemIs.Gem)
                {
                    if (splitData[0].Contain(Resources.Resources.General038_Vaal))
                    {
                        lOptions[Resources.Resources.General038_Vaal] = Strings.TrueOption;
                    }
                }
                else if ((itemIs.ItemLevel || itemIs.AreaLevel || itemIs.FilledCoffin) && lMods.Count < Modifier.NB_MAX_MODS)
                {
                    if (SkipBetweenBrackets(data[j], itemIs.Ultimatum))
                    {
                        continue;
                    }

                    bool impLogbook = itemIs.Logbook && affix.Implicit;
                    var desc = new ModDescription(data[j], impLogbook);
                    if (desc.IsParsed)
                    {
                        modDesc = desc;
                        continue;
                    }

                    double tierValMin = Modifier.EMPTYFIELD, tierValMax = Modifier.EMPTYFIELD;
                    string inputData = data[j];

                    // LOW priority Bug to fix :
                    // When there is no '(x-y)' example : Adds 1 to (4–5) Lightning Damage to Spells
                    if (!itemIs.ChargedCompass && !itemIs.Voidstone && !itemIs.MirroredTablet) // to handle text : (Tier 14+) & no tier needed
                    {
                        inputData = ParseTierValues(inputData, out Tuple<double, double> minmax);
                        tierValMin = minmax.Item1;
                        tierValMax = minmax.Item2;
                    }

                    inputData = ParseUnscalableValue(inputData, out bool unscalableValue);
                    inputData = Modifier.Parse(inputData, idLang, itemName, itemIs, modDesc.Name, out bool negativeValue);
                    if (negativeValue)
                    {
                        if (tierValMin.IsNotEmpty()) tierValMin = -tierValMin;
                        if (tierValMax.IsNotEmpty()) tierValMax = -tierValMax;
                    }

                    if (inputData.StartWith(Resources.Resources.General098_DeliriumReward))
                    {
                        inputData += " (×#)";
                    }

                    var modFilter = new ModFilter(inputData, data, j, itemIs, itemName, itemType, itemClass, out ModValue modVal);
                    if (modFilter.IsFetched)
                    {
                        var modFilterEntrie = modFilter.GetSerializable();
                        var mod = new ModLineViewModel(modFilterEntrie, modVal, itemIs, affix, modDesc, inputData, unparsedData, unscalableValue, tierValMin, tierValMax, idLang, negativeValue);

                        if (!itemIs.Unique)
                        {
                            totalStats.Fill(modFilterEntrie, mod.Current, idLang);
                        }

                        if (modFilterEntrie.ID.Contain(Strings.Stat.IncAs) && mod.ItemFilter.Min > 0 && mod.ItemFilter.Min < 999)
                        {
                            double val = lOptions[Strings.Stat.IncAs].ToDoubleDefault();
                            lOptions[Strings.Stat.IncAs] = (val + mod.ItemFilter.Min).ToString();
                        }
                        else if (modFilterEntrie.ID.Contain(Strings.Stat.IncPhys) && mod.ItemFilter.Min > 0 && mod.ItemFilter.Min < 9999)
                        {
                            double val = lOptions[Strings.Stat.IncPhys].ToDoubleDefault();
                            lOptions[Strings.Stat.IncPhys] = (val + mod.ItemFilter.Min).ToString();
                        }

                        lMods.Add(mod);
                    }
                }
            }
        }
        return lMods;
    }

    private static Dictionary<string, string> GetNewListOption()
    {
        Dictionary<string, string> lItemOption = new()
        {
            { Resources.Resources.General035_Quality, string.Empty },
            { Resources.Resources.General031_Lv, string.Empty },
            { Resources.Resources.General032_ItemLv, string.Empty },
            { Resources.Resources.General033_TalTier, string.Empty },
            { Resources.Resources.General034_MaTier, string.Empty },
            { Resources.Resources.General067_AreaLevel, string.Empty },
            { Resources.Resources.General036_Socket, string.Empty },
            { Resources.Resources.General055_Armour, string.Empty },
            { Resources.Resources.General056_Energy, string.Empty },
            { Resources.Resources.General057_Evasion, string.Empty },
            { Resources.Resources.General095_Ward, string.Empty },
            { Resources.Resources.General058_PhysicalDamage, string.Empty },
            { Resources.Resources.General059_ElementalDamage, string.Empty },
            { Resources.Resources.General060_ChaosDamage, string.Empty },
            { Resources.Resources.General061_AttacksPerSecond, string.Empty },
            { Resources.Resources.General041_Shaper, string.Empty },
            { Resources.Resources.General042_Elder, string.Empty },
            { Resources.Resources.General043_Crusader, string.Empty },
            { Resources.Resources.General044_Redeemer, string.Empty },
            { Resources.Resources.General045_Hunter, string.Empty },
            { Resources.Resources.General046_Warlord, string.Empty },
            { Resources.Resources.General047_Synthesis, string.Empty },
            { Resources.Resources.General037_Corrupt, string.Empty },
            { Resources.Resources.General109_Mirrored, string.Empty },
            { Resources.Resources.General110_FoilUnique, string.Empty },
            { Resources.Resources.General039_Unidentify, string.Empty },
            { Resources.Resources.General038_Vaal, string.Empty },
            { Strings.AlternateGem, string.Empty },
            { Strings.Stat.IncPhys, string.Empty },
            { Strings.Stat.IncAs, string.Empty },
            { Resources.Resources.Main154_tbFacetor, string.Empty },
            { Resources.Resources.General070_ReqSacrifice, string.Empty },
            { Resources.Resources.General071_Reward, string.Empty },
            { Resources.Resources.General099_ScourgedItem, string.Empty },
            { Resources.Resources.General114_SanctumResolve, string.Empty },
            { Resources.Resources.General115_SanctumInspiration, string.Empty },
            { Resources.Resources.General116_SanctumAureus, string.Empty },
            { Resources.Resources.General117_SanctumMinorBoons, string.Empty },
            { Resources.Resources.General118_SanctumMajorBoons, string.Empty },
            { Resources.Resources.General119_SanctumMinorAfflictions, string.Empty },
            { Resources.Resources.General120_SanctumMajorAfflictions, string.Empty },
            { Resources.Resources.General123_SanctumPacts, string.Empty },
            { Resources.Resources.General121_RewardsFloorCompletion, string.Empty },
            { Resources.Resources.General122_RewardsSanctumCompletion, string.Empty },
            { Resources.Resources.General128_Monster, string.Empty },
            { Resources.Resources.General129_CorpseLevel, string.Empty },
            { Resources.Resources.General130_MonsterCategory, string.Empty },
            { Resources.Resources.General136_ItemQuantity, string.Empty },
            { Resources.Resources.General137_ItemRarity, string.Empty },
            { Resources.Resources.General138_MonsterPackSize, string.Empty },
            { Resources.Resources.General139_MoreCurrency, string.Empty },
            { Resources.Resources.General140_MoreScarabs, string.Empty },
            { Resources.Resources.General141_MoreMaps, string.Empty },
            { Resources.Resources.General142_MoreDivinationCards, string.Empty },
            { Resources.Resources.General143_WaystoneTier, string.Empty },
            { Resources.Resources.General146_LightningDamage, string.Empty },
            { Resources.Resources.General147_CriticalHitChance, string.Empty },
            { Resources.Resources.General148_ColdDamage, string.Empty },
            { Resources.Resources.General149_FireDamage, string.Empty }
        };
        return lItemOption;
    }

    private static List<string> GetSanctumMods(Dictionary<string, string> lOptions)
    {
        List<string> lMods = new(), lEntrie = new();

        var majBoons = lOptions[Resources.Resources.General118_SanctumMajorBoons].Split(',', StringSplitOptions.TrimEntries);
        if (majBoons[0].Length > 0)
        {
            lEntrie.AddRange(majBoons);
        }
        var majAfflictions = lOptions[Resources.Resources.General120_SanctumMajorAfflictions].Split(',', StringSplitOptions.TrimEntries);
        if (majAfflictions[0].Length > 0)
        {
            lEntrie.AddRange(majAfflictions);
        }
        var pacts = lOptions[Resources.Resources.General123_SanctumPacts].Split(',', StringSplitOptions.TrimEntries);
        if (pacts[0].Length > 0)
        {
            lEntrie.AddRange(pacts);
        }

        /*
        StringBuilder sbMods = new(lOptions[Resources.Resources.General118_SanctumMajorBoons]);
        sbMods.AppendJoin(',', lOptions[Resources.Resources.General120_SanctumMajorAfflictions])
            .AppendJoin(',', lOptions[Resources.Resources.General123_SanctumPacts])
            .AppendJoin(',', lOptions[Resources.Resources.General121_RewardsFloorCompletion])
            .AppendJoin(',', lOptions[Resources.Resources.General122_RewardsSanctumCompletion]);
        var test = sbMods.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        */

        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var modEntry =
                    from result in DataManager.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contain(mod) && filt.Type is "sanctum"
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        lMods.Add(modTxt);
                    }
                }
            }
        }

        lEntrie = new();
        var floorRewards = lOptions[Resources.Resources.General121_RewardsFloorCompletion].Split(',', StringSplitOptions.TrimEntries);
        if (floorRewards[0].Length > 0)
        {
            lEntrie.AddRange(floorRewards);
        }
        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
                string modKind = RegexUtil.DecimalPattern().Replace(mod, "#").Replace("Orb ", "Orbs ").Replace("Mirror ", "Mirrors ");

                var modEntry =
                    from result in DataManager.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contain(modKind) && filt.ID.StartWith("sanctum.sanctum_floor_reward")
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        if (match.Count is 1)
                        {
                            modTxt = modTxt.Replace("#", match[0].Value);
                        }

                        lMods.Add(modTxt);
                    }
                }
            }
        }

        lEntrie = new();
        var sanctumRewards = lOptions[Resources.Resources.General122_RewardsSanctumCompletion].Split(',', StringSplitOptions.TrimEntries);
        if (sanctumRewards[0].Length > 0)
        {
            lEntrie.AddRange(sanctumRewards);
        }
        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
                string modKind = RegexUtil.DecimalPattern().Replace(mod, "#").Replace("Orb ", "Orbs ").Replace("Mirror ", "Mirrors ");

                var modEntry =
                    from result in DataManager.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contain(modKind) && filt.ID.StartWith("sanctum.sanctum_final_reward")
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        if (match.Count is 1)
                        {
                            modTxt = modTxt.Replace("#", match[0].Value);
                        }

                        lMods.Add(modTxt);
                    }
                }
            }
        }
        return lMods;
    }

    private static bool SkipBetweenBrackets(string data, bool ultimatum)
    {
        if (ultimatum)
        {
            return data.StartsWith('(') || data.EndsWith(')');
        }
        return data.StartsWith('(') && data.EndsWith(')');
    }

    private static string ParseTierValues(string data, out Tuple<double, double> minmax)
    {
        int watchdog = 0;
        int idx1, idx2;
        double tierValMin = Modifier.EMPTYFIELD, tierValMax = Modifier.EMPTYFIELD;
        StringBuilder sbParse = new(data);

        do
        {
            idx1 = sbParse.ToString().IndexOf('(', StringComparison.Ordinal);
            idx2 = sbParse.ToString().IndexOf(')', StringComparison.Ordinal);
            if (idx1 > -1 && idx2 > -1 && idx1 < idx2)
            {
                string tierRange = sbParse.ToString().Substring(idx1, idx2 - idx1 + 1);
                if (tierRange.Contain('-'))
                {
                    string[] extract = tierRange.Replace("(", string.Empty).Replace(")", string.Empty).Split('-');
                    _ = double.TryParse(extract[0], out double tValMin);
                    _ = double.TryParse(extract[1], out double tValMax);
                    if (tValMin is 0 || tValMax is 0)
                    {
                        tierValMin = tierValMax = Modifier.EMPTYFIELD;
                    }
                    else
                    {
                        tierValMin = tierValMin.IsEmpty() ? tValMin : (tierValMin + tValMin) / 2;
                        tierValMax = tierValMax.IsEmpty() ? tValMax : (tierValMax + tValMax) / 2;
                    }
                }
                else
                {
                    string extract = tierRange.Replace("(", string.Empty).Replace(")", string.Empty);
                    _ = double.TryParse(extract, out double tVal);
                    tierValMin = tVal is 0 ? Modifier.EMPTYFIELD : tVal;
                    tierValMax = tVal is 0 ? Modifier.EMPTYFIELD : tVal;
                }
                sbParse.Replace(tierRange, string.Empty);
            }
            watchdog++;
            if (watchdog > 10)
            {
                break;
            }
        } while (idx1 is not -1 || idx2 is not -1);

        if (tierValMin.IsNotEmpty()) tierValMin = Math.Truncate(tierValMin);
        if (tierValMax.IsNotEmpty()) tierValMax = Math.Truncate(tierValMax);

        minmax = new(tierValMin, tierValMax);

        return sbParse.ToString();
    }

    private static string ParseUnscalableValue(string data, out bool unscalable)
    {
        unscalable = false;
        var dataSplit = data.Split('—', StringSplitOptions.TrimEntries);
        if (dataSplit.Length > 1)
        {
            if (dataSplit[1] is Strings.UnscalableValue)
            {
                unscalable = true;
            }
        }
        return dataSplit[0]; // Remove : Unscalable Value - To modify if needed
    }

    internal void SearchCurrency(string str)
    {
        var exVm = str is "get" ? Bulk.Get :
            str is "pay" ? Bulk.Pay :
            str is "shop" ? Shop.Exchange : null;
        if (exVm is not null)
        {
            if (exVm.Search.Length >= 1)
            {
                SelectExchangeCurrency(str + "/contains", exVm.Search);
            }
            else
            {
                exVm.CategoryIndex = 0;
                exVm.CurrencyIndex = 0;
            }
        }
    }

    internal void SelectExchangeCurrency(string args, string currency, string tier = null)
    {
        var arg = args.Split('/');
        if (arg[0] is not "pay" and not "get" and not "shop")
        {
            return;
        }
        IEnumerable<(string, string, string Text)> cur;
        if (arg.Length > 1 && arg[1] is "contains") // contains requests to improve
        {
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

        if (cur.Any())
        {
            string curClass = cur.First().Item1;
            string curId = cur.First().Item2;
            string curText = cur.First().Text;

            string selectedCurrency = string.Empty, selectedTier = string.Empty;
            string selectedCategory = Strings.GetCategory(curClass, curId);

            if (selectedCategory.Length > 0)
            {
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
                        if (match.Count == 1)
                        {
                            selectedTier = "T" + match[0].Value.ToString();
                        }
                    }
                }
            }

            var bulk = arg[0] is "pay" ? Bulk.Pay
                : arg[0] is "get" ? Bulk.Get
                : arg[0] is "shop" ? Shop.Exchange
                : null;

            int idxCat = bulk.Category.IndexOf(selectedCategory);
            if (idxCat > -1)
            {
                bulk.CategoryIndex = idxCat;
            }

            int idxTier = bulk.Tier.IndexOf(selectedTier);
            if (idxTier > -1 && selectedTier.Length > 0)
            {
                bulk.TierIndex = idxTier;
            }

            // FIXES : 'bulk.Currency' ObservableCollection need to be loaded in View. 
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
        }
    }
}

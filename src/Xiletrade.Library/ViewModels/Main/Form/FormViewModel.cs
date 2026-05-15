using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
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
    private readonly DataManagerService _dm;

    [ObservableProperty]
    private string itemName = string.Empty;

    [ObservableProperty]
    private string itemNameColor = string.Empty;

    [ObservableProperty]
    private string itemBaseType = useBulk ? GetSearchExchangeTitle() : string.Empty;

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
    private int fracturedIndex = 0;

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
    private int identifiedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> corruption = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int corruptedIndex = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> doubleCorruption = new() { Resources.Resources.Main033_Any, Resources.Resources.Main034_No, Resources.Resources.Main035_Yes };

    [ObservableProperty]
    private int doubleCorruptedIndex = 0;

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
    private CustomSearchViewModel customSearch;

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
        
        visible = new(_serviceProvider, useBulk);
        panel = new(_serviceProvider);
        bulk = new(_serviceProvider); // mandatory (auto select currency item on price check)

        if (useBulk)
        {
            shop = new(_serviceProvider);
            customSearch = new(_serviceProvider, visible);
        }

        isPoeTwo = _dm.Config.Options.GameVersion is 1;

        UpdateMarket(useBulk);

        autoClose = _dm.Config.Options.Autoclose;
        sameUser = _dm.Config.Options.HideSameOccurs;
        opacity = _dm.Config.Options.Opacity;
        leagueIndex = _dm.GetDefaultLeagueIndex();
        league = _dm.GetLeagueAsyncCollection();
    }

    // TO REFACTOR
    internal FormViewModel(IServiceProvider serviceProvider, ItemData item, InfoDescription infoDesc, bool showMinMax) : this(serviceProvider, useBulk: false)
    {
        if (item.ModList?.Count > 0)
        {
            var modListVm = new AsyncObservableCollection<ModLineViewModel>();
            foreach (var mod in item.ModList)
            {
                modListVm.Add(new(_dm, mod, item.Flag, item.Lang, item.IsPoe2, showMinMax));
            }
            modList = modListVm;
        }
        var flag = item.Flag;
        var minMax = MinMaxModel.CreateDictionary();

        if (item.Options.Socket.Length > 0)
        {
            panel.Sockets = new(item, minMax);
            if (!item.IsPoe2)
            {
                condition.SocketColorsToolTip = panel.Sockets.GetSocketColors();
            }
        }

        if ((flag.Cluster || flag.Jewel) && flag.Unique && flag.Unidentified)
        {
            identifiedIndex = 1;
        }

        if (flag.Cluster && !flag.Fractured && !flag.Corrupted)
        {
            fracturedIndex = 1;
        }

        corruptedIndex = flag.Corrupted && _dm.Config.Options.AutoSelectCorrupt ? 2
            : flag.Normal ? 1 : 0;

        if (flag.Rare && !flag.Map && !flag.CapturedBeast) Tab.PoePriceEnable = true;

        visible.Corrupted = true;
        if (flag.Incubator)
        {
            visible.Corrupted = false;
        }

        var visibilityCond = flag.Unidentified || flag.Watchstone || flag.MapFragment
            || flag.Invitation || flag.CapturedBeast || flag.Chronicle || flag.Map
            || flag.Gems || flag.Currency || flag.Divcard || flag.Incubator;
        if (flag.Unique || visibilityCond)
        {
            visible.BtnPoeDb = false;
        }

        if (!item.IsPoe2)
        {
            var cond = flag.Weapon || flag.ArmourPiece || flag.Quivers;
            if (cond)
            {
                visible.Sockets = true;
            }
            if (cond || flag.Jewellery)
            {
                visible.Influences = true;
            }
            if (flag.Weapon || flag.ArmourPiece || flag.Jewellery || flag.Quivers)
            {
                minMax[StatPanel.CommonMemoryStrand].Min = item.Options.MemoryStrands;
            }
        }

        if (item.IsPoe2 && (flag.Weapon || flag.ArmourPiece))
        {
            visible.RuneSockets = true;
        }

        if (flag.SanctumResearch)
        {
            if (item.Options.Resolve is var resolve && resolve.Length is 2)
            {
                minMax[StatPanel.SanctumResolve].Min = resolve[0];
                minMax[StatPanel.SanctumMaxResolve].Max = resolve[1];
            }
            minMax[StatPanel.SanctumInspiration].Min = item.Options.Inspiration;
            minMax[StatPanel.SanctumAureus].Min = item.Options.Aureus;
        }

        var spec = "G";
        var cult = CultureInfo.InvariantCulture;
        var condTier = _dm.Config.Options.AutoSelectMinTierValue && !flag.Mirrored && !flag.Corrupted;

        var res = minMax[StatPanel.TotalElemResistance];
        var life = minMax[StatPanel.TotalLife];
        var globalEs = minMax[StatPanel.TotalGlobalEs];
        var attribute = minMax[StatPanel.TotalAttribute];

        if (!flag.Map && !flag.Flask && item.Stats?.CurrentResistance > 0)
        {
            res.Min = condTier && item.Stats.TierResistance > 0 ?
                item.Stats.TierResistance.ToString(spec, cult)
                : item.Stats.CurrentResistance.ToString(spec, cult);

            visible.TotalRes = true;
            if (_dm.Config.Options.AutoSelectRes
                && (res.Min.ToDoubleDefault() >= 36 || flag.Jewel))
            {
                res.Selected = true;
            }
        }
        if (item.Stats?.CurrentLife > 0)
        {
            life.Min = condTier && item.Stats.TierLife > 0 ?
                item.Stats.TierLife.ToString(spec, cult)
                : item.Stats.CurrentLife.ToString(spec, cult);

            visible.TotalLife = true;
            if (_dm.Config.Options.AutoSelectLife
                && (life.Min.ToDoubleDefault() >= 40 || flag.Jewel))
            {
                life.Selected = true;
            }
        }
        if (item.Stats?.CurrentEnergyShield > 0)
        {
            globalEs.Min = condTier && item.Stats.TierEnergyShield > 0 ?
                item.Stats.TierEnergyShield.ToString(spec, cult)
                : item.Stats.CurrentEnergyShield.ToString(spec, cult);

            if (!flag.ArmourPiece)
            {
                visible.TotalEs = true;
                if (_dm.Config.Options.AutoSelectGlobalEs
                    && (globalEs.Min.ToDoubleDefault() >= 38 || flag.Jewel))
                {
                    globalEs.Selected = true;
                }
            }
            else
            {
                globalEs.Min = string.Empty;
            }
        }
        if (item.Stats?.CurrentAttribute > 0)
        {
            attribute.Min = condTier && item.Stats.TierAttribute > 0 ?
                item.Stats.TierAttribute.ToString(spec, cult)
                : item.Stats.CurrentAttribute.ToString(spec, cult);

            visible.TotalAttr = true;
            if (_dm.Config.Options.AutoSelectAttr
                && attribute.Min.ToDoubleDefault() >= 20)
            {
                attribute.Selected = true;
            }
        }

        if (flag.ShowDetail)
        {
            detail = item.GetDetails(infoDesc);
        }
        else
        {
            var socket = minMax[StatPanel.CommonSocket];
            if (socket.Min is "6")
            {
                if (item.State.ImmutableSockets || Panel.Sockets?.WhiteColor is "6")
                {
                    condition.SocketColors = true;
                    socket.Selected = true;
                }
            }
            var link = minMax[StatPanel.CommonLink];
            if (link.Min is "6")
            {
                link.Selected = true;
            }

            if (!flag.Unidentified && flag.Weapon)
            {
                visible.Damage = true;

                var itemDps = new ItemDamage(item);
                dps = itemDps.TotalString;

                if (_dm.Config.Options.AutoSelectDps && itemDps.Total > 100)
                {
                    minMax[StatPanel.DamageTotal].Selected = true;
                }

                if (itemDps.TotalMin.Length > 0)
                {
                    minMax[StatPanel.DamageTotal].Min = itemDps.TotalMin;
                }
                if (itemDps.PysicalMin.Length > 0)
                {
                    minMax[StatPanel.DamagePhysical].Min = itemDps.PysicalMin;
                }
                if (itemDps.ElementalMin.Length > 0)
                {
                    minMax[StatPanel.DamageElemental].Min = itemDps.ElementalMin;
                }
                dpsTip = itemDps.Tip;
            }

            if (!flag.Unidentified && flag.ArmourPiece)
            {
                visible.Defense = true;

                var armour = item.Options.Armour;
                var energy = item.Options.Energy;
                var evasion = item.Options.Evasion;
                var ward = item.Options.Ward;

                if (armour.Length > 0)
                {
                    var ar = minMax[StatPanel.DefenseArmour];
                    if (_dm.Config.Options.AutoSelectArEsEva) ar.Selected = true;
                    ar.Min = armour;
                }
                if (energy.Length > 0)
                {
                    var es = minMax[StatPanel.DefenseEnergy];
                    if (_dm.Config.Options.AutoSelectArEsEva) es.Selected = true;
                    es.Min = energy;
                }
                if (evasion.Length > 0)
                {
                    var eva = minMax[StatPanel.DefenseEvasion];
                    if (_dm.Config.Options.AutoSelectArEsEva) eva.Selected = true;
                    eva.Min = evasion;
                }
                if (ward.Length > 0)
                {
                    var wrd = minMax[StatPanel.DefenseWard];
                    if (_dm.Config.Options.AutoSelectArEsEva) wrd.Selected = true;
                    wrd.Min = ward;
                    visible.Ward = true;
                }
                else
                {
                    visible.Armour = true;
                    visible.Energy = true;
                    visible.Evasion = true;
                }
            }
        }

        var poe2SkillWeapon = item.IsPoe2 && (flag.Wand || flag.Stave || flag.Sceptre);

        bool hasAnyFlag = flag.Unique || flag.Normal || flag.Currency || flag.Map
            || flag.Waystones || flag.Divcard || flag.CapturedBeast || flag.Gems
            || flag.Flask || flag.Tincture || flag.Watchstone || flag.Invitation
            || flag.Logbook || flag.Tablet || flag.Charm || flag.Graft
            || flag.Unidentified;

        byBase = item.State.SpecialBase || hasAnyFlag || _dm.Config.Options.SearchByType || poe2SkillWeapon;
        itemName = item.Name;
        itemBaseType = item.Type;

        rarity.Item = !flag.Waystones && (flag.MapFragment
            || flag.MiscMapItems || item.State.ExchangeCurrency
            || flag.Currency) ? Resources.Resources.General005_Any
            : flag.FoilVariant ? Resources.Resources.General110_FoilUnique
            : item.Rarity;

        itemNameColor = flag.Magic ? Strings.Color.DeepSkyBlue :
            flag.Rare ? Strings.Color.Gold :
            flag.FoilVariant ? Strings.Color.Green :
            flag.Unique ? Strings.Color.Peru : string.Empty;

        itemBaseTypeColor = flag.Gems ? Strings.Color.Teal : 
            item.State.ExchangeCurrency || flag.CapturedBeast ? Strings.Color.Moccasin : string.Empty;

        if ((flag.Map || flag.Waystones || flag.Watchstone || flag.Invitation || flag.Logbook || flag.ChargedCompass || flag.Voidstone) && !flag.Unique)
        {
            rarity.Item = Resources.Resources.General010_AnyNU;
            if (!flag.Corrupted)
            {
                corruptedIndex = 1;
            }
            if (flag.Voidstone)
            {
                byBase = false;
            }
        }

        visible.Rarity = true;
        visible.ByBase = true;
        visible.CheckAll = true;
        visible.Quality = true;
        visible.PanelStat = true;
        visible.PanelForm = true;

        if (rarity.Item.Length is 0)
        {
            rarity.Item = item.Rarity;
        }

        if (!item.IsPoe2 && !flag.Currency && !item.State.ExchangeCurrency
            && !flag.CapturedBeast && !flag.Map && !flag.MiscMapItems && !flag.Gems)
        {
            visible.Conditions = true;
        }

        var level = minMax[StatPanel.CommonItemLevel];

        // TO REWORK
        bool hideUserControls = !flag.Invitation && !flag.Map && !flag.AllflameEmber && (flag.Currency
            && !flag.Chronicle && !flag.FilledCoffin && (!flag.Ultimatum || flag.UltimatumPoe2)
            || (item.State.ExchangeCurrency && !flag.Tablet && !flag.Waystones)
            || flag.CapturedBeast || flag.MemoryLine);
        if (hideUserControls)
        {
            visible.Rarity = false;
            visible.ByBase = false;
            visible.CheckAll = false;
            visible.Quality = false;
            visible.PanelStat = false;
            visible.PanelForm = false;
            visible.Corrupted = false;
            if (flag.Facetor)
            {
                visible.Facetor = true;
                panel.FacetorMin = item.Options.StoredExperience;
            }
            if (flag.UncutGem || flag.Wombgift || flag.UltimatumPoe2 || flag.TrialCoins)
            {
                visible.PanelForm = true;
                level.Min = item.Options.ItemLevel;
                level.Selected = true;
            }
        }

        if (item.State.ExchangeCurrency && (!flag.Unique || flag.Map))
        {
            tab.BulkEnable = true;
        }

        tab.QuickEnable = true;
        tab.DetailEnable = true;

        // Open Xiletrade with Quick or Detail view tab
        var selectDetail = !(flag.Map && flag.Corrupted) && (flag.StackableCurrency
            || flag.Map || flag.Gems || flag.CapturedBeast || flag.UltimatumPoe2 
            || flag.UncutGem || flag.Wombgift || flag.TrialCoins
            || (item.State.ExchangeCurrency && !flag.Tablet && !flag.Waystones));
        tab.DetailSelected = selectDetail;
        tab.QuickSelected = !selectDetail;

        if (!(item.State.ExchangeCurrency && !flag.Tablet && !flag.Waystones && !flag.Map)
            && !flag.Chronicle && !flag.CapturedBeast && !flag.Ultimatum)
        {
            visible.ModSet = true;
            //visible.ModPercent = item.IsPoe2;
        }
        var qual = minMax[StatPanel.CommonQuality];
        if (!flag.Unique && (flag.Flask || flag.Tincture || (flag.Normal && item.IsPoe2)))
        {
            var iLvl = item.Options.ItemLevel;
            var baseLevelMin = item.IsPoe2 ? 79 : 84;
            if (int.TryParse(iLvl, out int result) && result >= baseLevelMin)
            {
                qual.Selected = item.Options.Quality.Length > 0
                    && int.Parse(item.Options.Quality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!hideUserControls || flag.Corpses)
        {
            level.Min = flag.Gems ? item.Options.Level : item.Options.ItemLevel;
            qual.Min = item.Options.Quality;
            influence.SetInfluences(item.Flag);

            if (flag.ArmourPiece || flag.Weapon || flag.Jewellery
                || flag.Flask || flag.Charm)
            {
                var lv = item.Options.Level;
                var req = item.Options.Requires;
                minMax[StatPanel.CommonRequiresLevel].Min = lv.Length > 0 ? lv : req;
            }

            if (flag.Unique && !visibilityCond && !item.IsPoe2)
            {
                string nameEn = string.Empty;
                if (item.Lang is Lang.English)
                {
                    nameEn = item.Name;
                }
                else
                {
                    var wordRes = _dm.Words.FindWordByName(item.Name);
                    if (wordRes is not null)
                    {
                        nameEn = wordRes.NameEn;
                    }
                }
                if (nameEn.Length > 0)
                {
                    var dust = _dm.DustLevel.FindDustByName(nameEn);
                    if (dust is not null)
                    {
                        var ilvl = Math.Clamp(level.Min.ToDoubleDefault(), 65, 84);
                        var valQual = qual.Min.ToDoubleDefault();
                        double qualMultiplier = 1;
                        if (valQual > 0)
                        {
                            qualMultiplier += valQual * 1 / 50;
                        }
                        var multiplier = (20 - (84 - ilvl)) * qualMultiplier;
                        var calc = Math.Truncate(dust.DustVal * 125 * multiplier);
                        dustValue = calc.FormatWithSuffix();
                        visible.BtnDust = true;
                    }
                }
            }

            checkComboCondition.Update(condition);
            checkComboInfluence.Update(influence);

            panel.SynthesisBlight = flag.MapBlight || flag.Synthesised;
            panel.BlighRavaged = flag.MapBlightRavaged;

            if (flag.Map)
            {
                level.Min = level.Max = item.Options.MapTier;
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;
                
                if (!item.State.ConquerorMap)
                {
                    visible.ByBase = false;
                }
                visible.MapStats = true;

                var mapQuant = minMax[StatPanel.MapQuantity];
                mapQuant.Min = item.Options.ItemQuantity;
                var mapRarity = minMax[StatPanel.MapRarity];
                mapRarity.Min = item.Options.ItemRarity;
                var mapPackSize = minMax[StatPanel.MapPackSize];
                mapPackSize.Min = item.Options.MonsterPackSize;
                var mapScarab = minMax[StatPanel.MapMoreScarab];
                mapScarab.Min = item.Options.MoreScarabs;
                var mapCurrency = minMax[StatPanel.MapMoreCurrency];
                mapCurrency.Min = item.Options.MoreCurrency;
                var mapDivCard = minMax[StatPanel.MapMoreDivCard];
                mapDivCard.Min = item.Options.MoreDiv;
                var mapMoreMap = minMax[StatPanel.MapMoreMap];
                mapMoreMap.Min = item.Options.MoreMaps;

                // new auto select behaviour
                if (mapQuant.Min.ToDoubleDefault() >= 100
                    && mapRarity.Min.ToDoubleDefault() >= 90
                    && mapPackSize.Min.ToDoubleDefault() >= 40)
                {
                    mapQuant.Selected = mapRarity.Selected = mapPackSize.Selected = true;
                    if (mapScarab.Min.ToDoubleDefault() >= 70)
                    {
                        mapScarab.Selected = true;
                    }
                    if (mapCurrency.Min.ToDoubleDefault() >= 70)
                    {
                        mapCurrency.Selected = true;
                    }
                    if (mapDivCard.Min.ToDoubleDefault() >= 70)
                    {
                        mapDivCard.Selected = true;
                    }
                    if (mapMoreMap.Min.ToDoubleDefault() >= 100)
                    {
                        mapMoreMap.Selected = true;
                    }
                }

                panel.SynthesisBlightLabel = "Blighted";
                if (flag.MapBlight || flag.MapBlightRavaged)
                {
                    visible.SynthesisBlight = true;
                    visible.BlightRavaged = true;
                }
                if (flag.MapValdo)
                {
                    StringBuilder sbReward = new(item.Options.Reward);
                    if (sbReward.ToString().Length > 0)
                    {
                        sbReward.Replace(Resources.Resources.General125_Foil, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
                        panel.Reward.Text = new(sbReward.ToString().Trim());
                        panel.Reward.FgColor = Strings.Color.Peru;
                        panel.Reward.Tip = Strings.Reward.FoilUnique;

                        visible.Reward = true;
                        visible.ModSet = false;
                    }
                }
            }
            else if (flag.Waystones)
            {
                level.Min = level.Max = item.Options.WaystoneTier;
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;

                visible.MapStats = true;
                minMax[StatPanel.MapQuantity].Min = item.Options.ItemQuantity;
                minMax[StatPanel.MapQuantity].Selected = true;
                minMax[StatPanel.MapRarity].Min = item.Options.ItemRarity;
                minMax[StatPanel.MapRarity].Selected = true;
                minMax[StatPanel.MapPackSize].Min = item.Options.MonsterPackSize;
                minMax[StatPanel.MapPackSize].Selected = true;
                minMax[StatPanel.MapMonsterRare].Min = item.Options.RareMonsters;
                minMax[StatPanel.MapMonsterRare].Selected = true;
                minMax[StatPanel.MapMonsterMagic].Min = item.Options.MagicMonsters;

                visible.ByBase = false;
                visible.Quality = false;
            }
            else if (flag.Gems)
            {
                level.Selected = true;
                minMax[StatPanel.CommonQuality].Selected = item.Options.Quality.Length > 0
                    && int.Parse(item.Options.Quality, CultureInfo.InvariantCulture) > 12;
                if (!flag.Corrupted)
                {
                    corruptedIndex = 1; // NO
                }
                visible.ByBase = false;
                visible.CheckAll = flag.Imbued;
                visible.ModSet = false;
                //visible.ModPercent = false;
                visible.Rarity = false;
            }
            else if (flag.FilledCoffin)
            {
                visible.ByBase = false;
                visible.Rarity = false;
                visible.Corrupted = false;
                visible.Quality = false;

                level.Min = item.Options.CorpseLevel;
                level.Selected = true;
            }
            else if (flag.AllflameEmber)
            {
                visible.Corrupted = false;
                visible.Quality = false;
                visible.ByBase = false;
                visible.CheckAll = false;
                visible.ModSet = false;
                visible.Rarity = false;

                level.Min = item.Options.ItemLevel;
                level.Selected = true;
            }
            else if (flag.ByType && flag.Normal)
            {
                level.Selected = level.Min.Length > 0
                    && int.Parse(level.Min, CultureInfo.InvariantCulture) > 82;
            }
            else if (!flag.Unique && flag.Cluster)
            {
                level.Selected = level.Min.Length > 0
                    && int.Parse(level.Min, CultureInfo.InvariantCulture) >= 78;
                if (level.Min.Length > 0)
                {
                    int minVal = int.Parse(level.Min, CultureInfo.InvariantCulture);
                    level.Min = minVal >= 84 ? "84" : minVal >= 78 ? "78" : level.Min;
                }
            }
        }

        if (flag.Logbook || flag.Corpses || flag.SanctumResearch
            || flag.Chronicle || flag.MirroredTablet
            || flag.TrialCoins || (flag.Ultimatum && IsPoeTwo)
            || (flag.Flask || flag.Tincture) && !flag.Unique)
        {
            level.Selected = true;
        }

        if (flag.Chronicle || flag.Ultimatum || flag.MirroredTablet
            || flag.SanctumResearch || flag.TrialCoins || flag.Logbook)
        {
            visible.Corrupted = false;
            visible.Rarity = false;
            visible.ByBase = false;
            visible.Quality = false;
            level.Text = Resources.Resources.General067_AreaLevel;
            var area = item.Options.AreaLevel;
            level.Min = area.Length > 0 ? area : item.Options.AreaLevelBis;

            if (flag.SanctumResearch)
            {
                bool isTome = _dm.Bases.FindBaseByNameEn(Strings.Unique.ForbiddenTome)?.Name == item.Type;
                if (!isTome)
                {
                    visible.SanctumFields = true;
                }
            }
            if (flag.Ultimatum && !IsPoeTwo)
            {
                visible.Reward = true;
                panel.Reward.UpdateReward(item.Options);
            }
        }

        if (level.Text.Length is 0)
        {
            level.Text = Resources.Resources.Main065_tbiLevel;
        }

        visible.Detail = flag.ShowDetail;
        visible.HeaderMod = !flag.ShowDetail;
        visible.HiddablePanel = visible.SynthesisBlight || visible.BlightRavaged;
        rarity.Index = rarity.ComboBox.IndexOf(rarity.Item);

        var minMaxVm = new AsyncObservableCollection<MinMaxViewModel>();
        foreach ((var id, var model) in minMax)
        {
            if (model.Min.Length is 0 && model.Max.Length is 0)
            {
                continue;
            }
            minMaxVm.Add(new(id, model));
        }
        panel.StatList = minMaxVm;
    }

    internal void ClearLists()
    {
        ModList?.Clear();
        Panel.StatList?.Clear();
        CustomSearch?.MinMaxList.Clear();
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

    // Can extend here
    private static readonly Dictionary<StatPanel,
        (Func<TotalStats, double> current, Func<TotalStats, double> tier)> TotalStatMap = new()
        {
            { StatPanel.TotalLife, (s => s.CurrentLife, s => s.TierLife) },
            { StatPanel.TotalElemResistance, (s => s.CurrentResistance, s => s.TierResistance) },
            { StatPanel.TotalGlobalEs, (s => s.CurrentEnergyShield, s => s.TierEnergyShield) },
            { StatPanel.TotalAttribute, (s => s.CurrentAttribute, s => s.TierAttribute) }
        };

    private void UpdateStats(ItemData item, bool useTier = false)
    {
        if (item?.Stats is null)
            return;

        foreach (var kvp in TotalStatMap)
        {
            var value = useTier ? kvp.Value.tier(item.Stats) : kvp.Value.current(item.Stats);

            var stat = Panel.StatList?.FirstOrDefault(x => x.Id == kvp.Key);
            if (stat is not null && stat.SlideValue > 0 && value > 0)
            {
                stat.SlideValue = value;
            }
        }
    }

    internal XiletradeItem GetXiletradeItem(bool customSearch = false)
    {
        var listPanel = customSearch ? CustomSearch.MinMaxList : Panel.StatList;

        var noSockets = Panel.Sockets is null;
        var socketRed = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.RedColor.ToDoubleEmptyField();
        var socketGreen = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.GreenColor.ToDoubleEmptyField();
        var socketBlue = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.BlueColor.ToDoubleEmptyField();
        var socketWhite = noSockets ? ModFilter.EMPTYFIELD : Panel.Sockets.WhiteColor.ToDoubleEmptyField();

        var item = new XiletradeItem()
        {
            InfShaper = Influence.Shaper,
            InfElder = Influence.Elder,
            InfCrusader = Influence.Crusader,
            InfRedeemer = Influence.Redeemer,
            InfHunter = Influence.Hunter,
            InfWarlord = Influence.Warlord,

            //itemOption.Corrupt = (byte)cbCorrupt.SelectedIndex;
            Corrupted = GetOption(CorruptedIndex),
            TwiceCorrupted = GetOption(DoubleCorruptedIndex),
            Identified = GetOption(IdentifiedIndex),
            Mirrored = GetOption(MirroredIndex),
            Fractured = GetOption(FracturedIndex),
            Split = GetOption(SplitIndex),
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

            SocketRed = socketRed,
            SocketGreen = socketGreen,
            SocketBlue = socketBlue,
            SocketWhite = socketWhite,
            FacetorExpMin = Panel.FacetorMin.ToDoubleEmptyField(),
            FacetorExpMax = Panel.FacetorMax.ToDoubleEmptyField(),
        };

        // add item filters
        if (ModList?.Count > 0)
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
        }

        void ApplyStat(StatPanel stat, Action<MinMaxViewModel> setter)
        {
            var minMaxVm = listPanel?.FirstOrDefault(x => x.Id == stat);
            if (minMaxVm is not null)
                setter(minMaxVm);
        }

        ApplyStat(StatPanel.CommonItemLevel, vm =>
        {
            item.ChkLv = vm.Selected;
            item.LvMin = vm.ItemMin;
            item.LvMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.CommonQuality, vm =>
        {
            item.ChkQuality = vm.Selected;
            item.QualityMin = vm.ItemMin;
            item.QualityMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.CommonSocket, vm =>
        {
            item.ChkSocket = vm.Selected;
            item.SocketMin = vm.ItemMin;
            item.SocketMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.CommonLink, vm =>
        {
            item.ChkLink = vm.Selected;
            item.LinkMin = vm.ItemMin;
            item.LinkMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.CommonSocketRune, vm =>
        {
            item.ChkRuneSockets = vm.Selected;
            item.RuneSocketsMin = vm.ItemMin;
            item.RuneSocketsMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.CommonSocketGem, vm =>
        {
            item.ChkGemSockets = vm.Selected;
            item.GemSocketsMin = vm.ItemMin;
            item.GemSocketsMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.CommonRequiresLevel, vm =>
        {
            item.ChkReqLevel = vm.Selected;
            item.ReqLevelMin = vm.ItemMin;
            item.ReqLevelMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.CommonMemoryStrand, vm =>
        {
            item.ChkMemoryStrand = vm.Selected;
            item.MemoryStrandMin = vm.ItemMin;
            item.MemoryStrandMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.DamageElemental, vm =>
        {
            item.ChkDpsElem = vm.Selected;
            item.DpsElemMin = vm.ItemMin;
            item.DpsElemMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.DamagePhysical, vm =>
        {
            item.ChkDpsPhys = vm.Selected;
            item.DpsPhysMin = vm.ItemMin;
            item.DpsPhysMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.DamageTotal, vm =>
        {
            item.ChkDpsTotal = vm.Selected;
            item.DpsTotalMin = vm.ItemMin;
            item.DpsTotalMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.DefenseArmour, vm =>
        {
            item.ChkArmour = vm.Selected;
            item.ArmourMin = vm.ItemMin;
            item.ArmourMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.DefenseEnergy, vm =>
        {
            item.ChkEnergy = vm.Selected;
            item.EnergyMin = vm.ItemMin;
            item.EnergyMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.DefenseEvasion, vm =>
        {
            item.ChkEvasion = vm.Selected;
            item.EvasionMin = vm.ItemMin;
            item.EvasionMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.DefenseWard, vm =>
        {
            item.ChkWard = vm.Selected;
            item.WardMin = vm.ItemMin;
            item.WardMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.MapPackSize, vm =>
        {
            item.ChkMapPack = vm.Selected;
            item.MapPackSizeMin = vm.ItemMin;
            item.MapPackSizeMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.MapQuantity, vm =>
        {
            item.ChkMapIiq = vm.Selected;
            item.MapItemQuantityMin = vm.ItemMin;
            item.MapItemQuantityMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.MapRarity, vm =>
        {
            item.ChkMapIir = vm.Selected;
            item.MapItemRarityMin = vm.ItemMin;
            item.MapItemRarityMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.SanctumAureus, vm =>
        {
            item.ChkAureus = vm.Selected;
            item.AureusMin = vm.ItemMin;
            item.AureusMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.SanctumInspiration, vm =>
        {
            item.ChkInspiration = vm.Selected;
            item.InspirationMin = vm.ItemMin;
            item.InspirationMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.SanctumMaxResolve, vm =>
        {
            item.ChkMaxResolve = vm.Selected;
            item.MaxResolveMin = vm.ItemMin;
            item.MaxResolveMax = vm.ItemMax;
        });

        ApplyStat(StatPanel.SanctumResolve, vm =>
        {
            item.ChkResolve = vm.Selected;
            item.ResolveMin = vm.ItemMin;
            item.ResolveMax = vm.ItemMax;
        });

        //pseudo
        void ApplyFilter(StatPanel stat, string pseudoId)
        {
            ApplyStat(stat, vm =>
            {
                if (!vm.Selected)
                    return;

                var useSlide = vm.SlideValue is not ModFilter.EMPTYFIELD;
                var filter = useSlide
                    ? new ItemFilter(_dm.Filter, pseudoId, vm.SlideValue, vm.Max)
                    : new ItemFilter(_dm.Filter, pseudoId, vm.Min, vm.Max);

                if (!string.IsNullOrEmpty(filter.Id))
                    item.ItemFilters.Add(filter);
            });
        }

        ApplyFilter(StatPanel.TotalElemResistance, Strings.Stat.Pseudo.TotalElemResistance);
        ApplyFilter(StatPanel.TotalLife, Strings.Stat.Pseudo.TotalLife);
        ApplyFilter(StatPanel.TotalAttribute, Strings.Stat.Pseudo.TotalAttribute);
        ApplyFilter(StatPanel.TotalGlobalEs, Strings.Stat.Pseudo.TotalEs);
        ApplyFilter(StatPanel.MapMoreScarab, Strings.Stat.Pseudo.MoreScarab);
        ApplyFilter(StatPanel.MapMoreCurrency, Strings.Stat.Pseudo.MoreCurrency);
        ApplyFilter(StatPanel.MapMoreDivCard, Strings.Stat.Pseudo.MoreDivCard);
        //ApplyFilter(StatPanel.MapMoreMap, "pseudo.pseudo_map_more_map_drops");

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

    private static DefaultOption GetOption(int index)
    {
        return index switch
        {
            1 => DefaultOption.False,
            2 => DefaultOption.True,
            _ => DefaultOption.Any,
        };
    }

    private static string GetSearchExchangeTitle()
    {
        return Resources.Resources.Main247_CustomSearch + " / "
            + Resources.Resources.Main032_cbTotalExchange;
    }
}

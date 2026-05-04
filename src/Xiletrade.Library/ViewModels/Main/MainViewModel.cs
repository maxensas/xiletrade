using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Application.Diagnostic;
using Xiletrade.Library.Models.CoE.Domain;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Poe.Contract.Two;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Models.Wiki.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Command;
using Xiletrade.Library.ViewModels.Main.Form;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class MainViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private FormViewModel form;

    [ObservableProperty]
    private ResultViewModel result;

    [ObservableProperty]
    private NinjaViewModel ninja;

    [ObservableProperty]
    private string notifyName;

    [ObservableProperty]
    private bool showMinMax;

    [ObservableProperty]
    private double viewScale;

    [ObservableProperty]
    private bool authenticated;

    [ObservableProperty]
    private bool authentication;

    internal string ClipboardText { get; set; } = string.Empty;
    public List<MouseGestureCom> GestureList { get; private set; } = new();

    //viewmodels split
    public MainCommand Commands { get; private set; }
    public TrayMenuCommand TrayCommands { get; private set; }

    //models
    internal ItemData Item { get; private set; }
    internal StopWatch StopWatch { get; } = new();
    internal TaskManager TaskManager { get; } = new();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        TrayCommands = new(_serviceProvider);
        Commands = new(this, _serviceProvider);
        NotifyName = "Xiletrade " + Common.GetFileVersion();
    }

    //internal methods
    internal void InitViewModels(bool useBulk = false)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        ViewScale = dm.Config.Options.Scale;

        Result = new(_serviceProvider);
        Ninja = new(_serviceProvider);
        Form = new(_serviceProvider, useBulk);
    }

    // clear item data and lists on closing main view.
    internal void ClearContentViewModels()
    {
        Item = null;
        Form.ClearLists();
        Result.ClearLists();

        // To release string caches from vm
        if (Form.CustomSearch is not null)
        {
            Form.CustomSearch.Search = null;
            Form.CustomSearch.Stat = null;
        }
    }

    internal Task OpenUrlTask(string url, UrlType type)
    {
        return Task.Run(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception)
            {
                var message = type is UrlType.PoeDb ? Resources.Resources.Main201_PoedbFail
                : type is UrlType.PoeWiki ? Resources.Resources.Main124_WikiFail
                : type is UrlType.Ninja ? Resources.Resources.Main125_NinjaFail
                : type is UrlType.CraftOfExile ? "Fail CoE"
                : string.Empty;
                var caption = type is UrlType.PoeDb ? "Redirection to poedb failed "
                : type is UrlType.PoeWiki ? "Redirection to wiki failed "
                : type is UrlType.Ninja ? "Redirection to ninja failed "
                : type is UrlType.CraftOfExile ? "Redirection to Craft of Exile failed "
                : string.Empty;

                var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                ms.Show(message, caption, MessageStatus.Warning);
            }
        });
    }

    internal async Task RunMainUpdaterTaskAsync(string fonction)
    {
        try
        {
            await TaskManager.CancelPreviousTasksAsync().ConfigureAwait(false);
            
            var token = TaskManager.GetMainUpdaterToken(initCts: true);

            bool openWikiOnly = fonction is Strings.Feature.wiki;
            bool openNinjaOnly = fonction is Strings.Feature.ninja;
            bool openCoeOnly = fonction is Strings.Feature.coe;
            bool openWindow = !openWikiOnly && !openNinjaOnly && !openCoeOnly;

            TaskManager.MainUpdaterTask = Task.Run(() =>
            {
                try
                {
#if DEBUG
                    var logger = _serviceProvider.GetRequiredService<ILogger<MainViewModel>>();
                    logger.LogInformation("Starting Main Updater Task.");
#endif
                    var infoDesc = new InfoDescription(ClipboardText);
                    if (!infoDesc.IsPoeItem)
                        return;

                    UpdateMainViewModel(infoDesc);
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    logger.LogInformation("Main view model updated.");
#endif
                    if (openWindow)
                    {
                        _serviceProvider.GetRequiredService<INavigationService>().ShowMainView();
                        UpdatePrices(minimumStock: 0);
                        return;
                    }

                    if (openWikiOnly)
                    {
                        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                        var poeWiki = new PoeWiki(dm, Item);
                        _ = OpenUrlTask(poeWiki.Link, UrlType.PoeWiki);
                        return;
                    }
                    if (openNinjaOnly)
                    {
                        _ = OpenUrlTask(Ninja.FullUrl, UrlType.Ninja);
                        return;
                    }
                    if (openCoeOnly)
                    {
                        var coe = new CraftOfExile(ClipboardText);
                        _ = OpenUrlTask(coe.Link, UrlType.CraftOfExile);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Task canceled: ignore
                }
                catch (Exception ex)
                {
                    var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    ms.Show(ex.GetFormated(), "Item parsing error : method UpdateMainViewModel", MessageStatus.Error);
                }
            }, token);
        }
        catch (Exception ex)
        {
            // Log cancel/initialization errors (this doesn't happen often, but better to be safe than sorry)
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Anti-spam task error", MessageStatus.Warning);
        }
    }

    internal void UpdatePrices(int minimumStock)
    {
        try
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();

            int maxFetch = 0;
            var entity = new List<string>[2];

            Form.FetchDetailIsEnabled = false;

            if (Form.Tab.QuickSelected || Form.Tab.DetailSelected)
            {
                Result.Detail.Total = Resources.Resources.Main005_PriceResearch;
                Result.Quick.RightString = Result.Detail.RightString = Resources.Resources.Main006_PriceCheck;
                Result.Quick.LeftString = Result.Detail.LeftString = string.Empty;
                Result.Quick.Total = string.Empty;
                Result.DetailList.Clear();

                maxFetch = (int)dm.Config.Options.SearchFetchDetail;

                if (dm.Config.Options.Language is not 8 and not 9)
                {
                    TaskManager.NinjaTask = Ninja.TryUpdatePriceTask();
                }
            }
            else if (Form.Tab.BulkSelected)
            {
                Result.Bulk.RightString = Resources.Resources.Main003_PriceFetching;
                Result.Bulk.Total = Resources.Resources.Main005_PriceResearch;
                Result.Bulk.LeftString = string.Empty;
                Result.BulkList.Clear();
                Result.BulkOffers.Clear();

                if (Form.Bulk.Pay.CurrencyIndex > 0 && Form.Bulk.Get.CurrencyIndex > 0)
                {
                    entity[0] = new() { Form.GetExchangeCurrencyTag(ExchangeType.Pay) };
                    entity[1] = new() { Form.GetExchangeCurrencyTag(ExchangeType.Get) };
                    maxFetch = (int)dm.Config.Options.SearchFetchBulk;
                }
            }
            else if (Form.Tab.ShopSelected)
            {
                Result.Shop.RightString = Resources.Resources.Main003_PriceFetching;
                Result.Shop.Total = Resources.Resources.Main005_PriceResearch;
                Result.Shop.LeftString = string.Empty;
                Result.ShopList.Clear();
                Result.ShopOffers.Clear();

                var curGetList = from list in Form.Shop.GetList select list.ToolTip;
                var curPayList = from list in Form.Shop.PayList select list.ToolTip;
                if (!curGetList.Any() || !curPayList.Any())
                {
                    return;
                }
                entity[0] = curPayList.ToList();
                entity[1] = curGetList.ToList();
            }

            if (entity[0] is null)
            {
                try
                {
                    entity[0] = new() { GetSerialized(Form.Market[Form.MarketIndex], useSaleType: true) };
                }
                catch (Exception ex)
                {
                    var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    ms.Show(ex.GetFormated(), "JSON serialization error", MessageStatus.Error);
                }
            }

            var priceInfo = new PricingInfo(entity, Form.League[Form.LeagueIndex]
                , Form.Market[Form.MarketIndex], minimumStock, maxFetch, Form.SameUser, Form.Tab.BulkSelected);
            Result.UpdateWithApi(priceInfo);
        }
        catch (Exception ex)
        {
            throw new Exception("Exception encountered : method UpdateItemPrices", ex);
        }
    }

    internal void LaunchCustomSearch()
    {
        if (!Form.Tab.CustomSearchSelected ||
            (string.IsNullOrEmpty(Form.CustomSearch.Search.SearchQuery) && Form.CustomSearch.UnidUniquesIndex is 0))
        {
            return;
        }

        try
        {
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();

            Result.InitData();
            Result.DetailList.Clear();

            var json = GetSerialized(Form.Market[Form.MarketIndex], customSearch: true);
            var maxFetch = (int)dm.Config.Options.SearchFetchDetail;

            var priceInfo = new PricingInfo([new() { json }, null], Form.League[Form.LeagueIndex]
                , Form.Market[Form.MarketIndex], minimumStock: 1, maxFetch
                , Form.SameUser, Form.Tab.BulkSelected);

            Result.UpdateWithApi(priceInfo);
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Custom search error", MessageStatus.Error);
        }
    }

    private void UpdateMainViewModel(InfoDescription infoDesc)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        Item = new ItemData(dm, infoDesc);
        if (Item.ModList?.Count > 0)
        {
            foreach (var mod in Item.ModList)
            {
                Form.ModList.Add(new(mod, ShowMinMax));
            }
        }
        var flag = Item.Flag;
        var minMax = MinMaxModel.CreateDictionary();

        if (Item.Option[Resources.Resources.General036_Socket].Length > 0)
        {
            Form.Panel.Sockets.Update(Item, minMax);
            if (!Item.IsPoe2)
            {
                Form.Condition.SocketColorsToolTip = Form.Panel.Sockets.GetSocketColors();
            }
        }

        if (flag.Mirrored || flag.Corrupted)
        {
            Form.SetModCurrent(this.Item, clear: false);
        }

        if ((flag.Cluster || flag.Jewel) && flag.Unique && flag.Unidentified)
        {
            Form.IdentifiedIndex = 1;
        }

        if (flag.Cluster && !flag.Fractured && !flag.Corrupted)
        {
            Form.FracturedIndex = 1;
        }

        Form.CorruptedIndex = flag.Corrupted && dm.Config.Options.AutoSelectCorrupt ? 2 
            : flag.Normal ? 1 : 0;

        if (flag.Rare && !flag.Map && !flag.CapturedBeast) Form.Tab.PoePriceEnable = true;

        Form.Visible.Corrupted = true;
        if (flag.Incubator)
        {
            Form.Visible.Corrupted = false;
        }

        var visibilityCond = flag.Unidentified || flag.Watchstone || flag.MapFragment
            || flag.Invitation || flag.CapturedBeast || flag.Chronicle || flag.Map 
            || flag.Gems || flag.Currency || flag.Divcard || flag.Incubator;
        if (flag.Unique || visibilityCond)
        {
            Form.Visible.BtnPoeDb = false;
        }

        if (!Item.IsPoe2)
        {
            var cond = flag.Weapon || flag.ArmourPiece || flag.Quivers;
            if (cond) 
            {
                Form.Visible.Sockets = true;
            }
            if (cond || flag.Jewellery)
            {
                Form.Visible.Influences = true;
            }
            if (flag.Weapon || flag.ArmourPiece || flag.Jewellery || flag.Quivers)
            {
                minMax[StatPanel.CommonMemoryStrand].Min 
                    = Item.Option[Resources.Resources.General156_MemoryStrands];
            }
        }

        if (Item.IsPoe2 && (flag.Weapon || flag.ArmourPiece))
        {
            Form.Visible.RuneSockets = true;
        }

        if (flag.SanctumResearch)
        {
            var resolve = Item.Option[Resources.Resources.General114_SanctumResolve].Split(' ')[0].Split('/', StringSplitOptions.TrimEntries);
            if (resolve.Length is 2)
            {
                minMax[StatPanel.SanctumResolve].Min = resolve[0];
                minMax[StatPanel.SanctumMaxResolve].Max = resolve[1];
            }
            minMax[StatPanel.SanctumInspiration]
                .Min = Item.Option[Resources.Resources.General115_SanctumInspiration];
            minMax[StatPanel.SanctumAureus]
                .Min = Item.Option[Resources.Resources.General116_SanctumAureus];
        }

        var spec = "G";
        var cult = CultureInfo.InvariantCulture;
        var condTier = dm.Config.Options.AutoSelectMinTierValue;

        var res = minMax[StatPanel.TotalElemResistance];
        var life = minMax[StatPanel.TotalLife];
        var globalEs = minMax[StatPanel.TotalGlobalEs];
        var attribute = minMax[StatPanel.TotalAttribute];

        if (!flag.Map && !flag.Flask && Item.Stats.CurrentResistance > 0)
        {
            res.Min = condTier && Item.Stats.TierResistance > 0 ?
                Item.Stats.TierResistance.ToString(spec, cult)
                : Item.Stats.CurrentResistance.ToString(spec, cult);
            
            Form.Visible.TotalRes = true;
            if (dm.Config.Options.AutoSelectRes
                && (res.Min.ToDoubleDefault() >= 36 || flag.Jewel))
            {
                res.Selected = true;
            }
        }
        if (Item.Stats.CurrentLife > 0)
        {
            life.Min = condTier && Item.Stats.TierLife > 0 ?
                Item.Stats.TierLife.ToString(spec, cult)
                : Item.Stats.CurrentLife.ToString(spec, cult);

            Form.Visible.TotalLife = true;
            if (dm.Config.Options.AutoSelectLife
                && (life.Min.ToDoubleDefault() >= 40 || flag.Jewel))
            {
                life.Selected = true;
            }
        }
        if (Item.Stats.CurrentEnergyShield > 0)
        {
            globalEs.Min = condTier && Item.Stats.TierEnergyShield > 0 ?
                Item.Stats.TierEnergyShield.ToString(spec, cult)
                : Item.Stats.CurrentEnergyShield.ToString(spec, cult);

            if (!flag.ArmourPiece)
            {
                Form.Visible.TotalEs = true; 
                if (dm.Config.Options.AutoSelectGlobalEs
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
        if (Item.Stats.CurrentAttribute > 0)
        {
            attribute.Min = condTier && Item.Stats.TierAttribute > 0 ?
                Item.Stats.TierAttribute.ToString(spec, cult)
                : Item.Stats.CurrentAttribute.ToString(spec, cult);

            Form.Visible.TotalAttr = true;
            if (dm.Config.Options.AutoSelectAttr
                && attribute.Min.ToDoubleDefault() >= 20)
            {
                attribute.Selected = true;
            }
        }

        if (flag.ShowDetail)
        {
            Form.Detail = Item.GetDetails(infoDesc);
        }
        else
        {
            var unmodSocket = Form.UpdateModList(Item);

            var socket = minMax[StatPanel.CommonSocket];
            if (socket.Min is "6")
            {
                if (unmodSocket || Form.Panel.Sockets.WhiteColor is "6")
                {
                    Form.Condition.SocketColors = true;
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
                Form.Visible.Damage = true;

                var itemDps = new ItemDamage(Item);
                Form.Dps = itemDps.TotalString;

                if (dm.Config.Options.AutoSelectDps && itemDps.Total > 100)
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
                Form.DpsTip = itemDps.Tip;
            }

            if (!flag.Unidentified && flag.ArmourPiece)
            {
                Form.Visible.Defense = true;

                string armour = RegexUtil.NumericalPattern().Replace(Item.Option[Resources.Resources.General055_Armour].Trim(), string.Empty);
                string energy = RegexUtil.NumericalPattern().Replace(Item.Option[Resources.Resources.General056_Energy].Trim(), string.Empty);
                string evasion = RegexUtil.NumericalPattern().Replace(Item.Option[Resources.Resources.General057_Evasion].Trim(), string.Empty);
                string ward = RegexUtil.NumericalPattern().Replace(Item.Option[Resources.Resources.General095_Ward].Trim(), string.Empty);

                if (armour.Length > 0)
                {
                    var ar = minMax[StatPanel.DefenseArmour];
                    if (dm.Config.Options.AutoSelectArEsEva) ar.Selected = true;
                    ar.Min = armour;
                }
                if (energy.Length > 0)
                {
                    var es = minMax[StatPanel.DefenseEnergy];
                    if (dm.Config.Options.AutoSelectArEsEva) es.Selected = true;
                    es.Min = energy;
                }
                if (evasion.Length > 0)
                {
                    var eva = minMax[StatPanel.DefenseEvasion];
                    if (dm.Config.Options.AutoSelectArEsEva) eva.Selected = true;
                    eva.Min = evasion;
                }
                if (ward.Length > 0)
                {
                    var wrd = minMax[StatPanel.DefenseWard];
                    if (dm.Config.Options.AutoSelectArEsEva) wrd.Selected = true;
                    wrd.Min = ward;
                    Form.Visible.Ward = true;
                }
                else
                {
                    Form.Visible.Armour = true;
                    Form.Visible.Energy = true;
                    Form.Visible.Evasion = true;
                }
            }
        }

        var poe2SkillWeapon = Item.IsPoe2 && (flag.Wand || flag.Stave || flag.Sceptre);

        bool hasAnyFlag = flag.Unique || flag.Normal || flag.Currency || flag.Map 
            || flag.Waystones || flag.Divcard || flag.CapturedBeast || flag.Gems 
            || flag.Flask || flag.Tincture || flag.Watchstone || flag.Invitation 
            || flag.Logbook || flag.Tablet || flag.Charm || flag.Graft 
            || flag.Unidentified;

        Form.ByBase = Item.IsSpecialBase || hasAnyFlag || dm.Config.Options.SearchByType || poe2SkillWeapon;
        Form.ItemName = Item.Name;
        Form.ItemBaseType = Item.Type;

        Form.Rarity.Item = !flag.Waystones && (flag.MapFragment 
            || flag.MiscMapItems || Item.IsExchangeCurrency
            || flag.Currency) ? Resources.Resources.General005_Any 
            : flag.FoilVariant ? Resources.Resources.General110_FoilUnique 
            : Item.Rarity;

        Form.ItemNameColor = flag.Magic ? Strings.Color.DeepSkyBlue :
            flag.Rare ? Strings.Color.Gold :
            flag.FoilVariant ? Strings.Color.Green :
            flag.Unique ? Strings.Color.Peru : string.Empty;

        Form.ItemBaseTypeColor = flag.Gems ? Strings.Color.Teal : flag.Currency ? Strings.Color.Moccasin : string.Empty;

        if ((flag.Map || flag.Waystones || flag.Watchstone || flag.Invitation || flag.Logbook || flag.ChargedCompass || flag.Voidstone) && !flag.Unique)
        {
            Form.Rarity.Item = Resources.Resources.General010_AnyNU;
            if (!flag.Corrupted)
            {
                Form.CorruptedIndex = 1;
            }
            if (flag.Voidstone)
            {
                Form.ByBase = false;
            }
        }

        Form.Visible.Rarity = true;
        Form.Visible.ByBase = true;
        Form.Visible.CheckAll = true;
        Form.Visible.Quality = true;
        Form.Visible.PanelStat = true;
        Form.Visible.PanelForm = true;

        if (Form.Rarity.Item.Length is 0)
        {
            Form.Rarity.Item = Item.Rarity;
        }

        if (!Item.IsPoe2 && !flag.Currency && !Item.IsExchangeCurrency
            && !flag.CapturedBeast && !flag.Map && !flag.MiscMapItems
            && !flag.Gems)
        {
            Form.Visible.Conditions = true;
        }

        bool hideUserControls = false;
        if (!flag.Invitation && !flag.Map && !flag.AllflameEmber && (flag.Currency
            && !flag.Chronicle && !flag.Ultimatum && !flag.FilledCoffin
            || (Item.IsExchangeCurrency && !flag.Tablet && !flag.Waystones)
            || flag.CapturedBeast || flag.MemoryLine))
        {
            hideUserControls = true;

            if (!flag.MirroredTablet && !flag.SanctumResearch && !flag.Corpses && !flag.TrialCoins)
            {
                Form.Visible.PanelForm = false;
            }
            else
            {
                Form.Visible.Quality = false;
            }
            Form.Visible.PanelStat = false;
            Form.Visible.ByBase = false;
            Form.Visible.Rarity = false;
            Form.Visible.Corrupted = false;
            Form.Visible.CheckAll = false;
        }
        if (hideUserControls && flag.Facetor)
        {
            Form.Visible.Facetor = true;
            Form.Panel.FacetorMin = Item.Option[Resources.Resources.Main154_tbFacetor].Replace(" ", string.Empty);
        }
        var level = minMax[StatPanel.CommonItemLevel];
        if (hideUserControls && (flag.UncutGem || flag.Wombgift))
        {
            Form.Visible.PanelForm = true;
            Form.Visible.Quality = false;
            level.Min = RegexUtil.NumericalPattern().Replace(Item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            level.Selected = true;
        }

        Form.Tab.QuickEnable = true;
        Form.Tab.DetailEnable = true;

        if (Item.IsExchangeCurrency && (!flag.Unique || flag.Map))
        {
            Form.Tab.BulkEnable = true;
            Form.Tab.ShopEnable = true;

            bool isMap = Item.MapName.Length > 0;

            Form.Bulk.AutoSelect = true;
            Form.Bulk.Args = "pay/equals";
            Form.Bulk.Currency = isMap ? Item.MapName : Item.Type;
            Form.Bulk.Tier = isMap ? Item.MapTier : string.Empty;
        }

        // Select Quick or Detail TAB
        if (!(flag.Map && flag.Corrupted) && (flag.StackableCurrency 
            || flag.Map || flag.Gems || flag.CapturedBeast || flag.UltimatumPoe2 || flag.UncutGem
            || flag.Wombgift
            || (Item.IsExchangeCurrency && !flag.Tablet && !flag.Waystones)))
        {
            Form.Tab.DetailSelected = true;
        }
        if (!Form.Tab.DetailSelected)
        {
            Form.Tab.QuickSelected = true;
        }

        if (!(Item.IsExchangeCurrency && !flag.Tablet && !flag.Waystones && !flag.Map) 
            && !flag.Chronicle && !flag.CapturedBeast && !flag.Ultimatum)
        {
            Form.Visible.ModSet = true;
            //Form.Visible.ModPercent = item.IsPoe2;
        }
        var qual = minMax[StatPanel.CommonQuality];
        if (!flag.Unique && (flag.Flask || flag.Tincture || (flag.Normal && Item.IsPoe2)))
        {
            var iLvl = RegexUtil.NumericalPattern().Replace(Item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            var baseLevelMin = Item.IsPoe2 ? 79 : 84;
            if (int.TryParse(iLvl, out int result) && result >= baseLevelMin)
            {
                qual.Selected = Item.Quality.Length > 0
                    && int.Parse(Item.Quality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!hideUserControls || flag.Corpses)
        {
            level.Min = RegexUtil.NumericalPattern().Replace(Item.Option[flag.Gems ?
                Resources.Resources.General031_Lv : Resources.Resources.General032_ItemLv].Trim(), string.Empty);

            qual.Min = Item.Quality;
            Form.Influence.SetInfluences(Item.Flag);

            if (flag.ArmourPiece || flag.Weapon || flag.Jewellery
                || flag.Flask || flag.Charm)
            {
                var lv = Item.Option[Resources.Resources.General031_Lv].Trim();
                var req = Item.Option[Resources.Resources.General155_Requires].Split(',')[0];
                minMax[StatPanel.CommonRequiresLevel].Min = lv.Length > 0 ? lv 
                    : RegexUtil.NumericalPattern().Replace(req, string.Empty);
            }

            if (flag.Unique && !visibilityCond && !Item.IsPoe2)
            {
                string nameEn = string.Empty;
                if (Item.Lang is Lang.English)
                {
                    nameEn = Item.Name;
                }
                else
                {
                    var wordRes = dm.Words.FindWordByName(Item.Name);
                    if (wordRes is not null)
                    {
                        nameEn = wordRes.NameEn;
                    }
                }
                if (nameEn.Length > 0)
                {
                    var dust = dm.DustLevel.FindDustByName(nameEn);
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
                        Form.DustValue = calc.FormatWithSuffix();
                        Form.Visible.BtnDust = true;
                    }
                }
            }

            Form.CheckComboCondition.Update(Form.Condition);
            Form.CheckComboInfluence.Update(Form.Influence);

            Form.Panel.SynthesisBlight = flag.MapBlight || flag.Synthesised;
            Form.Panel.BlighRavaged = flag.MapBlightRavaged;

            if (flag.Map)
            {
                level.Min = Item.Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty); // 0x20
                level.Max = Item.Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;
                Form.Panel.SynthesisBlightLabel = "Blighted";
                Form.Visible.SynthesisBlight = true;
                Form.Visible.BlightRavaged = true;

                if (!Item.IsConqMap)
                {
                    Form.Visible.ByBase = false;
                }
                Form.Visible.MapStats = true;

                var mapQuant = minMax[StatPanel.MapQuantity];
                mapQuant.Min = Item.Option[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                var mapRarity = minMax[StatPanel.MapRarity];
                mapRarity.Min = Item.Option[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                var mapPackSize = minMax[StatPanel.MapPackSize];
                mapPackSize.Min = Item.Option[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                var mapScarab = minMax[StatPanel.MapMoreScarab];
                mapScarab.Min = Item.Option[Resources.Resources.General140_MoreScarabs].Replace(" ", string.Empty);
                var mapCurrency = minMax[StatPanel.MapMoreCurrency];
                mapCurrency.Min = Item.Option[Resources.Resources.General139_MoreCurrency].Replace(" ", string.Empty);
                var mapDivCard = minMax[StatPanel.MapMoreDivCard];
                mapDivCard.Min = Item.Option[Resources.Resources.General142_MoreDivinationCards].Replace(" ", string.Empty);
                var mapMoreMap = minMax[StatPanel.MapMoreMap];
                mapMoreMap.Min = Item.Option[Resources.Resources.General141_MoreMaps].Replace(" ", string.Empty);

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

                if (level.Min is "17" && level.Max is "17")
                {
                    Form.Visible.SynthesisBlight = false;
                    Form.Visible.BlightRavaged = false;

                    StringBuilder sbReward = new(Item.Option[Resources.Resources.General071_Reward]);
                    if (sbReward.ToString().Length > 0)
                    {
                        sbReward.Replace(Resources.Resources.General125_Foil, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
                        Form.Panel.Reward.Text = new(sbReward.ToString().Trim());
                        Form.Panel.Reward.FgColor = Strings.Color.Peru;
                        Form.Panel.Reward.Tip = Strings.Reward.FoilUnique;

                        Form.Visible.Reward = true;
                        Form.Visible.ModSet = false;
                    }
                }
            }
            else if (flag.Waystones)
            {
                level.Min = Item.Option[Resources.Resources.General143_WaystoneTier].Replace(" ", string.Empty); // 0x20
                level.Max = Item.Option[Resources.Resources.General143_WaystoneTier].Replace(" ", string.Empty); // 0x20
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;

                Form.Visible.MapStats = true;
                minMax[StatPanel.MapQuantity].Min = Item.Option[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                minMax[StatPanel.MapQuantity].Selected = true;
                minMax[StatPanel.MapRarity].Min = Item.Option[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                minMax[StatPanel.MapRarity].Selected = true;
                minMax[StatPanel.MapPackSize].Min = Item.Option[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                minMax[StatPanel.MapPackSize].Selected = true;
                minMax[StatPanel.MapMonsterRare].Min = Item.Option[Resources.Resources.General162_RareMonsters].Replace(" ", string.Empty);
                minMax[StatPanel.MapMonsterRare].Selected = true;
                minMax[StatPanel.MapMonsterMagic].Min = Item.Option[Resources.Resources.General161_MagicMonsters].Replace(" ", string.Empty);
                
                Form.Visible.ByBase = false;
                Form.Visible.Quality = false;
            }
            else if (flag.Gems)
            {
                level.Selected = true;
                minMax[StatPanel.CommonQuality].Selected = Item.Quality.Length > 0
                    && int.Parse(Item.Quality, CultureInfo.InvariantCulture) > 12;
                if (!flag.Corrupted)
                {
                    Form.CorruptedIndex = 1; // NO
                }
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                //Form.Visible.ModPercent = false;
                Form.Visible.Rarity = false;
            }
            else if (flag.FilledCoffin)
            {
                Form.Visible.ByBase = false;
                Form.Visible.Rarity = false;
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;

                level.Min = Item.Option[Resources.Resources.General129_CorpseLevel].Replace(" ", string.Empty);
                level.Selected = true;
            }
            else if (flag.AllflameEmber)
            {
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                Form.Visible.Rarity = false;

                level.Min = RegexUtil.NumericalPattern().Replace(Item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
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

        if (flag.Logbook || flag.Corpses
            || (flag.Flask || flag.Tincture) && !flag.Unique)
        {
            level.Selected = true;
        }

        if (flag.Chronicle || flag.Ultimatum || flag.MirroredTablet 
            || flag.SanctumResearch || flag.TrialCoins)
        {
            Form.Visible.Corrupted = false;
            Form.Visible.Rarity = false;
            Form.Visible.ByBase = false;
            Form.Visible.Quality = false;
            level.Text = Resources.Resources.General067_AreaLevel;
            level.Min = Item.Option[Resources.Resources.General067_AreaLevel].Replace(" ", string.Empty);

            if (flag.SanctumResearch)
            {
                bool isTome = dm.Bases.FindBaseByNameEn(Strings.Unique.ForbiddenTome)?.Name == Item.Type;
                if (!isTome)
                {
                    Form.Visible.SanctumFields = true;
                }
            }
            if (flag.SanctumResearch || flag.Chronicle || flag.MirroredTablet 
                || flag.TrialCoins || (flag.Ultimatum && Form.IsPoeTwo))
            {
                level.Selected = true;
            }
            if (flag.Ultimatum && !Form.IsPoeTwo)
            {
                Form.Visible.Reward = true;
                Form.Panel.Reward.UpdateReward(Item.Option);
            }
        }

        if (level.Text.Length is 0)
        {
            level.Text = Resources.Resources.Main065_tbiLevel;
        }

        Form.Visible.Detail = flag.ShowDetail;
        Form.Visible.HeaderMod = !flag.ShowDetail;
        Form.Visible.HiddablePanel = Form.Visible.SynthesisBlight || Form.Visible.BlightRavaged;
        Form.Rarity.Index = Form.Rarity.ComboBox.IndexOf(Form.Rarity.Item);

        if (Form.Bulk.AutoSelect)
        {
            _ = Form.SelectExchangeCurrency(Form.Bulk.Args, Form.Bulk.Currency, Form.Bulk.Tier); // Select currency in 'Pay' section
        }

        foreach ((var id, var model) in minMax)
        {
            if (model.Min.Length is 0 && model.Max.Length is 0)
            {
                continue;
            }
            Form.Panel.StatList.Add(new(id, model));
        }

        Form.FillTime = StopWatch.StopAndGetTimeString();
    }

    internal string GetSerialized(string market, bool useSaleType = false, bool customSearch = false)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var isPoe2 = dm.Config.Options.GameVersion is 1;
        var xItem = Form.GetXiletradeItem(customSearch);

        if (!customSearch)
        {
            if (isPoe2)
            {
                var jsonDataTwo = new JsonDataTwoFactory(dm).Create(xItem, Item, useSaleType, market);
                return dm.Json.Serialize<JsonDataTwo>(jsonDataTwo);
            }
            var jsonData = new JsonDataFactory(dm).Create(xItem, Item, useSaleType, market);
            return dm.Json.Serialize<JsonData>(jsonData);
        }
        
        var search = Form.CustomSearch.Search.SearchQuery;
        var unid = Form.CustomSearch.UnidUniquesIndex > 0 ?
            Form.CustomSearch.UnidUniques[Form.CustomSearch.UnidUniquesIndex] : null;

        if (isPoe2)
        {
            var jsonDataTwo = new JsonDataTwoFactory(dm).Create(xItem, unid, market, search);
            return dm.Json.Serialize<JsonDataTwo>(jsonDataTwo);
        }
        var json = new JsonDataFactory(dm).Create(xItem, unid, market, search);
        return dm.Json.Serialize<JsonData>(json);
    }
}

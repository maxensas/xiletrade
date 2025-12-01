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

    /// <summary>
    /// Clear memory data related to price checking.
    /// </summary>
    /// <remarks>
    /// TOFIX : Do not use untill fixed. It break bindings with Bulk/Shop viewmodels
    /// </remarks>
    internal void ClearContentViewModels(bool disabled = true)
    {
        if (disabled) 
            return;

        Form = null;
        Result = null;
        Ninja = null;
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

                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(message, caption, MessageStatus.Warning);
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
                        _ = OpenUrlTask(Ninja.GetFullUrl(), UrlType.Ninja);
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
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show($"{ex.Source} Exception raised : {ex.Message}\r\n\r\n{ex.StackTrace}\r\n\r\n",
                        "Item parsing error : method UpdateMainViewModel", MessageStatus.Error);
                }
            }, token);
        }
        catch (Exception ex)
        {
            // Log cancel/initialization errors (this doesn't happen often, but better to be safe than sorry)
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show($"RunMainUpdaterTaskAsync failed: {ex.Message}\r\n{ex.StackTrace}",
                "Anti-spam task error", MessageStatus.Warning);
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
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "JSON serialization error", MessageStatus.Error);
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
            (string.IsNullOrEmpty(Form.CustomSearch.Search) && Form.CustomSearch.UnidUniquesIndex is 0))
        {
            return;
        }

        try
        {
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();

            Result.InitData();
            Result.DetailList.Clear();

            var json = GetSerialized(Form.Market[Form.MarketIndex]);
            var maxFetch = (int)dm.Config.Options.SearchFetchDetail;

            var priceInfo = new PricingInfo([new() { json }, null], Form.League[Form.LeagueIndex]
                , Form.Market[Form.MarketIndex], minimumStock: 1, maxFetch
                , Form.SameUser, Form.Tab.BulkSelected);

            Result.UpdateWithApi(priceInfo);
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Custom search error", MessageStatus.Error);
        }
    }

    //private methods
    private void UpdateMainViewModel(InfoDescription infodesc)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var item = Form.FillModList(infodesc);
        
        var minMaxList = MinMaxModel.GetNewMinMaxList();

        if (item.Option[Resources.Resources.General036_Socket].Length > 0)
        {
            Form.Panel.Sockets.Update(item, minMaxList);
            if (!item.IsPoe2)
            {
                Form.Condition.SocketColorsToolTip = Form.Panel.Sockets.GetSocketColors();
            }
        }

        if (item.IsPoe2 || item.Flag.Mirrored || item.Flag.Corrupted)
        {
            Form.SetModCurrent(clear: false);
        }

        Form.CorruptedIndex = item.Flag.Corrupted && dm.Config.Options.AutoSelectCorrupt ? 2 
            : item.Flag.Normal ? 1 : 0;

        if (item.Flag.Rare && !item.Flag.Map && !item.Flag.CapturedBeast) Form.Tab.PoePriceEnable = true;

        Form.Visible.Corrupted = true;
        if (item.Flag.Incubator)
        {
            Form.Visible.Corrupted = false;
        }

        var visibilityCond = item.Flag.Unidentified || item.Flag.Watchstone || item.Flag.MapFragment
            || item.Flag.Invitation || item.Flag.CapturedBeast || item.Flag.Chronicle || item.Flag.Map 
            || item.Flag.Gems || item.Flag.Currency || item.Flag.Divcard || item.Flag.Incubator;
        if (item.Flag.Unique || visibilityCond)
        {
            Form.Visible.BtnPoeDb = false;
        }

        item.UpdateItemData(infodesc.Item);

        string itemQuality = item.Quality;

        if (!item.IsPoe2)
        {
            var cond = item.Flag.Weapon || item.Flag.ArmourPiece || item.Flag.Quivers;
            if (cond) 
            {
                Form.Visible.Sockets = true;
            }
            if (cond || item.Flag.Jewellery)
            {
                Form.Visible.Influences = true;
            }
            if (item.Flag.Weapon || item.Flag.ArmourPiece || item.Flag.Jewellery || item.Flag.Quivers)
            {
                minMaxList.GetModel(StatPanel.CommonMemoryStrand).Min 
                    = item.Option[Resources.Resources.General156_MemoryStrands];
            }
        }

        if (item.IsPoe2 && (item.Flag.Weapon || item.Flag.ArmourPiece))
        {
            Form.Visible.RuneSockets = true;
        }

        if (item.Flag.SanctumResearch)
        {
            var resolve = item.Option[Resources.Resources.General114_SanctumResolve].Split(' ')[0].Split('/', StringSplitOptions.TrimEntries);
            if (resolve.Length is 2)
            {
                minMaxList.GetModel(StatPanel.SanctumResolve).Min = resolve[0];
                minMaxList.GetModel(StatPanel.SanctumMaxResolve).Max = resolve[1];
            }
            minMaxList.GetModel(StatPanel.SanctumInspiration)
                .Min = item.Option[Resources.Resources.General115_SanctumInspiration];
            minMaxList.GetModel(StatPanel.SanctumAureus)
                .Min = item.Option[Resources.Resources.General116_SanctumAureus];
        }

        string specifier = "G";
        var res = minMaxList.GetModel(StatPanel.TotalResistance);
        var life = minMaxList.GetModel(StatPanel.TotalLife);
        var globalEs = minMaxList.GetModel(StatPanel.TotalGlobalEs);
        if (!item.Flag.Map && item.Stats.Resistance > 0)
        {
            res.Min = item.Stats.Resistance.ToString(specifier, CultureInfo.InvariantCulture);
            if (res.Min.Length > 0)
            {
                Form.Visible.TotalRes = true;
                if (dm.Config.Options.AutoSelectRes
                    && (res.Min.ToDoubleDefault() >= 36 || item.Flag.Jewel))
                {
                    res.Selected = true;
                }
            }
        }
        if (item.Stats.Life > 0)
        {
            life.Min = item.Stats.Life.ToString(specifier, CultureInfo.InvariantCulture);
            if (life.Min.Length > 0)
            {
                Form.Visible.TotalLife = true;
                if (dm.Config.Options.AutoSelectLife
                    && (life.Min.ToDoubleDefault() >= 40 || item.Flag.Jewel))
                {
                    life.Selected = true;
                }
            }
        }
        if (item.Stats.EnergyShield > 0)
        {
            globalEs.Min = item.Stats.EnergyShield.ToString(specifier, CultureInfo.InvariantCulture);
            if (globalEs.Min.Length > 0)
            {
                if (!item.Flag.ArmourPiece)
                {
                    Form.Visible.TotalEs = true;//!item.IsPoe2;
                    if (dm.Config.Options.AutoSelectGlobalEs //&& !item.IsPoe2
                        && (globalEs.Min.ToDoubleDefault() >= 38 || item.Flag.Jewel))
                    {
                        globalEs.Selected = true;
                    }
                }
                else
                {
                    globalEs.Min = string.Empty;
                }
            }
        }

        if (item.Flag.ShowDetail)
        {
            //TOREDO from scratch:  Form.Detail
            if (item.Flag.Incubator || item.Flag.Gems || item.Flag.Pieces) // || is_essences
            {
                int i = item.Flag.Gems ? 3 : 1;
                Form.Detail = infodesc.Item.Length > 2 ? (item.Flag.Gems ?
                    infodesc.Item[i] : string.Empty) + infodesc.Item[i + 1] : string.Empty;
            }
            else
            {
                int i = item.Flag.Divcard || item.Flag.StackableCurrency ? 2 : 1;
                Form.Detail = infodesc.Item.Length > i + 1 ? infodesc.Item[i] + infodesc.Item[i + 1] : infodesc.Item[^1];

                if (infodesc.Item.Length > i + 1)
                {
                    int v = infodesc.Item[i - 1].TrimStart().IndexOf("Apply: ", StringComparison.Ordinal);
                    Form.Detail += v > -1 ? string.Empty + Strings.LF + Strings.LF + infodesc.Item[i - 1].TrimStart().Split(Strings.LF)[v == 0 ? 0 : 1].TrimEnd() : string.Empty;
                    if (item.Flag.SanctumResearch && infodesc.Item.Length >= 5)
                    {
                        Form.Detail += infodesc.Item[3] + infodesc.Item[4];
                    }
                }
            }

            if (item.Lang is Lang.English)
            {
                Form.Detail = Form.Detail.Replace(Resources.Resources.General097_SClickSplitItem, string.Empty);
                Form.Detail = RegexUtil.DetailPattern().Replace(Form.Detail, string.Empty);
            }
        }
        else
        {
            var unmodSocket = Form.UpdateModList(item);

            var socket = minMaxList.GetModel(StatPanel.CommonSocket);
            if (socket.Min is "6")
            {
                if (unmodSocket || Form.Panel.Sockets.WhiteColor is "6")
                {
                    Form.Condition.SocketColors = true;
                    socket.Selected = true;
                }
            }
            var link = minMaxList.GetModel(StatPanel.CommonLink);
            if (link.Min is "6")
            {
                link.Selected = true;
            }

            if (!item.Flag.Unidentified && item.Flag.Weapon)
            {
                Form.Visible.Damage = true;

                var itemDps = new ItemDamage(item, itemQuality);
                Form.Dps = itemDps.TotalString;

                if (dm.Config.Options.AutoSelectDps && itemDps.Total > 100)
                {
                    minMaxList.GetModel(StatPanel.DamageTotal).Selected = true;
                }

                if (itemDps.TotalMin.Length > 0)
                {
                    minMaxList.GetModel(StatPanel.DamageTotal).Min = itemDps.TotalMin;
                }
                if (itemDps.PysicalMin.Length > 0)
                {
                    minMaxList.GetModel(StatPanel.DamagePhysical).Min = itemDps.PysicalMin;
                }
                if (itemDps.ElementalMin.Length > 0)
                {
                    minMaxList.GetModel(StatPanel.DamageElemental).Min = itemDps.ElementalMin;
                }
                Form.DpsTip = itemDps.Tip;
            }

            if (!item.Flag.Unidentified && item.Flag.ArmourPiece)
            {
                Form.Visible.Defense = true;

                string armour = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General055_Armour].Trim(), string.Empty);
                string energy = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General056_Energy].Trim(), string.Empty);
                string evasion = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General057_Evasion].Trim(), string.Empty);
                string ward = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General095_Ward].Trim(), string.Empty);

                if (armour.Length > 0)
                {
                    var ar = minMaxList.GetModel(StatPanel.DefenseArmour);
                    if (dm.Config.Options.AutoSelectArEsEva) ar.Selected = true;
                    ar.Min = armour;
                }
                if (energy.Length > 0)
                {
                    var es = minMaxList.GetModel(StatPanel.DefenseEnergy);
                    if (dm.Config.Options.AutoSelectArEsEva) es.Selected = true;
                    es.Min = energy;
                }
                if (evasion.Length > 0)
                {
                    var eva = minMaxList.GetModel(StatPanel.DefenseEvasion);
                    if (dm.Config.Options.AutoSelectArEsEva) eva.Selected = true;
                    eva.Min = evasion;
                }
                if (ward.Length > 0)
                {
                    var wrd = minMaxList.GetModel(StatPanel.DefenseWard);
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
        item.UpdateNameAndType();

        var poe2SkillWeapon = item.IsPoe2 && (item.Flag.Wand || item.Flag.Stave || item.Flag.Sceptre);

        bool hasAnyFlag = item.Flag.Unique || item.Flag.Normal || item.Flag.Currency || item.Flag.Map 
            || item.Flag.Waystones || item.Flag.Divcard || item.Flag.CapturedBeast || item.Flag.Gems 
            || item.Flag.Flask || item.Flag.Tincture || item.Flag.Watchstone || item.Flag.Invitation 
            || item.Flag.Logbook || item.Flag.Tablet || item.Flag.Charm || item.Flag.Graft 
            || item.Flag.Unidentified;

        Form.ByBase = item.IsSpecialBase || hasAnyFlag || dm.Config.Options.SearchByType || poe2SkillWeapon;
        Form.ItemName = item.Name;
        Form.ItemBaseType = item.Type;

        var tier = item.UpdateMapNameAndExchangeFlag();

        Form.Rarity.Item = !item.Flag.Waystones && (item.Flag.MapFragment 
            || item.Flag.MiscMapItems || item.IsExchangeCurrency
            || item.Flag.Currency) ? Resources.Resources.General005_Any 
            : item.Flag.FoilVariant ? Resources.Resources.General110_FoilUnique 
            : item.Rarity;

        Form.ItemNameColor = item.Flag.Magic ? Strings.Color.DeepSkyBlue :
            item.Flag.Rare ? Strings.Color.Gold :
            item.Flag.FoilVariant ? Strings.Color.Green :
            item.Flag.Unique ? Strings.Color.Peru : string.Empty;

        Form.ItemBaseTypeColor = item.Flag.Gems ? Strings.Color.Teal : item.Flag.Currency ? Strings.Color.Moccasin : string.Empty;

        if ((item.Flag.Map || item.Flag.Waystones || item.Flag.Watchstone || item.Flag.Invitation || item.Flag.Logbook || item.Flag.ChargedCompass || item.Flag.Voidstone) && !item.Flag.Unique)
        {
            Form.Rarity.Item = Resources.Resources.General010_AnyNU;
            if (!item.Flag.Corrupted)
            {
                Form.CorruptedIndex = 1;
            }
            if (item.Flag.Voidstone)
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
            Form.Rarity.Item = item.Rarity;
        }

        if (!item.IsPoe2 && !item.Flag.Currency && !item.IsExchangeCurrency
            && !item.Flag.CapturedBeast && !item.Flag.Map && !item.Flag.MiscMapItems
            && !item.Flag.Gems)
        {
            Form.Visible.Conditions = true;
        }

        bool hideUserControls = false;
        if (!item.Flag.Invitation && !item.Flag.Map && !item.Flag.AllflameEmber && (item.Flag.Currency
            && !item.Flag.Chronicle && !item.Flag.Ultimatum && !item.Flag.FilledCoffin
            || (item.IsExchangeCurrency && !item.Flag.Tablet && !item.Flag.Waystones)
            || item.Flag.CapturedBeast || item.Flag.MemoryLine))
        {
            hideUserControls = true;

            if (!item.Flag.MirroredTablet && !item.Flag.SanctumResearch && !item.Flag.Corpses && !item.Flag.TrialCoins)
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
        if (hideUserControls && item.Flag.Facetor)
        {
            Form.Visible.Facetor = true;
            Form.Panel.FacetorMin = item.Option[Resources.Resources.Main154_tbFacetor].Replace(" ", string.Empty);
        }
        var level = minMaxList.GetModel(StatPanel.CommonItemLevel);
        if (hideUserControls && (item.Flag.UncutGem || item.Flag.Wombgift))
        {
            Form.Visible.PanelForm = true;
            Form.Visible.Quality = false;
            level.Min = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            level.Selected = true;
        }

        Form.Tab.QuickEnable = true;
        Form.Tab.DetailEnable = true;

        if (item.IsExchangeCurrency && (!item.Flag.Unique || item.Flag.Map))
        {
            Form.Tab.BulkEnable = true;
            Form.Tab.ShopEnable = true;

            bool isMap = item.MapName.Length > 0;

            Form.Bulk.AutoSelect = true;
            Form.Bulk.Args = "pay/equals";
            Form.Bulk.Currency = isMap ? item.MapName : item.Type;
            Form.Bulk.Tier = isMap ? tier : string.Empty;
        }

        // Select Quick or Detail TAB
        if (!(item.Flag.Map && item.Flag.Corrupted) && (item.Flag.StackableCurrency 
            || item.Flag.Map || item.Flag.Gems || item.Flag.CapturedBeast || item.Flag.UltimatumPoe2 || item.Flag.UncutGem
            || (item.IsExchangeCurrency && !item.Flag.Tablet && !item.Flag.Waystones)))
        {
            Form.Tab.DetailSelected = true;
        }
        if (!Form.Tab.DetailSelected)
        {
            Form.Tab.QuickSelected = true;
        }

        if (!(item.IsExchangeCurrency && !item.Flag.Tablet && !item.Flag.Waystones && !item.Flag.Map) 
            && !item.Flag.Chronicle && !item.Flag.CapturedBeast && !item.Flag.Ultimatum)
        {
            Form.Visible.ModSet = true;
            //Form.Visible.ModPercent = item.IsPoe2;
        }
        var qual = minMaxList.GetModel(StatPanel.CommonQuality);
        if (!item.Flag.Unique && (item.Flag.Flask || item.Flag.Tincture || (item.Flag.Normal && item.IsPoe2)))
        {
            var iLvl = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            var baseLevelMin = item.IsPoe2 ? 79 : 84;
            if (int.TryParse(iLvl, out int result) && result >= baseLevelMin)
            {
                qual.Selected = itemQuality.Length > 0
                    && int.Parse(itemQuality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!hideUserControls || item.Flag.Corpses)
        {
            level.Min = RegexUtil.NumericalPattern().Replace(item.Option[item.Flag.Gems ?
                Resources.Resources.General031_Lv : Resources.Resources.General032_ItemLv].Trim(), string.Empty);

            qual.Min = itemQuality;
            Form.Influence.SetInfluences(item.Option);

            if (item.Flag.ArmourPiece || item.Flag.Weapon || item.Flag.Jewellery
                || item.Flag.Flask || item.Flag.Charm)
            {
                var lv = item.Option[Resources.Resources.General031_Lv].Trim();
                var req = item.Option[Resources.Resources.General155_Requires].Split(',')[0];
                minMaxList.GetModel(StatPanel.CommonRequiresLevel).Min = lv.Length > 0 ? lv 
                    : RegexUtil.NumericalPattern().Replace(req, string.Empty);
            }

            if (item.Flag.Unique && !visibilityCond && !item.IsPoe2)
            {
                string nameEn = string.Empty;
                if (item.Lang is Lang.English)
                {
                    nameEn = item.Name;
                }
                else
                {
                    var wordRes = dm.Words.FirstOrDefault(x => x.Name == item.Name);
                    if (wordRes is not null)
                    {
                        nameEn = wordRes.NameEn;
                    }
                }
                if (nameEn.Length > 0)
                {
                    var dust = dm.DustLevel.FirstOrDefault(x => x.Name == nameEn);
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

            Commands.CheckInfluence(null);
            Commands.CheckCondition(null);

            Form.Panel.SynthesisBlight = item.Flag.Map && item.IsBlightMap
                || item.Option[Resources.Resources.General047_Synthesis] is Strings.TrueOption;
            Form.Panel.BlighRavaged = item.Flag.Map && item.IsBlightRavagedMap;

            if (item.Flag.Map)
            {
                level.Min = item.Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty); // 0x20
                level.Max = item.Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;
                Form.Panel.SynthesisBlightLabel = "Blighted";
                Form.Visible.SynthesisBlight = true;
                Form.Visible.BlightRavaged = true;

                if (!item.IsConqMap)
                {
                    Form.Visible.ByBase = false;
                }
                Form.Visible.MapStats = true;

                var mapQuant = minMaxList.GetModel(StatPanel.MapQuantity);
                mapQuant.Min = item.Option[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                var mapRarity = minMaxList.GetModel(StatPanel.MapRarity);
                mapRarity.Min = item.Option[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                var mapPackSize = minMaxList.GetModel(StatPanel.MapPackSize);
                mapPackSize.Min = item.Option[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                var mapScarab = minMaxList.GetModel(StatPanel.MapMoreScarab);
                mapScarab.Min = item.Option[Resources.Resources.General140_MoreScarabs].Replace(" ", string.Empty);
                var mapCurrency = minMaxList.GetModel(StatPanel.MapMoreCurrency);
                mapCurrency.Min = item.Option[Resources.Resources.General139_MoreCurrency].Replace(" ", string.Empty);
                var mapDivCard = minMaxList.GetModel(StatPanel.MapMoreDivCard);
                mapDivCard.Min = item.Option[Resources.Resources.General142_MoreDivinationCards].Replace(" ", string.Empty);
                var mapMoreMap = minMaxList.GetModel(StatPanel.MapMoreMap);
                mapMoreMap.Min = item.Option[Resources.Resources.General141_MoreMaps].Replace(" ", string.Empty);

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

                    StringBuilder sbReward = new(item.Option[Resources.Resources.General071_Reward]);
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
            else if (item.Flag.Waystones)
            {
                level.Min = item.Option[Resources.Resources.General143_WaystoneTier].Replace(" ", string.Empty); // 0x20
                level.Max = item.Option[Resources.Resources.General143_WaystoneTier].Replace(" ", string.Empty); // 0x20
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;

                Form.Visible.MapStats = true;
                minMaxList.GetModel(StatPanel.MapQuantity).Min = item.Option[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapQuantity).Selected = true;
                minMaxList.GetModel(StatPanel.MapRarity).Min = item.Option[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapRarity).Selected = true;
                minMaxList.GetModel(StatPanel.MapPackSize).Min = item.Option[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapPackSize).Selected = true;
                minMaxList.GetModel(StatPanel.MapMonsterRare).Min = item.Option[Resources.Resources.General162_RareMonsters].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapMonsterRare).Selected = true;
                minMaxList.GetModel(StatPanel.MapMonsterMagic).Min = item.Option[Resources.Resources.General161_MagicMonsters].Replace(" ", string.Empty);
                
                Form.Visible.ByBase = false;
                Form.Visible.Quality = false;
            }
            else if (item.Flag.Gems)
            {
                level.Selected = true;
                minMaxList.GetModel(StatPanel.CommonQuality).Selected = itemQuality.Length > 0
                    && int.Parse(itemQuality, CultureInfo.InvariantCulture) > 12;
                if (!item.Flag.Corrupted)
                {
                    Form.CorruptedIndex = 1; // NO
                }
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                //Form.Visible.ModPercent = false;
                Form.Visible.Rarity = false;
            }
            else if (item.Flag.FilledCoffin)
            {
                Form.Visible.ByBase = false;
                Form.Visible.Rarity = false;
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;

                level.Min = item.Option[Resources.Resources.General129_CorpseLevel].Replace(" ", string.Empty);
                level.Selected = true;
            }
            else if (item.Flag.AllflameEmber)
            {
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                Form.Visible.Rarity = false;

                level.Min = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
                level.Selected = true;
            }
            else if (item.Flag.ByType && item.Flag.Normal)
            {
                level.Selected = level.Min.Length > 0
                    && int.Parse(level.Min, CultureInfo.InvariantCulture) > 82;
            }
            else if (!item.Flag.Unique && item.Flag.Cluster)
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

        if (item.Flag.Logbook || item.Flag.Corpses
            || (item.Flag.Flask || item.Flag.Tincture) && !item.Flag.Unique)
        {
            level.Selected = true;
        }

        if (item.Flag.Chronicle || item.Flag.Ultimatum || item.Flag.MirroredTablet 
            || item.Flag.SanctumResearch || item.Flag.TrialCoins)
        {
            Form.Visible.Corrupted = false;
            Form.Visible.Rarity = false;
            Form.Visible.ByBase = false;
            Form.Visible.Quality = false;
            level.Text = Resources.Resources.General067_AreaLevel;
            level.Min = item.Option[Resources.Resources.General067_AreaLevel].Replace(" ", string.Empty);

            if (item.Flag.SanctumResearch)
            {
                bool isTome = dm.Bases.FirstOrDefault(x => x.NameEn is "Forbidden Tome").Name == item.Type;
                if (!isTome)
                {
                    Form.Visible.SanctumFields = true;
                }
            }
            if (item.Flag.SanctumResearch || item.Flag.Chronicle || item.Flag.MirroredTablet 
                || item.Flag.TrialCoins || (item.Flag.Ultimatum && Form.IsPoeTwo))
            {
                level.Selected = true;
            }
            if (item.Flag.Ultimatum && !Form.IsPoeTwo)
            {
                Form.Visible.Reward = true;
                Form.Panel.Reward.UpdateReward(item.Option);
            }
        }

        if (level.Text.Length is 0)
        {
            level.Text = Resources.Resources.Main065_tbiLevel;
        }

        Form.Visible.Detail = item.Flag.ShowDetail;
        Form.Visible.HeaderMod = !item.Flag.ShowDetail;
        Form.Visible.HiddablePanel = Form.Visible.SynthesisBlight || Form.Visible.BlightRavaged;
        Form.Rarity.Index = Form.Rarity.ComboBox.IndexOf(Form.Rarity.Item);

        if (Form.Bulk.AutoSelect)
        {
            _ = Form.SelectExchangeCurrency(Form.Bulk.Args, Form.Bulk.Currency, Form.Bulk.Tier); // Select currency in 'Pay' section
        }
        
        Form.Panel.Row.FillBottomFormLists(minMaxList);
        if (Form.Panel.Row.ThirdRow.Count > 0)
        {
            Form.Panel.Row.UseBorderThickness = true;
        }

        item.TranslateCurrentItemGateway();
        Item = item;

        Form.FillTime = StopWatch.StopAndGetTimeString();
    }

    internal string GetSerialized(string market, bool useSaleType)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var xItem = Form.GetXiletradeItem();
        var isPoe2 = dm.Config.Options.GameVersion is 1;
        if (isPoe2)
        {
            var jsonDataTwo = new JsonDataTwoFactory(dm).Create(xItem, Item, useSaleType, market);
            return dm.Json.Serialize<JsonDataTwo>(jsonDataTwo);
        }
        var jsonData = new JsonDataFactory(dm).Create(xItem, Item, useSaleType, market);
        return dm.Json.Serialize<JsonData>(jsonData);
    }

    internal string GetSerialized(string market)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var isPoe2 = dm.Config.Options.GameVersion is 1;
        var xItem = Form.GetXiletradeItem(customSearch: true);
        var search = Form.CustomSearch.Search;
        var unid = Form.CustomSearch.UnidUniquesIndex > 0 ?
            Form.CustomSearch.UnidUniques[Form.CustomSearch.UnidUniquesIndex] : null;

        if (isPoe2)
        {
            var jsonDataTwo = new JsonDataTwoFactory(dm).Create(xItem, unid, market, search);
            return dm.Json.Serialize<JsonDataTwo>(jsonDataTwo);
        }
        var jsonData = new JsonDataFactory(dm).Create(xItem, unid, market, search);
        return dm.Json.Serialize<JsonData>(jsonData);
    }
}

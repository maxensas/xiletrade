using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
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

    internal string ClipboardText { get; private set; } = string.Empty;
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
        GestureList.Add(new (Commands.WheelIncrementCommand, ModifierKey.None, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelDecrementCommand, ModifierKey.None, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Down));
    }

    //internal methods
    internal void InitViewModels(bool useBulk = false)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        ViewScale = dm.Config.Options.Scale;

        Form = new(_serviceProvider, useBulk);
        Result = new(_serviceProvider);
        Ninja = new(_serviceProvider);
    }

    internal void OpenUrlTask(string url, UrlType type)
    {
        Task.Run(() =>
        {
            try
            {
                TaskManager.MainUpdaterTask?.Wait();

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

    internal void RunMainUpdaterTask(string itemText, bool openWindow)
    {
        var token = TaskManager.GetMainUpdaterToken(initCts: true);
        TaskManager.MainUpdaterTask = Task.Run(() =>
        {
            try
            {
                var infoDesc = new InfoDescription(itemText);
                if (!infoDesc.IsPoeItem)
                {
                    return;
                }
                ClipboardText = itemText;
                UpdateMainViewModel(infoDesc.Item);
                token.ThrowIfCancellationRequested();
                if (openWindow)
                {
                    _serviceProvider.GetRequiredService<INavigationService>().ShowMainView();
                    UpdatePrices(minimumStock: 0);
                }
            }
            catch (OperationCanceledException)
            {
                //not used
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Exception raised : {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Item parsing error : method UpdateMainViewModel", MessageStatus.Error);
            }
        }, token);
    }

    internal void UpdatePrices(int minimumStock, bool isExchange = false)
    {
        try
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            XiletradeItem xiletradeItem = isExchange ? null : Form.GetXiletradeItem();

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

                if (dm.Config.Options.Language is not 8 and not 9 && !Form.IsPoeTwo)
                {
                    TaskManager.NinjaTask = Ninja.TryUpdatePriceTask(xiletradeItem);
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
                    entity[0] = new() { Json.GetSerialized(dm, xiletradeItem, Item, true, Form.Market[Form.MarketIndex]) };
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

    //private methods
    private void UpdateMainViewModel(string[] clipData)
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var item = Form.FillModList(clipData);
        
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

        Form.CorruptedIndex = item.Flag.Corrupted && dm.Config.Options.AutoSelectCorrupt ? 2 : 0;

        if (item.Flag.Rare && !item.Flag.Map && !item.Flag.CapturedBeast) Form.Tab.PoePriceEnable = true;

        Form.Visible.Corrupted = true;
        if (item.Flag.Incubator)
        {
            Form.Visible.Corrupted = false;
        }

        if (item.Flag.Unique || item.Flag.Unidentified || item.Flag.Watchstone || item.Flag.MapFragment
            || item.Flag.Invitation || item.Flag.CapturedBeast || item.Flag.Chronicle || item.Flag.Map || item.Flag.Gems || item.Flag.Currency || item.Flag.Divcard || item.Flag.Incubator)
        {
            Form.Visible.BtnPoeDb = false;
        }

        item.UpdateItemData(clipData);

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
        if (item.Stats.Resistance > 0)
        {
            res.Min = item.Stats.Resistance.ToString(specifier, CultureInfo.InvariantCulture);
            if (res.Min.Length > 0)
            {
                Form.Visible.TotalRes = !item.IsPoe2;
                if (dm.Config.Options.AutoSelectRes && !item.IsPoe2
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
                Form.Visible.TotalLife = !item.IsPoe2;
                if (dm.Config.Options.AutoSelectLife && !item.IsPoe2
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
                    Form.Visible.TotalEs = !item.IsPoe2;
                    if (dm.Config.Options.AutoSelectGlobalEs && !item.IsPoe2
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
                Form.Detail = clipData.Length > 2 ? (item.Flag.Gems ?
                    clipData[i] : string.Empty) + clipData[i + 1] : string.Empty;
            }
            else
            {
                int i = item.Flag.Divcard || item.Flag.StackableCurrency ? 2 : 1;
                Form.Detail = clipData.Length > i + 1 ? clipData[i] + clipData[i + 1] : clipData[^1];

                if (clipData.Length > i + 1)
                {
                    int v = clipData[i - 1].TrimStart().IndexOf("Apply: ", StringComparison.Ordinal);
                    Form.Detail += v > -1 ? string.Empty + Strings.LF + Strings.LF + clipData[i - 1].TrimStart().Split(Strings.LF)[v == 0 ? 0 : 1].TrimEnd() : string.Empty;
                    if (item.Flag.SanctumResearch && clipData.Length >= 5)
                    {
                        Form.Detail += clipData[3] + clipData[4];
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

        Form.ItemName = item.Name;

        var byBase = !item.Flag.Unique && !item.Flag.Normal && !item.Flag.Currency && !item.Flag.Map && !item.Flag.Divcard
            && !item.Flag.CapturedBeast && !item.Flag.Gems && !item.Flag.Flask && !item.Flag.Tincture && !item.Flag.Unidentified
            && !item.Flag.Watchstone && !item.Flag.Invitation && !item.Flag.Logbook && !item.IsSpecialBase && !item.Flag.Tablet
            && !item.Flag.Charm;

        var poe2SkillWeapon = item.IsPoe2 && (item.Flag.Wand || item.Flag.Stave || item.Flag.Sceptre);
        Form.ByBase = !byBase || dm.Config.Options.SearchByType || poe2SkillWeapon;
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
            && !item.Flag.Chronicle && !item.Flag.Ultimatum && !item.Flag.FilledCoffin || (item.IsExchangeCurrency && !item.Flag.Tablet && !item.Flag.Waystones)
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
        if (hideUserControls && item.Flag.UncutGem)
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
        if (!item.Flag.Unique && (item.Flag.Flask || item.Flag.Tincture))
        {
            var iLvl = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            if (int.TryParse(iLvl, out int result) && result >= 84)
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

                minMaxList.GetModel(StatPanel.MapQuantity).Min = item.Option[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapRarity).Min = item.Option[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapPackSize).Min = item.Option[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapMoreScarab).Min = item.Option[Resources.Resources.General140_MoreScarabs].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapMoreCurrency).Min = item.Option[Resources.Resources.General139_MoreCurrency].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapMoreDivCard).Min = item.Option[Resources.Resources.General142_MoreDivinationCards].Replace(" ", string.Empty);
                minMaxList.GetModel(StatPanel.MapMoreMap).Min = item.Option[Resources.Resources.General141_MoreMaps].Replace(" ", string.Empty);

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
            Form.SelectExchangeCurrency(Form.Bulk.Args, Form.Bulk.Currency, Form.Bulk.Tier); // Select currency in 'Pay' section
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
}

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
    private bool isSelectionEnabled = true;

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
                : string.Empty;
                var caption = type is UrlType.PoeDb ? "Redirection to poedb failed "
                : type is UrlType.PoeWiki ? "Redirection to wiki failed "
                : type is UrlType.Ninja ? "Redirection to ninja failed "
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

                maxFetch = (int)DataManager.Config.Options.SearchFetchDetail;

                if (DataManager.Config.Options.Language is not 8 and not 9 && !Form.IsPoeTwo)
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
                    maxFetch = (int)DataManager.Config.Options.SearchFetchBulk;
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
                entity[0] = new() { Json.GetSerialized(xiletradeItem, Item, true, Form.Market[Form.MarketIndex]) };
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
        var item = Form.FillModList(clipData);

        if (item.Option[Resources.Resources.General036_Socket].Length > 0)
        {
            Form.Panel.Common.Update(item);
            if (!item.IsPoe2)
            {
                Form.Condition.SocketColorsToolTip = Form.Panel.Common.GetSocketColors();
            }
        }

        if (item.Flag.ScourgedMap)
        {
            Form.Panel.Scourged = true;
        }

        if (item.IsPoe2 || item.Flag.Mirrored || item.Flag.Corrupted)
        {
            Form.SetModCurrent();
        }

        Form.CorruptedIndex = item.Flag.Corrupted && DataManager.Config.Options.AutoSelectCorrupt ? 2 : 0;

        if (item.Flag.Rare && !item.Flag.MapCategory && !item.Flag.CapturedBeast) Form.Tab.PoePriceEnable = true;

        Form.Visible.Corrupted = true;
        if (item.Flag.Incubator)
        {
            Form.Visible.Corrupted = false;
        }

        if (item.Flag.Unique || item.Flag.Unidentified || item.Flag.Watchstone || item.Flag.MapFragment
            || item.Flag.Invitation || item.Flag.CapturedBeast || item.Flag.Chronicle || item.Flag.MapCategory || item.Flag.Gems || item.Flag.Currency || item.Flag.Divcard || item.Flag.Incubator)
        {
            Form.Visible.BtnPoeDb = false;
        }

        item.UpdateItemData(clipData);

        string itemQuality = item.Quality;

        if (!item.IsPoe2 && (item.Flag.Weapon || item.Flag.ArmourPiece || item.Flag.Quivers))
        {
            Form.Visible.Sockets = true;
            Form.Visible.Influences = true;
        }

        if (item.IsPoe2 && (item.Flag.Weapon || item.Flag.ArmourPiece))
        {
            Form.Visible.RuneSockets = true;
        }

        bool showRes = false, showLife = false, showEs = false;
        if (Form.Panel.Total.Resistance.Min.Length > 0)
        {
            showRes = true;
            if (DataManager.Config.Options.AutoSelectRes && !item.IsPoe2
                && (Form.Panel.Total.Resistance.Min.ToDoubleDefault() >= 36 || item.Flag.Jewel))
            {
                Form.Panel.Total.Resistance.Selected = true;
            }
        }
        if (Form.Panel.Total.Life.Min.Length > 0)
        {
            showLife = true;
            if (DataManager.Config.Options.AutoSelectLife && !item.IsPoe2
                && (Form.Panel.Total.Life.Min.ToDoubleDefault() >= 40 || item.Flag.Jewel))
            {
                Form.Panel.Total.Life.Selected = true;
            }
        }
        if (Form.Panel.Total.GlobalEs.Min.Length > 0)
        {
            if (!item.Flag.ArmourPiece)
            {
                showEs = true;
                if (DataManager.Config.Options.AutoSelectGlobalEs && !item.IsPoe2
                    && (Form.Panel.Total.GlobalEs.Min.ToDoubleDefault() >= 38 || item.Flag.Jewel))
                {
                    Form.Panel.Total.GlobalEs.Selected = true;
                }
            }
            else
            {
                Form.Panel.Total.GlobalEs.Min = string.Empty;
            }
        }
        Form.Visible.TotalLife = showLife;
        Form.Visible.TotalRes = !item.IsPoe2 && showRes;
        Form.Visible.TotalEs = !item.IsPoe2 && showEs;

        if (item.Flag.ShowDetail)
        {
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
            Form.UpdateModList(item);

            if (!item.Flag.Unidentified && item.Flag.Weapon)
            {
                Form.Visible.Damage = true;

                var itemDps = new ItemDamage(item, itemQuality);
                Form.Dps = itemDps.TotalString;

                if (DataManager.Config.Options.AutoSelectDps && itemDps.Total > 100)
                {
                    Form.Panel.Damage.Total.Selected = true;
                }

                if (itemDps.TotalMin.Length > 0)
                {
                    Form.Panel.Damage.Total.Min = itemDps.TotalMin;
                }
                if (itemDps.PysicalMin.Length > 0)
                {
                    Form.Panel.Damage.Physical.Min = itemDps.PysicalMin;
                }
                if (itemDps.ElementalMin.Length > 0)
                {
                    Form.Panel.Damage.Elemental.Min = itemDps.ElementalMin;
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
                    if (DataManager.Config.Options.AutoSelectArEsEva) Form.Panel.Defense.Armour.Selected = true;
                    Form.Panel.Defense.Armour.Min = armour;
                }
                if (energy.Length > 0)
                {
                    if (DataManager.Config.Options.AutoSelectArEsEva) Form.Panel.Defense.Energy.Selected = true;
                    Form.Panel.Defense.Energy.Min = energy;
                }
                if (evasion.Length > 0)
                {
                    if (DataManager.Config.Options.AutoSelectArEsEva) Form.Panel.Defense.Evasion.Selected = true;
                    Form.Panel.Defense.Evasion.Min = evasion;
                }

                if (ward.Length > 0)
                {
                    if (DataManager.Config.Options.AutoSelectArEsEva) Form.Panel.Defense.Ward.Selected = true;
                    Form.Panel.Defense.Ward.Min = ward;
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
        item.UpdateBaseName();

        Form.ItemName = item.Base.Name;

        var byBase = !item.Flag.Unique && !item.Flag.Normal && !item.Flag.Currency && !item.Flag.MapCategory && !item.Flag.Divcard
            && !item.Flag.CapturedBeast && !item.Flag.Gems && !item.Flag.Flask && !item.Flag.Tincture && !item.Flag.Unidentified
            && !item.Flag.Watchstone && !item.Flag.Invitation && !item.Flag.Logbook && !item.Flag.SpecialBase && !item.Flag.Tablet;

        var poe2SkillWeapon = item.IsPoe2 && (item.Flag.Wand || item.Flag.Stave || item.Flag.Sceptre);
        Form.ByBase = !byBase || DataManager.Config.Options.SearchByType || poe2SkillWeapon;

        string qualType = Form.Panel.AlternateGemIndex is 1 ? Resources.Resources.General001_Anomalous :
            Form.Panel.AlternateGemIndex is 2 ? Resources.Resources.General002_Divergent :
            Form.Panel.AlternateGemIndex is 3 ? Resources.Resources.General003_Phantasmal : string.Empty;

        Form.ItemBaseType = qualType.Length > 0 ?
            item.Lang is Lang.French or Lang.Spanish ? item.Type + " " + qualType
            : item.Lang is Lang.German ? item.Type + " (" + qualType + ")"
            : item.Lang is Lang.Russian ? qualType + ": " + item.Type
            : qualType + " " + item.Type // en,kr,br,th,tw,cn
            : item.Type;

        string tier = item.Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
        item.UpdateMapFlag(tier);

        Form.Rarity.Item = !item.Flag.Waystones && (item.Flag.MapFragment 
            || item.Flag.MiscMapItems || item.Flag.ExchangeCurrency 
            || item.Flag.Currency) ? Resources.Resources.General005_Any 
            : item.Flag.FoilVariant ? Resources.Resources.General110_FoilUnique 
            : item.Rarity;

        Form.ItemNameColor = Form.Rarity.Item == Resources.Resources.General008_Magic ? Strings.Color.DeepSkyBlue :
            Form.Rarity.Item == Resources.Resources.General007_Rare ? Strings.Color.Gold :
            Form.Rarity.Item == Resources.Resources.General110_FoilUnique ? Strings.Color.Green :
            Form.Rarity.Item == Resources.Resources.General006_Unique ? Strings.Color.Peru : string.Empty;
        Form.ItemBaseTypeColor = item.Flag.Gems ? Strings.Color.Teal : item.Flag.Currency ? Strings.Color.Moccasin : string.Empty;

        if ((item.Flag.MapCategory || item.Flag.Waystones || item.Flag.Watchstone || item.Flag.Invitation || item.Flag.Logbook || item.Flag.ChargedCompass || item.Flag.Voidstone) && !item.Flag.Unique)
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

        if (!item.IsPoe2 && !item.Flag.Currency && !item.Flag.ExchangeCurrency 
            && !item.Flag.CapturedBeast && !item.Flag.Map && !item.Flag.MiscMapItems)
        {
            Form.Visible.Conditions = true;
        }

        bool hideUserControls = false;
        if (!item.Flag.Invitation && !item.Flag.MapCategory && !item.Flag.AllflameEmber && (item.Flag.Currency
            && !item.Flag.Chronicle && !item.Flag.Ultimatum && !item.Flag.FilledCoffin || item.Flag.ExchangeCurrency
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

        Form.Tab.QuickEnable = true;
        Form.Tab.DetailEnable = true;
        bool uniqueTag = Form.Rarity.Item == Resources.Resources.General006_Unique;
        if (item.Flag.ExchangeCurrency && (!uniqueTag || item.Flag.MapCategory)) // TODO update with item.Is.Unique
        {
            Form.Tab.BulkEnable = true;
            Form.Tab.ShopEnable = true;

            bool isMap = item.MapName.Length > 0;

            Form.Bulk.AutoSelect = true;
            Form.Bulk.Args = "pay/equals";
            Form.Bulk.Currency = isMap ? item.MapName : item.Type;
            Form.Bulk.Tier = isMap ? tier : string.Empty;
        }

        if (item.Flag.ExchangeCurrency || item.Flag.MapCategory || item.Flag.Gems || item.Flag.CapturedBeast) // Select Detailed TAB
        {
            if (!(item.Flag.MapCategory && item.Flag.Corrupted)) // checkMapDetails
            {
                Form.Tab.DetailSelected = true;
            }
        }
        if (!Form.Tab.DetailSelected)
        {
            Form.Tab.QuickSelected = true;
        }

        if (!item.Flag.ExchangeCurrency && !item.Flag.Chronicle && !item.Flag.CapturedBeast && !item.Flag.Ultimatum)
        {
            Form.Visible.ModSet = !item.IsPoe2;
            Form.Visible.ModPercent = item.IsPoe2;

            Form.Visible.ModCurrent = true;
        }

        if (!item.Flag.Unique && (item.Flag.Flask || item.Flag.Tincture))
        {
            var iLvl = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            if (int.TryParse(iLvl, out int result) && result >= 84)
            {
                Form.Panel.Common.Quality.Selected = itemQuality.Length > 0
                    && int.Parse(itemQuality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!hideUserControls || item.Flag.Corpses)
        {
            Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(item.Option[item.Flag.Gems ?
                Resources.Resources.General031_Lv : Resources.Resources.General032_ItemLv].Trim(), string.Empty);

            Form.Panel.Common.Quality.Min = itemQuality;
            Form.Influence.SetInfluences(item.Option);

            MainCommand.CheckInfluence(null);
            MainCommand.CheckCondition(null);

            Form.Panel.SynthesisBlight = item.Flag.MapCategory && item.Flag.BlightMap
                || item.Option[Resources.Resources.General047_Synthesis] is Strings.TrueOption;
            Form.Panel.BlighRavaged = item.Flag.MapCategory && item.Flag.BlightRavagedMap;

            if (item.Flag.MapCategory)
            {
                Form.Panel.Common.ItemLevel.Min = item.Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty); // 0x20
                Form.Panel.Common.ItemLevel.Max = item.Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);

                Form.Panel.Common.ItemLevelLabel = Resources.Resources.Main094_lbTier;

                Form.Panel.Common.ItemLevel.Selected = true;
                Form.Panel.SynthesisBlightLabel = "Blighted";
                Form.Visible.SynthesisBlight = true;
                Form.Visible.BlightRavaged = true;
                Form.Visible.Scourged = false;

                Form.Visible.ByBase = false;
                Form.Visible.ModSet = false;
                Form.Visible.ModCurrent = false;

                Form.Visible.MapStats = true;

                Form.Panel.Map.Quantity.Min = item.Option[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                Form.Panel.Map.Rarity.Min = item.Option[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                Form.Panel.Map.PackSize.Min = item.Option[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                Form.Panel.Map.MoreScarab.Min = item.Option[Resources.Resources.General140_MoreScarabs].Replace(" ", string.Empty);
                Form.Panel.Map.MoreCurrency.Min = item.Option[Resources.Resources.General139_MoreCurrency].Replace(" ", string.Empty);
                Form.Panel.Map.MoreDivCard.Min = item.Option[Resources.Resources.General142_MoreDivinationCards].Replace(" ", string.Empty);
                Form.Panel.Map.MoreMap.Min = item.Option[Resources.Resources.General141_MoreMaps].Replace(" ", string.Empty);

                if (Form.Panel.Common.ItemLevel.Min is "17" && Form.Panel.Common.ItemLevel.Max is "17")
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
                    }
                }
            }
            else if (item.Flag.Gems)
            {
                Form.Panel.Common.ItemLevel.Selected = true;
                Form.Panel.Common.Quality.Selected = itemQuality.Length > 0
                    && int.Parse(itemQuality, CultureInfo.InvariantCulture) > 12;
                if (!item.Flag.Corrupted)
                {
                    Form.CorruptedIndex = 1; // NO
                }
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                Form.Visible.ModCurrent = false;
                Form.Visible.Rarity = false;
            }
            else if (item.Flag.FilledCoffin)
            {
                Form.Visible.ByBase = false;
                Form.Visible.Rarity = false;
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;

                Form.Panel.Common.ItemLevel.Min = item.Option[Resources.Resources.General129_CorpseLevel].Replace(" ", string.Empty);
                Form.Panel.Common.ItemLevel.Selected = true;
            }
            else if (item.Flag.AllflameEmber)
            {
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                Form.Visible.ModCurrent = false;
                Form.Visible.Rarity = false;

                Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(item.Option[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
                Form.Panel.Common.ItemLevel.Selected = true;
            }
            else if (item.Flag.ByType && item.Flag.Normal)
            {
                Form.Panel.Common.ItemLevel.Selected = Form.Panel.Common.ItemLevel.Min.Length > 0
                    && int.Parse(Form.Panel.Common.ItemLevel.Min, CultureInfo.InvariantCulture) > 82;
            }
            else if (Form.Rarity.Item != Resources.Resources.General006_Unique && item.Flag.Cluster)
            {
                Form.Panel.Common.ItemLevel.Selected = Form.Panel.Common.ItemLevel.Min.Length > 0
                    && int.Parse(Form.Panel.Common.ItemLevel.Min, CultureInfo.InvariantCulture) >= 78;
                if (Form.Panel.Common.ItemLevel.Min.Length > 0)
                {
                    int minVal = int.Parse(Form.Panel.Common.ItemLevel.Min, CultureInfo.InvariantCulture);
                    if (minVal >= 84)
                    {
                        Form.Panel.Common.ItemLevel.Min = "84";
                    }
                    else if (minVal >= 78)
                    {
                        Form.Panel.Common.ItemLevel.Min = "78";
                    }
                }
            }
        }

        if ((item.Flag.Flask || item.Flag.Tincture) && !item.Flag.Unique)
        {
            Form.Panel.Common.ItemLevel.Selected = true;
        }

        if (item.Flag.Logbook)
        {
            Form.Panel.Common.ItemLevel.Selected = true;
        }

        if (item.Flag.ConqMap)
        {
            Form.Visible.ByBase = true;
        }

        if (item.Flag.Chronicle || item.Flag.Ultimatum || item.Flag.MirroredTablet || item.Flag.SanctumResearch || item.Flag.TrialCoins || item.Flag.Waystones)
        {
            Form.Visible.Corrupted = false;
            Form.Visible.Rarity = false;
            Form.Visible.ByBase = false;
            Form.Visible.Quality = false;
            Form.Panel.Common.ItemLevelLabel = Resources.Resources.General067_AreaLevel;

            Form.Panel.Common.ItemLevel.Min = item.Option[Resources.Resources.General067_AreaLevel].Replace(" ", string.Empty);

            if (item.Flag.SanctumResearch)
            {
                bool isTome = DataManager.Bases.FirstOrDefault(x => x.NameEn is "Forbidden Tome").Name == item.Type;
                if (!isTome)
                {
                    Form.Visible.SanctumFields = true;
                }
            }
            if (item.Flag.Chronicle || item.Flag.MirroredTablet || item.Flag.TrialCoins)
            {
                Form.Panel.Common.ItemLevel.Selected = true;
            }
            if (item.Flag.Ultimatum) // to update with 'Engraved Ultimatum'
            {
                Form.Visible.Reward = true;
                Form.Panel.Reward.UpdateReward(item.Option);
            }
            if (item.Flag.SanctumResearch)
            {
                Form.Panel.Common.ItemLevel.Selected = true;
            }
        }

        if (item.Flag.Corpses)
        {
            Form.Panel.Common.ItemLevel.Selected = true;
        }

        if (Form.Panel.Common.ItemLevelLabel.Length is 0)
        {
            Form.Panel.Common.ItemLevelLabel = Resources.Resources.Main065_tbiLevel;
        }

        int nbRows = 1;
        if (Form.Visible.Defense || Form.Visible.SanctumFields || Form.Visible.MapStats)
        {
            nbRows++;
            Form.Panel.Row.ArmourMaxHeight = 43;
        }
        if (Form.Visible.Damage || Form.Visible.MapStats)
        {
            nbRows++;
            Form.Panel.Row.WeaponMaxHeight = 43;
        }
        if (Form.Visible.TotalLife || Form.Visible.TotalEs || Form.Visible.TotalRes)
        {
            nbRows++;
            Form.Panel.Row.TotalMaxHeight = 43;
        }

        if (nbRows <= 2)
        {
            Form.Panel.Col.FirstMaxWidth = 0;
            Form.Panel.Col.LastMinWidth = 100;
            if (nbRows <= 1)
            {
                Form.Panel.UseBorderThickness = false;
            }
        }

        Form.Visible.Detail = item.Flag.ShowDetail;
        Form.Visible.HeaderMod = !item.Flag.ShowDetail;
        Form.Visible.HiddablePanel = Form.Visible.SynthesisBlight || Form.Visible.BlightRavaged || Form.Visible.Scourged;
        Form.Rarity.Index = Form.Rarity.ComboBox.IndexOf(Form.Rarity.Item);

        if (Form.Bulk.AutoSelect)
        {
            Form.SelectExchangeCurrency(Form.Bulk.Args, Form.Bulk.Currency, Form.Bulk.Tier); // Select currency in 'Pay' section
        }
        Form.FillTime = StopWatch.StopAndGetTimeString();

        item.Base.TranslateCurrentItemGateway();//temp
        Item = item;
    }
}

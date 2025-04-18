using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Logic;
using Xiletrade.Library.Models.Serializable;
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

    internal string ClipboardText { get; private set; } = string.Empty;
    internal ItemBaseName CurrentItem { get; private set; }
    internal TaskManager Task { get; private set; }
    internal MainPricing Price { get; private set; }

    public MainCommand Commands { get; private set; }
    public TrayMenuCommand TrayCommands { get; private set; }

    public List<MouseGestureCom> GestureList { get; private set; } = new();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        TrayCommands = new(_serviceProvider);
        Commands = new(this, _serviceProvider);
        Task = new(this, _serviceProvider);
        Price = new(this, _serviceProvider);
        NotifyName = "Xiletrade " + Common.GetFileVersion();
        GestureList.Add(new (Commands.WheelIncrementCommand, ModifierKey.None, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelDecrementCommand, ModifierKey.None, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Down));
    }

    internal void InitViewModels(bool useBulk = false)
    {
        Form = new(_serviceProvider, useBulk);
        Result = new(_serviceProvider);
        Ninja = new(_serviceProvider);
    }

    internal void RunMainUpdater(string itemText, bool openWindow, CancellationToken token)
    {
        try
        {
            StringBuilder sbItemText = new(itemText);
            // some "\r" are missing while copying directly from the game, not from website copy
            sbItemText.Replace(Strings.CRLF, Strings.LF).Replace(Strings.LF, Strings.CRLF).Replace("()", string.Empty);
            var clipData = sbItemText.ToString().ArrangeItemInfoDesc().Trim().Split([Strings.ItemInfoDelimiter], StringSplitOptions.None);
            if (clipData[0].StartsWith(Resources.Resources.General004_Rarity, StringComparison.Ordinal)) // Fix until GGG's update
            {
                clipData[0] = Resources.Resources.General126_ItemClassPrefix + " Null" + Strings.CRLF + clipData[0];
            }

            bool isPoeItem = clipData.Length > 1 &&
            clipData[0].StartsWith(Resources.Resources.General126_ItemClassPrefix, StringComparison.Ordinal);
            if (isPoeItem)
            {
                ClipboardText = itemText;
                if (clipData[^1].Contains("~b/o", StringComparison.Ordinal)
                || clipData[^1].Contains("~price", StringComparison.Ordinal))
                {
                    clipData = clipData.Where((source, index) => index != clipData.Length - 1).ToArray(); // clipDataWhitoutPrice
                }

                UpdateMainViewModel(clipData);

                if (openWindow)
                {
                    Form.PriceTime = Price.Watch.StopAndGetTimeString();
                    _serviceProvider.GetRequiredService<INavigationService>().ShowMainView();
                    UpdatePrices(minimumStock: 0);
                }
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
                    string influences = Form.GetInfluenceSate("/");
                    if (influences.Length is 0) influences = Resources.Resources.Main036_None;

                    var nInfo = new NinjaInfo(Form.League[Form.LeagueIndex]
                        , Form.Rarity.Item
                        , Form.Panel.Common.ItemLevel.Min.Trim()
                        , Form.Panel.Common.Quality.Min.Trim()
                        , Form.Panel.AlternateGemIndex
                        , Form.Panel.SynthesisBlight
                        , Form.Panel.BlighRavaged
                        , Form.Panel.Scourged
                        , influences);

                    Task.RunNinjaTask(nInfo, xiletradeItem);
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
                entity[0] = new() { Json.GetSerialized(xiletradeItem, CurrentItem, true, Form.Market[Form.MarketIndex]) };
            }

            var priceInfo = new PricingInfo(entity, Form.League[Form.LeagueIndex]
                , Form.Market[Form.MarketIndex], minimumStock, maxFetch, Form.SameUser);

            Task.RunPriceTask(priceInfo);
        }
        catch (Exception ex)
        {
            throw new Exception("Exception encountered : method UpdateItemPrices", ex);
        }
    }

    internal void UpdateMainViewModel(string[] clipData)
    {
        InitViewModels();

        var item = new ItemBaseName();
        var cultureEn = new CultureInfo(Strings.Culture[0]);
        var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));

        int idLang = DataManager.Config.Options.Language;
        bool isPoe2 = DataManager.Config.Options.GameVersion is 1;
        string specifier = "G";

        string itemInherits = string.Empty, itemId = string.Empty, mapName = string.Empty;

        var data = clipData[0].Trim().Split(Strings.CRLF, StringSplitOptions.None);

        string itemClass = data[0].Split(':')[1].Trim();

        var rarityPrefix = data[1].Split(':');
        string itemRarity = rarityPrefix.Length > 1 ? rarityPrefix[1].Trim() : string.Empty;

        string itemName = data.Length > 3 && data[2].Length > 0 ? data[2] ?? string.Empty : string.Empty;

        string itemType = data.Length > 3 && data[3].Length > 0 ? data[3] ?? string.Empty
            : data.Length > 2 && data[2].Length > 0 ? data[2] ?? string.Empty
            : data.Length > 1 && data[1].Length > 0 ? data[1] ?? string.Empty
            : string.Empty;

        if (DataManager.Config.Options.DevMode && DataManager.Config.Options.Language is not 0)
        {
            var tuple = GetTranslatedItemNameAndType(itemName, itemType);
            itemName = tuple.Item1;
            itemType = tuple.Item2;
        }

        string gemName = string.Empty;

        Strings.dicPublicID.TryGetValue(itemType, out string publicID);
        publicID ??= string.Empty;

        var itemIs = new ItemFlag(clipData, idLang, itemRarity, itemType, itemClass);
        if (itemIs.ScourgedMap)
        {
            itemType = itemType.Replace(Resources.Resources.General103_Scourged, string.Empty).Trim();
        }

        var totalStats = Form.FillModList(clipData, itemIs, itemName, itemType, itemClass, idLang, out Dictionary<string, string> listOptions);

        if (totalStats.Resistance > 0)
        {
            Form.Panel.Total.Resistance.Min = totalStats.Resistance.ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (totalStats.Life > 0)
        {
            Form.Panel.Total.Life.Min = totalStats.Life.ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (totalStats.EnergyShield > 0)
        {
            Form.Panel.Total.GlobalEs.Min = totalStats.EnergyShield.ToString(specifier, CultureInfo.InvariantCulture);
        }

        if (itemIs.SanctumResearch)
        {
            var resolve = listOptions[Resources.Resources.General114_SanctumResolve].Split(' ')[0].Split('/', StringSplitOptions.TrimEntries);
            if (resolve.Length is 2)
            {
                Form.Panel.Sanctum.Resolve.Min = resolve[0];
                Form.Panel.Sanctum.MaximumResolve.Max = resolve[1];
            }
            Form.Panel.Sanctum.Inspiration.Min = listOptions[Resources.Resources.General115_SanctumInspiration];
            Form.Panel.Sanctum.Aureus.Min = listOptions[Resources.Resources.General116_SanctumAureus];
        }

        itemIs.Unidentified = listOptions[Resources.Resources.General039_Unidentify] == Strings.TrueOption;
        itemIs.Corrupted = listOptions[Resources.Resources.General037_Corrupt] == Strings.TrueOption;
        itemIs.Mirrored = listOptions[Resources.Resources.General109_Mirrored] == Strings.TrueOption;
        itemIs.FoilVariant = listOptions[Resources.Resources.General110_FoilUnique] == Strings.TrueOption;
        itemIs.ScourgedItem = listOptions[Resources.Resources.General099_ScourgedItem] == Strings.TrueOption;
        itemIs.MapCategory = listOptions[Resources.Resources.General034_MaTier].Length > 0 && !itemIs.Divcard;

        if (!isPoe2 && listOptions[Resources.Resources.General036_Socket].Length > 0)
        {
            string socket = listOptions[Resources.Resources.General036_Socket];
            int white = socket.Length - socket.Replace("W", string.Empty).Length;
            int red = socket.Length - socket.Replace("R", string.Empty).Length;
            int green = socket.Length - socket.Replace("G", string.Empty).Length;
            int blue = socket.Length - socket.Replace("B", string.Empty).Length;

            var scklinks = socket.Split(' ');
            int lnkcnt = 0;
            for (int s = 0; s < scklinks.Length; s++)
            {
                if (lnkcnt < scklinks[s].Length)
                    lnkcnt = scklinks[s].Length;
            }
            int link = lnkcnt < 3 ? 0 : lnkcnt - (int)Math.Ceiling((double)lnkcnt / 2) + 1;

            Form.Panel.Common.Sockets.RedColor = red.ToString();
            Form.Panel.Common.Sockets.GreenColor = green.ToString();
            Form.Panel.Common.Sockets.BlueColor = blue.ToString();
            Form.Panel.Common.Sockets.WhiteColor = white.ToString();

            StringBuilder sbColors = new(Resources.Resources.Main210_cbSocketColorsTip);
            sbColors.AppendLine();
            sbColors.Append(Resources.Resources.Main209_cbSocketColors).Append(" : ");
            sbColors.Append(Form.Panel.Common.Sockets.RedColor).Append('R').Append(' ');
            sbColors.Append(Form.Panel.Common.Sockets.GreenColor).Append('G').Append(' ');
            sbColors.Append(Form.Panel.Common.Sockets.BlueColor).Append('B').Append(' ');
            sbColors.Append(Form.Panel.Common.Sockets.WhiteColor).Append('W');
            Form.Condition.SocketColorsToolTip = sbColors.ToString();

            Form.Panel.Common.Sockets.SocketMin = (white + red + green + blue).ToString();
            Form.Panel.Common.Sockets.LinkMin = link > 0 ? link.ToString() : string.Empty;
            Form.Panel.Common.Sockets.Selected = link > 4;
        }

        if (isPoe2 && listOptions[Resources.Resources.General036_Socket].Length > 0)
        {
            string socket = listOptions[Resources.Resources.General036_Socket];
            int count = socket.Split('S').Length - 1;
            Form.Panel.Common.RuneSockets.Selected = itemIs.Corrupted && count >= 1;
            Form.Panel.Common.RuneSockets.Min = count.ToString();
            if (itemIs.Corrupted)
            {
                Form.Panel.Common.RuneSockets.Max = Form.Panel.Common.RuneSockets.Min;
            }
        }

        if (itemIs.ScourgedMap)
        {
            Form.Panel.Scourged = true;
        }

        if (itemIs.Mirrored)
        {
            Form.SetModCurrent();
        }

        //if no option is selected in config, use item corruption status. Otherwise, use the config value
        Form.CorruptedIndex = itemIs.Divcard ? 0 : itemIs.Corrupted ? 2 : 0;

        if (itemIs.Rare && !itemIs.MapCategory && !itemIs.CapturedBeast) Form.Tab.PoePriceEnable = true;
        if (itemIs.Gem)
        {
            Form.Visible.AlternateGem = false; // TO remove
        }

        if (itemIs.Incubator)
        {
            Form.Visible.Corrupted = false;
        }

        if (itemIs.Unique || itemIs.Unidentified || itemIs.Watchstone || itemIs.MapFragment
            || itemIs.Invitation || itemIs.CapturedBeast || itemIs.Chronicle || itemIs.MapCategory || itemIs.Gem || itemIs.Currency || itemIs.Divcard || itemIs.Incubator)
        {
            Form.Visible.BtnPoeDb = false;
        }

        if (itemIs.CapturedBeast)
        {
            var tmpBaseType = DataManager.Monsters.FirstOrDefault(x => x.Name.Contains(itemType, StringComparison.Ordinal));
            if (tmpBaseType is not null)
            {
                itemId = tmpBaseType.Id;
                itemInherits = tmpBaseType.InheritsFrom;
            }
        }
        if (!itemIs.CapturedBeast)
        {
            if (itemIs.Gem)
            {
                Form.Panel.AlternateGemIndex = listOptions[Strings.AlternateGem] is Strings.Gem.Anomalous ? 1 :
                    listOptions[Strings.AlternateGem] is Strings.Gem.Divergent ? 2 :
                    listOptions[Strings.AlternateGem] is Strings.Gem.Phantasmal ? 3 : 0;

                StringBuilder sbType = new(itemType);
                sbType.Replace(Resources.Resources.General001_Anomalous, string.Empty)
                    .Replace(Resources.Resources.General002_Divergent, string.Empty)
                    .Replace(Resources.Resources.General003_Phantasmal, string.Empty).Replace("()", string.Empty);
                itemType = sbType.ToString().Trim();
                if (itemType.StartsWith(':'))
                {
                    itemType = itemType[1..].Trim();
                }

                if (listOptions[Resources.Resources.General037_Corrupt] is Strings.TrueOption
                    && listOptions[Resources.Resources.General038_Vaal] is Strings.TrueOption)
                {
                    for (int i = 3; i < clipData.Length; i++)
                    {
                        string seekVaal = clipData[i].Replace(Strings.CRLF, string.Empty).Trim();
                        var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == seekVaal);
                        if (tmpBaseType is not null)
                        {
                            gemName = itemType;
                            itemType = tmpBaseType.Name;
                            break;
                        }
                    }
                }
            }

            if ((itemIs.Unidentified || itemIs.Normal) && itemType.Contains(Resources.Resources.General030_Higher))
            {
                if (idLang is 2) // fr
                {
                    itemType = itemType.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                    itemType = itemType.Replace(Resources.Resources.General030_Higher + "e", string.Empty).Trim();
                }
                if (idLang is 3) // es
                {
                    itemType = itemType.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                }
                itemType = itemType.Replace(Resources.Resources.General030_Higher, string.Empty).Trim();
            }

            if (itemIs.MapCategory && itemType.Length > 5)
            {
                if (itemType.Contains(Resources.Resources.General040_Blighted, StringComparison.Ordinal))
                {
                    itemIs.BlightMap = true;
                }
                else if (itemType.Contains(Resources.Resources.General100_BlightRavaged, StringComparison.Ordinal))
                {
                    itemIs.BlightRavagedMap = true;
                }
            }
            else if (listOptions[Resources.Resources.General047_Synthesis] is Strings.TrueOption)
            {
                if (itemType.Contains(Resources.Resources.General048_Synthesised, StringComparison.Ordinal))
                {
                    if (idLang is 2)
                    {
                        itemType = itemType.Replace(Resources.Resources.General048_Synthesised + "e", string.Empty).Trim(); // french female item name
                    }
                    if (idLang is 4)
                    {
                        StringBuilder iType = new(itemType);
                        iType.Replace(Resources.Resources.General048_Synthesised + "s", string.Empty) // german
                            .Replace(Resources.Resources.General048_Synthesised + "r", string.Empty); // german
                        itemType = iType.ToString().Trim();
                    }
                    if (idLang is 6)
                    {
                        StringBuilder iType = new(itemType);
                        iType.Replace(Resources.Resources.General048_Synthesised + "ый", string.Empty) // russian
                            .Replace(Resources.Resources.General048_Synthesised + "ое", string.Empty) // russian
                            .Replace(Resources.Resources.General048_Synthesised + "ая", string.Empty); // russian
                        itemType = iType.ToString().Trim();
                    }
                    itemType = itemType.Replace(Resources.Resources.General048_Synthesised, string.Empty).Trim();
                }
            }

            if (!itemIs.Unidentified && !itemIs.MapCategory && itemIs.Magic)
            {
                var resultName =
                    from result in DataManager.Bases
                    where result.Name.Length > 0 && itemType.Contains(result.Name, StringComparison.Ordinal)
                    && !result.Id.StartsWith("Gems", StringComparison.Ordinal)
                    select result.Name;
                if (resultName.Any())
                {
                    //itemType = resultName.First();
                    string longestName = string.Empty;
                    foreach (var result in resultName)
                    {
                        if (result.Length > longestName.Length)
                        {
                            longestName = result;
                        }
                    }
                    if (itemIs.MemoryLine)
                    {
                        itemName = itemType;
                    }
                    itemType = longestName;
                }
            }
            var tmpBaseType2 = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);
            if (tmpBaseType2 is not null)
            {
                // 3.14 : to remove and replace by itemClass
                //Strings.lpublicID.TryGetValue(tmpBaseType2.NameEn, out publicID);
                itemIs.SpecialBase = Strings.lSpecialBases.Contains(tmpBaseType2.NameEn);
            }
        }

        if (itemInherits.Length is 0)
        {
            if (itemIs.MapCategory || itemIs.Waystones)
            {
                //bool isGuardian = IsGuardianMap(itemType, out string guardName);
                if (!itemIs.Unidentified && itemIs.Magic)
                {
                    var affixes =
                        from result in DataManager.Mods
                        from names in result.Name.Split('/')
                        where names.Length > 0 && itemType.Contains(names, StringComparison.Ordinal)
                        select names;
                    if (affixes.Any())
                    {
                        foreach (string str in affixes)
                        {
                            itemType = itemType.Replace(str, string.Empty).Trim();
                        }
                    }
                }

                string mapKind = itemIs.BlightMap || itemIs.BlightRavagedMap ? Strings.CurrencyTypePoe1.MapsBlighted :
                    itemIs.Unique ? Strings.CurrencyTypePoe1.MapsUnique : Strings.CurrencyTypePoe1.Maps;

                var mapId =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where (result.Id == mapKind || result.Id == Strings.CurrencyTypePoe2.Waystones)
                    && (Entrie.Text.StartsWith(itemType, StringComparison.Ordinal)
                    || Entrie.Text.EndsWith(itemType, StringComparison.Ordinal))
                    select Entrie.Id;
                if (mapId.Any())
                {
                    itemId = mapId.First();
                }

                itemInherits = itemIs.MapCategory ? "Maps/AbstractMap" : "Waystones";
            }
            else if (itemIs.Currency || itemIs.Divcard || itemIs.MapFragment || itemIs.Incubator)
            {
                var curResult =
                    from resultDat in DataManager.Currencies
                    from Entrie in resultDat.Entries
                    where Entrie.Text == itemType
                    select (Entrie.Id, resultDat.Id);
                if (curResult.Any())
                {
                    itemId = curResult.FirstOrDefault().Item1;
                    string cur = curResult.FirstOrDefault().Item2;

                    itemInherits = cur is Strings.CurrencyTypePoe1.Cards ? "DivinationCards/DivinationCardsCurrency"
                        : cur is Strings.CurrencyTypePoe1.DelveResonators ? "Delve/DelveSocketableCurrency"
                        : cur is Strings.CurrencyTypePoe1.Fragments && itemId != "ritual-vessel"
                        && itemId != "valdos-puzzle-box" ? "MapFragments/AbstractMapFragment"
                        : cur is Strings.CurrencyTypePoe1.Incubators ? "Legion/Incubator"
                        : "Currency/StackableCurrency";
                }
            }
            else if (itemIs.Gem)
            {
                var findGem = DataManager.Gems.FirstOrDefault(x => x.Name == itemType);
                if (findGem is not null)
                {
                    if (gemName.Length is 0 && findGem.Type != findGem.Name) // transfigured normal gem
                    {
                        itemType = findGem.Type;
                        itemInherits = Strings.Inherit.Gems + '/' + findGem.Disc;
                    }
                    if (gemName.Length > 0 && findGem.Type == findGem.Name)
                    {
                        var findGem2 = DataManager.Gems.FirstOrDefault(x => x.Name == gemName);
                        if (findGem2 is not null) // transfigured vaal gem
                        {
                            itemInherits = Strings.Inherit.Gems + '/' + findGem2.Disc;
                        }
                    }
                }
            }

            if (itemInherits.Length is 0)
            {
                var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);
                if (tmpBaseType is not null)
                {
                    itemId = tmpBaseType.Id;
                    itemInherits = tmpBaseType.InheritsFrom;
                }
            }
        }

        item.Inherits = itemInherits.Split('/')[0] is Strings.Inherit.Jewels or Strings.Inherit.Armours or Strings.Inherit.Weapons ? itemId.Split('/') : itemInherits.Split('/');

        if (itemIs.Chronicle || itemIs.Ultimatum || itemIs.MirroredTablet || itemIs.SanctumResearch) item.Inherits[1] = "Area";

        //string item_qualityOld = Regex.Replace(lOptions[Resources.Resources.General035_Quality].Trim(), "[^0-9]", string.Empty);
        string item_quality = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General035_Quality].Trim(), string.Empty);
        string inherit = item.Inherits[0]; // FLAG

        bool by_type = inherit is Strings.Inherit.Weapons or Strings.Inherit.Quivers or Strings.Inherit.Armours or Strings.Inherit.Amulets or Strings.Inherit.Rings or Strings.Inherit.Belts;

        if (!isPoe2 && inherit is Strings.Inherit.Weapons or Strings.Inherit.Quivers or Strings.Inherit.Armours)
        {
            Form.Visible.Sockets = true;
            Form.Visible.Influences = true;
        }

        if (isPoe2 && inherit is Strings.Inherit.Weapons or Strings.Inherit.Armours)
        {
            Form.Visible.RuneSockets = true;
        }

        bool showRes = false, showLife = false, showEs = false;
        if (Form.Panel.Total.Resistance.Min.Length > 0)
        {
            showRes = true;
            if (DataManager.Config.Options.AutoSelectRes && !isPoe2
                && (Form.Panel.Total.Resistance.Min.ToDoubleDefault() >= 36 || itemIs.Jewel))
            {
                Form.Panel.Total.Resistance.Selected = true;
            }
        }
        if (Form.Panel.Total.Life.Min.Length > 0)
        {
            showLife = true;
            if (DataManager.Config.Options.AutoSelectLife && !isPoe2
                && (Form.Panel.Total.Life.Min.ToDoubleDefault() >= 40 || itemIs.Jewel))
            {
                Form.Panel.Total.Life.Selected = true;
            }
        }
        if (Form.Panel.Total.GlobalEs.Min.Length > 0)
        {
            if (inherit is not Strings.Inherit.Armours)
            {
                showEs = true;
                if (DataManager.Config.Options.AutoSelectGlobalEs && !isPoe2
                    && (Form.Panel.Total.GlobalEs.Min.ToDoubleDefault() >= 38 || itemIs.Jewel))
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
        Form.Visible.TotalRes = !isPoe2 && showRes;
        Form.Visible.TotalEs = !isPoe2 && showEs;

        if (itemIs.ShowDetail)
        {
            var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);

            item.Type = tmpBaseType is null ? itemType : tmpBaseType.Name;
            item.TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn;

            if (itemIs.Incubator || inherit is Strings.Inherit.Gems or Strings.Inherit.UniqueFragments or Strings.Inherit.Labyrinth) // || is_essences
            {
                int i = inherit is Strings.Inherit.Gems ? 3 : 1;
                Form.Detail = clipData.Length > 2 ? (inherit is Strings.Inherit.Gems or Strings.Inherit.Labyrinth ?
                    clipData[i] : string.Empty) + clipData[i + 1] : string.Empty;
            }
            else
            {
                int i = inherit is Strings.Inherit.Delve ? 3 : itemIs.Divcard || inherit is Strings.Inherit.Currency ? 2 : 1;
                Form.Detail = clipData.Length > i + 1 ? clipData[i] + clipData[i + 1] : clipData[^1];

                if (clipData.Length > i + 1)
                {
                    int v = clipData[i - 1].TrimStart().IndexOf("Apply: ", StringComparison.Ordinal);
                    Form.Detail += v > -1 ? string.Empty + Strings.LF + Strings.LF + clipData[i - 1].TrimStart().Split(Strings.LF)[v == 0 ? 0 : 1].TrimEnd() : string.Empty;
                }
            }

            if (idLang is 0) // en
            {
                Form.Detail = Form.Detail.Replace(Resources.Resources.General097_SClickSplitItem, string.Empty);
                Form.Detail = RegexUtil.DetailPattern().Replace(Form.Detail, string.Empty);
            }
        }
        else
        {
            for (int i = 0; i < Form.ModLine.Count; i++)
            {
                var filter = Form.ModLine[i].ItemFilter;

                string englishMod = Form.ModLine[i].Mod;
                if (idLang is not 0) // ! "en-US"
                {
                    var affix = Form.ModLine[i].Affix[0];
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
                bool condLife = DataManager.Config.Options.AutoSelectLife && !isPoe2
                    && !itemIs.Unique && Modifier.IsTotalStat(englishMod, Stat.Life)
                    && !englishMod.ToLowerInvariant().Contains("to strength", StringComparison.Ordinal);
                bool condEs = DataManager.Config.Options.AutoSelectGlobalEs && !isPoe2
                    && !itemIs.Unique && Modifier.IsTotalStat(englishMod, Stat.Es) && inherit is not "Armours";
                bool condRes = DataManager.Config.Options.AutoSelectRes && !isPoe2
                    && !itemIs.Unique && Modifier.IsTotalStat(englishMod, Stat.Resist);
                bool implicitRegular = Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex].Name == Resources.Resources.General013_Implicit;
                bool implicitCorrupt = Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex].Name == Resources.Resources.General017_CorruptImp;
                bool implicitEnch = Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex].Name == Resources.Resources.General011_Enchant;
                bool implicitScourge = Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex].Name == Resources.Resources.General099_Scourge;

                if (implicitScourge) // Temporary
                {
                    Form.ModLine[i].Selected = false;
                    Form.ModLine[i].ItemFilter.Disabled = true;
                }

                if (implicitRegular || implicitCorrupt || implicitEnch)
                {
                    bool condImpAuto = DataManager.Config.Options.AutoCheckImplicits && implicitRegular;
                    bool condCorruptAuto = DataManager.Config.Options.AutoCheckCorruptions && implicitCorrupt;
                    bool condEnchAuto = DataManager.Config.Options.AutoCheckEnchants && implicitEnch;

                    bool specialImp = false;
                    var affix = Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex];
                    if (affix is not null)
                    {
                        specialImp = Strings.Stat.lSpecialImplicits.Contains(affix.ID);
                    }

                    if ((condImpAuto || condCorruptAuto || condEnchAuto) && !condLife && !condEs && !condRes || specialImp || filter.Id is Strings.Stat.MapOccupConq or Strings.Stat.MapOccupElder or Strings.Stat.AreaInflu)
                    {
                        Form.ModLine[i].Selected = true;
                        Form.ModLine[i].ItemFilter.Disabled = false;
                    }
                    if (filter.Id is Strings.Stat.MapOccupConq)
                    {
                        itemIs.ConqMap = true;
                    }
                }

                if (inherit.Length > 0 || itemIs.ChargedCompass || itemIs.Voidstone || itemIs.FilledCoffin) // && i >= Imp_cnt
                {
                    if (DataManager.Config.Options.AutoCheckUniques && itemIs.Unique ||
                            DataManager.Config.Options.AutoCheckNonUniques && !itemIs.Unique)
                    {
                        bool logbookRareMod = filter.Id.Contains(Strings.Stat.LogbookBoss, StringComparison.Ordinal)
                            || filter.Id.Contains(Strings.Stat.LogbookArea, StringComparison.Ordinal)
                            || filter.Id.Contains(Strings.Stat.LogbookTwice, StringComparison.Ordinal);
                        bool craftedCond = filter.Id.Contains(Strings.Stat.Crafted, StringComparison.Ordinal);
                        if (Form.ModLine[i].AffixIndex >= 0)
                        {
                            craftedCond = craftedCond || Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex].Name
                                == Resources.Resources.General012_Crafted && !DataManager.Config.Options.AutoCheckCrafted;
                        }
                        if (craftedCond || itemIs.Logbook && !logbookRareMod)
                        {
                            Form.ModLine[i].Selected = false;
                            Form.ModLine[i].ItemFilter.Disabled = true;
                        }
                        else if (!itemIs.Invitation && !itemIs.MapCategory && !craftedCond && !condLife && !condEs && !condRes)
                        {
                            bool condChronicle = false, condMirroredTablet = false;
                            if (itemIs.Chronicle)
                            {
                                var affix = Form.ModLine[i].Affix[0];
                                if (affix is not null)
                                {
                                    condChronicle = affix.ID.Contains(Strings.Stat.Room01, StringComparison.Ordinal) // Apex of Atzoatl
                                        || affix.ID.Contains(Strings.Stat.Room11, StringComparison.Ordinal) // Doryani's Institute
                                        || affix.ID.Contains(Strings.Stat.Room15, StringComparison.Ordinal) // Apex of Ascension
                                        || affix.ID.Contains(Strings.Stat.Room17, StringComparison.Ordinal); // Locus of Corruption
                                }
                            }
                            if (itemIs.MirroredTablet)
                            {
                                var affix = Form.ModLine[i].Affix[0];
                                if (affix is not null)
                                {
                                    condMirroredTablet = affix.ID.Contains(Strings.Stat.Tablet01, StringComparison.Ordinal) // Paradise
                                        || affix.ID.Contains(Strings.Stat.Tablet02, StringComparison.Ordinal) // Kalandra
                                        || affix.ID.Contains(Strings.Stat.Tablet03, StringComparison.Ordinal) // the Sun
                                        || affix.ID.Contains(Strings.Stat.Tablet04, StringComparison.Ordinal); // Angling
                                }
                            }
                            var unselectPoe2Mod = isPoe2 &&
                                ((DataManager.Config.Options.AutoSelectArEsEva && itemIs.ArmourPiece)
                                || (DataManager.Config.Options.AutoSelectDps && itemIs.Weapon));
                            if (unselectPoe2Mod)
                            {
                                var affix = Form.ModLine[i].Affix[0];
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
                                && (!itemIs.Chronicle && !itemIs.Ultimatum && !itemIs.MirroredTablet
                                || condChronicle || condMirroredTablet))
                            {
                                Form.ModLine[i].Selected = true;
                                Form.ModLine[i].ItemFilter.Disabled = false;
                            }
                        }
                    }

                    var idStat = Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex].ID.Split('.');
                    if (idStat.Length is 2)
                    {
                        if (itemIs.MapCategory &&
                            DataManager.Config.DangerousMapMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                        {
                            Form.ModLine[i].ModKind = Strings.ModKind.DangerousMod;
                        }
                        if (!itemIs.MapCategory &&
                            DataManager.Config.RareItemMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                        {
                            Form.ModLine[i].ModKind = Strings.ModKind.RareMod;
                        }
                    }
                }

                if (Form.ModLine[i].Selected)
                {
                    if (itemIs.Unique)
                    {
                        Form.ModLine[i].AffixCanBeEnabled = false;
                    }
                    else
                    {
                        Form.ModLine[i].AffixEnable = true;
                    }
                }

                if (Form.Panel.Common.Sockets.SocketMin is "6")
                {
                    bool condColors = false;
                    var affix = Form.ModLine[i].Affix[0];
                    if (affix is not null)
                    {
                        condColors = affix.ID.Contains(Strings.Stat.SocketsUnmodifiable, StringComparison.Ordinal);
                    }
                    if (condColors || Form.Panel.Common.Sockets.WhiteColor is "6")
                    {
                        Form.Condition.SocketColors = true;
                        Form.Panel.Common.Sockets.Selected = true;
                    }
                }
            }
            /*
            if (!itemIs.MapCategory && !itemIs.Invitation && checkAll)
            {
                Vm.Form.AllCheck = true;
            }
            */
            // DPS calculation
            if (!itemIs.Unidentified && inherit is Strings.Inherit.Weapons)
            {
                Form.Visible.Damage = true;

                double qualityDPS = item_quality.ToDoubleDefault();
                double physicalDPS = DamageToDPS(listOptions[Resources.Resources.General058_PhysicalDamage]);
                double elementalDPS = DamageToDPS(listOptions[Resources.Resources.General059_ElementalDamage])
                    + DamageToDPS(listOptions[Resources.Resources.General148_ColdDamage])
                    + DamageToDPS(listOptions[Resources.Resources.General149_FireDamage])
                    + DamageToDPS(listOptions[Resources.Resources.General146_LightningDamage]);
                double chaosDPS = DamageToDPS(listOptions[Resources.Resources.General060_ChaosDamage]);
                string aps = RegexUtil.NumericalPattern2().Replace(listOptions[Resources.Resources.General061_AttacksPerSecond], string.Empty);

                double attacksPerSecond = aps.ToDoubleDefault();

                physicalDPS = physicalDPS / 2 * attacksPerSecond;
                if (qualityDPS < 20 && !itemIs.Corrupted)
                {
                    double physInc = listOptions[Strings.Stat.IncPhys].ToDoubleDefault();
                    double physMulti = (physInc + qualityDPS + 100) / 100;
                    double basePhys = physicalDPS / physMulti;
                    physicalDPS = basePhys * ((physInc + 120) / 100);
                }
                elementalDPS = elementalDPS / 2 * attacksPerSecond;
                chaosDPS = chaosDPS / 2 * attacksPerSecond;

                // remove values after decimal to avoid difference with POE's rounded values while calculating dps weapons
                physicalDPS = Math.Truncate(physicalDPS);
                elementalDPS = Math.Truncate(elementalDPS);
                chaosDPS = Math.Truncate(chaosDPS);
                double totalDPS = physicalDPS + elementalDPS + chaosDPS;
                Form.Dps = Math.Round(totalDPS, 0).ToString() + " DPS";

                StringBuilder sbToolTip = new();

                if (DataManager.Config.Options.AutoSelectDps && totalDPS > 100)
                {
                    Form.Panel.Damage.Total.Selected = true;
                }

                // Allready rounded : example 0.46 => 0.5
                Form.Panel.Damage.Total.Min = totalDPS.ToString(specifier, CultureInfo.InvariantCulture);

                if (Math.Round(physicalDPS, 2) > 0)
                {
                    string qual = qualityDPS > 20 || itemIs.Corrupted ? qualityDPS.ToString() : "20";
                    sbToolTip.Append("PHYS. Q").Append(qual).Append(" : ").Append(Math.Round(physicalDPS, 0)).Append(" dps");

                    Form.Panel.Damage.Physical.Min = Math.Round(physicalDPS, 0).ToString(specifier, CultureInfo.InvariantCulture);
                }
                if (Math.Round(elementalDPS, 2) > 0)
                {
                    if (sbToolTip.ToString().Length > 0) sbToolTip.AppendLine();
                    sbToolTip.Append("ELEMENTAL : ").Append(Math.Round(elementalDPS, 0)).Append(" dps");

                    Form.Panel.Damage.Elemental.Min = Math.Round(elementalDPS, 0).ToString(specifier, CultureInfo.InvariantCulture);
                }
                if (Math.Round(chaosDPS, 2) > 0)
                {
                    if (sbToolTip.ToString().Length > 0) sbToolTip.AppendLine();
                    sbToolTip.Append("CHAOS : ").Append(Math.Round(chaosDPS, 0)).Append(" dps");
                }
                Form.DpsTip = sbToolTip.ToString();
            }

            if (!itemIs.Unidentified && inherit is Strings.Inherit.Armours)
            {
                Form.Visible.Defense = true;

                string armour = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General055_Armour].Trim(), string.Empty);
                string energy = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General056_Energy].Trim(), string.Empty);
                string evasion = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General057_Evasion].Trim(), string.Empty);
                string ward = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General095_Ward].Trim(), string.Empty);

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

            BaseResultData baseResult = null;
            if (itemIs.CapturedBeast)
            {
                baseResult = DataManager.Monsters.FirstOrDefault(x => x.Name.Contains(itemType, StringComparison.Ordinal));
                item.Type = baseResult is null ? itemType : baseResult.Name.Replace("\"", string.Empty);
                item.TypeEn = baseResult is null ? string.Empty : baseResult.NameEn.Replace("\"", string.Empty);
                itemName = string.Empty;
            }
            else
            {
                baseResult = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);
                item.Type = baseResult is null ? itemType : baseResult.Name;
                item.TypeEn = baseResult is null ? string.Empty : baseResult.NameEn;
                if (itemIs.BlightMap)
                {
                    item.Type = item.Type.Replace(Resources.Resources.General040_Blighted, string.Empty).Trim();
                    item.TypeEn = item.TypeEn.Replace(rm.GetString("General040_Blighted", cultureEn), string.Empty).Trim();
                }
                else if (itemIs.BlightRavagedMap)
                {
                    item.Type = item.Type.Replace(Resources.Resources.General100_BlightRavaged, string.Empty).Trim();
                    item.TypeEn = item.TypeEn.Replace(rm.GetString("General100_BlightRavaged", cultureEn), string.Empty).Trim();
                }
            }
        }
        if (item.TypeEn.Length is 0) //!itemIs.CapturedBeast
        {
            if (idLang is 0) // en
            {
                item.TypeEn = item.Type;
            }
            else
            {
                var enCur =
                    from result in DataManager.CurrenciesEn
                    from Entrie in result.Entries
                    where Entrie.Id == itemId
                    select Entrie.Text;
                if (enCur.Any())
                {
                    item.TypeEn = enCur.First();
                }
            }
        }

        item.Name = Form.ItemName = itemName;
        item.NameEn = string.Empty;
        if (idLang is 0) //en
        {
            item.NameEn = item.Name;
        }
        else if (itemName.Length > 0)
        {
            var wordRes = DataManager.Words.FirstOrDefault(x => x.Name == itemName);
            if (wordRes is not null)
            {
                item.NameEn = wordRes.NameEn;
            }
        }

        if (itemIs.FilledCoffin && item.NameEn.Length is 0) // for poe ninja
        {
            StringBuilder sb = new();
            int cpt = 0;
            foreach (var mod in Form.ModLine)
            {
                string modTextEnglish = mod.Mod;
                if (idLang is not 0)
                {
                    var affix = mod.Affix?[0];
                    if (affix is not null)
                    {
                        var enResult =
                            from result in DataManager.FilterEn.Result
                            from Entrie in result.Entries
                            where Entrie.ID == affix.ID
                            select Entrie.Text;
                        if (enResult.Any())
                        {
                            modTextEnglish = enResult.First();
                        }
                    }
                }
                StringBuilder sbMod = new(modTextEnglish);
                sbMod.Replace("#", mod.Min).Replace("+", string.Empty).Replace("%", string.Empty).Replace(" ", "-")
                    .Replace("2-other-Corpse-", "2-other-Corpses-");
                if (cpt > 0)
                {
                    sb.Append('-');
                }
                sb.Append(sbMod.ToString().ToLowerInvariant());
                cpt++;
            }
            item.NameEn = sb.ToString();
        }

        var byBase = !itemIs.Unique && !itemIs.Normal && !itemIs.Currency && !itemIs.MapCategory && !itemIs.Divcard
            && !itemIs.CapturedBeast && !itemIs.Gem && !itemIs.Flask && !itemIs.Tincture && !itemIs.Unidentified
            && !itemIs.Watchstone && !itemIs.Invitation && !itemIs.Logbook && !itemIs.SpecialBase && !itemIs.Tablet;

        var poe2SkillWeapon = isPoe2 && (itemIs.Wand || itemIs.Stave || itemIs.Sceptre);
        Form.ByBase = !byBase || DataManager.Config.Options.SearchByType || poe2SkillWeapon;

        string qualType = Form.Panel.AlternateGemIndex is 1 ? Resources.Resources.General001_Anomalous :
            Form.Panel.AlternateGemIndex is 2 ? Resources.Resources.General002_Divergent :
            Form.Panel.AlternateGemIndex is 3 ? Resources.Resources.General003_Phantasmal : string.Empty;

        Form.ItemBaseType = qualType.Length > 0 ?
            idLang is 2 or 3 ? itemType + " " + qualType // fr,es
            : idLang is 4 ? itemType + " (" + qualType + ")" // de
            : idLang is 6 ? qualType + ": " + itemType// ru
            : qualType + " " + itemType // en,kr,br,th,tw,cn
            : itemType;

        string tier = listOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
        if (itemIs.MapCategory && !itemIs.Unique && itemType.Length > 0)
        {
            var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contains(itemType, StringComparison.Ordinal)
                        && Entrie.Id.EndsWith(Strings.tierPrefix + tier, StringComparison.Ordinal)
                    select Entrie.Text;
            if (cur.Any())
            {
                itemIs.ExchangeCurrency = true;
                mapName = cur.First();
            }
        }
        if (!itemIs.Unidentified)
        {
            if (itemIs.MapCategory && itemIs.Unique && itemName.Length > 0)
            {
                var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contains(itemName, StringComparison.Ordinal)
                        && Entrie.Id.EndsWith(Strings.tierPrefix + tier, StringComparison.Ordinal)
                    select Entrie.Text;
                if (cur.Any())
                {
                    itemIs.ExchangeCurrency = true;
                    mapName = cur.First();
                }
            }
            else
            {
                var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text == itemType
                    select true;
                if (cur.Any() && cur.First())
                {
                    itemIs.ExchangeCurrency = true;
                }
            }
        }

        Form.Rarity.Item =
            itemIs.ExchangeCurrency && !itemIs.MapCategory && !itemIs.Invitation && !itemIs.Waystones ? Resources.Resources.General005_Any :
            itemIs.FoilVariant ? Resources.Resources.General110_FoilUnique : itemRarity;

        Form.ItemNameColor = Form.Rarity.Item == Resources.Resources.General008_Magic ? Strings.Color.DeepSkyBlue :
            Form.Rarity.Item == Resources.Resources.General007_Rare ? Strings.Color.Gold :
            Form.Rarity.Item == Resources.Resources.General110_FoilUnique ? Strings.Color.Green :
            Form.Rarity.Item == Resources.Resources.General006_Unique ? Strings.Color.Peru : string.Empty;
        Form.ItemBaseTypeColor = itemIs.Gem ? Strings.Color.Teal : itemIs.Currency ? Strings.Color.Moccasin : string.Empty;

        if ((itemIs.MapCategory || itemIs.Waystones || itemIs.Watchstone || itemIs.Invitation || itemIs.Logbook || itemIs.ChargedCompass || itemIs.Voidstone) && !itemIs.Unique)
        {
            Form.Rarity.Item = Resources.Resources.General010_AnyNU;
            if (!itemIs.Corrupted)
            {
                Form.CorruptedIndex = 1;
            }
            if (itemIs.Voidstone)
            {
                Form.ByBase = false;
            }
        }

        if (Form.Rarity.Item.Length is 0)
        {
            Form.Rarity.Item = itemRarity;
        }

        if (!isPoe2 && !itemIs.Currency && !itemIs.ExchangeCurrency && !itemIs.CapturedBeast)
        {
            Form.Visible.Conditions = true;
        }

        bool hideUserControls = false;
        if (!itemIs.Invitation && !itemIs.MapCategory && !itemIs.AllflameEmber && (itemIs.Currency
            && !itemIs.Chronicle && !itemIs.Ultimatum && !itemIs.FilledCoffin || itemIs.ExchangeCurrency
            || itemIs.CapturedBeast || itemIs.MemoryLine))
        {
            hideUserControls = true;

            if (!itemIs.MirroredTablet && !itemIs.SanctumResearch && !itemIs.Corpses && !itemIs.TrialCoins)
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
        if (hideUserControls && itemIs.Facetor)
        {
            Form.Visible.Facetor = true;
            Form.Panel.FacetorMin = listOptions[Resources.Resources.Main154_tbFacetor].Replace(" ", string.Empty);
        }

        Form.Tab.QuickEnable = true;
        Form.Tab.DetailEnable = true;
        bool uniqueTag = Form.Rarity.Item == Resources.Resources.General006_Unique;
        if (itemIs.ExchangeCurrency && (!uniqueTag || itemIs.MapCategory)) // TODO update with itemIs.Unique
        {
            Form.Tab.BulkEnable = true;
            Form.Tab.ShopEnable = true;

            bool isMap = mapName.Length > 0;

            Form.Bulk.AutoSelect = true;
            Form.Bulk.Args = "pay/equals";
            Form.Bulk.Currency = isMap ? mapName : itemType;
            Form.Bulk.Tier = isMap ? tier : string.Empty;
        }

        if (itemIs.ExchangeCurrency || itemIs.MapCategory || itemIs.Gem || itemIs.CapturedBeast) // Select Detailed TAB
        {
            if (!(itemIs.MapCategory && itemIs.Corrupted)) // checkMapDetails
            {
                Form.Tab.DetailSelected = true;
            }
        }
        if (!Form.Tab.DetailSelected)
        {
            Form.Tab.QuickSelected = true;
        }

        if (!itemIs.ExchangeCurrency && !itemIs.Chronicle && !itemIs.CapturedBeast && !itemIs.Ultimatum)
        {
            Form.Visible.ModSet = !isPoe2;
            Form.Visible.ModPercent = isPoe2;

            Form.Visible.ModCurrent = true;
        }

        if (!itemIs.Unique && (itemIs.Flask || itemIs.Tincture))
        {
            var iLvl = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            if (int.TryParse(iLvl, out int result) && result >= 84)
            {
                Form.Panel.Common.Quality.Selected = item_quality.Length > 0
                    && int.Parse(item_quality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!hideUserControls || itemIs.Corpses)
        {
            Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(listOptions[itemIs.Gem ?
                Resources.Resources.General031_Lv : Resources.Resources.General032_ItemLv].Trim(), string.Empty);

            Form.Panel.Common.Quality.Min = item_quality;

            Form.Influence.ShaperText = Resources.Resources.Main037_Shaper;
            Form.Influence.ElderText = Resources.Resources.Main038_Elder;
            Form.Influence.CrusaderText = Resources.Resources.Main039_Crusader;
            Form.Influence.RedeemerText = Resources.Resources.Main040_Redeemer;
            Form.Influence.HunterText = Resources.Resources.Main041_Hunter;
            Form.Influence.WarlordText = Resources.Resources.Main042_Warlord;

            Form.Influence.Shaper = listOptions[Resources.Resources.General041_Shaper] is Strings.TrueOption;
            Form.Influence.Elder = listOptions[Resources.Resources.General042_Elder] is Strings.TrueOption;
            Form.Influence.Crusader = listOptions[Resources.Resources.General043_Crusader] is Strings.TrueOption;
            Form.Influence.Redeemer = listOptions[Resources.Resources.General044_Redeemer] is Strings.TrueOption;
            Form.Influence.Hunter = listOptions[Resources.Resources.General045_Hunter] is Strings.TrueOption;
            Form.Influence.Warlord = listOptions[Resources.Resources.General046_Warlord] is Strings.TrueOption;

            MainCommand.CheckInfluence(null);
            MainCommand.CheckCondition(null);

            Form.Panel.SynthesisBlight = itemIs.MapCategory && itemIs.BlightMap
                || listOptions[Resources.Resources.General047_Synthesis] is Strings.TrueOption;
            Form.Panel.BlighRavaged = itemIs.MapCategory && itemIs.BlightRavagedMap;

            if (itemIs.MapCategory)
            {
                Form.Panel.Common.ItemLevel.Min = listOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty); // 0x20
                Form.Panel.Common.ItemLevel.Max = listOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);

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

                Form.Panel.Map.Quantity.Min = listOptions[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                Form.Panel.Map.Rarity.Min = listOptions[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                Form.Panel.Map.PackSize.Min = listOptions[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                Form.Panel.Map.MoreScarab.Min = listOptions[Resources.Resources.General140_MoreScarabs].Replace(" ", string.Empty);
                Form.Panel.Map.MoreCurrency.Min = listOptions[Resources.Resources.General139_MoreCurrency].Replace(" ", string.Empty);
                Form.Panel.Map.MoreDivCard.Min = listOptions[Resources.Resources.General142_MoreDivinationCards].Replace(" ", string.Empty);
                Form.Panel.Map.MoreMap.Min = listOptions[Resources.Resources.General141_MoreMaps].Replace(" ", string.Empty);

                if (Form.Panel.Common.ItemLevel.Min is "17" && Form.Panel.Common.ItemLevel.Max is "17")
                {
                    Form.Visible.SynthesisBlight = false;
                    Form.Visible.BlightRavaged = false;

                    StringBuilder sbReward = new(listOptions[Resources.Resources.General071_Reward]);
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
            else if (itemIs.Gem)
            {
                Form.Panel.Common.ItemLevel.Selected = true;
                Form.Panel.Common.Quality.Selected = item_quality.Length > 0
                    && int.Parse(item_quality, CultureInfo.InvariantCulture) > 12;
                if (!itemIs.Corrupted)
                {
                    Form.CorruptedIndex = 1; // NO
                }
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                Form.Visible.ModCurrent = false;
                Form.Visible.Rarity = false;
            }
            else if (itemIs.FilledCoffin)
            {
                Form.Visible.ByBase = false;
                Form.Visible.Rarity = false;
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;

                Form.Panel.Common.ItemLevel.Min = listOptions[Resources.Resources.General129_CorpseLevel].Replace(" ", string.Empty);
                Form.Panel.Common.ItemLevel.Selected = true;
            }
            else if (itemIs.AllflameEmber)
            {
                Form.Visible.Corrupted = false;
                Form.Visible.Quality = false;
                Form.Visible.ByBase = false;
                Form.Visible.CheckAll = false;
                Form.Visible.ModSet = false;
                Form.Visible.ModCurrent = false;
                Form.Visible.Rarity = false;

                Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
                Form.Panel.Common.ItemLevel.Selected = true;
            }
            else if (by_type && itemIs.Normal)
            {
                Form.Panel.Common.ItemLevel.Selected = Form.Panel.Common.ItemLevel.Min.Length > 0
                    && int.Parse(Form.Panel.Common.ItemLevel.Min, CultureInfo.InvariantCulture) > 82;
            }
            else if (Form.Rarity.Item != Resources.Resources.General006_Unique && itemIs.Cluster)
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

        if ((itemIs.Flask || itemIs.Tincture) && !itemIs.Unique)
        {
            Form.Panel.Common.ItemLevel.Selected = true;
        }

        if (itemIs.Logbook)
        {
            Form.Panel.Common.ItemLevel.Selected = true;
        }

        if (itemIs.ConqMap)
        {
            Form.Visible.ByBase = true;
        }

        if (itemIs.Chronicle || itemIs.Ultimatum || itemIs.MirroredTablet || itemIs.SanctumResearch || itemIs.TrialCoins || itemIs.Waystones)
        {
            Form.Visible.Corrupted = false;
            Form.Visible.Rarity = false;
            Form.Visible.ByBase = false;
            Form.Visible.Quality = false;
            Form.Panel.Common.ItemLevelLabel = Resources.Resources.General067_AreaLevel;

            Form.Panel.Common.ItemLevel.Min = listOptions[Resources.Resources.General067_AreaLevel].Replace(" ", string.Empty);

            if (itemIs.SanctumResearch)
            {
                bool isTome = DataManager.Bases.FirstOrDefault(x => x.NameEn is "Forbidden Tome").Name == itemType;
                if (!isTome)
                {
                    Form.Visible.SanctumFields = true;
                }
            }
            if (itemIs.Chronicle || itemIs.MirroredTablet || itemIs.TrialCoins)
            {
                Form.Panel.Common.ItemLevel.Selected = true;
            }
            if (itemIs.Ultimatum) // to update with 'Engraved Ultimatum'
            {
                bool cur = false, div = false;
                string seekCurrency = string.Empty;
                Form.Visible.Reward = true;

                int idxCur = listOptions[Resources.Resources.General070_ReqSacrifice].IndexOf(" x", StringComparison.Ordinal);
                if (idxCur > -1)
                {
                    seekCurrency = listOptions[Resources.Resources.General070_ReqSacrifice][..idxCur]; // .Substring(0, idxCur)
                    listOptions[Resources.Resources.General070_ReqSacrifice] = seekCurrency;
                    if (seekCurrency.Length > 0)
                    {
                        var isCur =
                            from result in DataManager.Currencies
                            from Entrie in result.Entries
                            where result.Id == Strings.CurrencyTypePoe1.Currency && Entrie.Text == seekCurrency
                            select true;
                        if (isCur.Any() && isCur.First())
                        {
                            cur = true;
                        }
                        if (!cur)
                        {
                            var isDiv =
                                from result in DataManager.Currencies
                                from Entrie in result.Entries
                                where result.Id == Strings.CurrencyTypePoe1.Cards && Entrie.Text == seekCurrency
                                select true;
                            if (isDiv.Any() && isDiv.First())
                            {
                                div = true;
                            }
                        }
                    }
                }
                bool condMirrored = listOptions[Resources.Resources.General071_Reward] == Resources.Resources.General072_RewardMirrored;
                Form.Panel.Reward.Text = cur || div ? seekCurrency : listOptions[Resources.Resources.General071_Reward];
                Form.Panel.Reward.FgColor = cur ? string.Empty : div ? Strings.Color.DeepSkyBlue
                    : condMirrored ? Strings.Color.Gold : Strings.Color.Peru;
                Form.Panel.Reward.Tip = cur ? Strings.Reward.DoubleCurrency : div ? Strings.Reward.DoubleDivCards
                    : condMirrored ? Strings.Reward.MirrorRare : Strings.Reward.ExchangeUnique;
            }
            if (itemIs.SanctumResearch)
            {
                Form.Panel.Common.ItemLevel.Selected = true;
            }
        }

        if (itemIs.Corpses)
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

        Form.Visible.Detail = itemIs.ShowDetail;
        Form.Visible.HeaderMod = !itemIs.ShowDetail;
        Form.Visible.HiddablePanel = Form.Visible.AlternateGem || Form.Visible.SynthesisBlight || Form.Visible.BlightRavaged || Form.Visible.Scourged;
        Form.Rarity.Index = Form.Rarity.ComboBox.IndexOf(Form.Rarity.Item);

        if (Form.Bulk.AutoSelect)
        {
            Form.SelectExchangeCurrency(Form.Bulk.Args, Form.Bulk.Currency, Form.Bulk.Tier); // Select currency in 'Pay' section
        }

        //temp
        item.TranslateCurrentItemGateway();

        CurrentItem = item;
    }

    /// <summary>
    /// Fix for item name/type not translated for non-english.
    /// </summary>
    /// <remarks>
    /// Only for unit tests in dev mode, not optimized.
    /// </remarks>
    /// <param name="itemName"></param>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private static Tuple<string, string> GetTranslatedItemNameAndType(string itemName, string itemType)
    {
        string name = itemName;
        string type = itemType;

        if (name.Length > 0)
        {
            var word = DataManager.Words.FirstOrDefault(x => x.NameEn == name);
            if (word is not null && !word.Name.Contains('/', StringComparison.Ordinal))
            {
                name = word.Name;
            }
        }

        var resultName =
                    from result in DataManager.Bases
                    where result.NameEn.Length > 0
                    && type.Contains(result.NameEn, StringComparison.Ordinal)
                    && !result.Id.StartsWith("Gems", StringComparison.Ordinal)
                    select result.NameEn;
        if (resultName.Any())
        {
            string longestName = string.Empty;
            foreach (var result in resultName)
            {
                if (result.Length > longestName.Length)
                {
                    longestName = result;
                }
            }
            type = longestName;
        }

        var baseType = DataManager.Bases.FirstOrDefault(x => x.NameEn == type);
        if (baseType is not null && !baseType.Name.Contains('/', StringComparison.Ordinal))
        {
            type = baseType.Name;
        }

        if (type == itemType)
        {
            bool isMap = type.Contains("Map");
            if (isMap)
            {
                CultureInfo cultureEn = new(Strings.Culture[0]);
                System.Resources.ResourceManager rm = new(typeof(Resources.Resources));
                type = type.Replace(rm.GetString("General040_Blighted", cultureEn), string.Empty)
                    .Replace(rm.GetString("General100_BlightRavaged", cultureEn), string.Empty).Trim();
            }

            var enCur =
                    from result in DataManager.CurrenciesEn
                    from Entrie in result.Entries
                    where isMap ? Entrie.Text.Contains(type) : Entrie.Text == type
                    select Entrie.Id;
            if (enCur.Any())
            {
                var cur = from result in DataManager.Currencies
                          from Entrie in result.Entries
                          where Entrie.Id == enCur.First()
                          select Entrie.Text;
                if (cur.Any())
                {
                    type = cur.First();
                    if (isMap)
                    {
                        type = type.Substring(0, type.IndexOf('(')).Trim();
                    }
                }
            }
        }

        if (type == itemType)
        {
            var findGem = DataManager.Gems.FirstOrDefault(x => x.NameEn == type);
            if (findGem is not null)
            {
                type = findGem.Name;
            }
        }

        return new Tuple<string, string>(name, type);
    }

    private static double DamageToDPS(string damage)
    {
        double dps = 0;
        try
        {
            var stmps = RegexUtil.LetterPattern().Replace(damage, string.Empty).Split(',');
            for (int t = 0; t < stmps.Length; t++)
            {
                var maidps = (stmps[t] ?? string.Empty).Trim().Split('-');
                if (maidps.Length is 2)
                {
                    double min = double.Parse(maidps[0].Trim());
                    double max = double.Parse(maidps[1].Trim());

                    dps += min + max;
                }
            }
        }
        catch (Exception)
        {
            //Shared.Util.Helper.Debug.Trace("Exception while calculating DPS : " + ex.Message);
        }
        return dps;
    }
}

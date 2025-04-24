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

    [ObservableProperty]
    private bool isSelectionEnabled = true;

    internal string ClipboardText { get; private set; } = string.Empty;
    public List<MouseGestureCom> GestureList { get; private set; } = new();

    //viewmodels split
    public MainCommand Commands { get; private set; }
    public TrayMenuCommand TrayCommands { get; private set; }

    //models
    internal ItemBase CurrentItem { get; private set; }
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

    internal void InitViewModels(bool useBulk = false)
    {
        Form = new(_serviceProvider, useBulk);
        Result = new(_serviceProvider);
        Ninja = new(_serviceProvider);
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
                entity[0] = new() { Json.GetSerialized(xiletradeItem, CurrentItem, true, Form.Market[Form.MarketIndex]) };
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

    internal void UpdateMainViewModel(string[] clipData)
    {
        string itemInherits = string.Empty, itemId = string.Empty, mapName = string.Empty, gemName = string.Empty, specifier = "G";
        int idLang = DataManager.Config.Options.Language;
        bool isPoe2 = DataManager.Config.Options.GameVersion is 1;
        
        var item = new ItemData(clipData, idLang);
        var listOptions = Form.FillModList(clipData, item, idLang);

        if (item.Flag.SanctumResearch)
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

        item.Flag.Unidentified = listOptions[Resources.Resources.General039_Unidentify] == Strings.TrueOption;
        item.Flag.Corrupted = listOptions[Resources.Resources.General037_Corrupt] == Strings.TrueOption;
        item.Flag.Mirrored = listOptions[Resources.Resources.General109_Mirrored] == Strings.TrueOption;
        item.Flag.FoilVariant = listOptions[Resources.Resources.General110_FoilUnique] == Strings.TrueOption;
        item.Flag.ScourgedItem = listOptions[Resources.Resources.General099_ScourgedItem] == Strings.TrueOption;
        item.Flag.MapCategory = listOptions[Resources.Resources.General034_MaTier].Length > 0 && !item.Flag.Divcard;

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
            Form.Panel.Common.RuneSockets.Selected = item.Flag.Corrupted && count >= 1;
            Form.Panel.Common.RuneSockets.Min = count.ToString();
            if (item.Flag.Corrupted)
            {
                Form.Panel.Common.RuneSockets.Max = Form.Panel.Common.RuneSockets.Min;
            }
        }

        if (item.Flag.ScourgedMap)
        {
            Form.Panel.Scourged = true;
        }

        if (item.Flag.Mirrored)
        {
            Form.SetModCurrent();
        }

        Form.CorruptedIndex = item.Flag.Corrupted && DataManager.Config.Options.AutoSelectCorrupt ? 2 : 0;

        if (item.Flag.Rare && !item.Flag.MapCategory && !item.Flag.CapturedBeast) Form.Tab.PoePriceEnable = true;
        if (item.Flag.Gem)
        {
            Form.Visible.AlternateGem = false; // TO remove
        }

        Form.Visible.Corrupted = true;
        if (item.Flag.Incubator)
        {
            Form.Visible.Corrupted = false;
        }

        if (item.Flag.Unique || item.Flag.Unidentified || item.Flag.Watchstone || item.Flag.MapFragment
            || item.Flag.Invitation || item.Flag.CapturedBeast || item.Flag.Chronicle || item.Flag.MapCategory || item.Flag.Gem || item.Flag.Currency || item.Flag.Divcard || item.Flag.Incubator)
        {
            Form.Visible.BtnPoeDb = false;
        }

        if (item.Flag.CapturedBeast)
        {
            var tmpBaseType = DataManager.Monsters.FirstOrDefault(x => x.Name.Contain(item.Type));
            if (tmpBaseType is not null)
            {
                itemId = tmpBaseType.Id;
                itemInherits = tmpBaseType.InheritsFrom;
            }
        }
        if (!item.Flag.CapturedBeast)
        {
            if (item.Flag.Gem)
            {
                Form.Panel.AlternateGemIndex = listOptions[Strings.AlternateGem] is Strings.Gem.Anomalous ? 1 :
                    listOptions[Strings.AlternateGem] is Strings.Gem.Divergent ? 2 :
                    listOptions[Strings.AlternateGem] is Strings.Gem.Phantasmal ? 3 : 0;

                StringBuilder sbType = new(item.Type);
                sbType.Replace(Resources.Resources.General001_Anomalous, string.Empty)
                    .Replace(Resources.Resources.General002_Divergent, string.Empty)
                    .Replace(Resources.Resources.General003_Phantasmal, string.Empty).Replace("()", string.Empty);
                item.Type = sbType.ToString().Trim();
                if (item.Type.StartsWith(':'))
                {
                    item.Type = item.Type[1..].Trim();
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
                            gemName = item.Type;
                            item.Type = tmpBaseType.Name;
                            break;
                        }
                    }
                }
            }

            if ((item.Flag.Unidentified || item.Flag.Normal) && item.Type.Contain(Resources.Resources.General030_Higher))
            {
                if (idLang is 2) // fr
                {
                    item.Type = item.Type.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                    item.Type = item.Type.Replace(Resources.Resources.General030_Higher + "e", string.Empty).Trim();
                }
                if (idLang is 3) // es
                {
                    item.Type = item.Type.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                }
                item.Type = item.Type.Replace(Resources.Resources.General030_Higher, string.Empty).Trim();
            }

            if (item.Flag.MapCategory && item.Type.Length > 5)
            {
                if (item.Type.Contain(Resources.Resources.General040_Blighted))
                {
                    item.Flag.BlightMap = true;
                }
                else if (item.Type.Contain(Resources.Resources.General100_BlightRavaged))
                {
                    item.Flag.BlightRavagedMap = true;
                }
            }
            else if (listOptions[Resources.Resources.General047_Synthesis] is Strings.TrueOption)
            {
                if (item.Type.Contain(Resources.Resources.General048_Synthesised))
                {
                    if (idLang is 2)
                    {
                        item.Type = item.Type.Replace(Resources.Resources.General048_Synthesised + "e", string.Empty).Trim(); // french female item name
                    }
                    if (idLang is 4)
                    {
                        StringBuilder iType = new(item.Type);
                        iType.Replace(Resources.Resources.General048_Synthesised + "s", string.Empty) // german
                            .Replace(Resources.Resources.General048_Synthesised + "r", string.Empty); // german
                        item.Type = iType.ToString().Trim();
                    }
                    if (idLang is 6)
                    {
                        StringBuilder iType = new(item.Type);
                        iType.Replace(Resources.Resources.General048_Synthesised + "ый", string.Empty) // russian
                            .Replace(Resources.Resources.General048_Synthesised + "ое", string.Empty) // russian
                            .Replace(Resources.Resources.General048_Synthesised + "ая", string.Empty); // russian
                        item.Type = iType.ToString().Trim();
                    }
                    item.Type = item.Type.Replace(Resources.Resources.General048_Synthesised, string.Empty).Trim();
                }
            }

            if (!item.Flag.Unidentified && !item.Flag.MapCategory && item.Flag.Magic)
            {
                var resultName =
                    from result in DataManager.Bases
                    where result.Name.Length > 0 && item.Type.Contain(result.Name)
                    && !result.Id.StartWith("Gems")
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
                    if (item.Flag.MemoryLine)
                    {
                        item.Name = item.Type;
                    }
                    item.Type = longestName;
                }
            }
            var tmpBaseType2 = DataManager.Bases.FirstOrDefault(x => x.Name == item.Type);
            if (tmpBaseType2 is not null)
            {
                // 3.14 : to remove and replace by itemClass
                //Strings.lpublicID.TryGetValue(tmpBaseType2.NameEn, out publicID);
                item.Flag.SpecialBase = Strings.lSpecialBases.Contains(tmpBaseType2.NameEn);
            }
        }

        if (itemInherits.Length is 0)
        {
            if (item.Flag.MapCategory || item.Flag.Waystones)
            {
                //bool isGuardian = IsGuardianMap(itemType, out string guardName);
                if (!item.Flag.Unidentified && item.Flag.Magic)
                {
                    var affixes =
                        from result in DataManager.Mods
                        from names in result.Name.Split('/')
                        where names.Length > 0 && item.Type.Contain(names)
                        select names;
                    if (affixes.Any())
                    {
                        foreach (string str in affixes)
                        {
                            item.Type = item.Type.Replace(str, string.Empty).Trim();
                        }
                    }
                }

                string mapKind = item.Flag.BlightMap || item.Flag.BlightRavagedMap ? Strings.CurrencyTypePoe1.MapsBlighted :
                    item.Flag.Unique ? Strings.CurrencyTypePoe1.MapsUnique : Strings.CurrencyTypePoe1.Maps;

                var mapId =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where (result.Id == mapKind || result.Id == Strings.CurrencyTypePoe2.Waystones)
                    && (Entrie.Text.StartWith(item.Type)
                    || Entrie.Text.EndWith(item.Type))
                    select Entrie.Id;
                if (mapId.Any())
                {
                    itemId = mapId.First();
                }

                itemInherits = item.Flag.MapCategory ? "Maps/AbstractMap" : "Waystones";
            }
            else if (item.Flag.Currency || item.Flag.Divcard || item.Flag.MapFragment || item.Flag.Incubator)
            {
                var curResult =
                    from resultDat in DataManager.Currencies
                    from Entrie in resultDat.Entries
                    where Entrie.Text == item.Type
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
            else if (item.Flag.Gem)
            {
                var findGem = DataManager.Gems.FirstOrDefault(x => x.Name == item.Type);
                if (findGem is not null)
                {
                    if (gemName.Length is 0 && findGem.Type != findGem.Name) // transfigured normal gem
                    {
                        item.Type = findGem.Type;
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
                var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == item.Type);
                if (tmpBaseType is not null)
                {
                    itemId = tmpBaseType.Id;
                    itemInherits = tmpBaseType.InheritsFrom;
                }
            }
        }

        item.Base.Inherits = itemInherits.Split('/')[0] is Strings.Inherit.Jewels or Strings.Inherit.Armours or Strings.Inherit.Weapons ? itemId.Split('/') : itemInherits.Split('/');

        if (item.Flag.Chronicle || item.Flag.Ultimatum || item.Flag.MirroredTablet || item.Flag.SanctumResearch) item.Base.Inherits[1] = "Area";

        //string item_qualityOld = Regex.Replace(lOptions[Resources.Resources.General035_Quality].Trim(), "[^0-9]", string.Empty);
        string item_quality = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General035_Quality].Trim(), string.Empty);
        string inherit = item.Base.Inherits[0]; // FLAG

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
                && (Form.Panel.Total.Resistance.Min.ToDoubleDefault() >= 36 || item.Flag.Jewel))
            {
                Form.Panel.Total.Resistance.Selected = true;
            }
        }
        if (Form.Panel.Total.Life.Min.Length > 0)
        {
            showLife = true;
            if (DataManager.Config.Options.AutoSelectLife && !isPoe2
                && (Form.Panel.Total.Life.Min.ToDoubleDefault() >= 40 || item.Flag.Jewel))
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
        Form.Visible.TotalRes = !isPoe2 && showRes;
        Form.Visible.TotalEs = !isPoe2 && showEs;

        if (item.Flag.ShowDetail)
        {
            var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == item.Type);

            item.Base.Type = tmpBaseType is null ? item.Type : tmpBaseType.Name;
            item.Base.TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn;

            if (item.Flag.Incubator || inherit is Strings.Inherit.Gems or Strings.Inherit.UniqueFragments or Strings.Inherit.Labyrinth) // || is_essences
            {
                int i = inherit is Strings.Inherit.Gems ? 3 : 1;
                Form.Detail = clipData.Length > 2 ? (inherit is Strings.Inherit.Gems or Strings.Inherit.Labyrinth ?
                    clipData[i] : string.Empty) + clipData[i + 1] : string.Empty;
            }
            else
            {
                int i = inherit is Strings.Inherit.Delve ? 3 : item.Flag.Divcard || inherit is Strings.Inherit.Currency ? 2 : 1;
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
                    && !item.Flag.Unique && Modifier.IsTotalStat(englishMod, Stat.Life)
                    && !englishMod.ToLowerInvariant().Contain("to strength");
                bool condEs = DataManager.Config.Options.AutoSelectGlobalEs && !isPoe2
                    && !item.Flag.Unique && Modifier.IsTotalStat(englishMod, Stat.Es) && inherit is not "Armours";
                bool condRes = DataManager.Config.Options.AutoSelectRes && !isPoe2
                    && !item.Flag.Unique && Modifier.IsTotalStat(englishMod, Stat.Resist);
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
                        item.Flag.ConqMap = true;
                    }
                }

                if (inherit.Length > 0 || item.Flag.ChargedCompass || item.Flag.Voidstone || item.Flag.FilledCoffin) // && i >= Imp_cnt
                {
                    if (DataManager.Config.Options.AutoCheckUniques && item.Flag.Unique ||
                            DataManager.Config.Options.AutoCheckNonUniques && !item.Flag.Unique)
                    {
                        bool logbookRareMod = filter.Id.Contain(Strings.Stat.LogbookBoss)
                            || filter.Id.Contain(Strings.Stat.LogbookArea)
                            || filter.Id.Contain(Strings.Stat.LogbookTwice);
                        bool craftedCond = filter.Id.Contain(Strings.Stat.Crafted);
                        if (Form.ModLine[i].AffixIndex >= 0)
                        {
                            craftedCond = craftedCond || Form.ModLine[i].Affix[Form.ModLine[i].AffixIndex].Name
                                == Resources.Resources.General012_Crafted && !DataManager.Config.Options.AutoCheckCrafted;
                        }
                        if (craftedCond || item.Flag.Logbook && !logbookRareMod)
                        {
                            Form.ModLine[i].Selected = false;
                            Form.ModLine[i].ItemFilter.Disabled = true;
                        }
                        else if (!item.Flag.Invitation && !item.Flag.MapCategory && !craftedCond && !condLife && !condEs && !condRes)
                        {
                            bool condChronicle = false, condMirroredTablet = false;
                            if (item.Flag.Chronicle)
                            {
                                var affix = Form.ModLine[i].Affix[0];
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
                                var affix = Form.ModLine[i].Affix[0];
                                if (affix is not null)
                                {
                                    condMirroredTablet = affix.ID.Contain(Strings.Stat.Tablet01) // Paradise
                                        || affix.ID.Contain(Strings.Stat.Tablet02) // Kalandra
                                        || affix.ID.Contain(Strings.Stat.Tablet03) // the Sun
                                        || affix.ID.Contain(Strings.Stat.Tablet04); // Angling
                                }
                            }
                            var unselectPoe2Mod = isPoe2 &&
                                ((DataManager.Config.Options.AutoSelectArEsEva && item.Flag.ArmourPiece)
                                || (DataManager.Config.Options.AutoSelectDps && item.Flag.Weapon));
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
                                && (!item.Flag.Chronicle && !item.Flag.Ultimatum && !item.Flag.MirroredTablet
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
                        if (item.Flag.MapCategory &&
                            DataManager.Config.DangerousMapMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                        {
                            Form.ModLine[i].ModKind = Strings.ModKind.DangerousMod;
                        }
                        if (!item.Flag.MapCategory &&
                            DataManager.Config.RareItemMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                        {
                            Form.ModLine[i].ModKind = Strings.ModKind.RareMod;
                        }
                    }
                }

                if (Form.ModLine[i].Selected)
                {
                    if (item.Flag.Unique)
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
                        condColors = affix.ID.Contain(Strings.Stat.SocketsUnmodifiable);
                    }
                    if (condColors || Form.Panel.Common.Sockets.WhiteColor is "6")
                    {
                        Form.Condition.SocketColors = true;
                        Form.Panel.Common.Sockets.Selected = true;
                    }
                }
            }
            /*
            if (!item.Is.MapCategory && !item.Is.Invitation && checkAll)
            {
                Vm.Form.AllCheck = true;
            }
            */
            // DPS calculation
            if (!item.Flag.Unidentified && inherit is Strings.Inherit.Weapons)
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
                if (qualityDPS < 20 && !item.Flag.Corrupted)
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
                    string qual = qualityDPS > 20 || item.Flag.Corrupted ? qualityDPS.ToString() : "20";
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

            if (!item.Flag.Unidentified && inherit is Strings.Inherit.Armours)
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
            if (item.Flag.CapturedBeast)
            {
                baseResult = DataManager.Monsters.FirstOrDefault(x => x.Name.Contain(item.Type));
                item.Base.Type = baseResult is null ? item.Type : baseResult.Name.Replace("\"", string.Empty);
                item.Base.TypeEn = baseResult is null ? string.Empty : baseResult.NameEn.Replace("\"", string.Empty);
                item.Name = string.Empty;
            }
            else
            {
                var cultureEn = new CultureInfo(Strings.Culture[0]);
                var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));
                baseResult = DataManager.Bases.FirstOrDefault(x => x.Name == item.Type);
                item.Base.Type = baseResult is null ? item.Type : baseResult.Name;
                item.Base.TypeEn = baseResult is null ? string.Empty : baseResult.NameEn;
                if (item.Flag.BlightMap)
                {
                    item.Base.Type = item.Base.Type.Replace(Resources.Resources.General040_Blighted, string.Empty).Trim();
                    item.Base.TypeEn = item.Base.TypeEn.Replace(rm.GetString("General040_Blighted", cultureEn), string.Empty).Trim();
                }
                else if (item.Flag.BlightRavagedMap)
                {
                    item.Base.Type = item.Base.Type.Replace(Resources.Resources.General100_BlightRavaged, string.Empty).Trim();
                    item.Base.TypeEn = item.Base.TypeEn.Replace(rm.GetString("General100_BlightRavaged", cultureEn), string.Empty).Trim();
                }
            }
        }
        if (item.Base.TypeEn.Length is 0) //!item.Is.CapturedBeast
        {
            if (idLang is 0) // en
            {
                item.Base.TypeEn = item.Base.Type;
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
                    item.Base.TypeEn = enCur.First();
                }
            }
        }

        item.Base.Name = Form.ItemName = item.Name;
        item.Base.NameEn = string.Empty;
        if (idLang is 0) //en
        {
            item.Base.NameEn = item.Base.Name;
        }
        else if (item.Name.Length > 0)
        {
            var wordRes = DataManager.Words.FirstOrDefault(x => x.Name == item.Name);
            if (wordRes is not null)
            {
                item.Base.NameEn = wordRes.NameEn;
            }
        }

        if (item.Flag.FilledCoffin && item.Base.NameEn.Length is 0) // for poe ninja
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
            item.Base.NameEn = sb.ToString();
        }

        var byBase = !item.Flag.Unique && !item.Flag.Normal && !item.Flag.Currency && !item.Flag.MapCategory && !item.Flag.Divcard
            && !item.Flag.CapturedBeast && !item.Flag.Gem && !item.Flag.Flask && !item.Flag.Tincture && !item.Flag.Unidentified
            && !item.Flag.Watchstone && !item.Flag.Invitation && !item.Flag.Logbook && !item.Flag.SpecialBase && !item.Flag.Tablet;

        var poe2SkillWeapon = isPoe2 && (item.Flag.Wand || item.Flag.Stave || item.Flag.Sceptre);
        Form.ByBase = !byBase || DataManager.Config.Options.SearchByType || poe2SkillWeapon;

        string qualType = Form.Panel.AlternateGemIndex is 1 ? Resources.Resources.General001_Anomalous :
            Form.Panel.AlternateGemIndex is 2 ? Resources.Resources.General002_Divergent :
            Form.Panel.AlternateGemIndex is 3 ? Resources.Resources.General003_Phantasmal : string.Empty;

        Form.ItemBaseType = qualType.Length > 0 ?
            idLang is 2 or 3 ? item.Type + " " + qualType // fr,es
            : idLang is 4 ? item.Type + " (" + qualType + ")" // de
            : idLang is 6 ? qualType + ": " + item.Type// ru
            : qualType + " " + item.Type // en,kr,br,th,tw,cn
            : item.Type;

        string tier = listOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
        if (item.Flag.MapCategory && !item.Flag.Unique && item.Type.Length > 0)
        {
            var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contain(item.Type)
                        && Entrie.Id.EndWith(Strings.tierPrefix + tier)
                    select Entrie.Text;
            if (cur.Any())
            {
                item.Flag.ExchangeCurrency = true;
                mapName = cur.First();
            }
        }
        if (!item.Flag.Unidentified)
        {
            if (item.Flag.MapCategory && item.Flag.Unique && item.Name.Length > 0)
            {
                var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contain(item.Name)
                        && Entrie.Id.EndWith(Strings.tierPrefix + tier)
                    select Entrie.Text;
                if (cur.Any())
                {
                    item.Flag.ExchangeCurrency = true;
                    mapName = cur.First();
                }
            }
            else
            {
                var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text == item.Type
                    select true;
                if (cur.Any() && cur.First())
                {
                    item.Flag.ExchangeCurrency = true;
                }
            }
        }

        Form.Rarity.Item =
            item.Flag.ExchangeCurrency && !item.Flag.MapCategory && !item.Flag.Invitation && !item.Flag.Waystones ? Resources.Resources.General005_Any :
            item.Flag.FoilVariant ? Resources.Resources.General110_FoilUnique : item.Rarity;

        Form.ItemNameColor = Form.Rarity.Item == Resources.Resources.General008_Magic ? Strings.Color.DeepSkyBlue :
            Form.Rarity.Item == Resources.Resources.General007_Rare ? Strings.Color.Gold :
            Form.Rarity.Item == Resources.Resources.General110_FoilUnique ? Strings.Color.Green :
            Form.Rarity.Item == Resources.Resources.General006_Unique ? Strings.Color.Peru : string.Empty;
        Form.ItemBaseTypeColor = item.Flag.Gem ? Strings.Color.Teal : item.Flag.Currency ? Strings.Color.Moccasin : string.Empty;

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

        if (!isPoe2 && !item.Flag.Currency && !item.Flag.ExchangeCurrency && !item.Flag.CapturedBeast)
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
            Form.Panel.FacetorMin = listOptions[Resources.Resources.Main154_tbFacetor].Replace(" ", string.Empty);
        }

        Form.Tab.QuickEnable = true;
        Form.Tab.DetailEnable = true;
        bool uniqueTag = Form.Rarity.Item == Resources.Resources.General006_Unique;
        if (item.Flag.ExchangeCurrency && (!uniqueTag || item.Flag.MapCategory)) // TODO update with item.Is.Unique
        {
            Form.Tab.BulkEnable = true;
            Form.Tab.ShopEnable = true;

            bool isMap = mapName.Length > 0;

            Form.Bulk.AutoSelect = true;
            Form.Bulk.Args = "pay/equals";
            Form.Bulk.Currency = isMap ? mapName : item.Type;
            Form.Bulk.Tier = isMap ? tier : string.Empty;
        }

        if (item.Flag.ExchangeCurrency || item.Flag.MapCategory || item.Flag.Gem || item.Flag.CapturedBeast) // Select Detailed TAB
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
            Form.Visible.ModSet = !isPoe2;
            Form.Visible.ModPercent = isPoe2;

            Form.Visible.ModCurrent = true;
        }

        if (!item.Flag.Unique && (item.Flag.Flask || item.Flag.Tincture))
        {
            var iLvl = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            if (int.TryParse(iLvl, out int result) && result >= 84)
            {
                Form.Panel.Common.Quality.Selected = item_quality.Length > 0
                    && int.Parse(item_quality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!hideUserControls || item.Flag.Corpses)
        {
            Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(listOptions[item.Flag.Gem ?
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

            Form.Panel.SynthesisBlight = item.Flag.MapCategory && item.Flag.BlightMap
                || listOptions[Resources.Resources.General047_Synthesis] is Strings.TrueOption;
            Form.Panel.BlighRavaged = item.Flag.MapCategory && item.Flag.BlightRavagedMap;

            if (item.Flag.MapCategory)
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
            else if (item.Flag.Gem)
            {
                Form.Panel.Common.ItemLevel.Selected = true;
                Form.Panel.Common.Quality.Selected = item_quality.Length > 0
                    && int.Parse(item_quality, CultureInfo.InvariantCulture) > 12;
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

                Form.Panel.Common.ItemLevel.Min = listOptions[Resources.Resources.General129_CorpseLevel].Replace(" ", string.Empty);
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

                Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(listOptions[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
                Form.Panel.Common.ItemLevel.Selected = true;
            }
            else if (by_type && item.Flag.Normal)
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

            Form.Panel.Common.ItemLevel.Min = listOptions[Resources.Resources.General067_AreaLevel].Replace(" ", string.Empty);

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
                bool cur = false, div = false;
                string seekCurrency = string.Empty;
                Form.Visible.Reward = true;

                int idxCur = listOptions[Resources.Resources.General070_ReqSacrifice].IndexOf(" x", StringComparison.Ordinal);
                if (idxCur > -1)
                {
                    seekCurrency = listOptions[Resources.Resources.General070_ReqSacrifice].AsSpan(0, idxCur).ToString();
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
        Form.Visible.HiddablePanel = Form.Visible.AlternateGem || Form.Visible.SynthesisBlight || Form.Visible.BlightRavaged || Form.Visible.Scourged;
        Form.Rarity.Index = Form.Rarity.ComboBox.IndexOf(Form.Rarity.Item);

        if (Form.Bulk.AutoSelect)
        {
            Form.SelectExchangeCurrency(Form.Bulk.Args, Form.Bulk.Currency, Form.Bulk.Tier); // Select currency in 'Pay' section
        }
        Form.FillTime = StopWatch.StopAndGetTimeString();

        item.Base.TranslateCurrentItemGateway();//temp
        CurrentItem = item.Base;
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

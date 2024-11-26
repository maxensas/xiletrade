using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using System.Globalization;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.ViewModels.Command;
using System.Text.RegularExpressions;
using Xiletrade.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Models.Logic;

/// <summary>Abstract helper class containing logic used to update main viewmodel.</summary>
internal abstract class MainUpdater : ModLineHelper
{
    private static IServiceProvider _serviceProvider;

    private static MainViewModel Vm { get; set; }
    private static MainResetHelper ResetHelper { get; set; }

    internal MainUpdater(MainViewModel vm, IServiceProvider serviceProvider)
    {
        Vm = vm;
        _serviceProvider = serviceProvider;
        ResetHelper = new(vm);
    }

    //internal virtual methods
    internal virtual void ResetViewModel()
    {
        ResetHelper.ResetMainViewModel();
    }

    internal virtual ItemBaseName FillViewModel(string[] clipData)
    {
        return FillMainViewModel(Vm, clipData);
    }

    internal virtual void RefreshViewModelStatus(bool exchange, string[] result)
    {
        RefreshMainViewModelStatus(Vm, exchange, result);
    }

    internal virtual void SelectViewModelExchangeCurrency(string args, string currency, string tier)
    {
        SelectMainViewModelExchangeCurrency(Vm, args, currency, tier);
    }

    //private static methods
    private static void SelectMainViewModelExchangeCurrency(MainViewModel vm, string args, string currency, string tier)
    {
        var arg = args.Split('/');
        if (arg[0] is not "pay" and not "get" and not "shop")
        {
            return;
        }
        IEnumerable<(string, string, string Text)> cur;
        if (arg.Length > 1 && arg[1] is "contains") // contains requests to improve
        {
            string[] curKeys = currency.ToLowerInvariant().Split(' ');
            if (curKeys.Length >= 3)
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal))
                && (Entrie.Text.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal))
                && (Entrie.Text.ToLowerInvariant().Contains(curKeys[2], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[2], StringComparison.Ordinal))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
            else if (curKeys.Length == 2)
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal))
                && (Entrie.Text.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
            else
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal))
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

            string selectedCategory = curClass is Strings.CurrencyType.Currency ?
                Strings.dicMainCur.TryGetValue(curId, out string curVal2) ? Resources.Resources.Main044_MainCur :
                Strings.dicExoticCur.TryGetValue(curId, out string curVal4) ? Resources.Resources.Main207_ExoticCurrency : Resources.Resources.Main045_OtherCur :
                //curClass is Strings.CurrencyType.Exotic ? Resources.Resources.Main207_ExoticCurrency :
                curClass is Strings.CurrencyType.Fragments ? Strings.dicStones.TryGetValue(curId, out string curVal3) ? Resources.Resources.Main047_Stones
                : curId.Contains(Strings.scarab, StringComparison.Ordinal) ? Resources.Resources.Main052_Scarabs : Resources.Resources.Main046_MapFrag :
                //curClass is Strings.CurrencyType.Splinters ? Resources.Resources.Main149_Shards :
                //curClass is Strings.CurrencyType.EldritchCurrency ? Resources.Resources.Main197_EldritchCurrency :
                curClass is Strings.CurrencyType.ScoutingReport ? Resources.Resources.Main198_ScoutingReports :
                curClass is Strings.CurrencyType.MemoryLine ? Resources.Resources.Main208_MemoryLine :
                curClass is Strings.CurrencyType.Expedition ? Resources.Resources.Main186_Expedition :
                curClass is Strings.CurrencyType.DeliriumOrbs ? Resources.Resources.Main048_Delirium :
                curClass is Strings.CurrencyType.Catalysts ? Resources.Resources.Main049_Catalysts :
                curClass is Strings.CurrencyType.Oils ? Resources.Resources.Main050_Oils :
                curClass is Strings.CurrencyType.Incubators ? Resources.Resources.Main051_Incubators :
                //curClass is Strings.CurrencyType.Scarabs ? Resources.Resources.Main052_Scarabs :
                curClass is Strings.CurrencyType.DelveFossils or Strings.CurrencyType.DelveResonators ? Resources.Resources.Main053_Fossils :
                curClass is Strings.CurrencyType.Essences ? Resources.Resources.Main054_Essences :
                //curClass is Strings.CurrencyType.TaintedCurrency ? Resources.Resources.Main196_TaintedCurrency :
                curClass is Strings.CurrencyType.Ancestor ? Resources.Resources.Main211_AncestorCurrency :
                curClass is Strings.CurrencyType.Sanctum ? Resources.Resources.Main212_Sanctum :
                //curClass is Strings.CurrencyType.Crucible ? Resources.Resources.Main213_Crucible :
                curClass is Strings.CurrencyType.Sentinel ? Resources.Resources.Main200_SentinelCurrency :
                curClass is Strings.CurrencyType.Cards ? Resources.Resources.Main055_Divination :
                curClass is Strings.CurrencyType.MapsUnique ? Resources.Resources.Main179_UniqueMaps :
                curClass is Strings.CurrencyType.Maps ? Resources.Resources.Main056_Maps :
                curClass is Strings.CurrencyType.MapsBlighted ? Resources.Resources.Main217_BlightedMaps :
                curClass is Strings.CurrencyType.MapsSpecial ? Resources.Resources.Main216_BossMaps :
                curClass is Strings.CurrencyType.Beasts ? Resources.Resources.Main219_Beasts :
                curClass is Strings.CurrencyType.Heist ? Resources.Resources.Main218_Heist :
                //curClass is Strings.CurrencyType.Embers ? Resources.Resources.ItemClass_allflame :
                //curClass is Strings.CurrencyType.Coffins ? Resources.Resources.General127_FilledCoffin :
                curClass is Strings.CurrencyType.Runes ? Resources.Resources.General132_Rune :
                string.Empty;

            if (selectedCategory.Length > 0)
            {
                selectedCurrency = curText;

                if (selectedCategory == Resources.Resources.Main055_Divination)
                {
                    //cbTier.SelectedValue = "T" + tier;
                    DivTiersResult tmpDiv = DataManager.DivTiers.FirstOrDefault(x => x.Tag == curId);
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
                        MatchCollection match = RegexUtil.DecimalNoPlusPattern().Matches(curText);
                        if (match.Count == 1)
                        {
                            selectedTier = "T" + match[0].Value.ToString();
                        }
                    }
                }
            }
            // int idx was declared here. TODO See if it fix UI issue.
            ExchangeViewModel bulk = arg[0] is "pay" ? vm.Form.Bulk.Pay
                : arg[0] is "get" ? vm.Form.Bulk.Get
                : arg[0] is "shop" ? vm.Form.Shop.Exchange
                : null;

            // TOFIX : Sometimes, currency is not checked in Bulk view after a currency price check
            var action = new Action(() =>
            {
                int idx = bulk.Category.IndexOf(selectedCategory);
                if (idx > -1)
                {
                    bulk.CategoryIndex = idx;
                }
                idx = bulk.Tier.IndexOf(selectedTier);
                if (idx > -1 && selectedTier.Length > 0)
                {
                    bulk.TierIndex = idx;
                }
                idx = bulk.Currency.IndexOf(selectedCurrency);
                if (idx > -1)
                {
                    bulk.CurrencyIndex = idx;
                }
            });
            _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(action);
        }
    }

    private static void RefreshMainViewModelStatus(MainViewModel vm, bool exchange, string[] result) // TO REDO : really dirty
    {
        //Thread.Sleep(500);
        //int idLang = DataManager.Config.Options.Language;
        if (result[1].Contains("ERROR", StringComparison.Ordinal)
            || result[1].Contains("NORESULT", StringComparison.Ordinal))
        {
            if (exchange)
            {
                if (vm.Form.Tab.BulkSelected)
                {
                    vm.Result.Bulk.Price = result[0];
                    vm.Result.Bulk.PriceBis = result[1];
                }
                if (vm.Form.Tab.ShopSelected)
                {
                    vm.Result.Shop.Price = result[0];
                    vm.Result.Shop.PriceBis = result[1];
                }
            }
            else
            {
                vm.Result.Quick.Price = result[0];
                vm.Result.Quick.PriceBis = result[1];
                vm.Result.Detail.Price = result[0];
                vm.Result.Detail.PriceBis = result[1];
            }
        }
        else if (exchange)
        {
            if (vm.Form.Tab.BulkSelected)
            {
                vm.Result.Bulk.Price = Resources.Resources.Main002_PriceLoaded; // "results loaded"
                vm.Result.Bulk.PriceBis = Resources.Resources.Main004_PriceRefresh; // "click to refresh"

                vm.Result.Bulk.Total = Resources.Resources.Main017_Results + " : " + vm.Logic.Task.Price.Buffer.StatsFetchBulk[1] + " " + Resources.Resources.Main018_ResultsDisplay; // resultsLoaded, resultCount
                vm.Result.Bulk.Total += " / " + vm.Logic.Task.Price.Buffer.StatsFetchBulk[2] + " " + Resources.Resources.Main020_ResultsListed;
                /*
                if (Task.Price.Buffer.StatsFetchBulk[2] == 200)
                {
                    vm.Result.Bulk.Total += " (" + Resources.Resources.Main023_ResultsMax + ")";
                }
                if (Task.Price.Buffer.StatsFetchBulk[1] < Task.Price.Buffer.StatsFetchBulk[2]) // loaded < result count
                {
                    vm.Form.Button.FetchBulksEnabled = true;
                }
                */
            }
            if (vm.Form.Tab.ShopSelected)
            {
                vm.Result.Shop.Price = Resources.Resources.Main002_PriceLoaded; // "results loaded"
                vm.Result.Shop.PriceBis = Resources.Resources.Main004_PriceRefresh; // "click to refresh"
            }
        }
        else
        {
            //liPriceDetail.Dispatcher.Thread.Join();
            int removed = vm.Logic.Task.Price.Buffer.StatsFetchDetail[4] - vm.Logic.Task.Price.Buffer.StatsFetchDetail[1];
            int unpriced = vm.Logic.Task.Price.Buffer.StatsFetchDetail[3];

            if (!exchange)
            {
                if (!vm.Result.Quick.PriceBis.Contains(Resources.Resources.Main024_ResultsSales, StringComparison.Ordinal) || result[1].Contains("ERROR", StringComparison.Ordinal)) // To rework IF we want to refresh price aswell on fetch.
                {
                    vm.Result.Quick.Price = result[0];
                    vm.Result.Quick.PriceBis = result[1];
                }
                if (!vm.Result.Detail.PriceBis.Contains(Resources.Resources.Main024_ResultsSales, StringComparison.Ordinal) || result[1].Contains("ERROR", StringComparison.Ordinal)) // To rework IF we want to refresh price aswell on fetch.
                {
                    bool cond = vm.Result.Detail.Price.Contains("(" + Resources.Resources.Main022_ResultsMin + ")", StringComparison.Ordinal) || vm.Result.Detail.Price.Contains(Resources.Resources.Main141_ResultsSingle, StringComparison.Ordinal);
                    if ((result[0].Contains("(" + Resources.Resources.Main022_ResultsMin + ")", StringComparison.Ordinal) || result[0].Contains(Resources.Resources.Main141_ResultsSingle, StringComparison.Ordinal)) && cond)
                    {
                        string tmpMin = vm.Result.Detail.Price.Contains(Resources.Resources.Main141_ResultsSingle, StringComparison.Ordinal) ?
                            vm.Result.Detail.Price.Replace(Strings.LF + Resources.Resources.Main141_ResultsSingle, string.Empty) + " (" + Resources.Resources.Main022_ResultsMin + ")" :
                            vm.Result.Detail.Price[..(vm.Result.Detail.Price.IndexOf(')', StringComparison.Ordinal) + 1)]; // .Substring(0, vm.Result.Detail.Price.IndexOf(')', StringComparison.Ordinal) + 1)

                        string tmpMax = result[0].Contains(Resources.Resources.Main141_ResultsSingle, StringComparison.Ordinal) ?
                            result[0].Replace(Strings.LF + Resources.Resources.Main141_ResultsSingle, string.Empty) + " (" + Resources.Resources.Main023_ResultsMax + ")" :
                            result[0][(result[0].IndexOf(Strings.LF, StringComparison.Ordinal) + 1)..]; // result[0].Substring(result[0].IndexOf('\n') + 1);

                        if (tmpMin.Replace(" (" + Resources.Resources.Main022_ResultsMin + ")", string.Empty) != tmpMax.Replace(" (" + Resources.Resources.Main023_ResultsMax + ")", string.Empty))
                        {
                            vm.Result.Detail.Price = tmpMin + Strings.LF + tmpMax;
                        }
                    }
                    else if (cond && result[0].Contains(Resources.Resources.Main008_PriceNoResult, StringComparison.Ordinal))
                    {
                        // no change
                    }
                    else
                    {
                        vm.Result.Detail.Price = result[0];
                    }

                    //tkPriceDetailBis.Text = result[1];
                    if (vm.Logic.Task.Price.Buffer.StatsFetchDetail[0] > 0)
                    {
                        vm.Result.Detail.PriceBis = Resources.Resources.Main017_Results + " : " + (vm.Logic.Task.Price.Buffer.StatsFetchDetail[0] - (removed + unpriced))
                            + " " + Resources.Resources.Main018_ResultsDisplay + " / " + vm.Logic.Task.Price.Buffer.StatsFetchDetail[0] + " " + Resources.Resources.Main019_ResultsFetched;
                        if (removed > 0 || unpriced > 0)
                        {
                            vm.Result.Detail.PriceBis += Strings.LF + Resources.Resources.Main010_PriceProcessed + " : ";
                            if (removed > 0)
                            {
                                vm.Result.Detail.PriceBis += removed + " " + Resources.Resources.Main025_ResultsAgregate;
                                if (unpriced > 0) vm.Result.Detail.PriceBis += Strings.LF + "          ";
                            }
                            if (unpriced > 0)
                            {
                                vm.Result.Detail.PriceBis += unpriced + " " + Resources.Resources.Main026_ResultsUnpriced;
                            }
                        }
                        if (vm.Logic.Task.Price.Buffer.StatsFetchDetail[0] < vm.Logic.Task.Price.Buffer.StatsFetchDetail[2])
                        {
                            vm.Form.FetchDetailIsEnabled = true;
                        }
                    }
                    else
                    {
                        vm.Result.Detail.PriceBis = string.Empty;
                    }
                }
                //tkPriceInfo2.Text = result + (result2 != "" ? " = " + result2 : "");
            }
            else
            {
                vm.Result.Quick.Price = Resources.Resources.Main009_PriceBulk;//"check bulk tab";
                vm.Result.Quick.PriceBis = string.Empty;

                //cbBulkGet2.IsEnabled = true;
                vm.Result.Detail.Price = Resources.Resources.Main002_PriceLoaded;
                vm.Result.Detail.PriceBis = Resources.Resources.Main004_PriceRefresh;
            }

            vm.Result.Detail.Total = vm.Logic.Task.Price.Buffer.DataToFetchDetail is not null ?
                Resources.Resources.Main027_ResultsTotal + " : " + vm.Logic.Task.Price.Buffer.StatsFetchDetail[2] + " " + Resources.Resources.Main020_ResultsListed
                + " / " + vm.Logic.Task.Price.Buffer.DataToFetchDetail.Total + " " + Resources.Resources.Main021_ResultsMatch
                : "ERROR : Can not retreive data from official website !";

            vm.Result.Quick.Total = vm.Logic.Task.Price.Buffer.StatsFetchDetail[4] > 0
                && !vm.Result.Quick.Total.Contains(Resources.Resources.Main011_PriceBase, StringComparison.Ordinal) ?
                Resources.Resources.Main011_PriceBase + " " + (vm.Logic.Task.Price.Buffer.StatsFetchDetail[0] - (removed + unpriced)) + " " + Resources.Resources.Main017_Results.ToLowerInvariant()
                : string.Empty;
        }
    }
    
    private static ItemBaseName FillMainViewModel(MainViewModel vm, string[] clipData)
    {
        ItemBaseName item = new();
        AsyncObservableCollection<ModLineViewModel> lMods = new();
        CultureInfo cultureEn = new(Strings.Culture[0]);
        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

        int idLang = DataManager.Config.Options.Language;
        string specifier = "G";

        string itemInherits = string.Empty, itemId = string.Empty, mapName = string.Empty;

        string[] data = clipData[0].Trim().Split(Strings.CRLF, StringSplitOptions.None);

        string itemClass = data[0].Split(':')[1].Trim();

        string[] rarityPrefix = data[1].Split(':');
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

        ItemFlag itemIs = new(clipData, idLang, itemRarity, itemType, itemClass);
        if (itemIs.ScourgedMap)
        {
            itemType = itemType.Replace(Resources.Resources.General103_Scourged, string.Empty).Trim();
        }

        vm.Form.ModLine = GetListMods(clipData, itemIs, itemName, itemType, itemClass, idLang, out TotalStats totalStats, out Dictionary<string, string> lOptions);

        if (totalStats.Resistance > 0)
        {
            vm.Form.Panel.Total.Resistance.Min = totalStats.Resistance.ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (totalStats.Life > 0)
        {
            vm.Form.Panel.Total.Life.Min = totalStats.Life.ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (totalStats.EnergyShield > 0)
        {
            vm.Form.Panel.Total.GlobalEs.Min = totalStats.EnergyShield.ToString(specifier, CultureInfo.InvariantCulture);
        }

        if (itemIs.SanctumResearch)
        {
            string[] resolve = lOptions[Resources.Resources.General114_SanctumResolve].Split(' ')[0].Split('/', StringSplitOptions.TrimEntries);
            if (resolve.Length == 2)
            {
                vm.Form.Panel.Sanctum.Resolve.Min = resolve[0];
                vm.Form.Panel.Sanctum.MaximumResolve.Max = resolve[1];
            }
            vm.Form.Panel.Sanctum.Inspiration.Min = lOptions[Resources.Resources.General115_SanctumInspiration];
            vm.Form.Panel.Sanctum.Aureus.Min = lOptions[Resources.Resources.General116_SanctumAureus];
        }

        if (lOptions[Resources.Resources.General036_Socket].Length > 0)
        {
            string socket = lOptions[Resources.Resources.General036_Socket];
            //Replace faster than LINQ count : var test = socket.Count(x => x == 'W');
            int white = socket.Length - socket.Replace("W", string.Empty).Length;
            int red = socket.Length - socket.Replace("R", string.Empty).Length;
            int green = socket.Length - socket.Replace("G", string.Empty).Length;
            int blue = socket.Length - socket.Replace("B", string.Empty).Length;

            string[] scklinks = socket.Split(' ');
            int lnkcnt = 0;
            for (int s = 0; s < scklinks.Length; s++)
            {
                if (lnkcnt < scklinks[s].Length)
                    lnkcnt = scklinks[s].Length;
            }
            int link = lnkcnt < 3 ? 0 : lnkcnt - (int)Math.Ceiling((double)lnkcnt / 2) + 1;

            vm.Form.Panel.Common.Sockets.RedColor = red.ToString();
            vm.Form.Panel.Common.Sockets.GreenColor = green.ToString();
            vm.Form.Panel.Common.Sockets.BlueColor = blue.ToString();
            vm.Form.Panel.Common.Sockets.WhiteColor = white.ToString();

            StringBuilder sbColors = new(Resources.Resources.Main210_cbSocketColorsTip);
            sbColors.AppendLine();
            sbColors.Append(Resources.Resources.Main209_cbSocketColors).Append(" : ");
            sbColors.Append(vm.Form.Panel.Common.Sockets.RedColor).Append('R').Append(' ');
            sbColors.Append(vm.Form.Panel.Common.Sockets.GreenColor).Append('G').Append(' ');
            sbColors.Append(vm.Form.Panel.Common.Sockets.BlueColor).Append('B').Append(' ');
            sbColors.Append(vm.Form.Panel.Common.Sockets.WhiteColor).Append('W');
            vm.Form.Condition.SocketColorsToolTip = sbColors.ToString();

            vm.Form.Panel.Common.Sockets.SocketMin = (white + red + green + blue).ToString();
            vm.Form.Panel.Common.Sockets.LinkMin = link > 0 ? link.ToString() : string.Empty;
            vm.Form.Panel.Common.Sockets.Selected = link > 4;
        }

        itemIs.Unidentified = lOptions[Resources.Resources.General039_Unidentify] == Strings.TrueOption;
        itemIs.Corrupted = lOptions[Resources.Resources.General037_Corrupt] == Strings.TrueOption;
        itemIs.Mirrored = lOptions[Resources.Resources.General109_Mirrored] == Strings.TrueOption;
        itemIs.FoilVariant = lOptions[Resources.Resources.General110_FoilUnique] == Strings.TrueOption;
        itemIs.ScourgedItem = lOptions[Resources.Resources.General099_ScourgedItem] == Strings.TrueOption;
        itemIs.MapCategory = lOptions[Resources.Resources.General034_MaTier].Length > 0 && !itemIs.Divcard;

        if (itemIs.ScourgedMap)
        {
            vm.Form.Panel.Scourged = true;
        }

        if (itemIs.Mirrored)
        {
            MainCommand.SetModCur();
        }

        //if no option is selected in config, use item corruption status. Otherwise, use the config value
        vm.Form.CorruptedIndex = itemIs.Divcard ? 0 : itemIs.Corrupted ? 2 : 0;

        if (itemIs.Rare && !itemIs.MapCategory && !itemIs.CapturedBeast) vm.Form.Tab.PoePriceEnable = true;
        if (itemIs.Gem)
        {
            vm.Form.Visible.AlternateGem = false; // TO remove
        }
        if (itemIs.Invitation || itemIs.MapCategory || itemIs.Gem || itemIs.Currency || itemIs.Divcard || itemIs.Flask || itemIs.Tincture
            || itemIs.Incubator || itemIs.Jewel || itemIs.Watchstone || itemIs.Chronicle || itemIs.Ultimatum) // Need more?
        {
            vm.Form.Visible.Sockets = false;
            vm.Form.Visible.Influences = false;
        }
        if (itemIs.Incubator)
        {
            vm.Form.Visible.Corrupted = false;
        }

        if (itemIs.Unique || itemIs.Unidentified || itemIs.Metamorph || itemIs.Watchstone || itemIs.MapFragment
            || itemIs.Invitation || itemIs.CapturedBeast || itemIs.Chronicle || itemIs.MapCategory || itemIs.Gem || itemIs.Currency || itemIs.Divcard || itemIs.Incubator)
        {
            vm.Form.Visible.BtnPoeDb = false;
        }

        if (itemIs.Metamorph)
        {
            itemId = itemInherits = "Entrailles/Entrails";

            int idx1 = itemType.LastIndexOf(' ');
            int idx2 = itemType.IndexOf(':');
            int idx3 = itemType.IndexOf('(');
            string itemTypeSub = idLang is 0 or 1 or 5 or 8 ? itemType[idx1..].Trim() // using range operator
                : idLang is 7 ? itemType[..2]
                : idLang is 9 ? itemType[^3..]
                : idx2 > -1 ? itemType[..idx2].Replace(" von", string.Empty, StringComparison.Ordinal).Trim()
                : idx3 > -1 ? itemType[..idx3].Trim()
                : string.Empty;

            if (itemTypeSub.Length > 0)
            {
                var meta =
                    from result in DataManager.Bases
                    where result.Id.Contains("Metamorphosis", StringComparison.Ordinal) && result.Name.Contains(itemTypeSub, StringComparison.Ordinal)
                    select result;
                if (meta.Any())
                {
                    BaseResultData metamorph = meta.First();
                    itemId = metamorph.Id;
                    itemInherits = metamorph.InheritsFrom;
                    itemType = metamorph.Name;
                }
            }

        }
        if (itemIs.CapturedBeast)
        {
            BaseResultData tmpBaseType = DataManager.Monsters.FirstOrDefault(x => x.Name.Contains(itemType, StringComparison.Ordinal));
            if (tmpBaseType is not null)
            {
                itemId = tmpBaseType.Id;
                itemInherits = tmpBaseType.InheritsFrom;
            }
        }
        if (!itemIs.Metamorph && !itemIs.CapturedBeast)
        {
            if (itemIs.Gem)
            {
                vm.Form.Panel.AlternateGemIndex = lOptions[Strings.AlternateGem] is Strings.Gem.Anomalous ? 1 :
                    lOptions[Strings.AlternateGem] is Strings.Gem.Divergent ? 2 :
                    lOptions[Strings.AlternateGem] is Strings.Gem.Phantasmal ? 3 : 0;

                StringBuilder sbType = new(itemType);
                sbType.Replace(Resources.Resources.General001_Anomalous, string.Empty)
                    .Replace(Resources.Resources.General002_Divergent, string.Empty)
                    .Replace(Resources.Resources.General003_Phantasmal, string.Empty).Replace("()", string.Empty);
                itemType = sbType.ToString().Trim();
                if (itemType.StartsWith(':'))
                {
                    itemType = itemType[1..].Trim();
                }

                if (lOptions[Resources.Resources.General037_Corrupt] is Strings.TrueOption
                    && lOptions[Resources.Resources.General038_Vaal] is Strings.TrueOption)
                {
                    for (int i = 3; i < clipData.Length; i++)
                    {
                        string seekVaal = clipData[i].Replace(Strings.CRLF, string.Empty).Trim();
                        BaseResultData tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == seekVaal);
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
            else if (lOptions[Resources.Resources.General047_Synthesis] is Strings.TrueOption)
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
                    where result.Name.Length > 0 && itemType.Contains(result.Name, StringComparison.Ordinal) && !result.Id.StartsWith("Gems", StringComparison.Ordinal)
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
            BaseResultData tmpBaseType2 = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);
            if (tmpBaseType2 is not null)
            {
                // 3.14 : to remove and replace by itemClass
                //Strings.lpublicID.TryGetValue(tmpBaseType2.NameEn, out publicID);
                itemIs.SpecialBase = Strings.lSpecialBases.Contains(tmpBaseType2.NameEn);
            }
        }

        if (itemInherits.Length == 0)
        {
            if (itemIs.MapCategory)
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
                /*
                string tierMap = itemIs.Blight ? "MapsBlighted" :
                    itemRarity.Equals(Resources.Resources.General006_Unique) ? "MapsUnique" :
                    "MapsTier" + lItemOption[Resources.Resources.General034_MaTier].Replace(" ", "");
                */
                string mapKind = itemIs.BlightMap || itemIs.BlightRavagedMap ? Strings.CurrencyType.MapsBlighted :
                    itemIs.Unique ? Strings.CurrencyType.MapsUnique : Strings.CurrencyType.Maps;
                /*+ lOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty)*/

                var mapId =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where result.Id == mapKind &&
                    (Entrie.Text.StartsWith(itemType, StringComparison.Ordinal)
                    || Entrie.Text.EndsWith(itemType, StringComparison.Ordinal))
                    select Entrie.Id;
                if (mapId.Any())
                {
                    itemId = mapId.First();
                }
                /*
                if (itemRarity.Equals(Resources.Resources.General006_Unique))
                {
                    itemID = ParseUniqueMaps(itemID);
                }
                */
                itemInherits = "Maps/AbstractMap";
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

                    itemInherits = cur is Strings.CurrencyType.Cards ? "DivinationCards/DivinationCardsCurrency"
                        : cur is Strings.CurrencyType.DelveResonators ? "Delve/DelveSocketableCurrency"
                        : cur is Strings.CurrencyType.Fragments && itemId != "ritual-vessel" && itemId != "valdos-puzzle-box" ? "MapFragments/AbstractMapFragment"
                        : cur is Strings.CurrencyType.Incubators ? "Legion/Incubator"
                        //: cur is Strings.CurrencyType.Scarabs ? "Scarabs/Scarab"
                        : "Currency/StackableCurrency";
                }
            }
            else if (itemIs.Gem)
            {
                GemResultData tmpGem = DataManager.Gems.FirstOrDefault(x => x.Name == itemType);
                if (tmpGem is not null)
                {
                    if (gemName.Length is 0 && tmpGem.Type != tmpGem.Name) // transfigured normal gem
                    {
                        itemType = tmpGem.Type;
                        itemInherits = Strings.Inherit.Gems + '/' + tmpGem.Disc;
                    }
                    if (gemName.Length > 0 && tmpGem.Type == tmpGem.Name)
                    {
                        GemResultData tmpGem2 = DataManager.Gems.FirstOrDefault(x => x.Name == gemName);
                        if (tmpGem2 is not null) // transfigured vaal gem
                        {
                            itemInherits = Strings.Inherit.Gems + '/' + tmpGem2.Disc;
                        }
                    }
                }
            }

            if (itemInherits.Length == 0)
            {
                BaseResultData tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);
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
        string item_quality = RegexUtil.NumericalPattern().Replace(lOptions[Resources.Resources.General035_Quality].Trim(), string.Empty);
        string inherit = item.Inherits[0]; // FLAG

        bool by_type = inherit is Strings.Inherit.Weapons or Strings.Inherit.Quivers or Strings.Inherit.Armours or Strings.Inherit.Amulets or Strings.Inherit.Rings or Strings.Inherit.Belts;

        if (inherit is Strings.Inherit.Weapons or Strings.Inherit.Quivers or Strings.Inherit.Armours)
        {
            vm.Form.Visible.Sockets = true;
        }

        bool showRes = false, showLife = false, showEs = false;
        if (vm.Form.Panel.Total.Resistance.Min.Length > 0)
        {
            showRes = true;
            if (DataManager.Config.Options.AutoSelectRes && (Common.StrToDouble(vm.Form.Panel.Total.Resistance.Min) >= 36 || itemIs.Jewel))
            {
                vm.Form.Panel.Total.Resistance.Selected = true;
            }
        }
        if (vm.Form.Panel.Total.Life.Min.Length > 0)
        {
            showLife = true;
            if (DataManager.Config.Options.AutoSelectLife && (Common.StrToDouble(vm.Form.Panel.Total.Life.Min) >= 40 || itemIs.Jewel))
            {
                vm.Form.Panel.Total.Life.Selected = true;
            }
        }
        if (vm.Form.Panel.Total.GlobalEs.Min.Length > 0)
        {
            if (inherit is not Strings.Inherit.Armours)
            {
                showEs = true;
                if (DataManager.Config.Options.AutoSelectGlobalEs && (Common.StrToDouble(vm.Form.Panel.Total.GlobalEs.Min) >= 38 || itemIs.Jewel))
                {
                    vm.Form.Panel.Total.GlobalEs.Selected = true;
                }
            }
            else
            {
                vm.Form.Panel.Total.GlobalEs.Min = string.Empty;
            }
        }
        if (showRes || showLife || showEs)
        {
            vm.Form.Visible.Total = true;
        }

        if (itemIs.ShowDetail)
        {
            BaseResultData tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);

            item.Type = tmpBaseType is null ? itemType : tmpBaseType.Name;
            item.TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn;

            if (itemIs.Incubator || inherit is Strings.Inherit.Gems or Strings.Inherit.UniqueFragments or Strings.Inherit.Labyrinth) // || is_essences
            {
                int i = inherit is Strings.Inherit.Gems ? 3 : 1;
                vm.Form.Detail = clipData.Length > 2 ? (inherit is Strings.Inherit.Gems or Strings.Inherit.Labyrinth ? clipData[i] : string.Empty) + clipData[i + 1] : string.Empty;
            }
            else
            {
                int i = inherit is Strings.Inherit.Delve ? 3 : itemIs.Divcard || inherit is Strings.Inherit.Currency ? 2 : 1;
                vm.Form.Detail = clipData.Length > i + 1 ? clipData[i] + clipData[i + 1] : clipData[^1];

                if (clipData.Length > i + 1)
                {
                    int v = clipData[i - 1].TrimStart().IndexOf("Apply: ", StringComparison.Ordinal);
                    vm.Form.Detail += v > -1 ? string.Empty + Strings.LF + Strings.LF + clipData[i - 1].TrimStart().Split(Strings.LF)[v == 0 ? 0 : 1].TrimEnd() : string.Empty;
                }
            }

            if (idLang == 0) // en
            {
                vm.Form.Detail = vm.Form.Detail.Replace(Resources.Resources.General097_SClickSplitItem, string.Empty);
                //var vmtest = Regex.Replace(Vm.Form.Detail, "<(uniqueitem|prophecy|divination|gemitem|magicitem|rareitem|whiteitem|corrupted|default|normal|augmented|size:[0-9]+)>", string.Empty);
                vm.Form.Detail = RegexUtil.DetailPattern().Replace(vm.Form.Detail, string.Empty);
            }
        }
        else
        {
            for (int i = 0; i < vm.Form.ModLine.Count; i++)
            {
                ItemFilter ifilter = vm.Form.ModLine[i].ItemFilter;

                string modTextEnglish = vm.Form.ModLine[i].Mod;
                if (idLang != 0) // !StringsTable.Culture[idLang].Equals("en-US")
                {
                    AffixFilterEntrie filterEntrie = vm.Form.ModLine[i].Affix[0];
                    if (filterEntrie is not null)
                    {
                        var enResult =
                            from result in DataManager.FilterEn.Result
                            from Entrie in result.Entries
                            where Entrie.ID == filterEntrie.ID
                            select Entrie.Text;
                        if (enResult.Any())
                        {
                            modTextEnglish = enResult.First();
                        }
                    }
                }
                bool condLife = DataManager.Config.Options.AutoSelectLife && !itemIs.Unique && Modifier.IsTotalStat(modTextEnglish, Stat.Life) && !modTextEnglish.ToLowerInvariant().Contains("to strength", StringComparison.Ordinal);
                bool condEs = DataManager.Config.Options.AutoSelectGlobalEs && !itemIs.Unique && Modifier.IsTotalStat(modTextEnglish, Stat.Es) && inherit is not "Armours";
                bool condRes = DataManager.Config.Options.AutoSelectRes && !itemIs.Unique && Modifier.IsTotalStat(modTextEnglish, Stat.Resist);
                bool implicitRegular = vm.Form.ModLine[i].Affix[vm.Form.ModLine[i].AffixIndex].Name == Resources.Resources.General013_Implicit;
                bool implicitCorrupt = vm.Form.ModLine[i].Affix[vm.Form.ModLine[i].AffixIndex].Name == Resources.Resources.General017_CorruptImp;
                bool implicitEnch = vm.Form.ModLine[i].Affix[vm.Form.ModLine[i].AffixIndex].Name == Resources.Resources.General011_Enchant;
                bool implicitScourge = vm.Form.ModLine[i].Affix[vm.Form.ModLine[i].AffixIndex].Name == Resources.Resources.General099_Scourge;

                if (implicitScourge) // Temporary
                {
                    vm.Form.ModLine[i].Selected = false;
                    vm.Form.ModLine[i].ItemFilter.Disabled = true;
                }

                if (implicitRegular || implicitCorrupt || implicitEnch)
                {
                    bool condImpAuto = DataManager.Config.Options.AutoCheckImplicits && implicitRegular;
                    bool condCorruptAuto = DataManager.Config.Options.AutoCheckCorruptions && implicitCorrupt;
                    bool condEnchAuto = DataManager.Config.Options.AutoCheckEnchants && implicitEnch;

                    bool specialImp = false;
                    AffixFilterEntrie filterEntrie = vm.Form.ModLine[i].Affix[vm.Form.ModLine[i].AffixIndex];
                    if (filterEntrie is not null)
                    {
                        specialImp = Strings.Stat.lSpecialImplicits.Contains(filterEntrie.ID);
                    }

                    if ((condImpAuto || condCorruptAuto || condEnchAuto) && !condLife && !condEs && !condRes || specialImp || ifilter.Id is Strings.Stat.MapOccupConq or Strings.Stat.MapOccupElder or Strings.Stat.AreaInflu)
                    {
                        vm.Form.ModLine[i].Selected = true;
                        vm.Form.ModLine[i].ItemFilter.Disabled = false;
                    }
                    if (ifilter.Id is Strings.Stat.MapOccupConq)
                    {
                        itemIs.ConqMap = true;
                    }
                }

                if (inherit.Length > 0 || itemIs.ChargedCompass || itemIs.Voidstone || itemIs.FilledCoffin) // && i >= Imp_cnt
                {
                    if (DataManager.Config.Options.AutoCheckUniques && itemIs.Unique ||
                            DataManager.Config.Options.AutoCheckNonUniques && !itemIs.Unique)
                    {
                        bool logbookRareMod = ifilter.Id.Contains(Strings.Stat.LogbookBoss, StringComparison.Ordinal) || ifilter.Id.Contains(Strings.Stat.LogbookArea, StringComparison.Ordinal) || ifilter.Id.Contains(Strings.Stat.LogbookTwice, StringComparison.Ordinal);
                        bool craftedCond = ifilter.Id.Contains(Strings.Stat.Crafted, StringComparison.Ordinal);
                        if (vm.Form.ModLine[i].AffixIndex >= 0)
                        {
                            craftedCond = craftedCond || vm.Form.ModLine[i].Affix[vm.Form.ModLine[i].AffixIndex].Name == Resources.Resources.General012_Crafted && !DataManager.Config.Options.AutoCheckCrafted;
                        }
                        if (craftedCond || itemIs.Logbook && !logbookRareMod)
                        {
                            vm.Form.ModLine[i].Selected = false;
                            vm.Form.ModLine[i].ItemFilter.Disabled = true;
                        }
                        else if (!itemIs.Invitation && !itemIs.MapCategory && !craftedCond && !condLife && !condEs && !condRes)
                        {
                            bool condChronicle = false, condMirroredTablet = false;
                            if (itemIs.Chronicle)
                            {
                                AffixFilterEntrie entry = vm.Form.ModLine[i].Affix[0];
                                if (entry is not null)
                                {
                                    condChronicle = entry.ID.Contains(Strings.Stat.Room01, StringComparison.Ordinal) // Apex of Atzoatl
                                        || entry.ID.Contains(Strings.Stat.Room11, StringComparison.Ordinal) // Doryani's Institute
                                        || entry.ID.Contains(Strings.Stat.Room15, StringComparison.Ordinal) // Apex of Ascension
                                        || entry.ID.Contains(Strings.Stat.Room17, StringComparison.Ordinal); // Locus of Corruption
                                }
                            }
                            if (itemIs.MirroredTablet)
                            {
                                AffixFilterEntrie entry = vm.Form.ModLine[i].Affix[0];
                                if (entry is not null)
                                {
                                    condMirroredTablet = entry.ID.Contains(Strings.Stat.Tablet01, StringComparison.Ordinal) // Paradise
                                        || entry.ID.Contains(Strings.Stat.Tablet02, StringComparison.Ordinal) // Kalandra
                                        || entry.ID.Contains(Strings.Stat.Tablet03, StringComparison.Ordinal) // the Sun
                                        || entry.ID.Contains(Strings.Stat.Tablet04, StringComparison.Ordinal); // Angling
                                }
                            }
                            if (!itemIs.Chronicle && !itemIs.Ultimatum && !itemIs.MirroredTablet || condChronicle || condMirroredTablet)
                            {
                                if (!implicitRegular && !implicitCorrupt && !implicitEnch && !implicitScourge)
                                {
                                    vm.Form.ModLine[i].Selected = true;
                                    vm.Form.ModLine[i].ItemFilter.Disabled = false;
                                }
                            }
                        }
                    }

                    string[] idStat = vm.Form.ModLine[i].Affix[vm.Form.ModLine[i].AffixIndex].ID.Split('.');
                    if (idStat.Length == 2)
                    {
                        if (itemIs.MapCategory &&
                            DataManager.Config.DangerousMapMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                        {
                            vm.Form.ModLine[i].ModKind = Strings.ModKind.DangerousMod;
                        }
                        if (!itemIs.MapCategory &&
                            DataManager.Config.RareItemMods.FirstOrDefault(x => x.Id.IndexOf(idStat[1], StringComparison.Ordinal) > -1) is not null)
                        {
                            vm.Form.ModLine[i].ModKind = Strings.ModKind.RareMod;
                        }
                    }
                }

                if (vm.Form.ModLine[i].Selected)
                {
                    if (itemIs.Unique)
                    {
                        vm.Form.ModLine[i].AffixCanBeEnabled = false;
                    }
                    else
                    {
                        vm.Form.ModLine[i].AffixEnable = true;
                    }
                }

                if (vm.Form.Panel.Common.Sockets.SocketMin is "6")
                {
                    bool condColors = false;
                    AffixFilterEntrie entry = vm.Form.ModLine[i].Affix[0];
                    if (entry is not null)
                    {
                        condColors = entry.ID.Contains(Strings.Stat.SocketsUnmodifiable, StringComparison.Ordinal);
                    }
                    if (condColors || vm.Form.Panel.Common.Sockets.WhiteColor is "6")
                    {
                        vm.Form.Condition.SocketColors = true;
                        vm.Form.Panel.Common.Sockets.Selected = true;
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
                vm.Form.Visible.Damage = true;

                double qualityDPS = Common.StrToDouble(item_quality);
                double physicalDPS = DamageToDPS(lOptions[Resources.Resources.General058_PhysicalDamage]);
                double elementalDPS = DamageToDPS(lOptions[Resources.Resources.General059_ElementalDamage]);
                double chaosDPS = DamageToDPS(lOptions[Resources.Resources.General060_ChaosDamage]);
                //double attacksPerSecond = StrToDouble(Regex.Replace(lItemOption[Restr.AttacksPerSecond], @"\([a-zA-Z]+\)", "").Trim(), 0); //return 0
                //string apsOld = Regex.Replace(lOptions[Resources.Resources.General061_AttacksPerSecond], "[^0-9.]", string.Empty);
                string aps = RegexUtil.NumericalPattern2().Replace(lOptions[Resources.Resources.General061_AttacksPerSecond], string.Empty);

                double attacksPerSecond = Common.StrToDouble(aps);

                physicalDPS = physicalDPS / 2 * attacksPerSecond;
                if (qualityDPS < 20 && !itemIs.Corrupted)
                {
                    double physInc = Common.StrToDouble(lOptions[Strings.Stat.IncPhys]);
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
                vm.Form.Dps = Math.Round(totalDPS, 0).ToString() + " DPS";

                StringBuilder sbToolTip = new();

                if (DataManager.Config.Options.AutoSelectDps && totalDPS > 100)
                {
                    vm.Form.Panel.Damage.Total.Selected = true;
                }

                // Allready rounded : example 0.46 => 0.5
                vm.Form.Panel.Damage.Total.Min = totalDPS.ToString(specifier, CultureInfo.InvariantCulture);

                if (Math.Round(physicalDPS, 2) > 0)
                {
                    string qual = qualityDPS > 20 || itemIs.Corrupted ? qualityDPS.ToString() : "20";
                    sbToolTip.Append("PHYS. Q").Append(qual).Append(" : ").Append(Math.Round(physicalDPS, 0)).Append(" dps");

                    vm.Form.Panel.Damage.Physical.Min = Math.Round(physicalDPS, 0).ToString(specifier, CultureInfo.InvariantCulture);
                }
                if (Math.Round(elementalDPS, 2) > 0)
                {
                    if (sbToolTip.ToString().Length > 0) sbToolTip.AppendLine();
                    sbToolTip.Append("ELEMENTAL : ").Append(Math.Round(elementalDPS, 0)).Append(" dps");

                    vm.Form.Panel.Damage.Elemental.Min = Math.Round(elementalDPS, 0).ToString(specifier, CultureInfo.InvariantCulture);
                }
                if (Math.Round(chaosDPS, 2) > 0)
                {
                    if (sbToolTip.ToString().Length > 0) sbToolTip.AppendLine();
                    sbToolTip.Append("CHAOS : ").Append(Math.Round(chaosDPS, 0)).Append(" dps");
                }
                vm.Form.DpsTip = sbToolTip.ToString();
            }

            if (!itemIs.Unidentified && inherit is Strings.Inherit.Armours)
            {
                vm.Form.Visible.Defense = true;

                //string armourOld = Regex.Replace(lOptions[Resources.Resources.General055_Armour].Trim(), "[^0-9]", string.Empty);
                //string energyOld = Regex.Replace(lOptions[Resources.Resources.General056_Energy].Trim(), "[^0-9]", string.Empty);
                //string evasionOld = Regex.Replace(lOptions[Resources.Resources.General057_Evasion].Trim(), "[^0-9]", string.Empty);
                //string wardOld = Regex.Replace(lOptions[Resources.Resources.General095_Ward].Trim(), "[^0-9]", string.Empty);

                string armour = RegexUtil.NumericalPattern().Replace(lOptions[Resources.Resources.General055_Armour].Trim(), string.Empty);
                string energy = RegexUtil.NumericalPattern().Replace(lOptions[Resources.Resources.General056_Energy].Trim(), string.Empty);
                string evasion = RegexUtil.NumericalPattern().Replace(lOptions[Resources.Resources.General057_Evasion].Trim(), string.Empty);
                string ward = RegexUtil.NumericalPattern().Replace(lOptions[Resources.Resources.General095_Ward].Trim(), string.Empty);


                if (armour.Length > 0)
                {
                    if (DataManager.Config.Options.AutoSelectArEsEva) vm.Form.Panel.Defense.Armour.Selected = true;
                    vm.Form.Panel.Defense.Armour.Min = armour;
                    //Vm.Form.Visible.Armour = true;
                }
                if (energy.Length > 0)
                {
                    if (DataManager.Config.Options.AutoSelectArEsEva) vm.Form.Panel.Defense.Energy.Selected = true;
                    vm.Form.Panel.Defense.Energy.Min = energy;
                    //Vm.Form.Visible.Energy = true;
                }
                if (evasion.Length > 0)
                {
                    if (DataManager.Config.Options.AutoSelectArEsEva) vm.Form.Panel.Defense.Evasion.Selected = true;
                    vm.Form.Panel.Defense.Evasion.Min = evasion;
                    //Vm.Form.Visible.Evasion = true;
                }

                if (ward.Length > 0)
                {
                    if (DataManager.Config.Options.AutoSelectArEsEva) vm.Form.Panel.Defense.Ward.Selected = true;
                    vm.Form.Panel.Defense.Ward.Min = ward;
                    vm.Form.Visible.Ward = true;
                }
                else
                {
                    vm.Form.Visible.Armour = true;
                    vm.Form.Visible.Energy = true;
                    vm.Form.Visible.Evasion = true;
                }
            }

            BaseResultData tmpBaseType = null;
            if (itemIs.CapturedBeast)
            {
                tmpBaseType = DataManager.Monsters.FirstOrDefault(x => x.Name.Contains(itemType, StringComparison.Ordinal));
                item.Type = tmpBaseType is null ? itemType : tmpBaseType.Name.Replace("\"", string.Empty);
                item.TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn.Replace("\"", string.Empty);
                itemName = string.Empty;

                //mItemBaseName.Inherits[0] = "LeagueBestiary";
            }
            else
            {
                tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == itemType);
                item.Type = tmpBaseType is null ? itemType : tmpBaseType.Name;
                item.TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn;
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
        if (item.TypeEn.Length == 0) //!itemIs.CapturedBeast
        {
            if (idLang == 0) // en
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

        item.Name = vm.Form.ItemName = itemName;
        item.NameEn = string.Empty;
        if (idLang == 0) //en
        {
            item.NameEn = item.Name;
        }
        else
        {
            if (itemName.Length > 0)
            {
                WordResultData wordRes = DataManager.Words.FirstOrDefault(x => x.Name == itemName);
                if (wordRes is not null)
                {
                    item.NameEn = wordRes.NameEn;
                }
            }
        }

        if (itemIs.FilledCoffin && item.NameEn.Length == 0) // for poe ninja
        {
            StringBuilder sb = new();
            int cpt = 0;
            foreach (var mod in vm.Form.ModLine)
            {
                string modTextEnglish = mod.Mod;
                if (idLang != 0)
                {
                    AffixFilterEntrie filterEntrie = mod.Affix?[0];
                    if (filterEntrie is not null)
                    {
                        var enResult =
                            from result in DataManager.FilterEn.Result
                            from Entrie in result.Entries
                            where Entrie.ID == filterEntrie.ID
                            select Entrie.Text;
                        if (enResult.Any())
                        {
                            modTextEnglish = enResult.First();
                        }
                    }
                }
                StringBuilder sbMod = new(modTextEnglish);
                sbMod.Replace("#", mod.Min).Replace("+", string.Empty).Replace("%", string.Empty).Replace(" ", "-").Replace("2-other-Corpse-", "2-other-Corpses-");
                if (cpt > 0)
                {
                    sb.Append('-');
                }
                sb.Append(sbMod.ToString().ToLowerInvariant());
                cpt++;
            }
            item.NameEn = sb.ToString();
        }

        var byBase = !itemIs.Unique && !itemIs.Normal && !itemIs.Currency && !itemIs.MapCategory && !itemIs.Divcard && !itemIs.CapturedBeast && !itemIs.Gem
            && !itemIs.Flask && !itemIs.Tincture && !itemIs.Unidentified && !itemIs.Watchstone && !itemIs.Invitation && !itemIs.Logbook && !itemIs.SpecialBase;
        vm.Form.ByBase = !byBase || DataManager.Config.Options.SearchByType;

        string qualType = vm.Form.Panel.AlternateGemIndex is 1 ? Resources.Resources.General001_Anomalous :
            vm.Form.Panel.AlternateGemIndex is 2 ? Resources.Resources.General002_Divergent :
            vm.Form.Panel.AlternateGemIndex is 3 ? Resources.Resources.General003_Phantasmal : string.Empty;

        vm.Form.ItemBaseType = qualType.Length > 0 ?
            idLang is 2 or 3 ? itemType + " " + qualType // fr,es
            : idLang is 4 ? itemType + " (" + qualType + ")" // de
            : idLang is 6 ? qualType + ": " + itemType// ru
            : qualType + " " + itemType // en,kr,br,th,tw,cn
            : itemType;

        string tier = lOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
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
                if (cur.Any())
                {
                    if (cur.First())
                    {
                        itemIs.ExchangeCurrency = true;
                    }
                }
            }
        }

        vm.Form.Rarity.Item =
            itemIs.ExchangeCurrency && !itemIs.MapCategory && !itemIs.Invitation ? Resources.Resources.General005_Any :
            itemIs.FoilVariant ? Resources.Resources.General110_FoilUnique : itemRarity;

        /*
        Vm.Form.ItemNameColor =
            itemIs.Normal ? Brushes.White :
            itemIs.Magic ? Brushes.DeepSkyBlue :
            itemIs.Rare ? Brushes.Gold :
            itemIs.Relic ? Brushes.Green :
            itemIs.Unique ? Brushes.Peru :
            Brushes.White;
        */
        vm.Form.ItemNameColor = vm.Form.Rarity.Item == Resources.Resources.General008_Magic ? Strings.Color.DeepSkyBlue :
            vm.Form.Rarity.Item == Resources.Resources.General007_Rare ? Strings.Color.Gold :
            vm.Form.Rarity.Item == Resources.Resources.General110_FoilUnique ? Strings.Color.Green :
            vm.Form.Rarity.Item == Resources.Resources.General006_Unique ? Strings.Color.Peru : string.Empty;
        //Vm.Form.Rarity.Item == Resources.Resources.General005_Any ? string.Empty :
        //Vm.Form.Rarity.Item == Resources.Resources.General009_Normal ? string.Empty :
        //Vm.Form.Rarity.Item == Resources.Resources.General010_AnyNU ? string.Empty : string.Empty;

        if ((itemIs.MapCategory || itemIs.Watchstone || itemIs.Invitation || itemIs.Logbook || itemIs.ChargedCompass || itemIs.Voidstone) && !itemIs.Unique)
        {
            vm.Form.Rarity.Item = Resources.Resources.General010_AnyNU;
            if (!itemIs.Corrupted)
            {
                vm.Form.CorruptedIndex = 1;
            }
            if (itemIs.Voidstone)
            {
                //Vm.Form.ItemBaseType = String.Empty;
                vm.Form.ByBase = false;
            }
        }

        if (vm.Form.Rarity.Item.Length == 0)
        {
            vm.Form.Rarity.Item = itemRarity;
        }

        if (/*!itemIs.Unique &&*/ !itemIs.Currency && !itemIs.ExchangeCurrency && !itemIs.CapturedBeast /*&& !itemIs.Prophecy*/ && !itemIs.Metamorph)
        {
            vm.Form.Visible.Conditions = true;
        }

        bool hideUserControls = false;
        if (!itemIs.Invitation && !itemIs.MapCategory && !itemIs.AllflameEmber && (itemIs.Currency && !itemIs.Chronicle && !itemIs.Ultimatum && !itemIs.FilledCoffin || itemIs.ExchangeCurrency || itemIs.CapturedBeast /*|| itemIs.Prophecy*/ || itemIs.Metamorph || itemIs.MemoryLine))
        {
            hideUserControls = true;

            if (!itemIs.Metamorph && !itemIs.MirroredTablet && !itemIs.SanctumResearch && !itemIs.Corpses)
            {
                vm.Form.Visible.PanelForm = false;
            }
            else
            {
                vm.Form.Visible.Quality = false;
            }
            vm.Form.Visible.PanelStat = false;

            vm.Form.Visible.Influences = false;
            vm.Form.Visible.ByBase = false;
            vm.Form.Visible.Rarity = false;
            vm.Form.Visible.Corrupted = false;
            vm.Form.Visible.CheckAll = false;
        }
        if (hideUserControls && itemIs.Facetor)
        {
            vm.Form.Visible.Facetor = true;
            vm.Form.Panel.FacetorMin = lOptions[Resources.Resources.Main154_tbFacetor].Replace(" ", string.Empty);
        }

        vm.Form.Tab.QuickEnable = true;
        vm.Form.Tab.DetailEnable = true;
        bool uniqueTag = vm.Form.Rarity.Item == Resources.Resources.General006_Unique;
        if (itemIs.ExchangeCurrency && (!uniqueTag || itemIs.MapCategory)) // TODO update with itemIs.Unique
        {
            vm.Form.Tab.BulkEnable = true;
            vm.Form.Tab.ShopEnable = true;

            bool isMap = mapName.Length > 0;

            vm.Form.Bulk.AutoSelect = true;
            vm.Form.Bulk.Args = "pay/equals";
            vm.Form.Bulk.Currency = isMap ? mapName : itemType;
            vm.Form.Bulk.Tier = isMap ? tier : string.Empty;
        }

        if (itemIs.ExchangeCurrency || itemIs.MapCategory || itemIs.Gem || itemIs.CapturedBeast /*|| itemIs.Prophecy*/ || itemIs.Metamorph) // Select Detailed TAB
        {
            if (!(itemIs.MapCategory && itemIs.Corrupted)) // checkMapDetails
            {
                vm.Form.Tab.DetailSelected = true;
            }
        }
        if (!vm.Form.Tab.DetailSelected)
        {
            vm.Form.Tab.QuickSelected = true;
        }

        if (!itemIs.ExchangeCurrency && !itemIs.Chronicle && !itemIs.Metamorph && !itemIs.CapturedBeast && !itemIs.Ultimatum)
        {
            vm.Form.Visible.ModSet = true;
        }

        if (!itemIs.Unique && (itemIs.Flask || itemIs.Tincture))
        {
            var iLvl = RegexUtil.NumericalPattern().Replace(lOptions[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
            if (int.TryParse(iLvl, out int result) && result >= 84)
            {
                vm.Form.Panel.Common.Quality.Selected = item_quality.Length > 0 && int.Parse(item_quality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!hideUserControls || itemIs.Metamorph || itemIs.Corpses)
        {
            //var vmtest = Regex.Replace(lOptions[itemIs.Gem ? Resources.Resources.General031_Lv : Resources.Resources.General032_ItemLv].Trim(), "[^0-9]", string.Empty);
            vm.Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(lOptions[itemIs.Gem ? Resources.Resources.General031_Lv : Resources.Resources.General032_ItemLv].Trim(), string.Empty);

            vm.Form.Panel.Common.Quality.Min = item_quality;

            vm.Form.Influence.ShaperText = Resources.Resources.Main037_Shaper;
            vm.Form.Influence.ElderText = Resources.Resources.Main038_Elder;
            vm.Form.Influence.CrusaderText = Resources.Resources.Main039_Crusader;
            vm.Form.Influence.RedeemerText = Resources.Resources.Main040_Redeemer;
            vm.Form.Influence.HunterText = Resources.Resources.Main041_Hunter;
            vm.Form.Influence.WarlordText = Resources.Resources.Main042_Warlord;

            vm.Form.Influence.Shaper = lOptions[Resources.Resources.General041_Shaper] is Strings.TrueOption;
            vm.Form.Influence.Elder = lOptions[Resources.Resources.General042_Elder] is Strings.TrueOption;
            vm.Form.Influence.Crusader = lOptions[Resources.Resources.General043_Crusader] is Strings.TrueOption;
            vm.Form.Influence.Redeemer = lOptions[Resources.Resources.General044_Redeemer] is Strings.TrueOption;
            vm.Form.Influence.Hunter = lOptions[Resources.Resources.General045_Hunter] is Strings.TrueOption;
            vm.Form.Influence.Warlord = lOptions[Resources.Resources.General046_Warlord] is Strings.TrueOption;

            vm.Commands.CheckInfluence(null);
            vm.Commands.CheckCondition(null);

            vm.Form.Panel.SynthesisBlight = itemIs.MapCategory && itemIs.BlightMap || lOptions[Resources.Resources.General047_Synthesis] is Strings.TrueOption;
            vm.Form.Panel.BlighRavaged = itemIs.MapCategory && itemIs.BlightRavagedMap;

            if (itemIs.MapCategory)
            {
                vm.Form.Panel.Common.ItemLevel.Min = lOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty); // 0x20
                vm.Form.Panel.Common.ItemLevel.Max = lOptions[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);

                vm.Form.Panel.Common.ItemLevelLabel = Resources.Resources.Main094_lbTier;

                vm.Form.Panel.Common.ItemLevel.Selected = true;
                vm.Form.Panel.SynthesisBlightLabel = "Blighted";
                vm.Form.Visible.SynthesisBlight = true;
                vm.Form.Visible.BlightRavaged = true;
                vm.Form.Visible.Scourged = false;
                vm.Form.Visible.Sockets = false;

                vm.Form.Visible.ByBase = false;
                vm.Form.Visible.Conditions = false;
                vm.Form.Visible.ModSet = false;

                vm.Form.Visible.MapStats = true;

                vm.Form.Panel.Map.Quantity.Min = lOptions[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
                vm.Form.Panel.Map.Rarity.Min = lOptions[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
                vm.Form.Panel.Map.PackSize.Min = lOptions[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
                vm.Form.Panel.Map.MoreScarab.Min = lOptions[Resources.Resources.General140_MoreScarabs].Replace(" ", string.Empty);
                vm.Form.Panel.Map.MoreCurrency.Min = lOptions[Resources.Resources.General139_MoreCurrency].Replace(" ", string.Empty);
                vm.Form.Panel.Map.MoreDivCard.Min = lOptions[Resources.Resources.General142_MoreDivinationCards].Replace(" ", string.Empty);
                vm.Form.Panel.Map.MoreMap.Min = lOptions[Resources.Resources.General141_MoreMaps].Replace(" ", string.Empty);

                if (vm.Form.Panel.Common.ItemLevel.Min is "17" && vm.Form.Panel.Common.ItemLevel.Max is "17")
                {
                    vm.Form.Visible.SynthesisBlight = false;
                    vm.Form.Visible.BlightRavaged = false;

                    StringBuilder sbReward = new(lOptions[Resources.Resources.General071_Reward]);
                    if (sbReward.ToString().Length > 0)
                    {
                        sbReward.Replace(Resources.Resources.General125_Foil, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
                        vm.Form.Panel.Reward.Text = new(sbReward.ToString().Trim());
                        vm.Form.Panel.Reward.FgColor = Strings.Color.Peru;
                        vm.Form.Panel.Reward.Tip = Strings.Reward.FoilUnique;

                        vm.Form.Visible.Reward = true;
                    }
                }
            }
            else if (itemIs.Gem)
            {
                vm.Form.Panel.Common.ItemLevel.Selected = true;
                vm.Form.Panel.Common.Quality.Selected = item_quality.Length > 0 && int.Parse(item_quality, CultureInfo.InvariantCulture) > 12;
                if (!itemIs.Corrupted)
                {
                    vm.Form.CorruptedIndex = 1; // NO
                }
                vm.Form.Visible.Influences = false;
                vm.Form.Visible.ByBase = false;
                vm.Form.Visible.CheckAll = false;
                vm.Form.Visible.Conditions = false;
                vm.Form.Visible.ModSet = false;
                vm.Form.Visible.Rarity = false;
            }
            else if (itemIs.FilledCoffin)
            {
                vm.Form.Visible.ByBase = false;
                vm.Form.Visible.Rarity = false;
                vm.Form.Visible.Corrupted = false;
                vm.Form.Visible.Quality = false;

                vm.Form.Panel.Common.ItemLevel.Min = lOptions[Resources.Resources.General129_CorpseLevel].Replace(" ", string.Empty);
                vm.Form.Panel.Common.ItemLevel.Selected = true;
            }
            else if (itemIs.AllflameEmber)
            {
                vm.Form.Visible.Corrupted = false;
                vm.Form.Visible.Quality = false;
                vm.Form.Visible.Influences = false;
                vm.Form.Visible.ByBase = false;
                vm.Form.Visible.CheckAll = false;
                vm.Form.Visible.Conditions = false;
                vm.Form.Visible.ModSet = false;
                vm.Form.Visible.Rarity = false;

                vm.Form.Panel.Common.ItemLevel.Min = RegexUtil.NumericalPattern().Replace(lOptions[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
                vm.Form.Panel.Common.ItemLevel.Selected = true;
            }
            else if (by_type && itemIs.Normal)
            {
                vm.Form.Panel.Common.ItemLevel.Selected = vm.Form.Panel.Common.ItemLevel.Min.Length > 0 && int.Parse(vm.Form.Panel.Common.ItemLevel.Min, CultureInfo.InvariantCulture) > 82;
            }
            else if (vm.Form.Rarity.Item != Resources.Resources.General006_Unique && itemIs.Cluster)
            {
                vm.Form.Panel.Common.ItemLevel.Selected = vm.Form.Panel.Common.ItemLevel.Min.Length > 0 && int.Parse(vm.Form.Panel.Common.ItemLevel.Min, CultureInfo.InvariantCulture) >= 78;
                if (vm.Form.Panel.Common.ItemLevel.Min.Length > 0)
                {
                    int minVal = int.Parse(vm.Form.Panel.Common.ItemLevel.Min, CultureInfo.InvariantCulture);
                    if (minVal >= 84)
                    {
                        vm.Form.Panel.Common.ItemLevel.Min = "84";
                    }
                    else if (minVal >= 78)
                    {
                        vm.Form.Panel.Common.ItemLevel.Min = "78";
                    }
                }
            }
        }

        if (itemIs.Metamorph || (itemIs.Flask || itemIs.Tincture) && !itemIs.Unique)
        {
            vm.Form.Panel.Common.ItemLevel.Selected = true;
        }
        
        if (itemIs.Logbook)
        {
            vm.Form.Panel.Common.ItemLevel.Selected = true;
            vm.Form.Visible.Influences = false;
        }

        if (itemIs.ConqMap)
        {
            vm.Form.Visible.ByBase = true;
        }
        
        if (itemIs.Chronicle || itemIs.Ultimatum || itemIs.MirroredTablet || itemIs.SanctumResearch) 
        {
            vm.Form.Visible.Corrupted = false;
            vm.Form.Visible.Rarity = false;
            vm.Form.Visible.ByBase = false;
            vm.Form.Visible.Quality = false;
            vm.Form.Panel.Common.ItemLevelLabel = Resources.Resources.General067_AreaLevel;

            vm.Form.Panel.Common.ItemLevel.Min = lOptions[Resources.Resources.General067_AreaLevel].Replace(" ", string.Empty);

            if (itemIs.SanctumResearch)
            {
                bool isTome = DataManager.Bases.FirstOrDefault(x => x.NameEn is "Forbidden Tome").Name == itemType;
                if (!isTome)
                {
                    vm.Form.Visible.SanctumFields = true;
                }
            }
            if (itemIs.Chronicle || itemIs.MirroredTablet)
            {
                vm.Form.Panel.Common.ItemLevel.Selected = true;
            }
            if (itemIs.Ultimatum) // to update with 'Engraved Ultimatum'
            {
                bool cur = false, div = false;
                string seekCurrency = string.Empty;
                vm.Form.Visible.Reward = true;

                int idxCur = lOptions[Resources.Resources.General070_ReqSacrifice].IndexOf(" x", StringComparison.Ordinal);
                if (idxCur > -1)
                {
                    seekCurrency = lOptions[Resources.Resources.General070_ReqSacrifice][..idxCur]; // .Substring(0, idxCur)
                    lOptions[Resources.Resources.General070_ReqSacrifice] = seekCurrency;
                    if (seekCurrency.Length > 0)
                    {
                        var isCur =
                            from result in DataManager.Currencies
                            from Entrie in result.Entries
                            where result.Id == Strings.CurrencyType.Currency && Entrie.Text == seekCurrency
                            select true;
                        if (isCur.Any())
                        {
                            if (isCur.First())
                            {
                                cur = true;
                            }
                        }
                        if (!cur)
                        {
                            var isDiv =
                            from result in DataManager.Currencies
                            from Entrie in result.Entries
                            where result.Id == Strings.CurrencyType.Cards && Entrie.Text == seekCurrency
                            select true;
                            if (isDiv.Any())
                            {
                                if (isDiv.First())
                                {
                                    div = true;
                                }
                            }
                        }
                    }
                }
                bool condMirrored = lOptions[Resources.Resources.General071_Reward] == Resources.Resources.General072_RewardMirrored;
                vm.Form.Panel.Reward.Text = cur || div ? seekCurrency : lOptions[Resources.Resources.General071_Reward];
                vm.Form.Panel.Reward.FgColor = cur ? string.Empty : div ? Strings.Color.DeepSkyBlue : condMirrored ? Strings.Color.Gold : Strings.Color.Peru;
                vm.Form.Panel.Reward.Tip = cur ? Strings.Reward.DoubleCurrency : div ? Strings.Reward.DoubleDivCards : condMirrored ? Strings.Reward.MirrorRare : Strings.Reward.ExchangeUnique;
            }
            if (itemIs.SanctumResearch)
            {
                vm.Form.Visible.Influences = false;
                vm.Form.Visible.Conditions = false;
                vm.Form.Panel.Common.ItemLevel.Selected = true;
            }
        }

        if (itemIs.Corpses)
        {
            vm.Form.Panel.Common.ItemLevel.Selected = true;
        }

        if (vm.Form.Panel.Common.ItemLevelLabel.Length == 0)
        {
            vm.Form.Panel.Common.ItemLevelLabel = Resources.Resources.Main065_tbiLevel;
        }

        int nbRows = 1;
        if (vm.Form.Visible.Defense || vm.Form.Visible.SanctumFields || vm.Form.Visible.MapStats)
        {
            nbRows++;
            vm.Form.Panel.Row.ArmourMaxHeight = 43;
        }
        if (vm.Form.Visible.Damage || vm.Form.Visible.MapStats)
        {
            nbRows++;
            vm.Form.Panel.Row.WeaponMaxHeight = 43;
        }
        if (vm.Form.Visible.Total)
        {
            nbRows++;
            vm.Form.Panel.Row.TotalMaxHeight = 43;
        }

        if (nbRows <= 2)
        {
            vm.Form.Panel.Col.FirstMaxWidth = 0;
            vm.Form.Panel.Col.LastMinWidth = 100;
            if (nbRows <= 1)
            {
                vm.Form.Panel.UseBorderThickness = false;
            }
        }

        vm.Form.Visible.Detail = itemIs.ShowDetail;
        vm.Form.Visible.HeaderMod = !itemIs.ShowDetail;
        vm.Form.Visible.HiddablePanel = vm.Form.Visible.AlternateGem || vm.Form.Visible.SynthesisBlight || vm.Form.Visible.BlightRavaged || vm.Form.Visible.Scourged;
        vm.Form.Rarity.Index = vm.Form.Rarity.ComboBox.IndexOf(vm.Form.Rarity.Item);

        if (vm.Form.Bulk.AutoSelect)
        {
            vm.Logic.SelectViewModelExchangeCurrency(vm.Form.Bulk.Args, vm.Form.Bulk.Currency, vm.Form.Bulk.Tier); // Select currency in 'Pay' section
        }

        return item;
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
            GemResultData tmpGem = DataManager.Gems.FirstOrDefault(x => x.NameEn == type);
            if (tmpGem is not null)
            {
                type = tmpGem.Name;
            }
        }

        /*
        var tmp = DataManager.Monsters.FirstOrDefault(x => x.NameEn.Contains(type, StringComparison.Ordinal));
        if (tmp is not null)
        {
            name = string.Empty;
            type = tmp.Name.Replace("\"", string.Empty);
        }
        */

        return new Tuple<string, string>(name, type);
    }

    private static double DamageToDPS(string damage)
    {
        double dps = 0;
        try
        {
            //string[] stmps = Regex.Replace(damage, @"\([a-zA-Z]+\)", string.Empty).Split(',');
            string[] stmps = RegexUtil.LetterPattern().Replace(damage, string.Empty).Split(',');
            for (int t = 0; t < stmps.Length; t++)
            {
                string[] maidps = (stmps[t] ?? string.Empty).Trim().Split('-');
                if (maidps.Length == 2)
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

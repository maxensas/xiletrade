using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Command;

public sealed class MainCommand
{
    private static MainViewModel Vm { get; set; }
    private static IServiceProvider _serviceProvider;

    private readonly DelegateCommand openSearch;
    private readonly DelegateCommand openWiki;
    private readonly DelegateCommand openPoeDb;
    private readonly DelegateCommand openNinja;
    private readonly DelegateCommand openDonateUrl;
    private readonly DelegateCommand searchPoeprices;
    private readonly DelegateCommand refreshSearch;
    private readonly DelegateCommand fetch;
    private readonly DelegateCommand invertBulk;
    private readonly DelegateCommand setModCurrent;
    private readonly DelegateCommand setModTier;
    private readonly DelegateCommand checkCondition;
    private readonly DelegateCommand checkInfluence;
    private readonly DelegateCommand change;
    private readonly DelegateCommand resetBulkImage;
    private readonly DelegateCommand selectBulk;
    private readonly DelegateCommand searchCurrency;
    private readonly DelegateCommand addShopList;
    private readonly DelegateCommand resetShopLists;
    private readonly DelegateCommand invertShopLists;
    private readonly DelegateCommand clearFocus;
    private readonly DelegateCommand switchTab;
    private readonly DelegateCommand wheelIncrement;
    private readonly DelegateCommand wheelIncrementTenth;
    private readonly DelegateCommand wheelIncrementHundredth;
    private readonly DelegateCommand wheelDecrement;
    private readonly DelegateCommand wheelDecrementTenth;
    private readonly DelegateCommand wheelDecrementHundredth;
    private readonly DelegateCommand selectMod;
    private readonly DelegateCommand autoClose;
    private readonly DelegateCommand updateOpacity;
    private readonly DelegateCommand expanderExpand;
    private readonly DelegateCommand expanderCollapse;
    private readonly DelegateCommand checkAllMods;
    private readonly DelegateCommand selectBulkIndex;
    private readonly DelegateCommand showBulkWhisper;
    private readonly DelegateCommand selectShopIndex;
    private readonly DelegateCommand showShopWhisper;
    private readonly DelegateCommand removeGetList;
    private readonly DelegateCommand removePayList;
    private readonly DelegateCommand updateMinimized;
    private readonly DelegateCommand windowLoaded;
    private readonly DelegateCommand windowClosed;
    private readonly DelegateCommand windowDeactivated;

    public ICommand OpenSearch => openSearch;
    public ICommand OpenWiki => openWiki;
    public ICommand OpenPoeDb => openPoeDb;
    public ICommand OpenNinja => openNinja;
    public ICommand OpenDonateUrl => openDonateUrl;
    public ICommand SearchPoeprices => searchPoeprices;
    public ICommand RefreshSearch => refreshSearch;
    public ICommand Fetch => fetch;
    public ICommand InvertBulk => invertBulk;
    public ICommand SetModCurrent => setModCurrent;
    public ICommand SetModTier => setModTier;
    public ICommand CheckCondition => checkCondition;
    public ICommand CheckInfluence => checkInfluence;
    public ICommand Change => change;
    public ICommand ResetBulkImage => resetBulkImage;
    public ICommand SelectBulk => selectBulk;
    public ICommand SearchCurrency => searchCurrency;
    public ICommand AddShopList => addShopList;
    public ICommand ResetShopLists => resetShopLists;
    public ICommand InvertShopLists => invertShopLists;
    public ICommand ClearFocus => clearFocus;
    public ICommand SwitchTab => switchTab;
    public ICommand WheelIncrement => wheelIncrement;
    public ICommand WheelIncrementTenth => wheelIncrementTenth;
    public ICommand WheelIncrementHundredth => wheelIncrementHundredth;
    public ICommand WheelDecrement => wheelDecrement;
    public ICommand WheelDecrementTenth => wheelDecrementTenth;
    public ICommand WheelDecrementHundredth => wheelDecrementHundredth;
    public ICommand SelectMod => selectMod;
    public ICommand AutoClose => autoClose;
    public ICommand UpdateOpacity => updateOpacity;
    public ICommand ExpanderExpand => expanderExpand;
    public ICommand ExpanderCollapse => expanderCollapse;
    public ICommand CheckAllMods => checkAllMods;
    public ICommand SelectBulkIndex => selectBulkIndex;
    public ICommand ShowBulkWhisper => showBulkWhisper;
    public ICommand SelectShopIndex => selectShopIndex;
    public ICommand ShowShopWhisper => showShopWhisper;
    public ICommand RemoveGetList => removeGetList;
    public ICommand RemovePayList => removePayList;
    public ICommand UpdateMinimized => updateMinimized;
    public ICommand WindowLoaded => windowLoaded;
    public ICommand WindowClosed => windowClosed;
    public ICommand WindowDeactivated => windowDeactivated;

    public MainCommand(MainViewModel vm, IServiceProvider serviceProvider)
    {
        Vm = vm;
        _serviceProvider = serviceProvider;
        openSearch = new(OnOpenSearch, CanOpenSearch);
        openWiki = new(OnOpenWiki, CanOpenWiki);
        openPoeDb = new(OnOpenPoeDb, CanOpenPoeDb);
        openNinja = new(OnOpenNinja, CanOpenNinja);
        openDonateUrl = new(OnOpenDonateUrl, CanOpenDonateUrl);
        searchPoeprices = new(OnSearchPoeprices, CanSearchPoeprices);
        refreshSearch = new(OnRefreshSearch, CanRefreshSearch);
        fetch = new(OnFetch, CanFetch);
        invertBulk = new(OnInvertBulk, CanInvertBulk);
        setModCurrent = new(OnSetModCurrent, CanSetModCurrent);
        setModTier = new(OnSetModTier, CanSetModTier);
        checkCondition = new(OnCheckCondition, CanCheckCondition);
        checkInfluence = new(OnCheckInfluence, CanCheckInfluence);
        change = new(OnChange, CanChange);
        resetBulkImage = new(OnResetBulkImage, CanResetBulkImage);
        selectBulk = new(OnSelectBulk, CanSelectBulk);
        searchCurrency = new(OnSearchCurrency, CanSearchCurrency);
        addShopList = new(OnAddShopList, CanAddShopList);
        resetShopLists = new(OnResetShopLists, CanResetShopLists);
        invertShopLists = new(OnInvertShopLists, CanInvertShopLists);
        clearFocus = new(OnClearFocus, CanClearFocus);
        switchTab = new(OnSwitchTab, CanSwitchTab);
        wheelIncrement = new(OnWheelIncrement, CanWheelAdjust);
        wheelIncrementTenth = new(OnWheelIncrementTenth, CanWheelAdjust);
        wheelIncrementHundredth = new(OnWheelIncrementHundredth, CanWheelAdjust);
        wheelDecrement = new(OnWheelDecrement, CanWheelAdjust);
        wheelDecrementTenth = new(OnWheelDecrementTenth, CanWheelAdjust);
        wheelDecrementHundredth = new(OnWheelDecrementHundredth, CanWheelAdjust);
        selectMod = new(OnSelectMod, CanSelectMod);
        autoClose = new(OnAutoClose, CanAutoClose);
        updateOpacity = new(OnUpdateOpacity, CanUpdateOpacity);
        expanderExpand = new(OnExpanderExpand, CanExpanderExpand);
        expanderCollapse = new(OnExpanderCollapse, CanExpanderCollapse);
        checkAllMods = new(OnCheckAllMods, CanCheckAllMods);
        selectBulkIndex = new(OnSelectBulkIndex, CanSelectBulkIndex);
        showBulkWhisper = new(OnShowBulkWhisper, CanShowBulkWhisper);
        selectShopIndex = new(OnSelectShopIndex, CanSelectShopIndex);
        showShopWhisper = new(OnShowShopWhisper, CanShowShopWhisper);
        removeGetList = new(OnRemoveGetList, CanRemoveGetList);
        removePayList = new(OnRemovePayList, CanRemovePayList);
        updateMinimized = new(OnUpdateMinimized, CanUpdateMinimized);
        windowLoaded = new(OnWindowLoaded, CanWindowLoaded);
        windowClosed = new(OnWindowClosed, CanWindowClosed);
        windowDeactivated = new(OnWindowDeactivated, CanWindowDeactivated);
    }

    private static bool CanOpenSearch(object commandParameter)
    {
        return true;
    }

    private static void OnOpenSearch(object commandParameter)
    {
        //CloseFading();
        string sEntity;
        string[] exchange = new string[2];
        string market = Vm.Form.Market[Vm.Form.MarketIndex];
        //string league = Restr.ServerType ;
        string league = Vm.Form.League[Vm.Form.LeagueIndex];

        if (Vm.Form.Tab.BulkSelected)
        {
            if (Vm.Form.Bulk.Pay.CurrencyIndex > 0 || Vm.Form.Bulk.Get.CurrencyIndex > 0)
            {
                if (Vm.Form.Bulk.Pay.CurrencyIndex > 0)
                {
                    BaseResultData tmpBase = DataManager.Bases.FirstOrDefault(y => y.Name == Vm.Form.Bulk.Pay.Currency[Vm.Form.Bulk.Pay.CurrencyIndex]);
                    if (tmpBase is null)
                    {
                        exchange[0] = Vm.Logic.GetExchangeCurrencyTag(Exchange.Pay);
                    }
                }
                if (Vm.Form.Bulk.Get.CurrencyIndex > 0)
                {
                    BaseResultData tmpBase = DataManager.Bases.FirstOrDefault(y => y.Name == Vm.Form.Bulk.Get.Currency[Vm.Form.Bulk.Get.CurrencyIndex]);
                    if (tmpBase is null)
                    {
                        exchange[1] = Vm.Logic.GetExchangeCurrencyTag(Exchange.Get);
                    }
                }
                if (exchange[0] is null && exchange[1] is null)
                {
                    return;
                }

                bool isInteger = int.TryParse(Vm.Form.Bulk.Stock, out int minimumStock);
                if (!isInteger)
                {
                    minimumStock = 1;
                    Vm.Form.Bulk.Stock = "1";
                }
                // TO update with data models
                StringBuilder urlBuilder = new("{\"exchange\":{\"status\":{\"option\":\"");
                urlBuilder.Append(market);

                if (exchange[0] is not null)
                {
                    urlBuilder.Append("\"},\"have\":[\"").Append(exchange[0]);
                }
                if (exchange[1] is not null)
                {
                    urlBuilder.Append("\"},\"want\":[\"").Append(exchange[1]);
                }
                urlBuilder.Append("\"],\"minimum\":").Append(minimumStock).Append("}}");
                string url = Strings.ExchangeUrl[DataManager.Config.Options.Language] + league + "/?q=" + Uri.EscapeDataString(urlBuilder.ToString());
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Failed to open PoE search window.", MessageStatus.Error);
                }
            }
            return;
        }

        if (Vm.Form.Tab.ShopSelected)
        {
            //todo
            var curGetList = from list in Vm.Form.Shop.GetList select list.ToolTip;
            var curPayList = from list in Vm.Form.Shop.PayList select list.ToolTip;
            if (curGetList.Any() && curPayList.Any())
            {
                bool isInteger = int.TryParse(Vm.Form.Shop.Stock, out int minimumStock);
                if (!isInteger)
                {
                    minimumStock = 1;
                    Vm.Form.Shop.Stock = "1";
                }

                StringBuilder sbPay = new(), sbGet = new();
                bool appended = false;
                foreach (var str in curPayList.ToList())
                {
                    if (str.Length > 0)
                    {
                        if (appended)
                        {
                            sbPay.Append(',');
                        }
                        sbPay.Append('"').Append(str).Append('"');
                        appended = true;
                    }
                }

                appended = false;
                foreach (var str in curGetList.ToList())
                {
                    if (str.Length > 0)
                    {
                        if (appended)
                        {
                            sbGet.Append(',');
                        }
                        sbGet.Append('"').Append(str).Append('"');
                        appended = true;
                    }
                }
                StringBuilder urlBuilder = new("{\"exchange\":{\"status\":{\"option\":\"");
                urlBuilder.Append(market).Append("\"},\"have\":[").Append(sbPay.ToString()).Append("],\"want\":[").Append(sbGet.ToString())
                    .Append("],\"minimum\":").Append(minimumStock).Append(",\"collapse\": true},\"engine\":\"new\"}");
                string url = Strings.ExchangeUrl[DataManager.Config.Options.Language] + league + "/?q=" + Uri.EscapeDataString(urlBuilder.ToString());

                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Failed to open PoE search window.", MessageStatus.Error);
                }
            }
            return;
        }
        if (Vm.Form.Tab.QuickSelected || Vm.Form.Tab.DetailSelected)
        {
            sEntity = Json.GetSerialized(Vm.Logic.GetItemFromViewModel(), Vm.Logic.CurrentItem, false, market);

            if (sEntity?.Length > 0)
            {
                _ = Vm.Logic.Task.OpenSearch(sEntity, league); //async
            }
        }
        //Hide();
    }

    private static bool CanSearchPoeprices(object commandParameter)
    {
        return true;
    }

    private static void OnSearchPoeprices(object commandParameter)
    {
        Vm.Logic.Task.UpdatePoePricesTab();
    }

    private static bool CanOpenNinja(object commandParameter)
    {
        return true;
    }

    private static void OnOpenNinja(object commandParameter)
    {
        Vm.Logic.Task.OpenNinja();
    }

    private static bool CanOpenWiki(object commandParameter)
    {
        return true;
    }

    private static void OnOpenWiki(object commandParameter)
    {
        Vm.Logic.Task.OpenWiki();
    }

    private static bool CanOpenPoeDb(object commandParameter)
    {
        return true;
    }

    private static void OnOpenPoeDb(object commandParameter)
    {
        Vm.Logic.Task.OpenPoeDb();
    }

    private static bool CanOpenDonateUrl(object commandParameter)
    {
        return true;
    }
    
    private static void OnOpenDonateUrl(object commandParameter)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = Strings.UrlPaypalDonate, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(Resources.Resources.Main126_PaypalFail, "Redirection to paypal failed ", MessageStatus.Warning);
        }
    }

    private static bool CanRefreshSearch(object commandParameter)
    {
        return true;
    }
    
    private static void OnRefreshSearch(object commandParameter)
    {
        try
        {
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
            if (Vm.Form.Tab.QuickSelected || Vm.Form.Tab.DetailSelected)
            {
                Vm.Logic.Task.UpdateItemPrices(Vm.Logic.GetItemFromViewModel(), 1);
                return;
            }
            if (Vm.Form.Tab.BulkSelected)
            {
                if (Vm.Form.Bulk.Pay.CurrencyIndex > 0 && Vm.Form.Bulk.Get.CurrencyIndex > 0)
                {
                    if (!int.TryParse(Vm.Form.Bulk.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        Vm.Form.Bulk.Stock = "1";
                    }
                    //getMoreBulk.IsEnabled = true;
                    Vm.Form.Bulk.Get.ImageLast = Vm.Form.Bulk.Get.Image;
                    Vm.Form.Bulk.Pay.ImageLast = Vm.Form.Bulk.Pay.Image;
                    Vm.Form.Visible.BulkLastSearch = true;

                    Vm.Logic.Task.UpdateItemPrices(null, minimumStock);
                    Vm.Logic.Task.UpdateNinjaChaosEq();
                    return;
                }

                Vm.Result.Bulk.Price = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                Vm.Result.Bulk.PriceBis = string.Empty;
                return;
            }
            if (Vm.Form.Tab.ShopSelected)
            {
                if (Vm.Form.Shop.GetList.Count > 0 && Vm.Form.Shop.PayList.Count > 0)
                {
                    if (!int.TryParse(Vm.Form.Shop.Stock, out int minimumStock))
                    {
                        minimumStock = 1;
                        Vm.Form.Shop.Stock = "1";
                    }
                    Vm.Logic.Task.UpdateItemPrices(null, minimumStock);
                    return;
                }

                Vm.Result.Shop.Price = Resources.Resources.Main001_PriceSelect; // "Select currencies :\nGET and PAY"
                Vm.Result.Shop.PriceBis = string.Empty;
            }
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Refreshing search error", MessageStatus.Error);
        }
    }

    private static bool CanFetch(object commandParameter)
    {
        return true;
    }

    private static void OnFetch(object commandParameter)
    {
        Vm.Form.FetchDetailIsEnabled = false;
        _ = Vm.Logic.Task.FetchDetailResults();
    }

    private static bool CanInvertBulk(object commandParameter)
    {
        return true;
    }

    private static void OnInvertBulk(object commandParameter)
    {
        int idxCategory = Vm.Form.Bulk.Get.CategoryIndex;
        int idxCurrency = Vm.Form.Bulk.Get.CurrencyIndex;
        int idxTier = Vm.Form.Bulk.Get.TierIndex;

        Vm.Form.Bulk.Get.CategoryIndex = Vm.Form.Bulk.Pay.CategoryIndex;
        if (Vm.Form.Bulk.Get.CategoryIndex > 0)
        {
            if (Vm.Form.Bulk.Pay.TierIndex >= 0)
            {
                Vm.Form.Bulk.Get.TierIndex = Vm.Form.Bulk.Pay.TierIndex;
            }
            Vm.Form.Bulk.Get.CurrencyIndex = Vm.Form.Bulk.Pay.CurrencyIndex;
        }

        Vm.Form.Bulk.Pay.CategoryIndex = idxCategory;
        if (idxCategory > 0)
        {
            if (idxTier >= 0)
            {
                Vm.Form.Bulk.Pay.TierIndex = idxTier;
            }
            Vm.Form.Bulk.Pay.CurrencyIndex = idxCurrency;
        }
    }

    private static bool CanSetModCurrent(object commandParameter)
    {
        return true;
    }

    private static void OnSetModCurrent(object commandParameter)
    {
        SetModCur();
    }

    public static void SetModCur()
    {
        List<bool> sameText = new();
        bool remove = true;

        if (Vm.Form.ModLine.Count > 0)
        {
            foreach (ModLineViewModel mod in Vm.Form.ModLine)
            {
                sameText.Add(mod.Min == mod.Current);
                mod.Min = mod.Current;
            }
            foreach (bool same in sameText) remove &= same;
            if (remove)
            {
                foreach (ModLineViewModel mod in Vm.Form.ModLine)
                {
                    if (mod.Min.Length > 0)
                    {
                        mod.Min = string.Empty;
                    }
                }
            }
        }
    }

    private static bool CanSetModTier(object commandParameter)
    {
        return true;
    }

    private static void OnSetModTier(object commandParameter)
    {
        if (Vm.Form.ModLine.Count <= 0)
        {
            return;
        }
        foreach (ModLineViewModel mod in Vm.Form.ModLine)
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

    private static bool CanCheckCondition(object commandParameter)
    {
        return true;
    }

    private static void OnCheckCondition(object commandParameter)
    {
        //var myTb = (TextBox)cbConditions.Template.FindName("PART_EditableTextBox", cbConditions);
        if (!Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.FreeSuffix && !Vm.Form.Condition.SocketColors)
        {
            Vm.Form.CheckComboCondition.Text = Resources.Resources.Main036_None;
            Vm.Form.CheckComboCondition.ToolTip = null;
            return;
        }

        bool prefixOnly = Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.FreeSuffix && !Vm.Form.Condition.SocketColors;
        bool suffixOnly = Vm.Form.Condition.FreeSuffix && !Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.SocketColors;
        bool colorsOnly = Vm.Form.Condition.SocketColors && !Vm.Form.Condition.FreePrefix && !Vm.Form.Condition.FreeSuffix;
        if (prefixOnly)
        {
            Vm.Form.CheckComboCondition.Text = Vm.Form.Condition.FreePrefixText;
            Vm.Form.CheckComboCondition.ToolTip = Vm.Form.Condition.FreePrefixToolTip;
            return;
        }
        if (suffixOnly)
        {
            Vm.Form.CheckComboCondition.Text = Vm.Form.Condition.FreeSuffixText;
            Vm.Form.CheckComboCondition.ToolTip = Vm.Form.Condition.FreeSuffixToolTip;
            return;
        }
        if (colorsOnly)
        {
            Vm.Form.CheckComboCondition.Text = Vm.Form.Condition.SocketColorsText;
            Vm.Form.CheckComboCondition.ToolTip = Vm.Form.Condition.SocketColorsToolTip;
            return;
        }

        List<KeyValuePair<bool, string>> condList = new();
        condList.Add(new KeyValuePair<bool, string>(Vm.Form.Condition.FreePrefix, Vm.Form.Condition.FreePrefixToolTip));
        condList.Add(new KeyValuePair<bool, string>(Vm.Form.Condition.FreeSuffix, Vm.Form.Condition.FreeSuffixToolTip));
        condList.Add(new KeyValuePair<bool, string>(Vm.Form.Condition.SocketColors, Vm.Form.Condition.SocketColorsToolTip));

        int nbCond = 0;
        StringBuilder toolTip = new();
        foreach (var cond in condList)
        {
            if (cond.Key)
            {
                nbCond++;
                if (toolTip.Length > 0)
                {
                    toolTip.AppendLine(); // "\n"
                }
                toolTip.Append(cond.Value);
            }
        }

        if (nbCond > 0)
        {
            Vm.Form.CheckComboCondition.Text = nbCond.ToString();
            Vm.Form.CheckComboCondition.ToolTip = toolTip.ToString();
        }
    }

    private static bool CanCheckInfluence(object commandParameter)
    {
        return true;
    }

    private static void OnCheckInfluence(object commandParameter)
    {
        string textVal = string.Empty;
        int checks = 0;
        foreach (KeyValuePair<string, bool> inf in Vm.Logic.GetInfluenceSate())
        {
            if (inf.Value)
            {
                checks++;
                if (textVal.Length > 0) textVal += " & ";
                textVal += inf.Key;
            }
        }

        if (textVal.Length > 0)
        {
            Vm.Form.CheckComboInfluence.Text = checks == 1 ? textVal : checks.ToString();
            Vm.Form.CheckComboInfluence.ToolTip = textVal;
            return;
        }
        Vm.Form.CheckComboInfluence.Text = Resources.Resources.Main036_None;
        Vm.Form.CheckComboInfluence.ToolTip = null;
    }

    private static bool CanChange(object commandParameter)
    {
        return true;
    }

    private static void OnChange(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            ExchangeViewModel exVm = @string.StartsWith("get", StringComparison.Ordinal) ? Vm.Form.Bulk.Get :
                @string.StartsWith("pay", StringComparison.Ordinal) ? Vm.Form.Bulk.Pay :
                @string.StartsWith("shop", StringComparison.Ordinal) ? Vm.Form.Shop.Exchange : null;
            if (exVm is null)
            {
                return;
            }

            if (exVm.CategoryIndex > 0 && exVm.CurrencyIndex > 0)
            {
                string tier = null;
                if (exVm.TierIndex > 0)
                {
                    tier = exVm.Tier[exVm.TierIndex].ToLowerInvariant().Replace("t", string.Empty);
                }
                exVm.Image = Common.GetCurrencyImageUri(exVm.Currency[exVm.CurrencyIndex], tier);
            }
            if (exVm.CurrencyIndex == 0)
            {
                exVm.Image = null;
            }
        }
    }

    private static bool CanResetBulkImage(object commandParameter)
    {
        return true;
    }

    // TODO convert foreach loops to LINQ queries
    private static void OnResetBulkImage(object commandParameter)
    {
        _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        if (commandParameter is string @string)
        {
            ExchangeViewModel exVm = @string.StartsWith("get", StringComparison.Ordinal) ? Vm.Form.Bulk.Get :
                @string.StartsWith("pay", StringComparison.Ordinal) ? Vm.Form.Bulk.Pay :
                @string.StartsWith("shop", StringComparison.Ordinal) ? Vm.Form.Shop.Exchange : null;
            if (exVm is null)
            {
                return;
            }

            if (@string.EndsWith("chaos", StringComparison.Ordinal))
            {
                exVm.CategoryIndex = 1;
                bool breakAll = false;
                foreach (CurrencyResultData resultDat in DataManager.Currencies)
                {
                    foreach (CurrencyEntrie entrieDat in resultDat.Entries)
                    {
                        if (entrieDat.ID is "chaos")
                        {
                            int idx = exVm.Currency.IndexOf(entrieDat.Text);
                            if (idx >= 0)
                            {
                                exVm.CurrencyIndex = idx;
                            }
                            breakAll = true;
                            break;
                        }
                    }
                    if (breakAll) break;
                }
            }
            if (@string.EndsWith("exalt", StringComparison.Ordinal))
            {
                exVm.CategoryIndex = 1;
                bool breakAll = false;
                foreach (CurrencyResultData resultDat in DataManager.Currencies)
                {
                    foreach (CurrencyEntrie entrieDat in resultDat.Entries)
                    {
                        if (entrieDat.ID is "exalted")
                        {
                            int idx = exVm.Currency.IndexOf(entrieDat.Text);
                            if (idx >= 0)
                            {
                                exVm.CurrencyIndex = idx;
                            }
                            breakAll = true;
                            break;
                        }
                    }
                    if (breakAll) break;
                }
            }
            if (@string.EndsWith("divine", StringComparison.Ordinal))
            {
                exVm.CategoryIndex = 1;
                bool breakAll = false;
                foreach (CurrencyResultData resultDat in DataManager.Currencies)
                {
                    foreach (CurrencyEntrie entrieDat in resultDat.Entries)
                    {
                        if (entrieDat.ID is "divine")
                        {
                            int idx = exVm.Currency.IndexOf(entrieDat.Text);
                            if (idx >= 0)
                            {
                                exVm.CurrencyIndex = idx;
                            }
                            breakAll = true;
                            break;
                        }
                    }
                    if (breakAll) break;
                }
            }
            if (@string.EndsWith("nothing", StringComparison.Ordinal))
            {
                exVm.CategoryIndex = 0;
                exVm.CurrencyIndex = 0;
                exVm.Search = string.Empty;
            }
        }
    }

    private static bool CanSelectBulk(object commandParameter)
    {
        return true;
    }

    // TODO prefer LINQ over foreach loops & remove else instructions as much as possible
    private static void OnSelectBulk(object commandParameter)
    {
        int idLang = DataManager.Config.Options.Language;
        ExchangeViewModel exchange = null;
        if (commandParameter is not string @string)
        {
            return;
        }
        bool isGet = @string.Contains("get", StringComparison.Ordinal);
        bool isPay = @string.Contains("pay", StringComparison.Ordinal);
        bool isShop = @string.Contains("shop", StringComparison.Ordinal);
        bool isTier = @string.Contains("tier", StringComparison.Ordinal);

        if (isGet)
        {
            exchange = Vm.Form.Bulk.Get;
            Vm.Form.Bulk.Get.Image = null;
        }
        else if (isPay)
        {
            exchange = Vm.Form.Bulk.Pay;
            Vm.Form.Bulk.Pay.Image = null;
        }
        else if (isShop)
        {
            exchange = Vm.Form.Shop.Exchange;
            Vm.Form.Shop.Exchange.Image = null;
        }
        else
        {
            return;
        }

        if (exchange.CategoryIndex > 0)
        {
            if (!isTier)
            {
                bool isMap = exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main056_Maps
                        || exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main179_UniqueMaps
                        || exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main217_BlightedMaps;
                bool isDiv = exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main055_Divination;
                //bool isHeist = exchange.Category[exchange.CategoryIndex] == Resources.Resources.Main218_Heist;
                if (isDiv || isMap)
                {
                    if (isDiv)
                    {
                        exchange.Tier = new() { "T1", "T2", "T3", "T4" };
                        exchange.TierIndex = 0;
                    }
                    else if (isMap)
                    {
                        exchange.Tier = new()
                        {
                            "T1","T2","T3","T4","T5",
                            "T6","T7","T8","T9","T10",
                            "T11","T12","T13","T14","T15","T16","T17"
                        };
                        exchange.TierIndex = 0;
                    }
                    exchange.TierVisible = true;
                }
                else
                {
                    exchange.Tier.Clear();
                    exchange.TierVisible = false;
                }
            }

            exchange.Currency.Clear();
            AsyncObservableCollection<string> listBulk = new();
            listBulk.Add("------------------------------------------------");
            exchange.CurrencyIndex = 0;

            string selValue = exchange.Category[exchange.CategoryIndex];

            string searchKind = (selValue == Resources.Resources.Main044_MainCur
                || selValue == Resources.Resources.Main207_ExoticCurrency
                || selValue == Resources.Resources.Main045_OtherCur) ? Strings.CurrencyType.Currency :
                //selValue == Resources.Resources.Main207_ExoticCurrency ? Strings.CurrencyType.Exotic :
                //selValue == Resources.Resources.Main149_Shards ? Strings.CurrencyType.Splinters :
                (selValue == Resources.Resources.Main046_MapFrag
                || selValue == Resources.Resources.Main047_Stones
                || selValue == Resources.Resources.Main052_Scarabs) ? Strings.CurrencyType.Fragments :
                //selValue == Resources.Resources.Main197_EldritchCurrency ? Strings.CurrencyType.EldritchCurrency :
                selValue == Resources.Resources.Main208_MemoryLine ? Strings.CurrencyType.MemoryLine :
                selValue == Resources.Resources.Main186_Expedition ? Strings.CurrencyType.Expedition :
                selValue == Resources.Resources.Main048_Delirium ? Strings.CurrencyType.DeliriumOrbs :
                selValue == Resources.Resources.Main049_Catalysts ? Strings.CurrencyType.Catalysts :
                selValue == Resources.Resources.Main050_Oils ? Strings.CurrencyType.Oils :
                selValue == Resources.Resources.Main051_Incubators ? Strings.CurrencyType.Incubators :
                //selValue == Resources.Resources.Main052_Scarabs ? Strings.CurrencyType.Scarabs :
                selValue == Resources.Resources.Main053_Fossils ? Strings.Delve : //StringsTable.CurrencyType.DelveFossils
                selValue == Resources.Resources.Main054_Essences ? Strings.CurrencyType.Essences :
                selValue == Resources.Resources.Main211_AncestorCurrency ? Strings.CurrencyType.Ancestor :
                selValue == Resources.Resources.Main212_Sanctum ? Strings.CurrencyType.Sanctum :
                //selValue == Resources.Resources.Main213_Crucible ? Strings.CurrencyType.Crucible :
                selValue == Resources.Resources.Main198_ScoutingReports ? Strings.CurrencyType.ScoutingReport :
                selValue == Resources.Resources.Main055_Divination ? Strings.CurrencyType.Cards :
                //selValue == Resources.Resources.Main196_TaintedCurrency ? Strings.CurrencyType.TaintedCurrency :
                selValue == Resources.Resources.Main200_SentinelCurrency ? Strings.CurrencyType.Sentinel :
                selValue == Resources.Resources.Main056_Maps ? Strings.CurrencyType.Maps :
                selValue == Resources.Resources.Main179_UniqueMaps ? Strings.CurrencyType.MapsUnique :
                selValue == Resources.Resources.Main216_BossMaps ? Strings.CurrencyType.MapsSpecial :
                selValue == Resources.Resources.Main217_BlightedMaps ? Strings.CurrencyType.MapsBlighted :
                selValue == Resources.Resources.Main218_Heist ? Strings.CurrencyType.Heist :
                selValue == Resources.Resources.Main219_Beasts ? Strings.CurrencyType.Beasts :
                selValue == Resources.Resources.ItemClass_allflame ? Strings.CurrencyType.Embers :
                selValue == Resources.Resources.General127_FilledCoffin ? Strings.CurrencyType.Coffins :
                string.Empty;

            if (searchKind.Length > 0)
            {
                IEnumerable<CurrencyResultData> tmpCurr = searchKind is Strings.Delve ?
                    DataManager.Currencies.Where(x => x.ID.Contains(searchKind, StringComparison.Ordinal))
                    : DataManager.Currencies.Where(x => x.ID.Equals(searchKind, StringComparison.Ordinal));

                foreach (CurrencyResultData resultDat in tmpCurr)
                {
                    foreach (CurrencyEntrie entrieDat in resultDat.Entries)
                    {
                        if (entrieDat.Text.Length == 0 || entrieDat.ID is Strings.sep)
                        {
                            continue;
                        }
                        
                        bool addItem = false;

                        if (searchKind is Strings.CurrencyType.Maps or Strings.CurrencyType.MapsUnique or Strings.CurrencyType.MapsBlighted)
                        {
                            if (exchange.TierIndex >= 0)
                            {
                                string tier = Strings.tierPrefix + exchange.Tier[exchange.TierIndex].Replace("T", string.Empty);
                                
                                addItem = entrieDat.ID.EndsWith(tier,StringComparison.Ordinal);
                                /*
                                if (addItem)
                                {
                                    entrieDat.Text = RegexUtil.BetweenBracketsPattern().Replace(entrieDat.Text, string.Empty).Trim();
                                }
                                */
                            }
                        }
                        else if (searchKind is Strings.CurrencyType.Cards)
                        {
                            DivTiersResult tmpDiv = DataManager.DivTiers.FirstOrDefault(x => x.Tag == entrieDat.ID);
                            if (tmpDiv is not null)
                            {
                                if (exchange.TierIndex >= 0)
                                {
                                    string tierVal = exchange.Tier[exchange.TierIndex].ToLowerInvariant().Replace("t", string.Empty);
                                    addItem = tierVal == tmpDiv.Tier;
                                }
                            }
                            else
                            {
                                if (!exchange.Tier.Contains(Resources.Resources.Main016_TierNothing))
                                {
                                    exchange.Tier.Add(Resources.Resources.Main016_TierNothing);
                                }

                                if (exchange.TierIndex >= 0)
                                {
                                    addItem = exchange.Tier[exchange.TierIndex].ToLowerInvariant() == Resources.Resources.Main016_TierNothing; // to check
                                }
                            }
                        }
                        else if (searchKind is Strings.CurrencyType.Currency)
                        {
                            //bool is_shard = entrieDat.ID.EndsWith(Strings.shard);
                            bool is_mainCur = Strings.dicMainCur.TryGetValue(entrieDat.ID, out string curVal2);
                            bool is_exoticCur = Strings.dicExoticCur.TryGetValue(entrieDat.ID, out string curVal3);
                            addItem = selValue == Resources.Resources.Main044_MainCur ? is_mainCur && !is_exoticCur
                                : selValue == Resources.Resources.Main207_ExoticCurrency ? !is_mainCur && is_exoticCur
                                : selValue == Resources.Resources.Main045_OtherCur ? !is_mainCur && !is_exoticCur
                                : addItem;
                        }
                        /*else if (searchKind is Strings.CurrencyType.Splinters)
                        {
                            addItem = selValue == Resources.Resources.Main149_Shards;
                        }*/
                        else if (searchKind is Strings.CurrencyType.Fragments)
                        {
                            bool is_scarab = entrieDat.ID.Contains(Strings.scarab);
                            //bool is_splinter = entrieDat.ID.StartsWith(Strings.splinter) || entrieDat.ID.EndsWith(Strings.splinter);
                            bool is_stone = Strings.dicStones.TryGetValue(entrieDat.ID, out string stoneVal);
                            addItem = selValue == Resources.Resources.Main047_Stones ? (is_stone && !is_scarab)
                                : selValue == Resources.Resources.Main046_MapFrag ? (!is_stone && !is_scarab)
                                : selValue == Resources.Resources.Main052_Scarabs ? (!is_stone && is_scarab)
                                : addItem;
                        }
                        else
                        {
                            addItem = true;
                        }

                        if (addItem) listBulk.Add(entrieDat.Text);
                    }
                }
            }
            exchange.Currency = listBulk;
            exchange.CurrencyVisible = true;
        }
        else
        {
            exchange.Tier.Clear();
            exchange.Currency.Clear();
            exchange.TierVisible = false;
            exchange.CurrencyVisible = false;
        }

        if (isGet)
        {
            Vm.Form.Bulk.Get = exchange;
        }
        else if (isPay)
        {
            Vm.Form.Bulk.Pay = exchange;
        }
        else if (isShop)
        {
            Vm.Form.Shop.Exchange = exchange;
        }
    }

    private static bool CanSearchCurrency(object commandParameter)
    {
        return true;
    }

    private static void OnSearchCurrency(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            if (@string is "get")
            {
                if (Vm.Form.Bulk.Get.Search.Length >= 1)
                {
                    Vm.Logic.SelectViewModelExchangeCurrency("get/contains", Vm.Form.Bulk.Get.Search, null);
                }
                else
                {
                    Vm.Form.Bulk.Get.CategoryIndex = 0;
                    Vm.Form.Bulk.Get.CurrencyIndex = 0;
                }
            }
            else if (@string is "pay")
            {
                if (Vm.Form.Bulk.Pay.Search.Length >= 1)
                {
                    Vm.Logic.SelectViewModelExchangeCurrency("pay/contains", Vm.Form.Bulk.Pay.Search, null);
                }
                else
                {
                    Vm.Form.Bulk.Pay.CategoryIndex = 0;
                    Vm.Form.Bulk.Pay.CurrencyIndex = 0;
                }
            }
            else if (@string is "shop")
            {
                if (Vm.Form.Shop.Exchange.Search.Length >= 1)
                {
                    Vm.Logic.SelectViewModelExchangeCurrency("shop/contains", Vm.Form.Shop.Exchange.Search, null);
                }
                else
                {
                    Vm.Form.Shop.Exchange.CategoryIndex = 0;
                    Vm.Form.Shop.Exchange.CurrencyIndex = 0;
                }
            }
            _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        }
    }

    private static bool CanAddShopList(object commandParameter)
    {
        return true;
    }
    private static void OnAddShopList(object commandParameter)
    {
        if (commandParameter is string @string)
        {
            var shopList = @string.Contains("get", StringComparison.Ordinal) ? Vm.Form.Shop.GetList :
                @string.Contains("pay", StringComparison.Ordinal) ? Vm.Form.Shop.PayList : null;
            if (shopList is null)
            {
                return;
            }
            if (Vm.Form.Shop.Exchange.CategoryIndex > 0 && Vm.Form.Shop.Exchange.CurrencyIndex > 0)
            {
                string currency = Vm.Form.Shop.Exchange.Currency[Vm.Form.Shop.Exchange.CurrencyIndex];
                bool addItem = true;
                foreach (var item in shopList)
                {
                    if (item.Content == currency)
                    {
                        addItem = false;
                        break;
                    }
                }
                if (addItem)
                {
                    shopList.Add(new ListItemViewModel { Index = shopList.Count, Content = currency, FgColor = Strings.Color.Azure, ToolTip = Vm.Logic.GetExchangeCurrencyTag(Exchange.Shop) });
                }
            }
        }
    }

    private static bool CanResetShopLists(object commandParameter)
    {
        return true;
    }
    private static void OnResetShopLists(object commandParameter)
    {
        Vm.Form.Shop.PayList.Clear();
        Vm.Form.Shop.GetList.Clear();
    }

    private static bool CanInvertShopLists(object commandParameter)
    {
        return true;
    }
    private static void OnInvertShopLists(object commandParameter)
    {
        var tempList = Vm.Form.Shop.PayList;
        Vm.Form.Shop.PayList = Vm.Form.Shop.GetList;
        Vm.Form.Shop.GetList = tempList;
    }

    private static bool CanClearFocus(object commandParameter)
    {
        return true;
    }
    private static void OnClearFocus(object commandParameter) => _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();

    private static bool CanSwitchTab(object commandParameter)
    {
        return true;
    }
    private static void OnSwitchTab(object commandParameter)
    {
        if (commandParameter is string tab)
        {
            if (tab is "quick" && Vm.Form.Tab.DetailEnable)
            {
                Vm.Form.Tab.DetailSelected = true;
            }
            if (tab is "detail")
            {
                if (Vm.Form.Tab.BulkEnable)
                {
                    Vm.Form.Tab.BulkSelected = true;
                    return;
                }
                /*
                if (Vm.Form.Tab.PoePriceEnable)
                {
                    Vm.Form.Tab.PoePriceSelected = true;
                    return;
                }*/
                Vm.Form.Tab.QuickSelected = true;
            }
            if (tab is "bulk")
            {
                Vm.Form.Tab.ShopSelected = true;
            }
            if (tab is "shop")
            {
                if (Vm.Form.Tab.QuickEnable)
                {
                    Vm.Form.Tab.QuickSelected = true;
                    return;
                }
                Vm.Form.Tab.BulkSelected = true;
            }
        }
    }

    private static bool CanWheelAdjust(object commandParameter)
    {
        return true;
    }
    private static void OnWheelIncrement(object commandParameter) => WheelAdjustValue(commandParameter, 1);

    private static void OnWheelIncrementTenth(object commandParameter) => WheelAdjustValue(commandParameter, 0.1);

    private static void OnWheelIncrementHundredth(object commandParameter) => WheelAdjustValue(commandParameter, 0.01);

    private static void OnWheelDecrement(object commandParameter) => WheelAdjustValue(commandParameter, -1);

    private static void OnWheelDecrementTenth(object commandParameter) => WheelAdjustValue(commandParameter, -0.1);

    private static void OnWheelDecrementHundredth(object commandParameter) => WheelAdjustValue(commandParameter, -0.01);

    private static void WheelAdjustValue(object param, double value) 
        => _serviceProvider.GetRequiredService<INavigationService>().UpdateControlValue(param, value);

    private static bool CanSelectMod(object commandParameter)
    {
        return true;
    }
    private static void OnSelectMod(object commandParameter)
        => _serviceProvider.GetRequiredService<INavigationService>().UpdateControlValue(commandParameter);

    private static bool CanAutoClose(object commandParameter)
    {
        return true;
    }
    private static void OnAutoClose(object commandParameter)
    {
        DataManager.Config.Options.Autoclose = Vm.AutoClose;
    }
    private static bool CanUpdateOpacity(object commandParameter)
    {
        return true;
    }
    private static void OnUpdateOpacity(object commandParameter)
    {
        if (DataManager.Instance is not null && DataManager.Config is not null)
        {
            Vm.Form.OpacityText = (Vm.Form.Opacity * 100) + "%";
            DataManager.Config.Options.Opacity = Vm.Form.Opacity;
        }
    }
    private static bool CanExpanderExpand(object commandParameter)
    {
        return true;
    }
    private static void OnExpanderExpand(object commandParameter)
    {
        Vm.Form.Expander.Width = 214;
    }
    private static bool CanExpanderCollapse(object commandParameter)
    {
        return true;
    }
    private static void OnExpanderCollapse(object commandParameter)
    {
        Vm.Form.Expander.Width = 40;
        string configToSave = Json.Serialize<ConfigData>(DataManager.Config);
        DataManager.Save_Config(configToSave, "cfg");
    }

    private static bool CanCheckAllMods(object commandParameter)
    {
        return true;
    }
    private static void OnCheckAllMods(object commandParameter)
    {
        if (Vm.Form.ModLine.Count > 0)
        {
            foreach (ModLineViewModel mod in Vm.Form.ModLine)
            {
                mod.Selected = Vm.Form.AllCheck;
            }
        }
    }

    private static bool CanSelectBulkIndex(object commandParameter)
    {
        return true;
    }
    private static void OnSelectBulkIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Result.SelectedIndex.Bulk = idx;
        }
    }

    private static bool CanShowBulkWhisper(object commandParameter)
    {
        return true;
    }
    private static void OnShowBulkWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = Vm.Result.BulkList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = Vm.Result.BulkOffers[idx];
            _serviceProvider.GetRequiredService<INavigationService>().ShowWhisperView(data);
        }
    }

    private static bool CanSelectShopIndex(object commandParameter)
    {
        return true;
    }
    private static void OnSelectShopIndex(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Result.SelectedIndex.Shop = idx;
        }
    }

    private static bool CanShowShopWhisper(object commandParameter)
    {
        return true;
    }
    private static void OnShowShopWhisper(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            var item = Vm.Result.ShopList.Where(x => x.Index == idx).FirstOrDefault();
            item.FgColor = Strings.Color.Gray;
            var data = Vm.Result.ShopOffers[idx];
            _serviceProvider.GetRequiredService<INavigationService>().ShowWhisperView(data);
        }
    }

    private static bool CanRemoveGetList(object commandParameter)
    {
        return true;
    }
    private static void OnRemoveGetList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Form.Shop.GetList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in Vm.Form.Shop.GetList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    private static bool CanRemovePayList(object commandParameter)
    {
        return true;
    }
    private static void OnRemovePayList(object commandParameter)
    {
        if (commandParameter is int idx)
        {
            Vm.Form.Shop.PayList.RemoveAt(idx);
            int newIdx = 0;
            foreach (var item in Vm.Form.Shop.PayList)
            {
                item.Index = newIdx;
                newIdx++;
            }
        }
    }

    private static bool CanUpdateMinimized(object commandParameter)
    {
        return true;
    }
    private static void OnUpdateMinimized(object commandParameter)
    {
        Vm.Form.Minimized = !Vm.Form.Minimized;
    }

    private static bool CanWindowLoaded(object commandParameter)
    {
        return true;
    }
    private static void OnWindowLoaded(object commandParameter)
    {
        _serviceProvider.GetRequiredService<INavigationService>().SetMainHandle(commandParameter);
    }

    private static bool CanWindowClosed(object commandParameter)
    {
        return true;
    }
    private static void OnWindowClosed(object commandParameter)
    {
        //nothing
    }

    private static bool CanWindowDeactivated(object commandParameter)
    {
        return true;
    }
    private static void OnWindowDeactivated(object commandParameter)
    {
        if (!Vm.Form.Tab.BulkSelected && !Vm.Form.Tab.ShopSelected
            && DataManager.Config.Options.Autoclose)
        {
            _serviceProvider.GetRequiredService<INavigationService>().CloseMainView();
        }
    }
}

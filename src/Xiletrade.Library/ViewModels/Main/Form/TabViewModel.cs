using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.ViewModels.Live;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class TabViewModel : ViewModelBase
{
    private readonly FormViewModel _form;

    [ObservableProperty]
    private bool quickEnable;

    [ObservableProperty]
    private bool quickSelected;

    [ObservableProperty]
    private bool detailEnable;

    [ObservableProperty]
    private bool detailSelected;

    [ObservableProperty]
    private bool bulkEnable;

    [ObservableProperty]
    private bool bulkVisible = true;

    [ObservableProperty]
    private bool bulkSelected;

    [ObservableProperty]
    private bool shopEnable;

    [ObservableProperty]
    private bool shopVisible = true;

    [ObservableProperty]
    private bool shopSelected;

    [ObservableProperty]
    private bool poePriceEnable;

    [ObservableProperty]
    private bool poePriceSelected;

    [ObservableProperty]
    private bool customSearchEnable;

    [ObservableProperty]
    private bool customSearchSelected;

    [ObservableProperty]
    private bool historyEnable;

    [ObservableProperty]
    private bool historySelected;

    /// <summary>
    /// Tabs of the live searches, one by number (see <see cref="LiveSearchService.MaxSessions"/>) :
    /// displayed with the custom search while their live search is listening.
    /// </summary>
    public LiveTabViewModel[] Lives { get; }

    /// <summary>Tab of a live search selected, null : none.</summary>
    internal LiveTabViewModel SelectedLive
    {
        get
        {
            foreach (var live in Lives)
            {
                if (live.IsSelected)
                {
                    return live;
                }
            }
            return null;
        }
    }

    partial void OnQuickSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: false);

    partial void OnDetailSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: false);

    partial void OnCustomSearchSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: false);

    partial void OnBulkSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: true);

    partial void OnShopSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: true);

    private void OnSelectedChanged(bool value, bool useBulk)
    {
        if (!value)
            return;

        // custom search / bulk view : title of the tab selected (custom search, exchange or shop)
        if (CustomSearchEnable)
        {
            _form.ItemBaseType = useBulk ? Resources.Resources.Main032_cbTotalExchange : Resources.Resources.Main247_CustomSearch;
        }
        _form.UpdateMarket(useBulk);
    }

    /// <param name="live">Live searches, displayed in their tabs with the custom search.</param>
    internal TabViewModel(FormViewModel form, LiveSearchService live, bool useCustomOrBulk = false)
    {
        _form = form;
        Lives = new LiveTabViewModel[LiveSearchService.MaxSessions];
        for (int i = 0; i < Lives.Length; i++)
        {
            Lives[i] = new(live, i + 1, RestoreLive);
        }
        if (useCustomOrBulk)
        {
            bulkEnable = shopEnable = customSearchEnable = customSearchSelected = true;
            // hidden for now : the custom search and the tabs of its live searches
            bulkVisible = shopVisible = false;

            // the live searches go on when this form is closed
            UpdateLives(live.GetSessions());
            WeakReferenceMessenger.Default.Register<TabViewModel, LiveSearchStateMessage>(this,
                static (vm, state) => vm.UpdateLives(state.Sessions));
        }
    }

    internal TabViewModel(FormViewModel form, LiveSearchService live, ItemData item) : this(form, live)
    {
        var flag = item.Flag;
        if (item.State.ExchangeCurrency && (!flag.Rarity.Unique || flag.Map.IsMap))
        {
            bulkEnable = true;
            shopEnable = true;
            HistoryEnable = true;
            HistorySelected = true;
        }
        if (!item.State.ExchangeCurrency)
        {
            quickEnable = true;
            detailEnable = true;

            // Open Xiletrade with Quick or Detail view tab
            var selectDetail = !(flag.Map.IsMap && flag.Tag.Corrupted) && (flag.StackableCurrency
                || flag.Map.IsMap || flag.Gem.IsGem || flag.Tag.CapturedBeast || flag.UltimatumPoe2
                || flag.Gem.Uncut || flag.Wombgift || flag.Area.TrialCoins || flag.Area.SanctumResearch
                || item.State.ExchangeCurrency);
            detailSelected = selectDetail;
            quickSelected = !selectDetail;
            if (flag.Rarity.Rare && !flag.Map.IsMap && !flag.Tag.CapturedBeast) poePriceEnable = true;
        }
    }

    // tab of each live search listening, the custom search selected when the one selected ends
    private void UpdateLives(LiveSearchInfo[] sessions)
    {
        foreach (var tab in Lives)
        {
            LiveSearchInfo info = null;
            foreach (var session in sessions)
            {
                if (session.Number == tab.Number)
                {
                    info = session;
                    break;
                }
            }
            var wasSelected = tab.IsSelected;
            tab.Update(info);
            if (info is null && wasSelected)
            {
                tab.IsSelected = false;
                CustomSearchSelected = true;
            }
        }
    }

    // criteria of a live search given back to the custom search, displayed
    private void RestoreLive(LiveSearchCriteria criteria)
    {
        _form.CustomSearch?.RestoreLive(criteria);
        CustomSearchSelected = true;
    }

    [RelayCommand]
    private void SwitchTab(object commandParameter)
    {
        if (commandParameter is string tab)
        {
            if (tab is "quick" && DetailEnable)
            {
                DetailSelected = true;
            }
            if (tab is "detail")
            {
                if (BulkEnable && BulkVisible)
                {
                    BulkSelected = true;
                    return;
                }
                QuickSelected = true;
            }
            if (tab is "bulk")
            {
                ShopSelected = true;
            }
            if (tab is "shop")
            {
                if (QuickEnable)
                {
                    QuickSelected = true;
                    return;
                }
                CustomSearchSelected = true;
            }
            if (tab is "custom" && BulkVisible)
            {
                BulkSelected = true;
            }
        }
    }
}

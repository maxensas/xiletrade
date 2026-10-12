using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Models.Poe.Domain.Parser;

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
    private bool bulkSelected;

    [ObservableProperty]
    private bool shopEnable;

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

    partial void OnQuickSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: false);

    partial void OnDetailSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: false);

    partial void OnCustomSearchSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: false);

    partial void OnBulkSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: true);

    partial void OnShopSelectedChanged(bool value) => OnSelectedChanged(value, useBulk: true);

    private void OnSelectedChanged(bool value, bool useBulk)
    {
        if (!value)
            return;

        _form.UpdateMarket(useBulk);
    }

    internal TabViewModel(FormViewModel form)
    {
        _form = form;

        bulkEnable = shopEnable = customSearchEnable = customSearchSelected = true;
    }

    internal TabViewModel(FormViewModel form, ItemData item)
    {
        _form = form;

        var flag = item.Flag;
        if (item.State.ExchangeCurrency && (!flag.Rarity.Unique || flag.Map.IsMap))
        {
            bulkEnable = true;
        }
        quickEnable = true;
        detailEnable = true;
        quickSelected = true;

        // Open Xiletrade with Quick or Detail view tab
        var selectDetail = !(flag.Map.IsMap && flag.Tag.Corrupted) && (flag.StackableCurrency
            || flag.Map.IsMap || flag.Gem.IsGem || flag.Tag.CapturedBeast || flag.UltimatumPoe2
            || flag.Gem.Uncut || flag.Wombgift || flag.Area.TrialCoins || flag.Area.SanctumResearch
            || item.State.ExchangeCurrency);
        detailSelected = selectDetail;
        quickSelected = !selectDetail;

        if (flag.Rarity.Rare && !flag.Map.IsMap && !flag.Tag.CapturedBeast) poePriceEnable = true;
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
                if (BulkEnable)
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
            if (tab is "custom")
            {
                BulkSelected = true;
            }
        }
    }
}

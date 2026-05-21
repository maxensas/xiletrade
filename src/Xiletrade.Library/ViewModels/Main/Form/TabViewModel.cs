using CommunityToolkit.Mvvm.ComponentModel;
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

    partial void OnQuickSelectedChanged(bool value)
    {
        if (!value)
            return;

        _form.UpdateMarket(useBulk: false);
    }

    partial void OnDetailSelectedChanged(bool value)
    {
        if (!value)
            return;

        _form.UpdateMarket(useBulk: false);
    }

    partial void OnBulkSelectedChanged(bool value)
    {
        if (!value)
            return;

        _form.UpdateMarket(useBulk: true);
    }

    partial void OnShopSelectedChanged(bool value)
    {
        if (!value)
            return;

        _form.UpdateMarket(useBulk: true);
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
        if (item.State.ExchangeCurrency && (!flag.Unique || flag.Map))
        {
            bulkEnable = true;
        }
        quickEnable = true;
        detailEnable = true;
        quickSelected = true;

        // Open Xiletrade with Quick or Detail view tab
        var selectDetail = !(flag.Map && flag.Corrupted) && (flag.StackableCurrency
            || flag.Map || flag.Gems || flag.CapturedBeast || flag.UltimatumPoe2
            || flag.UncutGem || flag.Wombgift || flag.TrialCoins
            || item.State.ExchangeCurrency);
        detailSelected = selectDetail;
        quickSelected = !selectDetail;

        if (flag.Rare && !flag.Map && !flag.CapturedBeast) poePriceEnable = true;
    }
}

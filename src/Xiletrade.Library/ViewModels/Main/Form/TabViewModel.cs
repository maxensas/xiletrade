using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Domain.Parser;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class TabViewModel : ViewModelBase
{
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

    internal TabViewModel(bool useBulk)
    {
        quickSelected = !useBulk;
        bulkEnable = shopEnable = customSearchEnable = customSearchSelected = useBulk;
    }

    internal TabViewModel(ItemData item) : this(useBulk : false)
    {
        var flag = item.Flag;
        if (item.State.ExchangeCurrency && (!flag.Unique || flag.Map))
        {
            bulkEnable = true;
        }

        quickEnable = true;
        detailEnable = true;

        // Open Xiletrade with Quick or Detail view tab
        var selectDetail = !(flag.Map && flag.Corrupted) && (flag.StackableCurrency
            || flag.Map || flag.Gems || flag.CapturedBeast || flag.UltimatumPoe2
            || flag.UncutGem || flag.Wombgift || flag.TrialCoins
            || (item.State.ExchangeCurrency && !flag.Tablet && !flag.Waystones));
        detailSelected = selectDetail;
        quickSelected = !selectDetail;

        if (flag.Rare && !flag.Map && !flag.CapturedBeast) poePriceEnable = true;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class RarityViewModel : ViewModelBase
{
    [ObservableProperty]
    private string item = string.Empty;

    [ObservableProperty]
    private int index = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> comboBox =
    [
        Resources.Resources.General005_Any,
        Resources.Resources.General009_Normal,
        Resources.Resources.General008_Magic,
        Resources.Resources.General007_Rare,
        Resources.Resources.General006_Unique,
        Resources.Resources.General110_FoilUnique,
        Resources.Resources.General010_AnyNU
    ];

    internal RarityViewModel()
    {

    }

    internal RarityViewModel(ItemData itemData)
    {
        var flag = itemData.Flag;
        item = flag.FoilVariant ? Resources.Resources.General110_FoilUnique
            : CanSelectAnyNonUnique(itemData) ? Resources.Resources.General010_AnyNU
            : CanSelectAny(itemData) ? Resources.Resources.General005_Any
            : string.IsNullOrEmpty(itemData.Rarity) ? Resources.Resources.General005_Any
            : itemData.Rarity;

        index = comboBox.IndexOf(item);
    }

    private static bool CanSelectAnyNonUnique(ItemData itemData)
    {
        var flag = itemData.Flag;
        return (flag.Map || flag.Waystones || flag.Watchstone || flag.Invitation ||
             flag.Logbook || flag.ChargedCompass || flag.Voidstone) && !flag.Unique;
    }

    private static bool CanSelectAny(ItemData itemData)
    {
        var flag = itemData.Flag;
        return !flag.Waystones &&
              (flag.MapFragment || flag.MiscMapItems || itemData.State.ExchangeCurrency || flag.Currency);
    }
}

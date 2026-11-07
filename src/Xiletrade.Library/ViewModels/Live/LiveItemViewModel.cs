using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Live;

/// <summary>
/// New listing of a live search, displayed in a toast : item name in the color of its rarity, price and actions.
/// </summary>
public sealed partial class LiveItemViewModel : ViewModelBase
{
    private readonly Func<string, Task> _travel;
    private readonly Action<string> _dismiss;
    private readonly string _hideoutToken;

    /// <summary>Id of the listing, also id of the toast.</summary>
    public string Id { get; }

    /// <summary>Name of the item : its name (unique, rare), its type otherwise.</summary>
    public string Title { get; }

    /// <summary>Type of a named item, null otherwise.</summary>
    public string TypeLine { get; }

    public ItemRarity Rarity { get; }

    /// <summary>Amount of the price, empty without price.</summary>
    public string Amount { get; }

    /// <summary>Currency of the price (image displayed), null without price.</summary>
    public CurrencyInfo Currency { get; }

    public string Account { get; }

    /// <summary>Item displayed in the tooltip, as in the results of a price check.</summary>
    public SaleItem Item { get; }

    /// <summary>Instant buyout : the seller can be joined in his hideout.</summary>
    public bool CanTravel { get; }

    /// <param name="travel">Travel with the hideout token, sent like the fetch of the live search.</param>
    internal LiveItemViewModel(DataManagerService dm, FetchDataInfo info, Func<string, Task> travel, Action<string> dismiss)
    {
        _travel = travel;
        _dismiss = dismiss;
        _hideoutToken = info.Listing?.HideoutToken;

        var item = info.Item;
        Id = info.Id;
        Item = new(dm, item);
        Rarity = new(item);
        var hasName = item?.Name?.Length > 0;
        Title = hasName ? item.Name : item?.TypeLine;
        TypeLine = hasName ? item.TypeLine : null;
        Account = info.Listing?.Account?.Name;
        CanTravel = _hideoutToken?.Length > 0;

        var price = info.Listing?.Price;
        if (price?.Currency?.Length > 0 && price.Amount > 0)
        {
            Amount = price.Amount.ToString("0.##", CultureInfo.InvariantCulture);
            Currency = dm.GetCurrencyInfo(price.Currency.ReplaceCurrencyChars(), dm.Config.Options.GameVersion is 1);
        }
        else
        {
            Amount = string.Empty;
        }
    }

    /// <summary>Text version of the toast (UI without template).</summary>
    internal string PriceText => Currency is null ? string.Empty : $"{Amount} {Currency.Name}";

    // the toast is closed once the request is sent
    [RelayCommand]
    private async Task TravelToHideout(object commandParameter)
    {
        await _travel(_hideoutToken);
        _dismiss(Id);
    }

    [RelayCommand]
    private void Close(object commandParameter) => _dismiss(Id);
}

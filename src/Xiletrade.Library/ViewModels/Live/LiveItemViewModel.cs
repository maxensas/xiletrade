using CommunityToolkit.Mvvm.ComponentModel;
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
    private readonly Func<string, string, Task<bool>> _travel;
    private readonly string _referer;
    private readonly Action<string> _dismiss;
    private readonly Func<LiveItemViewModel, bool> _pin;
    private readonly string _hideoutToken;
    private readonly DataManagerService _dm;
    private readonly ItemDataApi _item;
    private SaleItem _saleItem;
    private string _priceText;

    /// <summary>Id of the listing, also id of the toast.</summary>
    public string Id { get; }

    /// <summary>Number of its live search (several live searches).</summary>
    public int Number { get; }

    /// <summary>Name of the item : its name (unique, rare), its type otherwise.</summary>
    public string Title { get; }

    /// <summary>Type of a named item, null otherwise.</summary>
    public string TypeLine { get; }

    public ItemRarity Rarity { get; }

    /// <summary>Amount of the price, empty without price.</summary>
    public string Amount { get; }

    /// <summary>Currency of the price (image displayed), null without price.</summary>
    public CurrencyInfo Currency { get; }

    /// <summary>Item displayed in the tooltip, as in the results of a price check.</summary>
    /// <remarks>Built when the tooltip is shown first : most of the toasts are never hovered.</remarks>
    public SaleItem Item => _saleItem ??= new(_dm, _item);

    /// <summary>Instant buyout : the seller can be joined in his hideout.</summary>
    public bool CanTravel { get; }

    /// <summary>State shared by the toasts : pin red while no more toast can be pinned, number shown with several lives.</summary>
    public LiveToastStateViewModel State { get; }

    /// <summary>Pinned : kept at the top of the screen until it is closed, the pin button is hidden.</summary>
    [ObservableProperty]
    private bool isPinned;

    /// <param name="referer">Page of the live search : the hideout token is checked with it.</param>
    /// <param name="travel">Travel with the hideout token and the referer : false when it failed (the error is displayed).</param>
    /// <param name="pin">Displays the toast again, pinned : false when the maximum of pinned toasts is reached.</param>
    /// <param name="number">Number of its live search.</param>
    internal LiveItemViewModel(DataManagerService dm, FetchDataInfo info, int number, string referer, Func<string, string, Task<bool>> travel,
        Action<string> dismiss, Func<LiveItemViewModel, bool> pin, LiveToastStateViewModel state)
    {
        _referer = referer;
        Number = number;
        _travel = travel;
        _dismiss = dismiss;
        _pin = pin;
        State = state;
        _hideoutToken = info.Listing?.HideoutToken;

        var item = info.Item;
        Id = info.Id;
        _dm = dm;
        _item = item;
        Rarity = new(item);
        var hasName = item?.Name?.Length > 0;
        Title = hasName ? item.Name : item?.TypeLine;
        TypeLine = hasName ? item.TypeLine : null;
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
    internal string PriceText => _priceText ??= Currency is null ? string.Empty : string.Concat(Amount, " ", Currency.Name);

    // the toast is closed once the travel is sent, kept when it failed
    [RelayCommand]
    private async Task TravelToHideout(object commandParameter)
    {
        if (await _travel(_hideoutToken, _referer))
        {
            _dismiss(Id);
        }
    }

    [RelayCommand]
    private void Pin(object commandParameter)
    {
        if (IsPinned || !_pin(this))
            return;

        IsPinned = true;
    }

    [RelayCommand]
    private void Close(object commandParameter) => _dismiss(Id);
}

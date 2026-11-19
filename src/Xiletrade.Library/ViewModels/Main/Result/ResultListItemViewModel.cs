using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultListItemViewModel : ViewModelBase
{
    // live search : the item is built when it is displayed first (see Item)
    private readonly DataManagerService _dm;
    private readonly ItemDataApi _itemData;
    private SaleItem _item;

    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private string fgColor;

    /// <summary>The hideout of the seller has been visited (travel to hideout).</summary>
    [ObservableProperty]
    private bool isVisited;

    public string Content { get; }
    public string ToolTip { get; }
    public string Tag { get; }

    public SaleInfo Info { get; }
    /// <summary>Item of the result (tooltip, item level) : built when it is displayed first for a live search.</summary>
    public SaleItem Item => _item ??= _itemData is null ? null : new(_dm, _itemData);
    public CurrencyInfo Currency { get; }

    /// <summary>Live search : time the result appeared, instead of its age.</summary>
    public string Time { get; }

    public ResultListItemViewModel(string cont)
    {
        Content = cont;
    }

    // detail vm
    public ResultListItemViewModel(SaleItem saleItem, SaleInfo saleInfo, 
        CurrencyInfo curInfo, TradeStatus status) : this(string.Empty)
    {
        fgColor = Strings.Status.GetColorStatus(status);
        _item = saleItem;
        Info = saleInfo;
        Currency = curInfo;
    }

    // live search : the item is built when it is displayed (only the lines visible in the list)
    internal ResultListItemViewModel(DataManagerService dm, ItemDataApi itemData, SaleInfo saleInfo,
        CurrencyInfo curInfo, TradeStatus status, string time) : this(saleItem: null, saleInfo, curInfo, status)
    {
        _dm = dm;
        _itemData = itemData;
        Time = time;
    }

    // bulk and shop
    public ResultListItemViewModel(string cont, string tip, string controlTag, TradeStatus status)
        : this(cont)
    {
        fgColor = Strings.Status.GetColorStatus(status, isBulkTheme: true);
        ToolTip = tip;
        Tag = controlTag;
    }
    
    public ResultListItemViewModel(string cont, TradeStatus status)
        : this(cont, tip: null, controlTag: string.Empty, status)
    {

    }
    
    public ResultListItemViewModel(int idx, string cont, string tip, string controlTag, TradeStatus status) 
        : this(cont, tip, controlTag, status)
    {
        index = idx;
    }
    
    public ResultListItemViewModel(int idx, string cont, TradeStatus status)
        : this(idx, cont, tip: null, controlTag: string.Empty, status)
    {

    }

    public ResultListItemViewModel(int idx, string cont, string tip, string color) : this(cont)
    {
        fgColor = color;
        index = idx;
        ToolTip = tip;
        Tag = null;
    }

    // poe price
    public ResultListItemViewModel(string cont, string color) : this(cont)
    {
        fgColor = color;
        ToolTip = null;
        Tag = string.Empty;
    }
}

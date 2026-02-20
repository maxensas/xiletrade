using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class ListItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private string fgColor;

    public string Content { get; }
    public string ToolTip { get; }
    public string Tag { get; }

    public SaleInfo Info { get; }
    public SaleItem Item { get; }
    public CurrencyInfo Currency { get; }

    public ListItemViewModel(string cont)
    {
        Content = cont;
    }

    // detail vm
    public ListItemViewModel(DataManagerService dm, ItemDataApi itemData, SaleInfo salrInfo, 
        TradeStatus status, string cur, bool isPoe2) : this(string.Empty)
    {
        fgColor = Strings.Status.GetColorStatus(status);

        Item = new(dm, itemData);
        Info = salrInfo;
        //tag = !string.IsNullOrEmpty(itemData.Icon) ? itemData.Icon : string.Empty;
        var entry = dm.Currencies.FindEntryById(cur);
        Currency = entry is null ? new(cur, isPoe2) : new(cur, entry.Img, isPoe2);
    }

    // bulk and shop
    public ListItemViewModel(string cont, string tip, string controlTag, TradeStatus status)
        : this(cont)
    {
        fgColor = Strings.Status.GetColorStatus(status, isBulkTheme: true);

        ToolTip = tip;
        Tag = controlTag;
    }
    
    public ListItemViewModel(string cont, TradeStatus status)
        : this(cont, tip: null, controlTag: string.Empty, status)
    {

    }
    
    public ListItemViewModel(int idx, string cont, string tip, string controlTag, TradeStatus status) 
        : this(cont, tip, controlTag, status)
    {
        index = idx;
    }
    
    public ListItemViewModel(int idx, string cont, TradeStatus status)
        : this(idx, cont, tip: null, controlTag: string.Empty, status)
    {

    }

    public ListItemViewModel(int idx, string cont, string tip, string color) : this(cont)
    {
        fgColor = color;
        index = idx;

        ToolTip = tip;
        Tag = null;
    }

    // poe price
    public ListItemViewModel(string cont, string color) : this(cont)
    {
        fgColor = color;

        ToolTip = null;
        Tag = string.Empty;
    }
}

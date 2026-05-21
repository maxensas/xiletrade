using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultListItemViewModel : ViewModelBase
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

    public ResultListItemViewModel(string cont)
    {
        Content = cont;
    }

    // detail vm
    public ResultListItemViewModel(SaleItem saleItem, SaleInfo saleInfo, 
        CurrencyInfo curInfo, TradeStatus status) : this(string.Empty)
    {
        fgColor = Strings.Status.GetColorStatus(status);
        Item = saleItem;
        Info = saleInfo;
        Currency = curInfo;
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

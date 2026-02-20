using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class ListItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string content;

    [ObservableProperty]
    private string toolTip;

    [ObservableProperty]
    private string tag;

    [ObservableProperty]
    private ResultItemViewModel item;

    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private string fgColor;

    [ObservableProperty]
    private double ammount;

    [ObservableProperty]
    private CurrencyInfo currency;

    [ObservableProperty]
    private string qualityOrCount;

    [ObservableProperty]
    private string ageLabel;

    [ObservableProperty]
    private string age;

    [ObservableProperty]
    private string accountLabel = Resources.Resources.Main013_ListName;

    [ObservableProperty]
    private string account;

    public ListItemViewModel(string cont)
    {
        content = cont;
    }

    // detail vm
    public ListItemViewModel(DataManagerService dm, ItemDataApi itemData, TradeStatus status, 
        double ammnt, string cur, string qualOrCount, 
        string agee, string ageLbl, string acc, bool isPoe2) : this(string.Empty)
    {
        fgColor = Strings.Status.GetColorStatus(status);
        item = new ResultItemViewModel(dm, itemData);
        //tag = !string.IsNullOrEmpty(itemData.Icon) ? itemData.Icon : string.Empty;
        ammount = ammnt;
        qualityOrCount = qualOrCount;
        age = agee;
        ageLabel = ageLbl;
        account = acc;
        var entry = dm.Currencies.FindEntryById(cur);
        currency = entry is null ? new(cur, isPoe2) : new(cur, entry.Img, isPoe2);
    }

    // bulk and shop
    public ListItemViewModel(string cont, string tip, string controlTag, TradeStatus status)
        : this(cont)
    {
        toolTip = tip;
        tag = controlTag;
        fgColor = Strings.Status.GetColorStatus(status, isBulkTheme: true);
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
        index = idx;
        toolTip = tip;
        tag = null;
        fgColor = color;
    }

    // poe price
    public ListItemViewModel(string cont, string color) : this(cont)
    {
        toolTip = null;
        tag = string.Empty;
        fgColor = color;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
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
    private string currency;

    [ObservableProperty]
    private string currencyUri;

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

    public ListItemViewModel(string cont, string tip, string controlTag, string color)
        : this(cont)
    {
        toolTip = tip;
        tag = controlTag;
        fgColor = color;
    }

    public ListItemViewModel(DataManagerService dm, ItemDataApi itemData, string color, 
        double ammnt, string cur, string qualOrCount, 
        string agee, string ageLbl, string acc)
        : this(string.Empty) //
    {
        fgColor = color;
        item = new ResultItemViewModel(dm, itemData);
        //tag = !string.IsNullOrEmpty(itemData.Icon) ? itemData.Icon : string.Empty;
        ammount = ammnt;
        currency = cur;
        qualityOrCount = qualOrCount;
        age = agee;
        ageLabel = ageLbl;
        account = acc;

        var entry = dm.Currencies.FindEntryById(cur);
        if (entry is not null)
        {
            currencyUri = entry.Img;
        }
    }

    public ListItemViewModel(string cont, string color)
        : this(cont, tip: null, controlTag: string.Empty, color)
    {

    }

    public ListItemViewModel(int idx, string cont, string tip, string controlTag, string color) 
        : this(cont, tip, controlTag, color)
    {
        index = idx;
    }

    public ListItemViewModel(int idx, string cont, string color)
        : this(idx, cont, tip: null, controlTag: string.Empty, color)
    {

    }

    public ListItemViewModel(int idx, string cont, string tip, string color)
        : this(idx, cont, tip, controlTag: null, color)
    {

    }
}

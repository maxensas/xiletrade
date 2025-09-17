using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Serializable;
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

    public ListItemViewModel(string cont, ItemDataApi itemData, string color)
        : this(cont)
    {
        fgColor = color;
        item = new ResultItemViewModel(itemData);
        tag = !string.IsNullOrEmpty(itemData.Icon) ? itemData.Icon : string.Empty;
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

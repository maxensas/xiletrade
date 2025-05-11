using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class MinMaxViewModel : ViewModelBase
{
    [ObservableProperty]
    private StatPanel id = StatPanel.NoStat;

    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string min = string.Empty;

    [ObservableProperty]
    private string max = string.Empty;

    [ObservableProperty]
    private double minSlide = ModFilter.EMPTYFIELD;

    [ObservableProperty]
    private double minSlideDefault = ModFilter.EMPTYFIELD;

    [ObservableProperty]
    private bool selected = false;

    [ObservableProperty]
    private bool showSlide;
    /*
    [ObservableProperty]
    private bool visible;
    */
    public MinMaxViewModel(MinMaxModel model)
    {
        id = model.Id;
        text = model.Text;
        min = model.Min;
        max = model.Max;
        minSlide = model.MinSlide;
        minSlideDefault = model.MinSlideDefault;
        selected = model.Selected;
        showSlide = model.ShowSlide;
        //visible = model.Visible;

        if (min.Length is 0 || max.Length > 0)
        {
            showSlide = false;
            return;
        }
        minSlide = minSlideDefault = min.ToDoubleEmptyField();
        showSlide = true;
        return;
    }
}

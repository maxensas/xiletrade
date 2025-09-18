using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

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
    private double slideValue = ModFilter.EMPTYFIELD;

    [ObservableProperty]
    private double slideValueDefault = ModFilter.EMPTYFIELD;

    [ObservableProperty]
    private bool selected = false;

    [ObservableProperty]
    private bool showSlide;

    [ObservableProperty]
    private bool isReversed;

    public double ItemMin => 
        IsReversed ? Max.ToDoubleEmptyField()
        : SlideValue is not ModFilter.EMPTYFIELD ? SlideValue : Min.ToDoubleEmptyField();

    public double ItemMax =>
        !IsReversed ? Max.ToDoubleEmptyField()
        : SlideValue is not ModFilter.EMPTYFIELD ? SlideValue : Min.ToDoubleEmptyField();

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
        slideValue = model.MinSlide;
        slideValueDefault = model.MinSlideDefault;
        selected = model.Selected;
        showSlide = model.ShowSlide;
        //visible = model.Visible;

        if (min.Length is 0 || max.Length > 0)
        {
            showSlide = false;
            return;
        }
        slideValue = slideValueDefault = min.ToDoubleEmptyField();
        showSlide = true;
        return;
    }
}

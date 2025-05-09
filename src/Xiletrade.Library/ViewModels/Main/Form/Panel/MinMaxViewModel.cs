using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class MinMaxViewModel : ViewModelBase
{
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

    internal void UpdateMinSlide()
    {
        if (Min.Length is 0 || Max.Length > 0)
        {
            ShowSlide = false;
            return;
        }
        MinSlide = MinSlideDefault = Min.ToDoubleEmptyField();
        ShowSlide = true;
        return;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class InfluenceViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool shaper;

    [ObservableProperty]
    private bool elder;

    [ObservableProperty]
    private bool crusader;

    [ObservableProperty]
    private bool redeemer;

    [ObservableProperty]
    private bool hunter;

    [ObservableProperty]
    private bool warlord;

    [ObservableProperty]
    private string shaperText = string.Empty;

    [ObservableProperty]
    private string elderText = string.Empty;

    [ObservableProperty]
    private string crusaderText = string.Empty;

    [ObservableProperty]
    private string redeemerText = string.Empty;

    [ObservableProperty]
    private string hunterText = string.Empty;

    [ObservableProperty]
    private string warlordText = string.Empty;
}

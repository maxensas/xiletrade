using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool synthesisBlight;

    [ObservableProperty]
    private bool blighRavaged;

    [ObservableProperty]
    private bool scourged;

    [ObservableProperty]
    private string synthesisBlightLabel = "Synthblight";// = string.Empty;

    [ObservableProperty]
    private string blighRavagedtLabel = "Ravaged";// = string.Empty;

    [ObservableProperty]
    private string scourgedLabel = "Scourged";// = string.Empty;

    [ObservableProperty]
    private string facetorMin = string.Empty;

    [ObservableProperty]
    private string facetorMax = string.Empty;

    [ObservableProperty]
    private int alternateGemIndex = 0;

    [ObservableProperty]
    private bool useBorderThickness = true;

    [ObservableProperty]
    private CommonStatViewModel common = new();

    [ObservableProperty]
    private DefenseViewModel defense = new();

    [ObservableProperty]
    private DamageViewModel damage = new();

    [ObservableProperty]
    private TotalStatViewModel total = new();

    [ObservableProperty]
    private RewardViewModel reward = new();

    [ObservableProperty]
    private SanctumViewModel sanctum = new();

    [ObservableProperty]
    private MapViewModel map = new();

    [ObservableProperty]
    private PanelRowViewModel row = new();

    [ObservableProperty]
    private PanelColViewModel col = new();
}

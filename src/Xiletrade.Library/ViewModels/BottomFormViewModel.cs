using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public sealed partial class BottomFormViewModel : ViewModelBase
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
    private string facetorMin;

    [ObservableProperty]
    private string facetorMax;

    [ObservableProperty]
    private int alternateGemIndex = -1;

    [ObservableProperty]
    private bool useBorderThickness = true;

    [ObservableProperty]
    private CommonClass common = new();

    [ObservableProperty]
    private DefenseClass defense = new();

    [ObservableProperty]
    private DamageClass damage = new();

    [ObservableProperty]
    private TotalClass total = new();

    [ObservableProperty]
    private RewardClass reward = new();

    [ObservableProperty]
    private SanctumClass sanctum = new();

    [ObservableProperty]
    private MapClass map = new();

    [ObservableProperty]
    private RowPanel row = new();

    [ObservableProperty]
    private ColPanel col = new();

    public sealed partial class CommonClass : ViewModelBase
    {
        [ObservableProperty]
        private string itemLevelLabel = string.Empty;

        [ObservableProperty]
        private Form itemLevel = new();

        [ObservableProperty]
        private Form quality = new();

        [ObservableProperty]
        private Sockets sockets = new();
    }

    public sealed partial class DefenseClass : ViewModelBase
    {
        [ObservableProperty]
        private Form armour = new();

        [ObservableProperty]
        private Form energy = new();

        [ObservableProperty]
        private Form evasion = new();

        [ObservableProperty]
        private Form ward = new();
    }

    public sealed partial class DamageClass : ViewModelBase
    {
        [ObservableProperty]
        private Form total = new();

        [ObservableProperty]
        private Form physical = new();

        [ObservableProperty]
        private Form elemental = new();
    }

    public sealed partial class TotalClass : ViewModelBase
    {
        [ObservableProperty]
        private Form resistance = new();

        [ObservableProperty]
        private Form life = new();

        [ObservableProperty]
        private Form globalEs = new();
    }

    public sealed partial class SanctumClass : ViewModelBase
    {
        [ObservableProperty]
        private Form resolve = new();

        [ObservableProperty]
        private Form maximumResolve = new();

        [ObservableProperty]
        private Form inspiration = new();

        [ObservableProperty]
        private Form aureus = new();
    }

    public sealed partial class MapClass : ViewModelBase
    {
        [ObservableProperty]
        private Form quantity = new();

        [ObservableProperty]
        private Form rarity = new();

        [ObservableProperty]
        private Form packSize = new();

        [ObservableProperty]
        private Form moreScarab = new();

        [ObservableProperty]
        private Form moreCurrency = new();

        [ObservableProperty]
        private Form moreDivCard = new();

        [ObservableProperty]
        private Form moreMap = new();
    }

    public sealed partial class Form : ViewModelBase
    {
        [ObservableProperty]
        private string min = string.Empty;

        [ObservableProperty]
        private string max = string.Empty;

        [ObservableProperty]
        private bool selected;
    }

    public sealed partial class RewardClass : ViewModelBase
    {
        [ObservableProperty]
        private string text = string.Empty;

        [ObservableProperty]
        private string tip = string.Empty;

        [ObservableProperty]
        private string fgColor;
    }

    public sealed partial class Sockets : ViewModelBase
    {
        [ObservableProperty]
        private string socketMin;

        [ObservableProperty]
        private string socketMax;

        [ObservableProperty]
        private string linkMin;

        [ObservableProperty]
        private string linkMax;

        [ObservableProperty]
        private string redColor;

        [ObservableProperty]
        private string greenColor;

        [ObservableProperty]
        private string blueColor;

        [ObservableProperty]
        private string whiteColor;

        [ObservableProperty]
        private bool selected;
    }

    public sealed partial class RowPanel : ViewModelBase
    {
        [ObservableProperty]
        private double armourMaxHeight;

        [ObservableProperty]
        private double weaponMaxHeight;

        [ObservableProperty]
        private double totalMaxHeight;
    }

    public sealed partial class ColPanel : ViewModelBase
    {
        [ObservableProperty]
        private double firstMaxWidth = 14;

        [ObservableProperty]
        private double lastMinWidth = 86;
    }
}

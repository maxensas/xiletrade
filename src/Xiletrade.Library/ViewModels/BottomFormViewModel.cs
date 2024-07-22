namespace Xiletrade.Library.ViewModels;

public sealed class BottomFormViewModel : BaseViewModel
{
    private bool synthesisBlight;
    private bool blighRavaged;
    private bool scourged;
    private string synthesisBlightLabel = "Synthblight";// = string.Empty;
    private string blighRavagedtLabel = "Ravaged";// = string.Empty;
    private string scourgedLabel = "Scourged";// = string.Empty;
    private string facetorMin;
    private string facetorMax;
    private int alternateGemIndex = -1;
    private bool useBorderThickness = true;
    private CommonClass common = new();
    private DefenseClass defense = new();
    private DamageClass damage = new();
    private TotalClass total = new();
    private RewardClass reward = new();
    private SanctumClass sanctum = new();
    private RowPanel row = new();
    private ColPanel col = new();

    public bool SynthesisBlight { get => synthesisBlight; set => SetProperty(ref synthesisBlight, value); }
    public bool BlighRavaged { get => blighRavaged; set => SetProperty(ref blighRavaged, value); }
    public bool Scourged { get => scourged; set => SetProperty(ref scourged, value); }
    public string SynthesisBlightLabel { get => synthesisBlightLabel; set => SetProperty(ref synthesisBlightLabel, value); }
    public string BlighRavagedtLabel { get => blighRavagedtLabel; set => SetProperty(ref blighRavagedtLabel, value); }
    public string ScourgedLabel { get => scourgedLabel; set => SetProperty(ref scourgedLabel, value); }
    public string FacetorMin { get => facetorMin; set => SetProperty(ref facetorMin, value); }
    public string FacetorMax { get => facetorMax; set => SetProperty(ref facetorMax, value); }
    public int AlternateGemIndex { get => alternateGemIndex; set => SetProperty(ref alternateGemIndex, value); }
    public bool UseBorderThickness { get => useBorderThickness; set => SetProperty(ref useBorderThickness, value); }
    public CommonClass Common { get => common; set => SetProperty(ref common, value); }
    public DefenseClass Defense { get => defense; set => SetProperty(ref defense, value); }
    public DamageClass Damage { get => damage; set => SetProperty(ref damage, value); }
    public TotalClass Total { get => total; set => SetProperty(ref total, value); }
    public RewardClass Reward { get => reward; set => SetProperty(ref reward, value); }
    public SanctumClass Sanctum { get => sanctum; set => SetProperty(ref sanctum, value); }
    public RowPanel Row { get => row; set => SetProperty(ref row, value); }
    public ColPanel Col { get => col; set => SetProperty(ref col, value); }

    public sealed class CommonClass : BaseViewModel
    {
        private string itemLevelLabel = string.Empty;
        private Form itemLevel = new();
        private Form quality = new();
        private Sockets sockets = new();

        public string ItemLevelLabel { get => itemLevelLabel; set => SetProperty(ref itemLevelLabel, value); }
        public Form ItemLevel { get => itemLevel; set => SetProperty(ref itemLevel, value); }
        public Form Quality { get => quality; set => SetProperty(ref quality, value); }
        public Sockets Sockets { get => sockets; set => SetProperty(ref sockets, value); }
    }
    public sealed class DefenseClass : BaseViewModel
    {
        private Form armour = new();
        private Form energy = new();
        private Form evasion = new();
        private Form ward = new();

        public Form Armour { get => armour; set => SetProperty(ref armour, value); }
        public Form Energy { get => energy; set => SetProperty(ref energy, value); }
        public Form Evasion { get => evasion; set => SetProperty(ref evasion, value); }
        public Form Ward { get => ward; set => SetProperty(ref ward, value); }
    }
    public sealed class DamageClass : BaseViewModel
    {
        private Form total = new();
        private Form physical = new();
        private Form elemental = new();

        public Form Total { get => total; set => SetProperty(ref total, value); }
        public Form Physical { get => physical; set => SetProperty(ref physical, value); }
        public Form Elemental { get => elemental; set => SetProperty(ref elemental, value); }
    }
    public sealed class TotalClass : BaseViewModel
    {
        private Form resistance = new();
        private Form life = new();
        private Form globalEs = new();

        public Form Resistance { get => resistance; set => SetProperty(ref resistance, value); }
        public Form Life { get => life; set => SetProperty(ref life, value); }
        public Form GlobalEs { get => globalEs; set => SetProperty(ref globalEs, value); }
    }
    public sealed class SanctumClass : BaseViewModel
    {
        private Form resolve = new();
        private Form maximumResolve = new();
        private Form inspiration = new();
        private Form aureus = new();

        public Form Resolve { get => resolve; set => SetProperty(ref resolve, value); }
        public Form MaximumResolve { get => maximumResolve; set => SetProperty(ref maximumResolve, value); }
        public Form Inspiration { get => inspiration; set => SetProperty(ref inspiration, value); }
        public Form Aureus { get => aureus; set => SetProperty(ref aureus, value); }
    }

    public sealed class Form : BaseViewModel
    {
        private string min = string.Empty;
        private string max = string.Empty;
        private bool selected;

        public string Min { get => min; set => SetProperty(ref min, value); }
        public string Max { get => max; set => SetProperty(ref max, value); }
        public bool Selected { get => selected; set => SetProperty(ref selected, value); }
    }

    public sealed class RewardClass : BaseViewModel
    {
        private string text = string.Empty;
        private string tip = string.Empty;
        private string fgColor;

        public string Text { get => text; set => SetProperty(ref text, value); }
        public string Tip { get => tip; set => SetProperty(ref tip, value); }
        public string FgColor { get => fgColor; set => SetProperty(ref fgColor, value); }
    }

    public sealed class Sockets : BaseViewModel
    {
        private string socketMin;
        private string socketMax;
        private string linkMin;
        private string linkMax;
        private string redColor;
        private string greenColor;
        private string blueColor;
        private string whiteColor;
        private bool selected;

        public string SocketMin { get => socketMin; set => SetProperty(ref socketMin, value); }
        public string SocketMax { get => socketMax; set => SetProperty(ref socketMax, value); }
        public string LinkMin { get => linkMin; set => SetProperty(ref linkMin, value); }
        public string LinkMax { get => linkMax; set => SetProperty(ref linkMax, value); }
        public string RedColor { get => redColor; set => SetProperty(ref redColor, value); }
        public string GreenColor { get => greenColor; set => SetProperty(ref greenColor, value); }
        public string BlueColor { get => blueColor; set => SetProperty(ref blueColor, value); }
        public string WhiteColor { get => whiteColor; set => SetProperty(ref whiteColor, value); }
        public bool Selected { get => selected; set => SetProperty(ref selected, value); }
    }

    public sealed class RowPanel : BaseViewModel
    {
        private double armourMaxHeight;
        private double weaponMaxHeight;
        private double totalMaxHeight;

        public double ArmourMaxHeight { get => armourMaxHeight; set => SetProperty(ref armourMaxHeight, value); }
        public double WeaponMaxHeight { get => weaponMaxHeight; set => SetProperty(ref weaponMaxHeight, value); }
        public double TotalMaxHeight { get => totalMaxHeight; set => SetProperty(ref totalMaxHeight, value); }
    }
    public sealed class ColPanel : BaseViewModel
    {
        private double firstMaxWidth = 14;
        private double lastMinWidth = 86;

        public double FirstMaxWidth { get => firstMaxWidth; set => SetProperty(ref firstMaxWidth, value); }
        public double LastMinWidth { get => lastMinWidth; set => SetProperty(ref lastMinWidth, value); }
    }
}

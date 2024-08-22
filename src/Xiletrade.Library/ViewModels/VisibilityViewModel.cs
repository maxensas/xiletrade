namespace Xiletrade.Library.ViewModels;

public sealed class VisibilityViewModel : BaseViewModel
{
    private bool corrupted = true;
    private bool btnPoeDb = true;
    private bool influences = true;
    private bool conditions;
    private bool panelForm = true;
    private bool panelStat = true;
    private bool total;
    private bool damage;
    private bool defense;
    private bool alternateGem;
    private bool quality = true;
    private bool sockets;
    private bool byBase = true;
    private bool rarity = true;
    private bool checkAll = true;
    private bool facetor;
    private bool modSet;
    private bool detail;
    private bool headerMod;
    private bool reward;
    private bool synthesisBlight;
    private bool blightRavaged;
    private bool scourged;
    private bool hiddablePanel;
    private bool armour;
    private bool energy;
    private bool evasion;
    private bool ward;
    private bool poeprices = true;
    private bool wiki;
    private bool bulkLastSearch;
    private bool sanctumFields;
    private bool mapStats;

    public bool Corrupted { get => corrupted; set => SetProperty(ref corrupted, value); }
    public bool BtnPoeDb { get => btnPoeDb; set => SetProperty(ref btnPoeDb, value); }
    public bool Influences { get => influences; set => SetProperty(ref influences, value); }
    public bool Conditions { get => conditions; set => SetProperty(ref conditions, value); }
    public bool PanelForm { get => panelForm; set => SetProperty(ref panelForm, value); }
    public bool PanelStat { get => panelStat; set => SetProperty(ref panelStat, value); }
    public bool Total { get => total; set => SetProperty(ref total, value); }
    public bool Damage { get => damage; set => SetProperty(ref damage, value); }
    public bool Defense { get => defense; set => SetProperty(ref defense, value); }
    public bool AlternateGem { get => alternateGem; set => SetProperty(ref alternateGem, value); }
    public bool Quality { get => quality; set => SetProperty(ref quality, value); }
    public bool Sockets { get => sockets; set => SetProperty(ref sockets, value); }
    public bool ByBase { get => byBase; set => SetProperty(ref byBase, value); }
    public bool Rarity { get => rarity; set => SetProperty(ref rarity, value); }
    public bool CheckAll { get => checkAll; set => SetProperty(ref checkAll, value); }
    public bool Facetor { get => facetor; set => SetProperty(ref facetor, value); }
    public bool ModSet { get => modSet; set => SetProperty(ref modSet, value); }
    public bool Detail { get => detail; set => SetProperty(ref detail, value); }
    public bool HeaderMod { get => headerMod; set => SetProperty(ref headerMod, value); }
    public bool Reward { get => reward; set => SetProperty(ref reward, value); }
    public bool SynthesisBlight { get => synthesisBlight; set => SetProperty(ref synthesisBlight, value); }
    public bool BlightRavaged { get => blightRavaged; set => SetProperty(ref blightRavaged, value); }
    public bool Scourged { get => scourged; set => SetProperty(ref scourged, value); }
    public bool HiddablePanel { get => hiddablePanel; set => SetProperty(ref hiddablePanel, value); }
    public bool Armour { get => armour; set => SetProperty(ref armour, value); }
    public bool Energy { get => energy; set => SetProperty(ref energy, value); }
    public bool Evasion { get => evasion; set => SetProperty(ref evasion, value); }
    public bool Ward { get => ward; set => SetProperty(ref ward, value); }
    public bool Poeprices { get => poeprices; set => SetProperty(ref poeprices, value); }
    public bool Wiki { get => wiki; set => SetProperty(ref wiki, value); }
    public bool BulkLastSearch { get => bulkLastSearch; set => SetProperty(ref bulkLastSearch, value); }
    public bool SanctumFields { get => sanctumFields; set => SetProperty(ref sanctumFields, value); }
    public bool MapStats { get => mapStats; set => SetProperty(ref mapStats, value); }
}

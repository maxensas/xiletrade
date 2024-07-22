namespace Xiletrade.Library.ViewModels;

public sealed class CommonKeysViewModel : BaseViewModel
{
    private HotkeyViewModel priceCheck = new();
    private HotkeyViewModel openBulk = new();
    private HotkeyViewModel openConfig = new();
    private HotkeyViewModel closeWindow = new();
    private HotkeyViewModel openSyndicate = new();
    private HotkeyViewModel openIncursion = new();
    private HotkeyViewModel tcpLogout = new();
    private HotkeyViewModel openWiki = new();
    private HotkeyViewModel openNinja = new();
    private HotkeyViewModel openPoeLab = new();
    private HotkeyViewModel openPoeDb = new();
    private HotkeyViewModel openCustomFirst = new();
    private HotkeyViewModel openCustomSecond = new();

    public HotkeyViewModel PriceCheck { get => priceCheck; set => SetProperty(ref priceCheck, value); }
    public HotkeyViewModel OpenBulk { get => openBulk; set => SetProperty(ref openBulk, value); }
    public HotkeyViewModel OpenConfig { get => openConfig; set => SetProperty(ref openConfig, value); }
    public HotkeyViewModel CloseWindow { get => closeWindow; set => SetProperty(ref closeWindow, value); }
    public HotkeyViewModel OpenSyndicate { get => openSyndicate; set => SetProperty(ref openSyndicate, value); }
    public HotkeyViewModel OpenIncursion { get => openIncursion; set => SetProperty(ref openIncursion, value); }
    public HotkeyViewModel TcpLogout { get => tcpLogout; set => SetProperty(ref tcpLogout, value); }
    public HotkeyViewModel OpenWiki { get => openWiki; set => SetProperty(ref openWiki, value); }
    public HotkeyViewModel OpenNinja { get => openNinja; set => SetProperty(ref openNinja, value); }
    public HotkeyViewModel OpenPoeLab { get => openPoeLab; set => SetProperty(ref openPoeLab, value); }
    public HotkeyViewModel OpenPoeDb { get => openPoeDb; set => SetProperty(ref openPoeDb, value); }
    public HotkeyViewModel OpenCustomFirst { get => openCustomFirst; set => SetProperty(ref openCustomFirst, value); }
    public HotkeyViewModel OpenCustomSecond { get => openCustomSecond; set => SetProperty(ref openCustomSecond, value); }

    public CommonKeysViewModel()
    {

    }
}

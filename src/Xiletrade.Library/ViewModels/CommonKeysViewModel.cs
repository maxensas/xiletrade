using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public sealed partial class CommonKeysViewModel : ViewModelBase
{
    [ObservableProperty]
    private HotkeyViewModel priceCheck = new();

    [ObservableProperty]
    private HotkeyViewModel openBulk = new();

    [ObservableProperty]
    private HotkeyViewModel openConfig = new();

    [ObservableProperty]
    private HotkeyViewModel closeWindow = new();

    [ObservableProperty]
    private HotkeyViewModel openSyndicate = new();

    [ObservableProperty]
    private HotkeyViewModel openIncursion = new();

    [ObservableProperty]
    private HotkeyViewModel tcpLogout = new();

    [ObservableProperty]
    private HotkeyViewModel openWiki = new();

    [ObservableProperty]
    private HotkeyViewModel openNinja = new();

    [ObservableProperty]
    private HotkeyViewModel openPoeLab = new();

    [ObservableProperty]
    private HotkeyViewModel openPoeDb = new();

    [ObservableProperty]
    private HotkeyViewModel openCustomFirst = new();

    [ObservableProperty]
    private HotkeyViewModel openCustomSecond = new();

    [ObservableProperty]
    private HotkeyViewModel openRegexManager = new();
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels;

public sealed partial class AdditionalKeysViewModel : ViewModelBase
{
    [ObservableProperty]
    private HotkeyViewModel chatKey = new();

    [ObservableProperty]
    private HotkeyViewModel hideout = new();

    [ObservableProperty]
    private HotkeyViewModel charSelection = new();

    [ObservableProperty]
    private HotkeyViewModel pasteWhisper = new();

    [ObservableProperty]
    private HotkeyViewModel chatCommandFirst = new();

    [ObservableProperty]
    private HotkeyViewModel chatCommandSecond = new();

    [ObservableProperty]
    private HotkeyViewModel chatCommandThird = new();

    [ObservableProperty]
    private HotkeyViewModel inviteLast = new();

    [ObservableProperty]
    private HotkeyViewModel tradeLast = new();

    [ObservableProperty]
    private HotkeyViewModel whoisLast = new();

    [ObservableProperty]
    private HotkeyViewModel replyLast = new();

    [ObservableProperty]
    private HotkeyViewModel tradeChan = new();

    [ObservableProperty]
    private HotkeyViewModel globalChan = new();

    [ObservableProperty]
    private HotkeyViewModel setAfk = new();

    [ObservableProperty]
    private HotkeyViewModel setAutoReply = new();

    [ObservableProperty]
    private HotkeyViewModel setDnd = new();

    [ObservableProperty]
    private HotkeyViewModel partyInvite = new();

    [ObservableProperty]
    private HotkeyViewModel partyKick = new();

    [ObservableProperty]
    private HotkeyViewModel partyLeave = new();
}

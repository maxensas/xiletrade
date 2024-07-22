namespace Xiletrade.Library.ViewModels;

public sealed class AdditionalKeysViewModel : BaseViewModel
{
    private HotkeyViewModel chatKey = new();
    private HotkeyViewModel hideout = new();
    private HotkeyViewModel charSelection = new();
    private HotkeyViewModel pasteWhisper = new();
    private HotkeyViewModel chatCommandFirst = new();
    private HotkeyViewModel chatCommandSecond = new();
    private HotkeyViewModel chatCommandThird = new();
    private HotkeyViewModel inviteLast = new();
    private HotkeyViewModel tradeLast = new();
    private HotkeyViewModel whoisLast = new();
    private HotkeyViewModel replyLast = new();
    private HotkeyViewModel tradeChan = new();
    private HotkeyViewModel globalChan = new();
    private HotkeyViewModel setAfk = new();
    private HotkeyViewModel setAutoReply = new();
    private HotkeyViewModel setDnd = new();
    private HotkeyViewModel partyInvite = new();
    private HotkeyViewModel partyKick = new();
    private HotkeyViewModel partyLeave = new();

    public HotkeyViewModel ChatKey { get => chatKey; set => SetProperty(ref chatKey, value); }
    public HotkeyViewModel Hideout { get => hideout; set => SetProperty(ref hideout, value); }
    public HotkeyViewModel CharSelection { get => charSelection; set => SetProperty(ref charSelection, value); }
    public HotkeyViewModel PasteWhisper { get => pasteWhisper; set => SetProperty(ref pasteWhisper, value); }
    public HotkeyViewModel ChatCommandFirst { get => chatCommandFirst; set => SetProperty(ref chatCommandFirst, value); }
    public HotkeyViewModel ChatCommandSecond { get => chatCommandSecond; set => SetProperty(ref chatCommandSecond, value); }
    public HotkeyViewModel ChatCommandThird { get => chatCommandThird; set => SetProperty(ref chatCommandThird, value); }
    public HotkeyViewModel InviteLast { get => inviteLast; set => SetProperty(ref inviteLast, value); }
    public HotkeyViewModel TradeLast { get => tradeLast; set => SetProperty(ref tradeLast, value); }
    public HotkeyViewModel WhoisLast { get => whoisLast; set => SetProperty(ref whoisLast, value); }
    public HotkeyViewModel ReplyLast { get => replyLast; set => SetProperty(ref replyLast, value); }
    public HotkeyViewModel TradeChan { get => tradeChan; set => SetProperty(ref tradeChan, value); }
    public HotkeyViewModel GlobalChan { get => globalChan; set => SetProperty(ref globalChan, value); }
    public HotkeyViewModel SetAfk { get => setAfk; set => SetProperty(ref setAfk, value); }
    public HotkeyViewModel SetAutoReply { get => setAutoReply; set => SetProperty(ref setAutoReply, value); }
    public HotkeyViewModel SetDnd { get => setDnd; set => SetProperty(ref setDnd, value); }
    public HotkeyViewModel PartyInvite { get => partyInvite; set => SetProperty(ref partyInvite, value); }
    public HotkeyViewModel PartyKick { get => partyKick; set => SetProperty(ref partyKick, value); }
    public HotkeyViewModel PartyLeave { get => partyLeave; set => SetProperty(ref partyLeave, value); }

    public AdditionalKeysViewModel()
    {

    }
}

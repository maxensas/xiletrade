using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class AdditionalKeysViewModel(IServiceProvider sp) : ViewModelBase
{
    [ObservableProperty]
    private HotkeyViewModel chatKey = new(sp, Resources.Resources.Config105_lbChatKey, Resources.Resources.Config118_lbChatKeyTip, useCb: false);

    [ObservableProperty]
    private HotkeyViewModel hideout = new(sp, Resources.Resources.Config106_lbHideout, Resources.Resources.Config119_lbHideoutTip);

    [ObservableProperty]
    private HotkeyViewModel charSelection = new(sp, Resources.Resources.Config109_lbExit, Resources.Resources.Config122_lbExitTip);

    [ObservableProperty]
    private HotkeyViewModel pasteWhisper = new(sp, Resources.Resources.Config108_lbPaste, Resources.Resources.Config121_lbPasteTip);

    [ObservableProperty]
    private HotkeyViewModel chatCommandFirst = new(sp, string.Empty, string.Empty, initList: true);

    [ObservableProperty]
    private HotkeyViewModel chatCommandSecond = new(sp, string.Empty, string.Empty, initList: true);

    [ObservableProperty]
    private HotkeyViewModel chatCommandThird = new(sp, string.Empty, string.Empty, initList: true);

    [ObservableProperty]
    private HotkeyViewModel inviteLast = new(sp, "Invite", Resources.Resources.Config129_lbWhisperInviteTip);

    [ObservableProperty]
    private HotkeyViewModel tradeLast = new(sp, "Trade", Resources.Resources.Config130_lbWhisperTradeTip);

    [ObservableProperty]
    private HotkeyViewModel whoisLast = new(sp, "Whois", Resources.Resources.Config131_lbWhisperWhoisTip);

    [ObservableProperty]
    private HotkeyViewModel replyLast = new(sp, "Reply : ", Resources.Resources.Config132_lbWhisperReplyTip);

    [ObservableProperty]
    private HotkeyViewModel tradeChan = new(sp, Resources.Resources.Config112_lbJoinTrade, Resources.Resources.Config124_lbJoinTradeTip);

    [ObservableProperty]
    private HotkeyViewModel globalChan = new(sp, Resources.Resources.Config113_lbJoinGlobal, Resources.Resources.Config125_lbJoinGlobalTip);

    [ObservableProperty]
    private HotkeyViewModel setAfk = new(sp, Resources.Resources.Config114_lbAfk, Resources.Resources.Config126_lbAfkTip);

    [ObservableProperty]
    private HotkeyViewModel setAutoReply = new(sp, Resources.Resources.Config115_lbAutoReply, Resources.Resources.Config127_lbAutoReplyTip);

    [ObservableProperty]
    private HotkeyViewModel setDnd = new(sp, Resources.Resources.Config116_lbDnd, Resources.Resources.Config128_lbDndTip);

    [ObservableProperty]
    private HotkeyViewModel partyInvite = new(sp, "Invite", Resources.Resources.Config133_lbGroupInviteTip);

    [ObservableProperty]
    private HotkeyViewModel partyKick = new(sp, "Kick", Resources.Resources.Config134_lbGroupKickTip);

    [ObservableProperty]
    private HotkeyViewModel partyLeave = new(sp, "Leave", Resources.Resources.Config135_lbGroupLeaveTip);

    internal IEnumerable<HotkeyViewModel> GetListHotkey()
    {
        return [ChatKey, Hideout
            , CharSelection, PasteWhisper, ChatCommandFirst
            , ChatCommandSecond, ChatCommandThird, InviteLast, TradeLast, WhoisLast
            , ReplyLast, TradeChan, GlobalChan, SetAfk, SetAutoReply, SetDnd
            , PartyInvite, PartyKick, PartyLeave];
    }
}

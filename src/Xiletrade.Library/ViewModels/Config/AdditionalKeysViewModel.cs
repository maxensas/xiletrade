using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class AdditionalKeysViewModel : ViewModelBase
{
    [ObservableProperty]
    private HotkeyViewModel chatKey;

    [ObservableProperty]
    private HotkeyViewModel hideout;

    [ObservableProperty]
    private HotkeyViewModel charSelection;

    [ObservableProperty]
    private HotkeyViewModel pasteWhisper;

    [ObservableProperty]
    private HotkeyViewModel chatCommandFirst;

    [ObservableProperty]
    private HotkeyViewModel chatCommandSecond;

    [ObservableProperty]
    private HotkeyViewModel chatCommandThird;

    [ObservableProperty]
    private HotkeyViewModel inviteLast;

    [ObservableProperty]
    private HotkeyViewModel tradeLast;

    [ObservableProperty]
    private HotkeyViewModel whoisLast;

    [ObservableProperty]
    private HotkeyViewModel replyLast;

    [ObservableProperty]
    private HotkeyViewModel tradeChan;

    [ObservableProperty]
    private HotkeyViewModel globalChan;

    [ObservableProperty]
    private HotkeyViewModel setAfk;

    [ObservableProperty]
    private HotkeyViewModel setAutoReply;

    [ObservableProperty]
    private HotkeyViewModel setDnd;

    [ObservableProperty]
    private HotkeyViewModel partyInvite;

    [ObservableProperty]
    private HotkeyViewModel partyKick;

    [ObservableProperty]
    private HotkeyViewModel partyLeave;

    internal AdditionalKeysViewModel(IKeyboardAdapterService key, IMessageAdapterService message, ConfigViewModel vm)
    {
        chatKey = new(key, message, vm, Resources.Resources.Config105_lbChatKey, Resources.Resources.Config118_lbChatKeyTip, useCb: false);
        hideout = new(key, message, vm, Resources.Resources.Config106_lbHideout, Resources.Resources.Config119_lbHideoutTip);
        charSelection = new(key, message, vm, Resources.Resources.Config109_lbExit, Resources.Resources.Config122_lbExitTip);
        pasteWhisper = new(key, message, vm, Resources.Resources.Config108_lbPaste, Resources.Resources.Config121_lbPasteTip);
        chatCommandFirst = new(key, message, vm, string.Empty, string.Empty, initList: true);
        chatCommandSecond = new(key, message, vm, string.Empty, string.Empty, initList: true);
        chatCommandThird = new(key, message, vm, string.Empty, string.Empty, initList: true);
        inviteLast = new(key, message, vm, "Invite", Resources.Resources.Config129_lbWhisperInviteTip);
        tradeLast = new(key, message, vm, "Trade", Resources.Resources.Config130_lbWhisperTradeTip);
        whoisLast = new(key, message, vm, "Whois", Resources.Resources.Config131_lbWhisperWhoisTip);
        replyLast = new(key, message, vm, "Reply : ", Resources.Resources.Config132_lbWhisperReplyTip);
        tradeChan = new(key, message, vm, Resources.Resources.Config112_lbJoinTrade, Resources.Resources.Config124_lbJoinTradeTip);
        globalChan = new(key, message, vm, Resources.Resources.Config113_lbJoinGlobal, Resources.Resources.Config125_lbJoinGlobalTip);
        setAfk = new(key, message, vm, Resources.Resources.Config114_lbAfk, Resources.Resources.Config126_lbAfkTip);
        setAutoReply = new(key, message, vm, Resources.Resources.Config115_lbAutoReply, Resources.Resources.Config127_lbAutoReplyTip);
        setDnd = new(key, message, vm, Resources.Resources.Config116_lbDnd, Resources.Resources.Config128_lbDndTip);
        partyInvite = new(key, message, vm, "Invite", Resources.Resources.Config133_lbGroupInviteTip);
        partyKick = new(key, message, vm, "Kick", Resources.Resources.Config134_lbGroupKickTip);
        partyLeave = new(key, message, vm, "Leave", Resources.Resources.Config135_lbGroupLeaveTip);

        for (int i = 0; i < vm.Config.ChatCommands.Length; i++)
        {
            var cmd = vm.Config.ChatCommands[i]?.Command;
            if (vm.Config.ChatCommands[i] is null || cmd.Length is 0)
            {
                continue;
            }
            cmd = "/" + cmd;
            chatCommandFirst.List.Add(cmd);
            chatCommandSecond.List.Add(cmd);
            chatCommandThird.List.Add(cmd);
        }
    }

    internal IEnumerable<HotkeyViewModel> GetListHotkey()
    {
        return [ChatKey, Hideout
            , CharSelection, PasteWhisper, ChatCommandFirst
            , ChatCommandSecond, ChatCommandThird, InviteLast, TradeLast, WhoisLast
            , ReplyLast, TradeChan, GlobalChan, SetAfk, SetAutoReply, SetDnd
            , PartyInvite, PartyKick, PartyLeave];
    }
}

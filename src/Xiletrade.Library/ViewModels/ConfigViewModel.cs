using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels;

public sealed partial class ConfigViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private GeneralViewModel general = new();

    [ObservableProperty]
    private CommonKeysViewModel commonKeys = new();

    [ObservableProperty]
    private AdditionalKeysViewModel additionalKeys = new();

    public ConfigCommand Commands { get; private set; }

    // members
    public ConfigData Config { get; set; }
    public string ConfigBackup { get; set; }

    public ConfigViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Commands = new(this, _serviceProvider);

        ConfigBackup = DataManager.Load_Config(Strings.File.Config); //parentWindow
        Config = Json.Deserialize<ConfigData>(ConfigBackup);

        Initialize();
    }

    public void Initialize()
    {
        InitLeagueList();

        General.Language = new()
        {
            "English",
            "한국어",
            "Français",
            "Castellano",
            "Deutsch",
            "Português",
            "Русский",
            "ภาษาไทย",
            "正體中文",
            "简体中文",
            "日本語"
        };
        General.LanguageIndex = Config.Options.Language;

        General.SearchDayLimit = new()
        {
            "0", "1", "3", "7", "14"
        };
        int dayLimitIdx = General.SearchDayLimit.IndexOf(Config.Options.SearchBeforeDay.ToString());
        General.SearchDayLimitIndex = dayLimitIdx == -1 ? 0 : dayLimitIdx;

        General.MaxFetch = new()
        {
            "10", "20", "30", "40", "50", "60", "70", "80"
        };
        int maxFetchIdx = General.MaxFetch.IndexOf(((int)Config.Options.SearchFetchDetail).ToString());
        General.MaxFetchIndex = maxFetchIdx == -1 ? 1 : maxFetchIdx;

        General.MaxWaitRequest = new()
        {
            "5", "10", "15", "30", "60", "120"
        };
        int timeoutIdx = General.MaxWaitRequest.IndexOf(Config.Options.TimeoutTradeApi.ToString());
        General.MaxWaitRequestIndex = timeoutIdx == -1 ? 1 : timeoutIdx;

        General.BtnUpdateText = Resources.Resources.Config010_btnUpdate;
        General.BtnUpdateEnable = true;

        General.StartupMessage = Config.Options.DisableStartupMessage;
        General.RegroupResults = Config.Options.HideSameOccurs;
        General.CheckCorrupted = Config.Options.AutoSelectCorrupt;
        General.CheckPseudoAffix = Config.Options.AutoSelectPseudo;
        General.ByBaseType = Config.Options.SearchByType;
        //checkStashWheel.IsChecked = Config.Options.CtrlWheel;
        General.AutoUpdate = Config.Options.CheckUpdates;
        General.AutoFilter = Config.Options.CheckFilters;

        General.CheckTotalLife = Config.Options.AutoSelectLife;
        General.CheckGlobalEs = Config.Options.AutoSelectGlobalEs;
        General.CheckTotalResists = Config.Options.AutoSelectRes;
        General.CheckTotalArmourStats = Config.Options.AutoSelectArEsEva;
        General.CheckTotalDps = Config.Options.AutoSelectDps;
        General.CheckMinTier = Config.Options.AutoSelectMinTierValue;

        //checkMods.IsChecked = mConfig.Options.AutoCheckMods;
        General.CheckExplicitsUniques = Config.Options.AutoCheckUniques;
        General.CheckExplicitsNonUniques = Config.Options.AutoCheckNonUniques;
        General.CheckImplicits = Config.Options.AutoCheckImplicits;
        General.CheckEnchants = Config.Options.AutoCheckEnchants;
        General.CheckCrafted = Config.Options.AutoCheckCrafted;
        General.CheckCorruptions = Config.Options.AutoCheckCorruptions;
        General.DevMode = Config.Options.DevMode;
        General.AutoWhisper = Config.Options.Autopaste;
        General.CtrlWheel = Config.Options.CtrlWheel;

        AdditionalKeys.ChatCommandFirst.List = new();
        AdditionalKeys.ChatCommandSecond.List = new();
        AdditionalKeys.ChatCommandThird.List = new();

        for (int i = 0; i < Config.ChatCommands.Length; i++ ) 
        {
            var cmd = Config.ChatCommands[i]?.Command;
            if (Config.ChatCommands[i] is null || cmd.Length is 0)
            {
                continue;
            }
            cmd = "/" + cmd;
            AdditionalKeys.ChatCommandFirst.List.Add(cmd);
            AdditionalKeys.ChatCommandSecond.List.Add(cmd);
            AdditionalKeys.ChatCommandThird.List.Add(cmd);
        }

        //System.Windows.Forms.KeysConverter kc = new();
        var kc = _serviceProvider.GetRequiredService<System.ComponentModel.TypeConverter>();
        foreach (var item in Config.Shortcuts)
        {
            if (item.Keycode > 0 && item.Value?.Length > 0)
            {
                switch (item.Fonction)
                {
                    case Strings.Feature.run:
                        CommonKeys.PriceCheck.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.PriceCheck.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.bulk:
                        CommonKeys.OpenBulk.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenBulk.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.config:
                        CommonKeys.OpenConfig.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenConfig.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.close:
                        CommonKeys.CloseWindow.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.CloseWindow.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.syndicate:
                        CommonKeys.OpenSyndicate.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenSyndicate.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.incursion:
                        CommonKeys.OpenIncursion.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenIncursion.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.tcp:
                        CommonKeys.TcpLogout.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.TcpLogout.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.wiki:
                        CommonKeys.OpenWiki.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenWiki.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.ninja:
                        CommonKeys.OpenNinja.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenNinja.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.lab:
                        CommonKeys.OpenPoeLab.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenPoeLab.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.poedb:
                        CommonKeys.OpenPoeDb.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenPoeDb.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.link1:
                        CommonKeys.OpenCustomFirst.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenCustomFirst.Val = item.Value;
                        CommonKeys.OpenCustomFirst.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.link2:
                        CommonKeys.OpenCustomSecond.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        CommonKeys.OpenCustomSecond.Val = item.Value;
                        CommonKeys.OpenCustomSecond.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.chatkey:
                        AdditionalKeys.ChatKey.Hotkey = kc.ConvertToString(item.Keycode);
                        //modPoeDb.SelectedIndex = GetModIndex(item.Modifier);
                        //cbPoeDb.IsChecked = modPoeDb.IsEnabled = tbPoeDb.IsEnabled = item.Enable;
                        break;
                    case Strings.Feature.hideout:
                        AdditionalKeys.Hideout.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.Hideout.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.whispertrade:
                        AdditionalKeys.PasteWhisper.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.PasteWhisper.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.exitchar:
                        AdditionalKeys.CharSelection.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.CharSelection.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.invlast:
                        AdditionalKeys.InviteLast.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.InviteLast.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.replylast:
                        AdditionalKeys.ReplyLast.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.ReplyLast.Val = item.Value;
                        AdditionalKeys.ReplyLast.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.tradelast:
                        AdditionalKeys.TradeLast.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.TradeLast.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.whoislast:
                        AdditionalKeys.WhoisLast.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.WhoisLast.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.tradechan:
                        AdditionalKeys.TradeChan.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.TradeChan.Val = item.Value;
                        AdditionalKeys.TradeChan.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.globalchan:
                        AdditionalKeys.GlobalChan.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.GlobalChan.Val = item.Value;
                        AdditionalKeys.GlobalChan.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.invite:
                        AdditionalKeys.PartyInvite.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.PartyInvite.Val = item.Value;
                        AdditionalKeys.PartyInvite.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.kick:
                        AdditionalKeys.PartyKick.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.PartyKick.Val = item.Value;
                        AdditionalKeys.PartyKick.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.leave:
                        AdditionalKeys.PartyLeave.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.PartyLeave.Val = item.Value;
                        AdditionalKeys.PartyLeave.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.afk:
                        AdditionalKeys.SetAfk.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.SetAfk.Val = item.Value;
                        AdditionalKeys.SetAfk.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.autoreply:
                        AdditionalKeys.SetAutoReply.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.SetAutoReply.Val = item.Value;
                        AdditionalKeys.SetAutoReply.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.dnd:
                        AdditionalKeys.SetDnd.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.SetDnd.Val = item.Value;
                        AdditionalKeys.SetDnd.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.chat1:
                        AdditionalKeys.ChatCommandFirst.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.ChatCommandFirst.ListIndex = int.Parse(item.Value, CultureInfo.InvariantCulture);
                        AdditionalKeys.ChatCommandFirst.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.chat2:
                        AdditionalKeys.ChatCommandSecond.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.ChatCommandSecond.ListIndex = int.Parse(item.Value, CultureInfo.InvariantCulture);
                        AdditionalKeys.ChatCommandSecond.IsEnable = item.Enable;
                        break;
                    case Strings.Feature.chat3:
                        AdditionalKeys.ChatCommandThird.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                        AdditionalKeys.ChatCommandThird.ListIndex = int.Parse(item.Value, CultureInfo.InvariantCulture);
                        AdditionalKeys.ChatCommandThird.IsEnable = item.Enable;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void InitLeagueList()
    {
        if (DataManager.League.Result.Length >= 2)
        {
            General.League.Clear();
            foreach (LeagueResult res in DataManager.League.Result)
            {
                General.League.Add(res.Id);
            }
        }
        int leagueIdx = General.League.IndexOf(DataManager.Config.Options.League);
        General.LeagueIndex = leagueIdx == -1 ? 0 : leagueIdx;
    }

    public void SaveConfigForm()
    {
        Config.Options.Language = General.LanguageIndex;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(Strings.Culture[Config.Options.Language]);

        Config.Options.League = General.League[General.LeagueIndex];

        Config.Options.SearchBeforeDay = int.Parse(General.SearchDayLimit[General.SearchDayLimitIndex], CultureInfo.InvariantCulture);

        Config.Options.SearchFetchDetail = decimal.Parse(General.MaxFetch[General.MaxFetchIndex], CultureInfo.InvariantCulture);
        //Config.Options.SearchFetchBulk = decimal.Parse(cbFetchBulk.SelectedValue.ToString(), CultureInfo.InvariantCulture);
        //Config.Options.UniqueMinValuePercent = double.Parse(cbMinUniques.SelectedValue.ToString(), CultureInfo.InvariantCulture);
        //Config.Options.UniqueMaxValuePercent = double.Parse(cbMaxUniques.SelectedValue.ToString(), CultureInfo.InvariantCulture);
        //Config.Options.MinValuePercent = double.Parse(cbMinOthers.SelectedValue.ToString(), CultureInfo.InvariantCulture);
        //Config.Options.MaxValuePercent = double.Parse(cbMaxOthers.SelectedValue.ToString(), CultureInfo.InvariantCulture);

        Config.Options.TimeoutTradeApi = int.Parse(General.MaxWaitRequest[General.MaxWaitRequestIndex], CultureInfo.InvariantCulture);

        Config.Options.DisableStartupMessage = General.StartupMessage;
        Config.Options.HideSameOccurs = General.RegroupResults;
        Config.Options.AutoSelectCorrupt = General.CheckCorrupted;
        Config.Options.AutoSelectPseudo = General.CheckPseudoAffix;
        Config.Options.SearchByType = General.ByBaseType;
        //Config.Options.CtrlWheel = bool.Parse(checkStashWheel.IsChecked.ToString());
        Config.Options.CheckUpdates = General.AutoUpdate;
        Config.Options.CheckFilters = General.AutoFilter;


        Config.Options.AutoSelectLife = General.CheckTotalLife;
        Config.Options.AutoSelectGlobalEs = General.CheckGlobalEs;
        Config.Options.AutoSelectRes = General.CheckTotalResists;
        Config.Options.AutoSelectArEsEva = General.CheckTotalArmourStats;
        Config.Options.AutoSelectDps = General.CheckTotalDps;
        Config.Options.AutoSelectMinTierValue = General.CheckMinTier;

        Config.Options.AutoCheckUniques = General.CheckExplicitsUniques;
        Config.Options.AutoCheckNonUniques = General.CheckExplicitsNonUniques;
        Config.Options.AutoCheckImplicits = General.CheckImplicits;
        Config.Options.AutoCheckEnchants = General.CheckEnchants;
        Config.Options.AutoCheckCrafted = General.CheckCrafted;
        Config.Options.AutoCheckCorruptions = General.CheckCorruptions;
        Config.Options.DevMode = General.DevMode;
        Config.Options.Autopaste = General.AutoWhisper;
        Config.Options.CtrlWheel = General.CtrlWheel;

        foreach (var item in Config.Shortcuts)
        {
            if (item.Keycode > 0 && item.Value?.Length > 0)
            {
                switch (item.Fonction)
                {
                    case Strings.Feature.run:
                        item.Modifier = GetModCode(CommonKeys.PriceCheck.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.PriceCheck, item.Keycode);
                        item.Enable = CommonKeys.PriceCheck.IsEnable;
                        break;
                    case Strings.Feature.bulk:
                        item.Modifier = GetModCode(CommonKeys.OpenBulk.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenBulk, item.Keycode);
                        item.Enable = CommonKeys.OpenBulk.IsEnable;
                        break;
                    case Strings.Feature.config:
                        item.Modifier = GetModCode(CommonKeys.OpenConfig.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenConfig, item.Keycode);
                        item.Enable = CommonKeys.OpenConfig.IsEnable;
                        break;
                    case Strings.Feature.close:
                        item.Modifier = GetModCode(CommonKeys.CloseWindow.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.CloseWindow, item.Keycode);
                        item.Enable = CommonKeys.CloseWindow.IsEnable;
                        break;
                    case Strings.Feature.syndicate:
                        item.Modifier = GetModCode(CommonKeys.OpenSyndicate.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenSyndicate, item.Keycode);
                        item.Enable = CommonKeys.OpenSyndicate.IsEnable;
                        break;
                    case Strings.Feature.incursion:
                        item.Modifier = GetModCode(CommonKeys.OpenIncursion.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenIncursion, item.Keycode);
                        item.Enable = CommonKeys.OpenIncursion.IsEnable;
                        break;
                    case Strings.Feature.tcp:
                        item.Modifier = GetModCode(CommonKeys.TcpLogout.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.TcpLogout, item.Keycode);
                        item.Enable = CommonKeys.TcpLogout.IsEnable;
                        break;
                    case Strings.Feature.wiki:
                        item.Modifier = GetModCode(CommonKeys.OpenWiki.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenWiki, item.Keycode);
                        item.Enable = CommonKeys.OpenWiki.IsEnable;
                        break;
                    case Strings.Feature.ninja:
                        item.Modifier = GetModCode(CommonKeys.OpenNinja.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenNinja, item.Keycode);
                        item.Enable = CommonKeys.OpenNinja.IsEnable;
                        break;
                    case Strings.Feature.lab:
                        item.Modifier = GetModCode(CommonKeys.OpenPoeLab.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenPoeLab, item.Keycode);
                        item.Enable = CommonKeys.OpenPoeLab.IsEnable;
                        break;
                    case Strings.Feature.poedb:
                        item.Modifier = GetModCode(CommonKeys.OpenPoeDb.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenPoeDb, item.Keycode);
                        item.Enable = CommonKeys.OpenPoeDb.IsEnable;
                        break;
                    case Strings.Feature.link1:
                        item.Modifier = GetModCode(CommonKeys.OpenCustomFirst.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenCustomFirst, item.Keycode);
                        item.Value = CommonKeys.OpenCustomFirst.Val;
                        item.Enable = CommonKeys.OpenCustomFirst.IsEnable;
                        break;
                    case Strings.Feature.link2:
                        item.Modifier = GetModCode(CommonKeys.OpenCustomSecond.Hotkey);
                        item.Keycode = VerifyKeycode(CommonKeys.OpenCustomSecond, item.Keycode);
                        item.Value = CommonKeys.OpenCustomSecond.Val;
                        item.Enable = CommonKeys.OpenCustomSecond.IsEnable;
                        break;
                    case Strings.Feature.chatkey:
                        //item.Modifier = GetModifier(modChatKey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.ChatKey, item.Keycode);
                        //item.Enable = Boolean.Parse(cbChatKey.IsChecked.ToString());
                        break;
                    case Strings.Feature.hideout:
                        item.Modifier = GetModCode(AdditionalKeys.Hideout.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.Hideout, item.Keycode);
                        item.Enable = AdditionalKeys.Hideout.IsEnable;
                        break;
                    case Strings.Feature.whispertrade:
                        item.Modifier = GetModCode(AdditionalKeys.PasteWhisper.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.PasteWhisper, item.Keycode);
                        item.Enable = AdditionalKeys.PasteWhisper.IsEnable;
                        break;
                    case Strings.Feature.exitchar:
                        item.Modifier = GetModCode(AdditionalKeys.CharSelection.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.CharSelection, item.Keycode);
                        item.Enable = AdditionalKeys.CharSelection.IsEnable;
                        break;
                    case Strings.Feature.invlast:
                        item.Modifier = GetModCode(AdditionalKeys.InviteLast.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.InviteLast, item.Keycode);
                        item.Enable = AdditionalKeys.InviteLast.IsEnable;
                        break;
                    case Strings.Feature.replylast:
                        item.Modifier = GetModCode(AdditionalKeys.ReplyLast.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.ReplyLast, item.Keycode);
                        item.Enable = AdditionalKeys.ReplyLast.IsEnable;
                        item.Value = AdditionalKeys.ReplyLast.Val;
                        break;
                    case Strings.Feature.tradelast:
                        item.Modifier = GetModCode(AdditionalKeys.TradeLast.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.TradeLast, item.Keycode);
                        item.Enable = AdditionalKeys.TradeLast.IsEnable;
                        break;
                    case Strings.Feature.whoislast:
                        item.Modifier = GetModCode(AdditionalKeys.WhoisLast.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.WhoisLast, item.Keycode);
                        item.Enable = AdditionalKeys.WhoisLast.IsEnable;
                        break;
                    case Strings.Feature.tradechan:
                        item.Modifier = GetModCode(AdditionalKeys.TradeChan.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.TradeChan, item.Keycode);
                        item.Value = AdditionalKeys.TradeChan.Val;
                        item.Enable = AdditionalKeys.TradeChan.IsEnable;
                        break;
                    case Strings.Feature.globalchan:
                        item.Modifier = GetModCode(AdditionalKeys.GlobalChan.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.GlobalChan, item.Keycode);
                        item.Value = AdditionalKeys.GlobalChan.Val;
                        item.Enable = AdditionalKeys.GlobalChan.IsEnable;
                        break;
                    case Strings.Feature.invite:
                        item.Modifier = GetModCode(AdditionalKeys.PartyInvite.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.PartyInvite, item.Keycode);
                        item.Value = AdditionalKeys.PartyInvite.Val;
                        item.Enable = AdditionalKeys.PartyInvite.IsEnable;
                        break;
                    case Strings.Feature.kick:
                        item.Modifier = GetModCode(AdditionalKeys.PartyKick.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.PartyKick, item.Keycode);
                        item.Value = AdditionalKeys.PartyKick.Val;
                        item.Enable = AdditionalKeys.PartyKick.IsEnable;
                        break;
                    case Strings.Feature.leave:
                        item.Modifier = GetModCode(AdditionalKeys.PartyLeave.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.PartyLeave, item.Keycode);
                        item.Value = AdditionalKeys.PartyLeave.Val;
                        item.Enable = AdditionalKeys.PartyLeave.IsEnable;
                        break;
                    case Strings.Feature.afk:
                        item.Modifier = GetModCode(AdditionalKeys.SetAfk.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.SetAfk, item.Keycode);
                        item.Value = AdditionalKeys.SetAfk.Val;
                        item.Enable = AdditionalKeys.SetAfk.IsEnable;
                        break;
                    case Strings.Feature.autoreply:
                        item.Modifier = GetModCode(AdditionalKeys.SetAutoReply.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.SetAutoReply, item.Keycode);
                        item.Value = AdditionalKeys.SetAutoReply.Val;
                        item.Enable = AdditionalKeys.SetAutoReply.IsEnable;
                        break;
                    case Strings.Feature.dnd:
                        item.Modifier = GetModCode(AdditionalKeys.SetDnd.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.SetDnd, item.Keycode);
                        item.Value = AdditionalKeys.SetDnd.Val;
                        item.Enable = AdditionalKeys.SetDnd.IsEnable;
                        break;
                    case Strings.Feature.chat1:
                        item.Modifier = GetModCode(AdditionalKeys.ChatCommandFirst.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.ChatCommandFirst, item.Keycode);
                        item.Enable = AdditionalKeys.ChatCommandFirst.IsEnable;
                        item.Value = AdditionalKeys.ChatCommandFirst.ListIndex.ToString();
                        break;
                    case Strings.Feature.chat2:
                        item.Modifier = GetModCode(AdditionalKeys.ChatCommandSecond.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.ChatCommandSecond, item.Keycode);
                        item.Enable = AdditionalKeys.ChatCommandSecond.IsEnable;
                        item.Value = AdditionalKeys.ChatCommandSecond.ListIndex.ToString();
                        break;
                    case Strings.Feature.chat3:
                        item.Modifier = GetModCode(AdditionalKeys.ChatCommandThird.Hotkey);
                        item.Keycode = VerifyKeycode(AdditionalKeys.ChatCommandThird, item.Keycode);
                        item.Enable = AdditionalKeys.ChatCommandThird.IsEnable;
                        item.Value = AdditionalKeys.ChatCommandThird.ListIndex.ToString();
                        break;
                    default:
                        break;
                }
            }
        }

        string configToSave = Json.Serialize<ConfigData>(Config);

        HotKey.RemoveRegisterHotKey(true);
        DataManager.Save_Config(configToSave, "cfg"); // parentWindow
        HotKey.InstallRegisterHotKey();
    }

    private static int GetModCode(string modifier) => _serviceProvider.GetRequiredService<INavigationService>().GetModifierCode(modifier);

    private static string GetModText(int modifier) => _serviceProvider.GetRequiredService<INavigationService>().GetModifierText(modifier);

    private static int VerifyKeycode(HotkeyViewModel hotkey, int keycode)
    {
        int returnKey;
        //System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(System.Windows.Forms.Keys));
        //System.Windows.Forms.KeysConverter kc = new();
        var kc = _serviceProvider.GetRequiredService<System.ComponentModel.TypeConverter>();
        string modRet = string.Empty;
        try
        {
            string key;
            if (hotkey.Hotkey.Contains('+', StringComparison.Ordinal))
            {
                modRet = hotkey.Hotkey.Substring(0, hotkey.Hotkey.LastIndexOf("+", StringComparison.Ordinal) + 1);
                key = hotkey.Hotkey[(hotkey.Hotkey.LastIndexOf('+') + 1)..]; // box.Text.Substring(box.Text.LastIndexOf("+")+1);
            }
            else
            {
                key = hotkey.Hotkey;
            }
            returnKey = (int)kc.ConvertFromString(key); //converter old
        }
        catch // exception not used
        {
            returnKey = keycode;
            hotkey.Hotkey = modRet + kc.ConvertToString(keycode); // error
        }

        return returnKey;
    }
}

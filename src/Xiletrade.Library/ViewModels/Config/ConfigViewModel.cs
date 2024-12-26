using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels.Config;

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

        General.Gateway = new()
        {
            "US", "KR", "FR", "ES", "DE",
            "BR", "RU", "TH", "TW", "CN",
            "JP"
        };
        General.GatewayIndex = Config.Options.Gateway;

        General.GameIndex = Config.Options.GameVersion;

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
        General.AutoUpdate = Config.Options.CheckUpdates;
        General.AutoFilter = Config.Options.CheckFilters;
        General.CheckTotalLife = Config.Options.AutoSelectLife;
        General.CheckGlobalEs = Config.Options.AutoSelectGlobalEs;
        General.CheckTotalResists = Config.Options.AutoSelectRes;
        General.CheckTotalArmourStats = Config.Options.AutoSelectArEsEva;
        General.CheckTotalDps = Config.Options.AutoSelectDps;
        General.CheckMinTier = Config.Options.AutoSelectMinTierValue;
        General.CheckMinPercentage = Config.Options.AutoSelectMinPercentValue;
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

        var kc = _serviceProvider.GetRequiredService<System.ComponentModel.TypeConverter>();

        var listKv = GetListHotkey();
        var listKvValue = GetListHotkeyWitchValue();
        var listKvChat = GetListHotkeyChat();

        foreach (var item in Config.Shortcuts)
        {
            var condition = item.Keycode > 0 && item.Value?.Length > 0;
            if (!condition)
            {
                continue;
            }
            if (item.Fonction is Strings.Feature.chatkey)
            {
                AdditionalKeys.ChatKey.Hotkey = kc.ConvertToString(item.Keycode);
            }
            if (listKv.ContainsKey(item.Fonction))
            {
                var hkVm = listKv.GetValueOrDefault(item.Fonction);
                hkVm.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                hkVm.IsEnable = item.Enable;
            }
            if (listKvValue.ContainsKey(item.Fonction))
            {
                var hkVm = listKvValue.GetValueOrDefault(item.Fonction);
                hkVm.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                hkVm.Val = item.Value;
                hkVm.IsEnable = item.Enable;
            }
            if (listKvChat.ContainsKey(item.Fonction))
            {
                var hkVm = listKvChat.GetValueOrDefault(item.Fonction);
                hkVm.Hotkey = GetModText(item.Modifier) + kc.ConvertToString(item.Keycode);
                hkVm.ListIndex = int.Parse(item.Value, CultureInfo.InvariantCulture);
                hkVm.IsEnable = item.Enable;
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
        Config.Options.Gateway = General.GatewayIndex;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(Strings.Culture[Config.Options.Language]);

        Config.Options.League = General.League[General.LeagueIndex];
        Config.Options.GameVersion = General.GameIndex;

        Config.Options.SearchBeforeDay = int.Parse(General.SearchDayLimit[General.SearchDayLimitIndex], CultureInfo.InvariantCulture);
        Config.Options.SearchFetchDetail = decimal.Parse(General.MaxFetch[General.MaxFetchIndex], CultureInfo.InvariantCulture);
        Config.Options.TimeoutTradeApi = int.Parse(General.MaxWaitRequest[General.MaxWaitRequestIndex], CultureInfo.InvariantCulture);

        Config.Options.DisableStartupMessage = General.StartupMessage;
        Config.Options.HideSameOccurs = General.RegroupResults;
        Config.Options.AutoSelectCorrupt = General.CheckCorrupted;
        Config.Options.AutoSelectPseudo = General.CheckPseudoAffix;
        Config.Options.SearchByType = General.ByBaseType;
        Config.Options.CheckUpdates = General.AutoUpdate;
        Config.Options.CheckFilters = General.AutoFilter;


        Config.Options.AutoSelectLife = General.CheckTotalLife;
        Config.Options.AutoSelectGlobalEs = General.CheckGlobalEs;
        Config.Options.AutoSelectRes = General.CheckTotalResists;
        Config.Options.AutoSelectArEsEva = General.CheckTotalArmourStats;
        Config.Options.AutoSelectDps = General.CheckTotalDps;
        Config.Options.AutoSelectMinTierValue = General.CheckMinTier;
        Config.Options.AutoSelectMinPercentValue = General.CheckMinPercentage;

        Config.Options.AutoCheckUniques = General.CheckExplicitsUniques;
        Config.Options.AutoCheckNonUniques = General.CheckExplicitsNonUniques;
        Config.Options.AutoCheckImplicits = General.CheckImplicits;
        Config.Options.AutoCheckEnchants = General.CheckEnchants;
        Config.Options.AutoCheckCrafted = General.CheckCrafted;
        Config.Options.AutoCheckCorruptions = General.CheckCorruptions;
        Config.Options.DevMode = General.DevMode;
        Config.Options.Autopaste = General.AutoWhisper;
        Config.Options.CtrlWheel = General.CtrlWheel;

        var listKv = GetListHotkey();
        var listKvValue = GetListHotkeyWitchValue();
        var listKvChat = GetListHotkeyChat();

        foreach (var item in Config.Shortcuts)
        {
            var condition = item.Keycode > 0 && item.Value?.Length > 0;
            if (!condition)
            {
                continue;
            }
            if (item.Fonction is Strings.Feature.chatkey)
            {
                item.Keycode = VerifyKeycode(AdditionalKeys.ChatKey, item.Keycode);
            }
            if (listKv.ContainsKey(item.Fonction))
            {
                var hkVm = listKv.GetValueOrDefault(item.Fonction);
                item.Modifier = GetModCode(hkVm.Hotkey);
                item.Keycode = VerifyKeycode(hkVm, item.Keycode);
                item.Enable = hkVm.IsEnable;
            }
            if (listKvValue.ContainsKey(item.Fonction))
            {
                var hkVm = listKvValue.GetValueOrDefault(item.Fonction);
                item.Modifier = GetModCode(hkVm.Hotkey);
                item.Keycode = VerifyKeycode(hkVm, item.Keycode);
                item.Value = hkVm.Val;
                item.Enable = hkVm.IsEnable;
            }
            if (listKvChat.ContainsKey(item.Fonction))
            {
                var hkVm = listKvChat.GetValueOrDefault(item.Fonction);
                item.Modifier = GetModCode(hkVm.Hotkey);
                item.Keycode = VerifyKeycode(hkVm, item.Keycode);
                item.Value = hkVm.ListIndex.ToString();
                item.Enable = hkVm.IsEnable;
            }
        }

        string configToSave = Json.Serialize<ConfigData>(Config);

        HotKey.RemoveRegisterHotKey(true);
        DataManager.Save_Config(configToSave, "cfg"); // parentWindow
        HotKey.InstallRegisterHotKey();
    }

    private Dictionary<string, HotkeyViewModel> GetListHotkey()
    {
        Dictionary<string, HotkeyViewModel> listKv = new()
        {
            { Strings.Feature.run, CommonKeys.PriceCheck },
            { Strings.Feature.bulk, CommonKeys.OpenBulk },
            { Strings.Feature.config, CommonKeys.OpenConfig },
            { Strings.Feature.close, CommonKeys.CloseWindow },
            { Strings.Feature.syndicate, CommonKeys.OpenSyndicate },
            { Strings.Feature.incursion, CommonKeys.OpenIncursion },
            { Strings.Feature.tcp, CommonKeys.TcpLogout },
            { Strings.Feature.wiki, CommonKeys.OpenWiki },
            { Strings.Feature.ninja, CommonKeys.OpenNinja },
            { Strings.Feature.lab, CommonKeys.OpenPoeLab },
            { Strings.Feature.poedb, CommonKeys.OpenPoeDb },
            { Strings.Feature.regex, CommonKeys.OpenRegexManager },
            { Strings.Feature.hideout, AdditionalKeys.Hideout },
            { Strings.Feature.whispertrade, AdditionalKeys.PasteWhisper },
            { Strings.Feature.exitchar, AdditionalKeys.CharSelection },
            { Strings.Feature.invlast, AdditionalKeys.InviteLast },
            { Strings.Feature.tradelast, AdditionalKeys.TradeLast },
            { Strings.Feature.whoislast, AdditionalKeys.WhoisLast }
        };
        return listKv;
    }

    private Dictionary<string, HotkeyViewModel> GetListHotkeyWitchValue()
    {
        Dictionary<string, HotkeyViewModel> listKvValue = new()
        {
            { Strings.Feature.link1, CommonKeys.OpenCustomFirst },
            { Strings.Feature.link2, CommonKeys.OpenCustomSecond },
            { Strings.Feature.replylast, AdditionalKeys.ReplyLast },
            { Strings.Feature.tradechan, AdditionalKeys.TradeChan },
            { Strings.Feature.globalchan, AdditionalKeys.GlobalChan },
            { Strings.Feature.invite, AdditionalKeys.PartyInvite },
            { Strings.Feature.kick, AdditionalKeys.PartyKick },
            { Strings.Feature.leave, AdditionalKeys.PartyLeave },
            { Strings.Feature.afk, AdditionalKeys.SetAfk },
            { Strings.Feature.autoreply, AdditionalKeys.SetAutoReply },
            { Strings.Feature.dnd, AdditionalKeys.SetDnd }
        };

        return listKvValue;
    }

    private Dictionary<string, HotkeyViewModel> GetListHotkeyChat()
    {
        Dictionary<string, HotkeyViewModel> listKvChat = new()
        {
            { Strings.Feature.chat1, AdditionalKeys.ChatCommandFirst },
            { Strings.Feature.chat2, AdditionalKeys.ChatCommandSecond },
            { Strings.Feature.chat3, AdditionalKeys.ChatCommandThird }
        };
        return listKvChat;
    }

    private static int GetModCode(string modifier) => _serviceProvider.GetRequiredService<INavigationService>().GetModifierCode(modifier);

    private static string GetModText(int modifier) => _serviceProvider.GetRequiredService<INavigationService>().GetModifierText(modifier);

    private static int VerifyKeycode(HotkeyViewModel hotkey, int keycode)
    {
        int returnKey;
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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class ConfigViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly DataManagerService _dm;

    [ObservableProperty]
    private bool canSave = true;

    [ObservableProperty]
    private double viewScale = 1;

    [ObservableProperty]
    private GeneralViewModel general;

    [ObservableProperty]
    private CommonKeysViewModel commonKeys;

    [ObservableProperty]
    private AdditionalKeysViewModel additionalKeys;

    public ConfigCommand Commands { get; private set; }

    // members
    internal ConfigData Config { get; set; }
    internal string ConfigBackup { get; set; }

    public ConfigViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Commands = new(this, _serviceProvider);

        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        ConfigBackup = _dm.LoadConfiguration(Strings.File.Config); //parentWindow
        Config = _dm.Json.Deserialize<ConfigData>(ConfigBackup);

        Initialize(true);
    }

    internal void Initialize(bool initIndexCollections)
    {
        General = new(_dm, Config.Options, initIndexCollections);
        ViewScale = Config.Options.Scale;
        InitShortcuts();
    }

    internal void SaveConfigForm()
    {
        Config.Options.Language = General.LanguageIndex;
        Config.Options.Gateway = General.GatewayIndex;
        Config.Options.Scale = General.ViewScale;
        Config.Options.Opacity = General.OpacityLevel;
        Config.Options.Autoclose = General.AutoCloseMain;

        Thread.CurrentThread.CurrentUICulture = new CultureInfo(Strings.Culture[Config.Options.Language]);

        var gameSwitch = Config.Options.GameVersion != General.GameIndex;
        Config.Options.GameVersion = General.GameIndex;
        Config.Options.League = General.League[General.LeagueIndex];

        Config.Options.SearchBeforeDay = General.SearchDayLimit;
        Config.Options.SearchFetchDetail = General.MaxFetch;
        Config.Options.TimeoutTradeApi = General.TimeoutRequest;

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
        Config.Options.AutoSelectAttr = General.CheckTotalAttributes;
        Config.Options.AutoSelectArEsEva = General.CheckTotalArmourStats;
        Config.Options.AutoSelectDps = General.CheckTotalDps;
        Config.Options.AutoSelectMinTierValue = General.CheckMinTier;
        Config.Options.AutoSelectMinPercentValue = General.CheckMinPercentage;

        Config.Options.AutoUnSelectBelowModLevel = General.CheckModLevel;
        Config.Options.ModLevel = Math.Clamp(General.ModLevel, 1, 100);

        Config.Options.AutoCheckUniques = General.CheckExplicitsUniques;
        Config.Options.AutoCheckNonUniques = General.CheckExplicitsNonUniques;
        Config.Options.AutoCheckImplicits = General.CheckImplicits;
        Config.Options.AutoCheckEnchants = General.CheckEnchants;
        Config.Options.AutoCheckCrafted = General.CheckCrafted;
        Config.Options.AutoCheckCorruptions = General.CheckCorruptions;
        Config.Options.DevMode = General.DevMode;
        Config.Options.Autopaste = General.AutoWhisper;
        Config.Options.CtrlWheel = General.CtrlWheel;
        Config.Options.AsyncMarketDefault = General.AsyncMarketDefault;
        Config.Options.FastInputs = General.FastInputs;

        var listKv = GetListHotkey();
        var listKvValue = GetListHotkeyWithValue();
        var listKvChat = GetListHotkeyChat();

        foreach (var item in Config.Shortcuts)
        {
            if (item.Fonction is Strings.Feature.chatkey 
                && AdditionalKeys.ChatKey.Hotkey.Length > 0)
            {
                item.Keycode = VerifyKeycode(AdditionalKeys.ChatKey, item.Keycode);
                continue;
            }
            if (listKv.ContainsKey(item.Fonction))
            {
                var hkVm = listKv.GetValueOrDefault(item.Fonction);
                item.Modifier = GetModCode(hkVm.Hotkey);
                item.Keycode = VerifyKeycode(hkVm, item.Keycode);
                item.Enable = hkVm.IsEnable;
                continue;
            }
            if (listKvValue.ContainsKey(item.Fonction))
            {
                var hkVm = listKvValue.GetValueOrDefault(item.Fonction);
                item.Modifier = GetModCode(hkVm.Hotkey);
                item.Keycode = VerifyKeycode(hkVm, item.Keycode);
                item.Value = hkVm.Val;
                item.Enable = hkVm.IsEnable;
                continue;
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

        var configToSave = _dm.Json.Serialize<ConfigData>(Config);

        var hk = _serviceProvider.GetRequiredService<HotKeyService>();
        hk.DisableHotkeys();
        _dm.SaveConfiguration(configToSave); // parentWindow
        hk.EnableHotkeys();
        if (gameSwitch)
        {
            _ = _serviceProvider.GetRequiredService<PoeNinjaService>().LoadStateAsync();
        }
    }

    internal void InitShortcuts()
    {
        CommonKeys = new(_serviceProvider);
        AdditionalKeys = new(_serviceProvider, Config);

        var kc = _serviceProvider.GetRequiredService<IKeysConverter>();

        var listKv = GetListHotkey();
        var listKvValue = GetListHotkeyWithValue();
        var listKvChat = GetListHotkeyChat();

        foreach (var item in Config.Shortcuts)
        {
            if (item.Fonction is Strings.Feature.chatkey)
            {
                AdditionalKeys.ChatKey.Hotkey = kc.ConvertToInvariantString(item.Keycode);
                continue;
            }
            if (listKv.ContainsKey(item.Fonction))
            {
                UpdateHotkey(item, listKv.GetValueOrDefault(item.Fonction));
                continue;
            }
            if (listKvValue.ContainsKey(item.Fonction))
            {
                UpdateHotkey(item, listKvValue.GetValueOrDefault(item.Fonction), haveValue: true);
                continue;
            }
            if (listKvChat.ContainsKey(item.Fonction))
            {
                UpdateHotkey(item, listKvChat.GetValueOrDefault(item.Fonction), isChat: true);
            }
        }
    }

    private static void UpdateHotkey(ConfigShortcut item, HotkeyViewModel hkVm, bool haveValue = false, bool isChat = false)
    {
        hkVm.IsEnable = item.Enable;
        if (item.Keycode > 0)
        {
            var kc = _serviceProvider.GetRequiredService<IKeysConverter>();
            hkVm.Hotkey = GetModText(item.Modifier) + kc.ConvertToInvariantString(item.Keycode);
            if (isChat)
            {
                hkVm.ListIndex = int.Parse(item.Value, CultureInfo.InvariantCulture);
                return;
            }
            if (haveValue)
            {
                hkVm.Val = item.Value;
                return;
            }
        }
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
            { Strings.Feature.coe, CommonKeys.OpenCoe },
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

    private Dictionary<string, HotkeyViewModel> GetListHotkeyWithValue()
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
        var kc = _serviceProvider.GetRequiredService<IKeysConverter>();
        string modRet = string.Empty;
        try
        {
            if (string.IsNullOrEmpty(hotkey.Hotkey)) return 0;

            string key;
            if (hotkey.Hotkey.Contain('+'))
            {
                var idx = hotkey.Hotkey.LastIndexOf('+') + 1;
                modRet = hotkey.Hotkey.AsSpan(0, idx).ToString();
                key = hotkey.Hotkey.AsSpan(idx, hotkey.Hotkey.Length - idx).ToString();
            }
            else
            {
                key = hotkey.Hotkey;
            }
            return (int)kc.ConvertFromString(key);
        }
        catch
        {
            hotkey.Hotkey = modRet + kc.ConvertToString(keycode);
        }
        return keycode;
    }
}

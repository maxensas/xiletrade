using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary>Static helper class containing all hotkeys registering management.</summary>
internal static class HotKey
{
    private static IServiceProvider _serviceProvider;
    private static System.Timers.Timer _registerTimer;
    private static nint _hookHwnd;

    internal const int SHIFTHOTKEYID = 10001;
    internal static bool IsAllHotKeysRegistered { get; set; } = false;
    internal static bool FirstHotkeyRegistering { get; set; } = true;

    internal static string ChatKey { get; private set; } = string.Empty;

    internal static Action hotkeyHandler = new(() =>
    {
        if (Native.FindWindow(null, Strings.WindowName.Config).ToInt32() is not 0)
        {
            if (IsAllHotKeysRegistered)
            {
                RemoveRegisterHotKey(true);
            }
            return;
        }
        if (FirstHotkeyRegistering || !IsAllHotKeysRegistered
            && Native.GetForegroundWindow().Equals(Native.FindWindow(Strings.PoeClass, Strings.PoeCaption))) // IF you have POE game window in focus
        {
            InstallRegisterHotKey();
            return;
        }
        if (IsAllHotKeysRegistered)
        {
            RemoveRegisterHotKey(false);
        }
        if (DataManager.Config.Options.Autopaste)
        {
            ClipboardHelper.SendWhisperMessage(null);
        }
    });

    internal static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _hookHwnd = _serviceProvider.GetRequiredService<IHookService>().Hwnd;

        // If the SynchronizingObject property is null, the handler runs on a thread pool thread.
        _registerTimer?.Stop();
        _registerTimer = new(100);
        _registerTimer.Elapsed += AutoRegisterHotkey_Tick;
        _registerTimer.Start();
    }

    private static void AutoRegisterHotkey_Tick(object sender, EventArgs e)
    {
        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(hotkeyHandler);
    }

    internal static void InstallRegisterHotKey()
    {
        IsAllHotKeysRegistered = true;
        for (int i = 0; i < DataManager.Config.Shortcuts.Length; i++)
        {
            ConfigShortcut shortcut = DataManager.Config.Shortcuts[i];
            if (shortcut.Keycode > 0 && shortcut.Value?.Length > 0
                && (Strings.Feature.Unregisterable.Contains(shortcut.Fonction.ToLowerInvariant()) || FirstHotkeyRegistering))
            {
                if (shortcut.Fonction.ToLowerInvariant() is Strings.Feature.chatkey)
                {
                    CultureInfo cultureEn = new("en-US");
                    //System.Windows.Forms.KeysConverter kc = new();
                    
                    var kc = _serviceProvider.GetRequiredService<System.ComponentModel.TypeConverter>();
                    ChatKey = "{" + kc.ConvertToString(null, cultureEn, shortcut.Keycode).ToUpper() + "}";
                    continue;
                }
                if (shortcut.Enable)
                {
                    Native.RegisterHotKey(_hookHwnd, SHIFTHOTKEYID + i, Convert.ToUInt32(shortcut.Modifier), (uint)Math.Abs(shortcut.Keycode));
                }
            }
        }
        FirstHotkeyRegistering = false;
    }

    internal static void RemoveRegisterHotKey(bool reInit)
    {
        IsAllHotKeysRegistered = false;
        if (reInit)
        {
            FirstHotkeyRegistering = true;
        }
        for (int i = 0; i < DataManager.Config.Shortcuts.Length; i++)
        {
            ConfigShortcut shortcut = DataManager.Config.Shortcuts[i];
            if (shortcut.Keycode > 0 && shortcut.Value?.Length > 0)
            {
                string fonction = shortcut.Fonction.ToLowerInvariant();
                if (shortcut.Enable && (reInit || Strings.Feature.Unregisterable.Contains(fonction)))
                {
                    Native.UnregisterHotKey(_hookHwnd, SHIFTHOTKEYID + i);
                }
            }
        }
    }
}

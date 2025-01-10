using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
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
                if (DataManager.Config.Options.CtrlWheel)
                {
                    _serviceProvider.GetRequiredService<ISendInputService>().StopMouseWheelCapture();
                }
            }
            return;
        }
        var isPoeFocused = Native.GetForegroundWindow().Equals(Native.FindWindow(Strings.PoeClass, Strings.PoeCaption));
        if (FirstHotkeyRegistering || !IsAllHotKeysRegistered && isPoeFocused) // || IsXiletradeWindowFocused()
        {
            InstallRegisterHotKey();
            if (DataManager.Config.Options.CtrlWheel)
            {
                _serviceProvider.GetRequiredService<ISendInputService>().StartMouseWheelCapture();
            }
            return;
        }
        if (IsAllHotKeysRegistered)
        {
            RemoveRegisterHotKey(false);
            if (DataManager.Config.Options.CtrlWheel)
            {
                _serviceProvider.GetRequiredService<ISendInputService>().StopMouseWheelCapture();
            }
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
            var shortcut = DataManager.Config.Shortcuts[i];
            var isValidShortcut = shortcut.Keycode > 0 && shortcut.Value?.Length > 0;
            if (!isValidShortcut)
            {
                continue;
            }
            string fonction = shortcut.Fonction.ToLowerInvariant();
            var isRegisterable = FirstHotkeyRegistering || Strings.Feature.Unregisterable.Contains(fonction);
            if (!isRegisterable)
            {
                continue;
            }
            if (fonction is Strings.Feature.chatkey)
            {
                var cultureEn = new CultureInfo("en-US");
                var kc = _serviceProvider.GetRequiredService<System.ComponentModel.TypeConverter>();
                ChatKey = "{" + kc.ConvertToString(null, cultureEn, shortcut.Keycode).ToUpper() + "}";
                continue;
            }
            /*
            if (fonction is Strings.Feature.close && !IsXiletradeWindowFocused())
            {
                continue;
            }
            */
            if (shortcut.Enable)
            {
                Native.RegisterHotKey(_hookHwnd, SHIFTHOTKEYID + i, Convert.ToUInt32(shortcut.Modifier), (uint)Math.Abs(shortcut.Keycode));
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
            var shortcut = DataManager.Config.Shortcuts[i];
            var isValidShortcut = shortcut.Keycode > 0 && shortcut.Value?.Length > 0;
            if (!isValidShortcut)
            {
                continue;
            }
            string fonction = shortcut.Fonction.ToLowerInvariant();
            if (shortcut.Enable && (reInit || Strings.Feature.Unregisterable.Contains(fonction)))
            {
                /*
                if (fonction is Strings.Feature.close && IsXiletradeWindowFocused())
                {
                    continue;
                }
                */
                Native.UnregisterHotKey(_hookHwnd, SHIFTHOTKEYID + i);
            }
        }
    }

    private static bool IsXiletradeWindowFocused()
    {
        var mainHwnd = _serviceProvider.GetRequiredService<XiletradeService>().MainHwnd;
        if (Native.GetForegroundWindow().Equals(mainHwnd))
        {
            return true;
        }

        foreach (var win in Strings.WindowName.XiletradeWindowList)
        {
            nint findHwnd = Native.FindWindow(null, win);
            if (findHwnd.ToInt32() > 0 && Native.GetForegroundWindow().Equals(findHwnd))
            {
                return true;
            }
        }
        return false;
    }
}

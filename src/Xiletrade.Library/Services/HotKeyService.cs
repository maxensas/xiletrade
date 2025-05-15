using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary>Service containing all hotkeys registering management.</summary>
public sealed class HotKeyService
{
    private static IServiceProvider _serviceProvider;
    //private readonly DataManager _dm;
    private const int SHIFTHOTKEYID = 10001;

    //TODO remove static != DI
    private static System.Timers.Timer _registerTimer;
    private static nint _hookHwnd;
    private static bool _isAllHotKeysRegistered = false;
    private static bool _firstHotkeyRegistering = true;
    private static bool _capturingMouse = false;
    private static bool _configViewOpened = false;
    private static string _chatKey = string.Empty;

    internal int ShiftHotkeyId { get { return SHIFTHOTKEYID; } }

    public string ChatKey { get { return _chatKey; } }

    public HotKeyService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        //_dm = _serviceProvider.GetRequiredService<DataManager>();

        _hookHwnd = _serviceProvider.GetRequiredService<IHookService>().Hwnd;

        // If the SynchronizingObject property is null, the handler runs on a thread pool thread.
        _registerTimer?.Stop();
        _registerTimer = new(100);
        _registerTimer.Elapsed += AutoRegisterHotkey_Tick;
        _registerTimer.Start();
    }

    internal Action hotkeyHandler = new(() =>
    {
        var isPoeFocused = Native.GetForegroundWindow().Equals(Native.FindWindow(Strings.PoeClass, Strings.PoeCaption));
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        if (!_capturingMouse && isPoeFocused && dm.Config.Options.CtrlWheel)
        {
            _serviceProvider.GetRequiredService<ISendInputService>().StartMouseWheelCapture();
            _capturingMouse = true;
        }
        if (_capturingMouse && !isPoeFocused)
        {
            _serviceProvider.GetRequiredService<ISendInputService>().StopMouseWheelCapture();
            _capturingMouse = false;
        }

        if (Native.FindWindow(null, Strings.WindowName.Config).ToInt32() is not 0)
        {
            if (!_configViewOpened)
            {
                RemoveRegisterHotKey(true);
                _configViewOpened = true;
            }
            return;
        }
        if (_configViewOpened)
        {
            _configViewOpened = false;
        }

        if (_firstHotkeyRegistering || !_isAllHotKeysRegistered && (isPoeFocused || IsXiletradeWindowOpened()))
        {
            InstallRegisterHotKey();
            return;
        }

        if (_isAllHotKeysRegistered && !isPoeFocused && !IsXiletradeWindowOpened())
        {
            RemoveRegisterHotKey(false);
        }

        if (dm.Config.Options.Autopaste)
        {
            _serviceProvider.GetRequiredService<ClipboardService>().SendWhisperMessage(null);
        }
    });
    /*
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
    */
    private void AutoRegisterHotkey_Tick(object sender, EventArgs e)
    {
        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(hotkeyHandler);
    }

    internal void EnableHotkeys() => InstallRegisterHotKey();

    internal static void InstallRegisterHotKey()
    {
        _isAllHotKeysRegistered = true;
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        for (int i = 0; i < dm.Config.Shortcuts.Length; i++)
        {
            var shortcut = dm.Config.Shortcuts[i];
            var isValidShortcut = shortcut.Keycode > 0 && shortcut.Value?.Length > 0;
            if (!isValidShortcut)
            {
                continue;
            }
            string fonction = shortcut.Fonction.ToLowerInvariant();
            var isRegisterable = _firstHotkeyRegistering || Strings.Feature.Unregisterable.Contains(fonction);
            if (!isRegisterable)
            {
                continue;
            }
            if (fonction is Strings.Feature.chatkey)
            {
                var cultureEn = new CultureInfo("en-US");
                var kc = _serviceProvider.GetRequiredService<System.ComponentModel.TypeConverter>();
                _chatKey = "{" + kc.ConvertToString(null, cultureEn, shortcut.Keycode).ToUpper() + "}";
                continue;
            }
            if (fonction is Strings.Feature.close && !IsXiletradeWindowOpened())
            {
                continue;
            }
            if (shortcut.Enable)
            {
                Native.RegisterHotKey(_hookHwnd, SHIFTHOTKEYID + i, Convert.ToUInt32(shortcut.Modifier), (uint)Math.Abs(shortcut.Keycode));
            }
        }
        _firstHotkeyRegistering = false;
    }

    internal void DisableHotkeys() => RemoveRegisterHotKey(true);

    internal static void RemoveRegisterHotKey(bool reInit)
    {
        _isAllHotKeysRegistered = false;
        if (reInit)
        {
            _firstHotkeyRegistering = true;
        }
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        for (int i = 0; i < dm.Config.Shortcuts.Length; i++)
        {
            var shortcut = dm.Config.Shortcuts[i];
            var isValidShortcut = shortcut.Keycode > 0 && shortcut.Value?.Length > 0;
            if (!isValidShortcut)
            {
                continue;
            }
            string fonction = shortcut.Fonction.ToLowerInvariant();
            if (shortcut.Enable && (reInit || Strings.Feature.Unregisterable.Contains(fonction)))
            {
                if (fonction is Strings.Feature.close && IsXiletradeWindowOpened())
                {
                    continue;
                }
                Native.UnregisterHotKey(_hookHwnd, SHIFTHOTKEYID + i);
            }
        }
    }

    private static bool IsXiletradeWindowOpened()
    {
        if (_serviceProvider.GetRequiredService<INavigationService>().IsVisibleMainView())
        {
            return true;
        }

        foreach (var win in Strings.WindowName.XiletradeWindowList)
        {
            nint findHwnd = Native.FindWindow(null, win);
            if (findHwnd.ToInt32() > 0)
            {
                return true;
            }
        }
        return false;
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Windows;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary>Service containing all hotkeys registering management.</summary>
public sealed class HotKeyService
{
    private readonly INavigationService _navigation;
    private readonly IHookService _hook;
    private readonly IKeysConverter _keyConverter;
    private readonly ISendInputService _input;
    private readonly DataManagerService _dm;
    private readonly ClipboardService _clipboard;
    private readonly UIService _ui;

    private readonly Action _hotkeyHandler;
    private System.Timers.Timer _registerTimer;
    private nint _hookHwnd;

    private static bool _isAllHotKeysRegistered = false;
    private static bool _firstHotkeyRegistering = true;
    private static bool _capturingMouse = false;
    private static bool _configViewOpened = false;

    public const int SHIFTHOTKEYID = 10001;

    public HotKeyService(ILogger<HotKeyService> logger, 
        INavigationService navigation, IHookService hook,
        IKeysConverter keyConverter, ISendInputService input, 
        DataManagerService dm, ClipboardService clipboard, UIService ui)
    {
        _navigation = navigation;
        _hook = hook;
        _keyConverter = keyConverter;
        _input = input;
        _dm = dm;
        _clipboard = clipboard;
        _ui = ui;

        _hotkeyHandler = new(() =>
        {
            var isPoeFocused = Native.GetForegroundWindow().Equals(Native.FindWindow(Strings.PoeClass, Strings.PoeCaption));
            if (!_capturingMouse && isPoeFocused && _dm.Config.Options.CtrlWheel)
            {
                Input.MouseHook.StartMouseWheelCapture();
                _capturingMouse = true;
            }
            if (_capturingMouse && !isPoeFocused)
            {
                Input.MouseHook.StopMouseWheelCapture();
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
                _clipboard.SendWhisperMessage([]);
            }
        });

        StartAutoRegister();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private void StartAutoRegister()
    {
        _hookHwnd = _hook.Hwnd;

        // If the SynchronizingObject property is null, the handler runs on a thread pool thread.
        _registerTimer?.Stop();
        _registerTimer = new(100);
        _registerTimer.Elapsed += AutoRegisterHotkey_Tick;
        _registerTimer.Start();
    }

    private void AutoRegisterHotkey_Tick(object sender, EventArgs e) 
        => _ui.DelegateActionToUiThread(_hotkeyHandler);

    internal void EnableHotkeys() => InstallRegisterHotKey();

    private void InstallRegisterHotKey()
    {
        _isAllHotKeysRegistered = true;
        for (int i = 0; i < _dm.Config.Shortcuts.Length; i++)
        {
            var shortcut = _dm.Config.Shortcuts[i];
            var isValidShortcut = shortcut.Keycode > 0;
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
                _input.ChatKey = ("{" + 
                    _keyConverter.ConvertToString(null, cultureEn, shortcut.Keycode).ToUpper() + "}", 
                    (ushort)shortcut.Keycode);
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

    private void RemoveRegisterHotKey(bool reInit)
    {
        _isAllHotKeysRegistered = false;
        if (reInit)
        {
            _firstHotkeyRegistering = true;
        }
        for (int i = 0; i < _dm.Config.Shortcuts.Length; i++)
        {
            var shortcut = _dm.Config.Shortcuts[i];
            var isValidShortcut = shortcut.Keycode > 0;
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

    private bool IsXiletradeWindowOpened()
    {
        if (_navigation.IsVisibleMainView())
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

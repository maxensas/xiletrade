using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Services;

/// <summary>Service responsible for hotkey registrations and global input interactions.</summary>
public sealed class InputService
{
    private readonly INavigationService _navigation;
    private readonly IHookService _hook;
    private readonly IKeyboardAdapterService _keyboard;
    private readonly IPoeActionService _poe;
    private readonly DataManagerService _dm;
    private readonly ClipboardService _clipboard;
    private readonly IUIService _ui;

    private readonly Action _inputHandler;
    private System.Timers.Timer _startupTimer;
    private nint _hookHwnd;

    private static bool _isAllHotKeysRegistered = false;
    private static bool _firstHotkeyRegistering = true;
    private static bool _capturingMouse = false;
    private static bool _configViewOpened = false;

    public const int SHIFTHOTKEYID = 10001;

    public InputService(ILogger<InputService> logger, 
        INavigationService navigation, IHookService hook,
        IKeyboardAdapterService keyboard, IPoeActionService poe, 
        DataManagerService dm, ClipboardService clipboard, IUIService ui)
    {
        _navigation = navigation;
        _hook = hook;
        _keyboard = keyboard;
        _poe = poe;
        _dm = dm;
        _clipboard = clipboard;
        _ui = ui;

        _inputHandler = new(() =>
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

        StartTimer();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private void StartTimer()
    {
        _hookHwnd = _hook.Hwnd;

        // If the SynchronizingObject property is null, the handler runs on a thread pool thread.
        _startupTimer?.Stop();
        _startupTimer = new(100);
        _startupTimer.Elapsed += OnRegisterTimerElapsed;
        _startupTimer.Start();
    }

    private void OnRegisterTimerElapsed(object sender, EventArgs e) => _ui.Invoke(_inputHandler);

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
                _poe.ChatKey = ("{" + _keyboard.Converter
                    .ConvertToString(null, cultureEn, shortcut.Keycode).ToUpper() + "}", (ushort)shortcut.Keycode);
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

using System;
using System.Globalization;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary>Service containing all hotkeys registering management.</summary>
public sealed class HotKeyService
{
    private readonly INavigationService _navigation;
    private readonly ISendInputService _input;
    private readonly IHookService _hook;
    private readonly IKeysConverter _keyConverter;
    private readonly DataManagerService _dm;
    private readonly ClipboardService _clipboard;

    private readonly Action hotkeyHandler;

    // not testable
    private static bool _started;
    private static System.Timers.Timer _registerTimer;
    private static nint _hookHwnd;
    private static bool _isAllHotKeysRegistered = false;
    private static bool _firstHotkeyRegistering = true;
    private static bool _capturingMouse = false;
    private static bool _configViewOpened = false;
    private static (string, ushort) _chatKey = (string.Empty, 0);

    private const int SHIFTHOTKEYID = 10001;

    internal int ShiftHotkeyId => SHIFTHOTKEYID;

    public string ChatKey => _chatKey.Item1;
    public ushort ChatKeyCode => _chatKey.Item2;

    public HotKeyService(INavigationService navigation, ISendInputService input, IHookService hook,
        IKeysConverter keyConverter, DataManagerService dm, ClipboardService clipboard)
    {
        _navigation = navigation;
        _input = input;
        _hook = hook;
        _keyConverter = keyConverter;
        _dm = dm;
        _clipboard = clipboard;

        hotkeyHandler = new(() =>
        {
            var isPoeFocused = Native.GetForegroundWindow().Equals(Native.FindWindow(Strings.PoeClass, Strings.PoeCaption));
            if (!_capturingMouse && isPoeFocused && _dm.Config.Options.CtrlWheel)
            {
                _input.StartMouseWheelCapture();
                _capturingMouse = true;
            }
            if (_capturingMouse && !isPoeFocused)
            {
                _input.StopMouseWheelCapture();
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
    }

    internal void StartAutoRegister()
    {
        if (_started)
        {
            throw new Exception(Resources.Resources.Main188_Alreadystarted);
        }

        _hookHwnd = _hook.Hwnd;

        // If the SynchronizingObject property is null, the handler runs on a thread pool thread.
        _registerTimer?.Stop();
        _registerTimer = new(100);
        _registerTimer.Elapsed += AutoRegisterHotkey_Tick;
        _registerTimer.Start();

        _started = true;
    }

    private void AutoRegisterHotkey_Tick(object sender, EventArgs e) 
        => _navigation.DelegateActionToUiThread(hotkeyHandler);

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
                _chatKey.Item1 = "{" + _keyConverter.ConvertToString(null, cultureEn, shortcut.Keycode).ToUpper() + "}";
                _chatKey.Item2 = (ushort)shortcut.Keycode;
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

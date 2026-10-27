using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Runtime.Versioning;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Service responsible for hotkey registrations and global input interactions like auto sending whisper.
/// </summary>
/// <remarks>
/// Same behavior as the Windows service : the shortcuts are registered while Path of Exile
/// or a window of Xiletrade has the focus, and released otherwise.
/// <br/>The capture of Ctrl + mouse wheel (low level mouse hook on Windows) is not available.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxInputService : IInputService, IDisposable
{
    private readonly IViewManager _view;
    private readonly LinuxHookService _hook;
    private readonly IKeyboardAdapterService _keyboard;
    private readonly IPoeActionService _poe;
    private readonly DataManagerService _dm;
    private readonly IClipboardService _clipboard;
    private readonly IUIService _ui;
    private readonly LinuxDesktopService _desktop;

    private readonly Action _inputHandler;
    private System.Timers.Timer _startupTimer;

    // The clipboard is read by running a tool : the automatic paste is checked less often than the focus.
    private const int AutopasteIntervalMs = 300;
    private long _lastAutopasteTick;

    private bool _isAllHotKeysRegistered;
    private bool _firstHotkeyRegistering = true;
    private bool _configViewOpened;

    public LinuxInputService(ILogger<LinuxInputService> logger, IViewManager view, IHookService hook,
        IKeyboardAdapterService keyboard, IPoeActionService poe, DataManagerService dm,
        IClipboardService clipboard, IUIService ui, LinuxDesktopService desktop, IMessageAdapterService message)
    {
        _view = view;
        _hook = hook as LinuxHookService; // null : the shortcuts are not available
        _keyboard = keyboard;
        _poe = poe;
        _dm = dm;
        _clipboard = clipboard;
        _ui = ui;
        _desktop = desktop;

        if (_hook is not { IsAvailable: true })
        {
            message.Show("Global shortcuts are unavailable : no X11 display was found.\n"
                + "Xiletrade needs an X11 session or XWayland (the DISPLAY variable must be set).",
                "Xiletrade", MessageStatus.Warning);
        }

        _inputHandler = new(() =>
        {
            var isPoeFocused = _desktop.IsPoeFocused();

            if (_desktop.IsWindowOpened(Strings.WindowName.Config))
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

            var isXiletradeOpened = IsXiletradeWindowOpened();
            if (_firstHotkeyRegistering || !_isAllHotKeysRegistered && (isPoeFocused || isXiletradeOpened))
            {
                InstallRegisterHotKey();
                return;
            }

            if (_isAllHotKeysRegistered && !isPoeFocused && !isXiletradeOpened)
            {
                RemoveRegisterHotKey(false);
            }

            if (_dm.Config.Options.Autopaste && Environment.TickCount64 - _lastAutopasteTick >= AutopasteIntervalMs)
            {
                _lastAutopasteTick = Environment.TickCount64;
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
        // If the SynchronizingObject property is null, the handler runs on a thread pool thread.
        _startupTimer?.Stop();
        _startupTimer = new(100);
        _startupTimer.Elapsed += OnRegisterTimerElapsed;
        _startupTimer.Start();
    }

    private void OnRegisterTimerElapsed(object sender, EventArgs e) => _ui.Invoke(_inputHandler);

    public void EnableHotkeys() => InstallRegisterHotKey();

    public void DisableHotkeys() => RemoveRegisterHotKey(true);

    public void Dispose()
    {
        _startupTimer?.Stop();
        _startupTimer?.Dispose();
        _startupTimer = null;
    }

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
                _hook?.RegisterHotKey(IInputService.SHIFTHOTKEYID + i, shortcut.Modifier, Math.Abs(shortcut.Keycode));
            }
        }
        _firstHotkeyRegistering = false;
    }

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
                _hook?.UnregisterHotKey(IInputService.SHIFTHOTKEYID + i);
            }
        }
    }

    private bool IsXiletradeWindowOpened() => _view.IsVisibleMainView() || _desktop.IsXiletradeWindowOpened();
}

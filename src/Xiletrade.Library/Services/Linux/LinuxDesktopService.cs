using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using Xiletrade.Library.Services.Linux.Helper;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Gives access to the windows and to the keyboard of the desktop session.
/// <br/>This is the Linux counterpart of what the Win32 API provides to the Windows services
/// (FindWindow, GetForegroundWindow, SetForegroundWindow, SendInput...).
/// </summary>
/// <remarks>
/// Requires an X11 session or XWayland, where Path of Exile (Wine / Proton) runs.
/// <br/>Window ids are X11 window ids, 0 means "no window".
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxDesktopService : IDisposable
{
    private const int ReconnectDelayMs = 10_000;
    private const int SnapshotLifetimeMs = 50;
    private const int TitleLifetimeMs = 2_000;
    private const int SwitchWindowTimeoutMs = 5_000;

    private readonly Lock _lock = new();
    private readonly Dictionary<nuint, (string Title, long Tick)> _titles = [];

    private X11Display _display;
    private bool _connectionTried;
    private long _connectionTick;

    private List<(nuint Id, string Title)> _snapshot;
    private long _snapshotTick;

    /// <summary>
    /// True when the connection to the X server is established.
    /// </summary>
    public bool IsAvailable => GetDisplay() is not null;

    // -------- Windows --------

    /// <summary>Gets the id of the window having the focus, 0 if there is none.</summary>
    public nuint GetActiveWindow() => GetDisplay()?.GetActiveWindow() ?? 0;

    /// <summary>Finds a window by its exact title, 0 if there is none.</summary>
    public nuint FindWindowByTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return 0;
        }
        lock (_lock)
        {
            var windows = GetWindows();
            foreach (var window in windows)
            {
                if (string.Equals(window.Title, title, StringComparison.Ordinal))
                {
                    return window.Id;
                }
            }
            return 0;
        }
    }

    /// <summary>Finds the Path of Exile window, 0 if the game is not launched.</summary>
    public nuint FindPoeWindow() => FindWindowByTitle(Strings.PoeCaption);

    public bool IsWindowOpened(string title) => FindWindowByTitle(title) is not 0;

    /// <summary>True if one of the windows of Xiletrade (config, editor, whisper, popup, regex) is opened.</summary>
    public bool IsXiletradeWindowOpened()
        => Strings.WindowName.XiletradeWindowList.Any(IsWindowOpened);

    public bool IsPoeFocused()
    {
        nuint poe = FindPoeWindow();
        return poe is not 0 && GetActiveWindow() == poe;
    }

    public PoeState GetPoeState()
    {
        nuint poe = FindPoeWindow();
        if (poe is 0)
        {
            return PoeState.NotLaunched;
        }
        return GetActiveWindow() == poe ? PoeState.LaunchedAndFocused : PoeState.Launched;
    }

    /// <summary>Gets the id of the process owning the game window, 0 if it is not launched or unknown.</summary>
    public int GetPoeProcessId()
    {
        nuint poe = FindPoeWindow();
        return poe is 0 ? 0 : GetDisplay()?.GetWindowPid(poe) ?? 0;
    }

    /// <summary>
    /// Gives the focus to a window and waits for it (5 seconds at most).
    /// </summary>
    public bool SwitchWindow(nuint window)
    {
        var display = GetDisplay();
        if (display is null || window is 0)
        {
            return false;
        }
        if (display.GetActiveWindow() == window)
        {
            return true;
        }

        display.ActivateWindow(window);
        InvalidateSnapshot();

        var watchdog = System.Diagnostics.Stopwatch.StartNew();
        while (display.GetActiveWindow() != window)
        {
            if (watchdog.ElapsedMilliseconds >= SwitchWindowTimeoutMs)
            {
                return false;
            }
            Thread.Sleep(10);
        }
        return true;
    }

    /// <summary>Asks a window to close, like its close button would.</summary>
    public bool CloseWindow(nuint window)
    {
        var display = GetDisplay();
        if (display is null || window is 0)
        {
            return false;
        }
        bool sent = display.CloseWindow(window);
        InvalidateSnapshot();
        return sent;
    }

    /// <summary>Closes the first opened window of Xiletrade (config, editor, whisper, popup, regex).</summary>
    /// <returns>True if a window was found.</returns>
    public bool CloseXiletradeWindow()
    {
        foreach (var title in Strings.WindowName.XiletradeWindowList)
        {
            nuint window = FindWindowByTitle(title);
            if (window is not 0)
            {
                CloseWindow(window);
                return true;
            }
        }
        return false;
    }

    // -------- Keyboard --------

    /// <summary>
    /// Releases the modifier keys physically held (the keys of the shortcut which launched the feature),
    /// so that they do not alter the keys sent to the game afterwards (Alt+Ctrl+C instead of Ctrl+C, for instance).
    /// </summary>
    /// <remarks>They are not pressed again: that would leave them stuck if the user released them meanwhile.</remarks>
    public void ReleaseHeldModifiers()
    {
        var display = GetDisplay();
        if (display is null)
        {
            return;
        }
        try
        {
            foreach (var keycode in display.GetHeldModifierKeycodes())
            {
                display.FakeKey(keycode, press: false);
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            // libXtst is not installed : SendKey reports it
        }
    }

    /// <summary>Presses or releases a key (Windows virtual-key code) in the focused window.</summary>
    /// <returns>False when the key cannot be sent (unknown key, no X server, XTest extension missing).</returns>
    public bool SendKey(ushort virtualKey, bool press)
    {
        var display = GetDisplay();
        if (display is null || !X11Keys.TryGetKeysym(virtualKey, out var keysym))
        {
            return false;
        }
        try
        {
            byte keycode = display.KeysymToKeycode(keysym);
            return keycode is not 0 && display.FakeKey(keycode, press);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _display?.Dispose();
            _display = null;
            _snapshot = null;
            _titles.Clear();
        }
    }

    // -------- private --------

    private X11Display GetDisplay()
    {
        lock (_lock)
        {
            if (_display is null && (!_connectionTried
                || Environment.TickCount64 - _connectionTick >= ReconnectDelayMs))
            {
                _connectionTried = true;
                _connectionTick = Environment.TickCount64;
                _display = X11Display.TryOpen();
            }
            return _display;
        }
    }

    private void InvalidateSnapshot()
    {
        lock (_lock)
        {
            _snapshot = null;
        }
    }

    /// <summary>
    /// Lists the windows with their title. The list is reused for a short time, and
    /// the titles for a few seconds : the callers poll every 100 ms.
    /// </summary>
    // caller holds _lock
    private List<(nuint Id, string Title)> GetWindows()
    {
        long now = Environment.TickCount64;
        if (_snapshot is not null && now - _snapshotTick < SnapshotLifetimeMs)
        {
            return _snapshot;
        }

        var display = GetDisplay();
        var windows = new List<(nuint Id, string Title)>();
        if (display is not null)
        {
            var ids = display.GetClientList();
            foreach (var id in ids)
            {
                if (!_titles.TryGetValue(id, out var entry) || now - entry.Tick >= TitleLifetimeMs)
                {
                    entry = (display.GetWindowTitle(id), now);
                    _titles[id] = entry;
                }
                windows.Add((id, entry.Title));
            }

            if (_titles.Count > ids.Length)
            {
                var alive = new HashSet<nuint>(ids);
                foreach (var closed in _titles.Keys.Where(id => !alive.Contains(id)).ToList())
                {
                    _titles.Remove(closed);
                }
            }
        }

        _snapshot = windows;
        _snapshotTick = now;
        return windows;
    }
}

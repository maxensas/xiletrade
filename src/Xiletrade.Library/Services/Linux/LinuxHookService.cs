using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux.Helper;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Receives the global shortcuts of the desktop session (X11 key grabs, also working with XWayland).
/// </summary>
/// <remarks>
/// A dedicated thread owns a connection to the X server: it grabs the keys on the root window and waits for them.
/// <br/>It is the equivalent of the hidden message window of the Windows implementation (RegisterHotKey / WM_HOTKEY).
/// <br/>A native Wayland session without XWayland has no equivalent : <see cref="IsAvailable"/> is then false.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxHookService : IHookService, IDisposable
{
    /// <summary>
    /// Message sent to the action when a registered shortcut is pressed (same value as WM_HOTKEY).
    /// </summary>
    public const int HotkeyMessage = 0x0312;

    private const int PollDelayMs = 10;
    private const int CommandTimeoutMs = 2_000;

    private readonly Action<int, nint> _action;
    private readonly ConcurrentQueue<Command> _commands = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly ManualResetEventSlim _ready = new();
    private readonly Thread _thread;

    // used by the hotkey thread only
    private readonly Dictionary<int, (int Keycode, uint Modifiers)> _hotkeys = [];
    private readonly Dictionary<(int Keycode, uint Modifiers), int> _ids = [];
    private X11Display _display;

    private volatile bool _available;
    private bool _disposed;

    /// <summary>Root window of the display, 0 when the hook is not available.</summary>
    public nint Hwnd { get; private set; }

    /// <summary>True when the connection to the X server is established and shortcuts can be registered.</summary>
    public bool IsAvailable => _available;

    /// <param name="action">Called on the hotkey thread with <see cref="HotkeyMessage"/> and the id of the shortcut.</param>
    public LinuxHookService(Action<int, nint> action)
    {
        _action = action;
        _thread = new Thread(Run)
        {
            IsBackground = true,
            Name = "Xiletrade X11 shortcuts"
        };
        _thread.Start();
        _ready.Wait(CommandTimeoutMs);
    }

    /// <summary>
    /// Registers a global shortcut. An already registered id is replaced.
    /// </summary>
    /// <param name="id">Identifier given back to the action when the shortcut is pressed.</param>
    /// <param name="windowsModifiers">Modifiers, RegisterHotKey bitmask : Alt = 1, Control = 2, Shift = 4, Win = 8.</param>
    /// <param name="virtualKey">Windows virtual-key code, as saved in the configuration.</param>
    /// <returns>False if the key is unknown or already used by another program.</returns>
    public bool RegisterHotKey(int id, int windowsModifiers, int virtualKey)
        => Execute(() => RegisterCore(id, windowsModifiers, virtualKey));

    public void UnregisterHotKey(int id) => Execute(() => UnregisterCore(id));

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _cts.Cancel();
        if (_thread.IsAlive && Environment.CurrentManagedThreadId != _thread.ManagedThreadId)
        {
            _thread.Join(CommandTimeoutMs);
        }
        _cts.Dispose();
        _ready.Dispose();
    }

    // -------- hotkey thread --------

    private void Run()
    {
        try
        {
            _display = X11Display.TryOpen();
            if (_display is not null)
            {
                _display.SelectRootKeyPress();
                Hwnd = (nint)_display.Root;
                _available = true;
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            _display = null;
        }
        finally
        {
            _ready.Set();
        }

        if (_display is null)
        {
            return;
        }

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                while (_commands.TryDequeue(out var command))
                {
                    command.Run();
                }

                while (_display.PendingEvents() > 0)
                {
                    if (_display.NextKeyPress(out int keycode, out uint state))
                    {
                        OnKeyPress(keycode, state);
                    }
                }

                Thread.Sleep(PollDelayMs);
            }
        }
        finally
        {
            _available = false;
            while (_commands.TryDequeue(out var command))
            {
                command.Run(); // wake up the callers : _available is false, nothing is executed
            }
            _display.Dispose(); // the server releases the grabs of a closed connection
            _display = null;
        }
    }

    private void OnKeyPress(int keycode, uint state)
    {
        if (!_ids.TryGetValue((keycode, state & X11Keys.RelevantStateMask), out int id))
        {
            return;
        }

        // The grab is active until the keys are released : the keys sent by the feature
        // (Ctrl+C...) would come back here instead of reaching the game.
        _display.ReleaseKeyboardGrab();
        try
        {
            _action(HotkeyMessage, id);
        }
        catch (Exception)
        {
            // the dispatcher reports its own errors : never stop the hook
        }
    }

    private bool RegisterCore(int id, int windowsModifiers, int virtualKey)
    {
        UnregisterCore(id);

        if (!X11Keys.TryGetKeysym(virtualKey, out nuint keysym))
        {
            return false;
        }
        byte keycode = _display.KeysymToKeycode(keysym);
        if (keycode is 0)
        {
            return false;
        }

        uint modifiers = X11Keys.ToX11Modifiers(windowsModifiers);
        if (_ids.ContainsKey((keycode, modifiers)) || !_display.GrabKey(keycode, modifiers))
        {
            return false;
        }

        _hotkeys[id] = (keycode, modifiers);
        _ids[(keycode, modifiers)] = id;
        return true;
    }

    private bool UnregisterCore(int id)
    {
        if (!_hotkeys.Remove(id, out var hotkey))
        {
            return false;
        }
        _ids.Remove(hotkey);
        _display.UngrabKey(hotkey.Keycode, hotkey.Modifiers);
        return true;
    }

    // -------- commands : the X connection is used by the hotkey thread only --------

    private bool Execute(Func<bool> work)
    {
        if (!_available || _disposed)
        {
            return false;
        }
        if (Environment.CurrentManagedThreadId == _thread.ManagedThreadId)
        {
            return work();
        }

        var command = new Command(work, this);
        _commands.Enqueue(command);
        return command.Wait(CommandTimeoutMs) && command.Result;
    }

    private sealed class Command(Func<bool> work, LinuxHookService owner)
    {
        private readonly ManualResetEventSlim _done = new();

        internal bool Result { get; private set; }

        internal void Run()
        {
            try
            {
                if (owner._available)
                {
                    Result = work();
                }
            }
            catch (Exception)
            {
                Result = false;
            }
            finally
            {
                _done.Set();
            }
        }

        internal bool Wait(int timeoutMs) => _done.Wait(timeoutMs);
    }
}

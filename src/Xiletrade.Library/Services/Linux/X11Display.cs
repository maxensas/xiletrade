using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using Xiletrade.Library.Services.Linux.Helper;
using Xiletrade.Library.Shared.Interop.Linux;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// One connection to the X server (X11 or XWayland).
/// </summary>
/// <remarks>
/// A connection is safe to use from several threads (every call is serialized),
/// but the calls blocking the server round trip are cheap, so one connection is
/// shared by the callers needing it, and the hotkey thread owns its own.
/// </remarks>
[SupportedOSPlatform("linux")]
internal sealed unsafe class X11Display : IDisposable
{
    // The error handler of Xlib is global to the process.
    private static readonly Lock ErrorLock = new();
    private static int _errorCount;

    private readonly Lock _lock = new();
    private readonly Dictionary<string, nuint> _atoms = [];
    private nint _display;

    internal nuint Root { get; }

    private X11Display(nint display)
    {
        _display = display;
        Root = X11.XDefaultRootWindow(display);
    }

    /// <summary>
    /// Opens a connection to the display designated by the DISPLAY environment variable.
    /// </summary>
    /// <returns>Null when X11 is not available (no DISPLAY, no libX11, unsupported architecture...).</returns>
    internal static X11Display TryOpen()
    {
        try
        {
            if (nint.Size != 8 || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
            {
                return null;
            }
            nint display = X11.XOpenDisplay(nint.Zero);
            return display == nint.Zero ? null : new X11Display(display);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            return null;
        }
    }

    // -------- Windows --------

    /// <summary>Gets the id of the active window (EWMH _NET_ACTIVE_WINDOW), 0 if none.</summary>
    internal nuint GetActiveWindow()
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return 0;
            }
            var values = GetLongs(Root, "_NET_ACTIVE_WINDOW", X11.XA_WINDOW, 1);
            return values.Length > 0 ? values[0] : 0;
        }
    }

    /// <summary>Gets the ids of the windows managed by the window manager (EWMH _NET_CLIENT_LIST).</summary>
    internal nuint[] GetClientList()
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return [];
            }
            return GetLongs(Root, "_NET_CLIENT_LIST", X11.XA_WINDOW, 4096);
        }
    }

    /// <summary>Gets the title of a window, an empty string when it has none.</summary>
    internal string GetWindowTitle(nuint window)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return string.Empty;
            }
            var title = GetText(window, "_NET_WM_NAME", Atom("UTF8_STRING"));
            title ??= GetText(window, "WM_NAME", X11.AnyPropertyType);
            return title ?? string.Empty;
        }
    }

    /// <summary>Gets the process id of the owner of a window (EWMH _NET_WM_PID), 0 if unknown.</summary>
    internal int GetWindowPid(nuint window)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return 0;
            }
            var values = GetLongs(window, "_NET_WM_PID", X11.XA_CARDINAL, 1);
            return values.Length > 0 ? (int)values[0] : 0;
        }
    }

    /// <summary>Asks the window manager to give the focus to a window.</summary>
    internal bool ActivateWindow(nuint window)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return false;
            }
            var values = GetLongs(Root, "_NET_ACTIVE_WINDOW", X11.XA_WINDOW, 1);
            nuint current = values.Length > 0 ? values[0] : 0;
            // data : source indication (2 = pager, always honored), timestamp, currently active window
            return SendRootMessage(window, "_NET_ACTIVE_WINDOW", 2, 0, current);
        }
    }

    /// <summary>Asks the window manager to close a window, like its close button would.</summary>
    internal bool CloseWindow(nuint window)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return false;
            }
            // data : timestamp, source indication (2 = pager)
            return SendRootMessage(window, "_NET_CLOSE_WINDOW", 0, 2, 0);
        }
    }

    // -------- Keyboard : synthetic events --------

    internal byte KeysymToKeycode(nuint keysym)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return 0;
            }
            return X11.XKeysymToKeycode(_display, keysym);
        }
    }

    /// <summary>Sends a key press or release to the focused window (XTest extension).</summary>
    internal bool FakeKey(byte keycode, bool press)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return false;
            }
            int status = X11.XTestFakeKeyEvent(_display, keycode, press ? 1 : 0, 0);
            X11.XFlush(_display);
            return status != 0;
        }
    }

    /// <summary>Gets the keycodes of the modifier keys currently held down.</summary>
    internal List<byte> GetHeldModifierKeycodes()
    {
        var held = new List<byte>();
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return held;
            }
            byte* keymap = stackalloc byte[32];
            X11.XQueryKeymap(_display, keymap);

            foreach (var keysym in X11Keys.ModifierKeysyms)
            {
                byte keycode = X11.XKeysymToKeycode(_display, keysym);
                if (keycode is 0 || held.Contains(keycode))
                {
                    continue;
                }
                if ((keymap[keycode / 8] & (1 << (keycode % 8))) != 0)
                {
                    held.Add(keycode);
                }
            }
        }
        return held;
    }

    // -------- Keyboard : global shortcuts --------

    /// <summary>Selects the key press events of the root window, where the shortcuts are grabbed.</summary>
    internal void SelectRootKeyPress()
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return;
            }
            X11.XSelectInput(_display, Root, (nint)X11.KeyPressMask);
            X11.XFlush(_display);
        }
    }

    /// <summary>
    /// Grabs a key combination whatever the state of CapsLock and NumLock.
    /// </summary>
    /// <returns>False when the combination cannot be grabbed (typically already used by another program).</returns>
    internal bool GrabKey(int keycode, uint modifiers)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return false;
            }
            bool ok = RunChecked(display =>
            {
                foreach (var lockState in LockStates)
                {
                    X11.XGrabKey(display, keycode, modifiers | lockState, Root, 0, X11.GrabModeAsync, X11.GrabModeAsync);
                }
            });
            if (!ok)
            {
                UngrabKeyCore(keycode, modifiers);
            }
            return ok;
        }
    }

    internal void UngrabKey(int keycode, uint modifiers)
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return;
            }
            UngrabKeyCore(keycode, modifiers);
        }
    }

    /// <summary>
    /// Ends the keyboard grab activated by a shortcut, so that the keys sent afterwards
    /// reach the focused window instead of this connection.
    /// </summary>
    internal void ReleaseKeyboardGrab()
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return;
            }
            X11.XUngrabKeyboard(_display, 0);
            X11.XFlush(_display);
        }
    }

    internal int PendingEvents()
    {
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return 0;
            }
            return X11.XPending(_display);
        }
    }

    /// <summary>
    /// Reads the next event of the queue.
    /// </summary>
    /// <returns>True if the event is a key press, false for any other event (which is dropped).</returns>
    internal bool NextKeyPress(out int keycode, out uint state)
    {
        keycode = 0;
        state = 0;
        lock (_lock)
        {
            if (_display == nint.Zero)
            {
                return false;
            }
            byte* ev = stackalloc byte[X11.XEventSize];
            X11.XNextEvent(_display, ev);
            if (*(int*)(ev + X11.OffsetType) != X11.KeyPress)
            {
                return false;
            }
            keycode = (int)*(uint*)(ev + X11.OffsetKeyCode);
            state = *(uint*)(ev + X11.OffsetKeyState);
            return true;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_display != nint.Zero)
            {
                X11.XCloseDisplay(_display);
                _display = nint.Zero;
            }
        }
    }

    // -------- private --------

    // CapsLock, NumLock, and both : the grab must match whatever their state
    private static readonly uint[] LockStates = [0, X11.LockMask, X11.Mod2Mask, X11.LockMask | X11.Mod2Mask];

    // caller holds _lock
    private void UngrabKeyCore(int keycode, uint modifiers)
    {
        RunChecked(display =>
        {
            foreach (var lockState in LockStates)
            {
                X11.XUngrabKey(display, keycode, modifiers | lockState, Root);
            }
        });
    }

    [UnmanagedCallersOnly]
    private static int OnXError(nint display, nint errorEvent)
    {
        Interlocked.Increment(ref _errorCount);
        return 0;
    }

    /// <summary>
    /// Runs requests while the Xlib error handler counts errors instead of terminating the process.
    /// </summary>
    /// <returns>True if the server reported no error.</returns>
    private bool RunChecked(Action<nint> requests)
    {
        lock (ErrorLock)
        {
            int before = Volatile.Read(ref _errorCount);
            nint previous = X11.XSetErrorHandler(&OnXError);
            try
            {
                requests(_display);
                X11.XSync(_display, 0); // errors are asynchronous : wait for them
            }
            finally
            {
                X11.XSetErrorHandler((delegate* unmanaged<nint, nint, int>)(void*)previous);
            }
            return Volatile.Read(ref _errorCount) == before;
        }
    }

    private nuint Atom(string name)
    {
        if (!_atoms.TryGetValue(name, out var atom))
        {
            atom = X11.XInternAtom(_display, name, 0);
            _atoms[name] = atom;
        }
        return atom;
    }

    // caller holds _lock
    /// <summary>
    /// Reads a window property. The window may be destroyed at any time : the error reported by the server
    /// (BadWindow) must not reach the default Xlib handler, which terminates the process.
    /// </summary>
    private bool TryGetProperty(nuint window, nuint property, int maxItems, nuint type,
        out nuint actualType, out int format, out nuint itemCount, out byte* data)
    {
        lock (ErrorLock)
        {
            int before = Volatile.Read(ref _errorCount);
            nint previous = X11.XSetErrorHandler(&OnXError);
            int status;
            try
            {
                status = X11.XGetWindowProperty(_display, window, property, 0, maxItems, 0, type,
                    out actualType, out format, out itemCount, out _, out data);
            }
            finally
            {
                X11.XSetErrorHandler((delegate* unmanaged<nint, nint, int>)(void*)previous);
            }

            if (status == 0 && data is not null && Volatile.Read(ref _errorCount) == before)
            {
                return true;
            }
            if (data is not null)
            {
                X11.XFree(data);
                data = null;
            }
            return false;
        }
    }

    // caller holds _lock
    private nuint[] GetLongs(nuint window, string property, nuint type, int maxItems)
    {
        if (!TryGetProperty(window, Atom(property), maxItems, type,
            out nuint actualType, out int format, out nuint itemCount, out byte* data))
        {
            return [];
        }
        try
        {
            if (actualType is 0 || format is not 32 || itemCount is 0)
            {
                return [];
            }
            // Xlib stores the 32-bit items in an array of C "long" (64-bit)
            var result = new nuint[(int)itemCount];
            var longs = (nuint*)data;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = longs[i];
            }
            return result;
        }
        finally
        {
            X11.XFree(data);
        }
    }

    // caller holds _lock
    private string GetText(nuint window, string property, nuint type)
    {
        if (!TryGetProperty(window, Atom(property), 256, type,
            out nuint actualType, out int format, out nuint itemCount, out byte* data))
        {
            return null;
        }
        try
        {
            if (actualType is 0 || format is not 8 || itemCount is 0)
            {
                return null;
            }
            // WM_NAME is Latin-1 when its type is STRING, UTF-8 otherwise
            return actualType == X11.XA_STRING
                ? Encoding.Latin1.GetString(data, (int)itemCount)
                : Encoding.UTF8.GetString(data, (int)itemCount);
        }
        finally
        {
            X11.XFree(data);
        }
    }

    // caller holds _lock
    private bool SendRootMessage(nuint window, string messageType, nuint data0, nuint data1, nuint data2)
    {
        byte* ev = stackalloc byte[X11.XEventSize];
        new Span<byte>(ev, X11.XEventSize).Clear();

        *(int*)(ev + X11.OffsetType) = X11.ClientMessage;
        *(int*)(ev + X11.OffsetSendEvent) = 1;
        *(nuint*)(ev + X11.OffsetWindow) = window;
        *(nuint*)(ev + X11.OffsetMessageType) = Atom(messageType);
        *(int*)(ev + X11.OffsetFormat) = 32;
        var data = (nuint*)(ev + X11.OffsetData);
        data[0] = data0;
        data[1] = data1;
        data[2] = data2;

        nint mask = (nint)(X11.SubstructureRedirectMask | X11.SubstructureNotifyMask);
        int status = X11.XSendEvent(_display, Root, 0, mask, ev);
        X11.XFlush(_display);
        return status != 0;
    }
}

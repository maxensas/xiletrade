using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Xiletrade.Library.Shared.Interop;

/// <summary>Static class used to call win32 functions and constants associated.</summary>
public static class Native
{
#if Windows
    // -------- p/invoke --------
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] 
    public static extern nint FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")] 
    public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll")] 
    public static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")] 
    public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")] 
    public static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll")]
    public static extern int GetDpiForWindow(nint hWnd);

    //sponge
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName,
        int dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern ushort RegisterClass([In] ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    //[DllImport("user32.dll")] public static extern nint SetClipboardViewer(nint hWnd);
    //[DllImport("user32.dll")] public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);
    //[DllImport("user32.dll")] public static extern int GetWindowLong(nint hWnd, int nIndex);

    // -------- delegate --------
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    // -------- Codes --------
    public const int WM_KEYUP = 0x0101;
    public const int WM_CLOSE = 0x0010;
    public const int WM_HOTKEY = 0x312;
    public const int WM_CLIPBOARDUPDATE = 0x031D;
    //public const int WM_DRAWCLIPBOARD = 0x0308;
    //public const int WM_CHANGECBCHAIN = 0x030D;
    //public const int WM_INPUT = 0x00FF;
    //public const int WM_DRAWITEM = 0x1C;
    //public const int GWL_EXSTYLE = -20;
    //public const int WS_EX_NOACTIVATE = 0x08000000;

    // -------- Struct --------
    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASS
    {
        public uint style;
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    /*
    public static nint SetClipboardViewer(nint hWnd) => NativeMethods.SetClipboardViewer(hWnd);

    public static nint FindWindow(string lpClassName, string lpWindowName) => NativeMethods.FindWindow(lpClassName, lpWindowName);

    public static nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam) => NativeMethods.SendMessage(hWnd, Msg, wParam, lParam);

    public static nint GetForegroundWindow() => NativeMethods.GetForegroundWindow();

    public static bool SetForegroundWindow(nint hWnd) => NativeMethods.SetForegroundWindow(hWnd);

    public static uint GetCurrentThreadId() => NativeMethods.GetCurrentThreadId();

    public static bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach) => NativeMethods.AttachThreadInput(idAttach, idAttachTo, fAttach);

    public static int SetWindowLong(nint hWnd, int nIndex, int dwNewLong) => NativeMethods.SetWindowLong(hWnd, nIndex, dwNewLong);

    public static int GetWindowLong(nint hWnd, int nIndex) => NativeMethods.GetWindowLong(hWnd, nIndex);

    public static bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk) => NativeMethods.RegisterHotKey(hWnd, id, fsModifiers, vk);

    public static bool UnregisterHotKey(nint hWnd, int id) => NativeMethods.UnregisterHotKey(hWnd, id);

    public static uint GetWindowThreadProcessId(nint hWnd, out uint processId) => NativeMethods.GetWindowThreadProcessId(hWnd, out processId);

    public static int GetDpiForWindow(nint hWnd) => NativeMethods.GetDpiForWindow(hWnd);
    */

    //helper
    public static bool SwitchWindow(nint windowHandle)
    {
        if (GetForegroundWindow() == windowHandle)
            return true;

        nint foregroundWindowHandle = GetForegroundWindow();
        uint currentThreadId = GetCurrentThreadId();
        uint temp;
        uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindowHandle, out temp);
        AttachThreadInput(currentThreadId, foregroundThreadId, true);
        SetForegroundWindow(windowHandle);
        AttachThreadInput(currentThreadId, foregroundThreadId, false);

        int watchdog = 0;
        while (GetForegroundWindow() != windowHandle)
        {
            if (watchdog >= 500) // 5 seconds max
            {
                return false;
            }
            Thread.Sleep(10);
            watchdog++;
        }
        return true;
    }
#endif
#if Linux
    // TODO
#endif
#if OSX
    // TODO
#endif
}
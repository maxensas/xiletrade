using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Xiletrade.Library.Shared.Interop.Windows;

/// <summary>Static class used to call win32 functions and constants associated.</summary>
[SupportedOSPlatform("windows")]
public static partial class Native
{
    // -------- Codes --------
    public const int WM_KEYUP = 0x0101;
    public const int WM_CLOSE = 0x0010;
    public const int WM_HOTKEY = 0x312;
    //public const int WM_CLIPBOARDUPDATE = 0x031D;
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
        public nint lpfnWndProc;     // pointeur de fonction (au lieu du delegate)
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public nint lpszMenuName;    // nint (au lieu de string)
        public nint lpszClassName;   // nint (au lieu de string)
    }

    // -------- p/invoke --------
    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint FindWindow(string lpClassName, string lpWindowName);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    public static partial nint SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    public static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll")]
    public static partial int GetDpiForWindow(nint hWnd);

    // internal
    [LibraryImport("kernel32.dll")]
    internal static partial uint GetCurrentThreadId();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hWnd, int id);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    //sponge
    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName,
        int dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW")]
    internal static partial nint DefWindowProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DestroyWindow(nint hWnd);

    [LibraryImport("user32.dll", EntryPoint = "RegisterClassW", SetLastError = true)]
    internal static partial ushort RegisterClass(in WNDCLASS lpWndClass);

    [LibraryImport("user32.dll", EntryPoint = "UnregisterClassW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterClass(string lpClassName, nint hInstance);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(nint hWnd);

    // -------- delegate --------
    internal delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    //helper
    internal static bool SwitchWindow(nint windowHandle)
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
}
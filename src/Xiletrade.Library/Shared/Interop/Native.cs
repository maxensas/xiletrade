using System.Runtime.InteropServices;
using System.Threading;

namespace Xiletrade.Library.Shared.Interop;

/// <summary>Static class used to call win32 functions and constants associated.</summary>
public static class Native
{
#if Windows
    public const int WM_DRAWCLIPBOARD = 0x0308;
    public const int WM_CHANGECBCHAIN = 0x030D;
    public const int WM_KEYUP = 0x0101;
    public const int WM_CLOSE = 0x0010;
    public const int WM_INPUT = 0x00FF;
    public const int WM_HOTKEY = 0x312;
    public const int VK_CONTROL = 0x11;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_NOACTIVATE = 0x08000000;

    [DllImport("user32.dll")] public static extern nint SetClipboardViewer(nint hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern nint FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")] public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll")] public static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")] public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")] public static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll")] public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll")]
    public static extern int GetDpiForWindow(nint hWnd);

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
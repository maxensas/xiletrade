using System;
using System.Runtime.InteropServices;

namespace Xiletrade.Library.Shared.Interop;

/// <summary>
/// Do not use with .NET9
/// </summary>
internal static partial class NativeMethods
{
#if Windows
    /*
    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial nint SetClipboardViewer(nint hWnd);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial nint FindWindow(string lpClassName, string lpWindowName);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll", SetLastError = false)]
    internal static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetForegroundWindow(nint hWnd);

    [LibraryImport("kernel32.dll", SetLastError = false)]
    internal static partial uint GetCurrentThreadId();

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial int GetWindowLong(nint hWnd, int nIndex);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hWnd, int id);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial int GetDpiForWindow(nint hWnd);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr GetModuleHandle(string lpModuleName);

    [LibraryImport("user32.dll")] 
    internal static partial IntPtr SetWindowsHookEx(int idHook, Mouse.Hook.LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnhookWindowsHookEx(IntPtr hhk);

    [LibraryImport("user32.dll")] 
    internal static partial short GetKeyState(int nVirtKey);

    [LibraryImport("user32.dll")] 
    internal static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll")]
    internal static partial uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Mouse.Send.INPUT[] pInputs, int cbSize);*/
#endif
}

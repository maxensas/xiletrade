using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services.Windows;

/// <summary>
/// Service used to access Clipboard by using win32 APIs
/// </summary>
public sealed class WindowsClipboardAdapterService : IClipboardAdapterService
{
    private static readonly Lock _clipboardLock = new();

    private const uint CF_TEXT = 1;
    private const uint CF_UNICODETEXT = 13;

    private const uint GMEM_MOVEABLE = 0x0002;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    public WindowsClipboardAdapterService(ILogger<WindowsClipboardAdapterService> logger)
    {
#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public bool ContainsTextData()
    {
        lock (_clipboardLock)
        {
            return IsClipboardFormatAvailable(CF_TEXT);
        }
    }

    public bool ContainsUnicodeTextData()
    {
        lock (_clipboardLock)
        {
            return IsClipboardFormatAvailable(CF_UNICODETEXT);
        }
    }

    public void Clear()
    {
        lock (_clipboardLock)
        {
            ExecuteWithClipboard(() =>
            {
                if (!EmptyClipboard())
                {
                    ThrowLastWin32Error();
                }
            });
        }
    }

    public void SetClipboard(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        lock (_clipboardLock)
        {
            ExecuteWithClipboard(() =>
            {
                if (!EmptyClipboard())
                {
                    ThrowLastWin32Error();
                }

                // CF_UNICODETEXT expects a \0 terminated UTF-16 string.
                byte[] bytes = Encoding.Unicode.GetBytes(data + '\0');

                IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);

                if (hGlobal == IntPtr.Zero)
                {
                    ThrowLastWin32Error();
                }

                try
                {
                    IntPtr memory = GlobalLock(hGlobal);

                    if (memory == IntPtr.Zero)
                    {
                        ThrowLastWin32Error();
                    }

                    try
                    {
                        Marshal.Copy(bytes, 0, memory, bytes.Length);
                    }
                    finally
                    {
                        GlobalUnlock(hGlobal);
                    }

                    /*
                    * After a successful SetClipboardData call,
                    * Windows takes ownership of hGlobal. 
                    *
                    * Therefore, you must absolutely not call
                    * GlobalFree() in this case. 
                    */
                    if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                    {
                        ThrowLastWin32Error();
                    }

                    hGlobal = IntPtr.Zero;
                }
                finally
                {
                    // Free only if SetClipboardData
                    // has not transferred ownership to Windows.
                    if (hGlobal != IntPtr.Zero)
                    {
                        GlobalFree(hGlobal);
                    }
                }
            });
        }
    }

    public string GetClipboard(bool clear)
    {
        lock (_clipboardLock)
        {
            return ExecuteWithClipboard(() =>
            {
                uint format;

                if (IsClipboardFormatAvailable(CF_UNICODETEXT))
                {
                    format = CF_UNICODETEXT;
                }
                else if (IsClipboardFormatAvailable(CF_TEXT))
                {
                    format = CF_TEXT;
                }
                else
                {
                    return string.Empty;
                }

                IntPtr hGlobal = GetClipboardData(format);

                if (hGlobal == IntPtr.Zero)
                {
                    ThrowLastWin32Error();
                }

                IntPtr memory = GlobalLock(hGlobal);

                if (memory == IntPtr.Zero)
                {
                    ThrowLastWin32Error();
                }

                try
                {
                    string result;

                    if (format == CF_UNICODETEXT)
                    {
                        result = Marshal.PtrToStringUni(memory) ?? string.Empty;
                    }
                    else
                    {
                        result = Marshal.PtrToStringAnsi(memory) ?? string.Empty;
                    }

                    if (clear)
                    {
                        if (!EmptyClipboard())
                        {
                            ThrowLastWin32Error();
                        }
                    }

                    return result;
                }
                finally
                {
                    GlobalUnlock(hGlobal);
                }
            });
        }
    }

    private static void ExecuteWithClipboard(Action action)
    {
        OpenClipboardWithRetry();

        try
        {
            action();
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static T ExecuteWithClipboard<T>(Func<T> action)
    {
        OpenClipboardWithRetry();

        try
        {
            return action();
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static void OpenClipboardWithRetry(int maxAttempts = 10, int delayMilliseconds = 10)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                return;
            }

            Thread.Sleep(delayMilliseconds);
        }

        ThrowLastWin32Error();
    }

    private static void ThrowLastWin32Error()
    {
        int error = Marshal.GetLastWin32Error();

        throw new Win32Exception(error);
    }
}
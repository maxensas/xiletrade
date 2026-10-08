using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.Library.Services.Windows;

/// <summary>
/// Provides a native Windows window for receiving system messages.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsHookService : IHookService, IDisposable
{
    public nint Hwnd { get; private set; }

    private const string WindowClassName = "SpongeWindowClass";

    private readonly string _className = $"{WindowClassName}_{Guid.NewGuid():N}";

    // Kept in a field so the GC never collects the delegate while Windows holds its function pointer.
    private readonly Native.WndProcDelegate _wndProcDelegate;

    private event EventHandler<NativeMessage> WndProcCalled;

    private bool _disposed;

    public WindowsHookService(Action<int, nint> action)
    {
        _wndProcDelegate = WndProc;

        nint classNamePtr = Marshal.StringToHGlobalUni(_className);
        try
        {
            var wc = new Native.WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                lpszClassName = classNamePtr
            };

            if (Native.RegisterClass(in wc) is 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        finally
        {
            // RegisterClass copies the class name, the buffer can be freed.
            Marshal.FreeHGlobal(classNamePtr);
        }

        Hwnd = Native.CreateWindowEx(0, WindowClassName, "", 0, 0, 0, 0, 0,
            nint.Zero, nint.Zero, nint.Zero, nint.Zero);

        //Native.AddClipboardFormatListener(Hwnd);

        // Hook the message handler
        WndProcCalled += (s, e) => action(e.Msg, e.WParam);
    }

    private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        WndProcCalled?.Invoke(this, new NativeMessage
        {
            HWnd = hWnd,
            Msg = (int)msg,
            WParam = wParam,
            LParam = lParam
        });

        return Native.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    ~WindowsHookService() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (Hwnd != nint.Zero)
        {
            // Only succeeds if called from the creating thread
            Native.DestroyWindow(Hwnd);
            Hwnd = nint.Zero;
        }

        Native.UnregisterClass(_className, nint.Zero);
    }

    // Message struct
    private class NativeMessage : EventArgs
    {
        public nint HWnd { get; set; }
        public int Msg { get; set; }
        public nint WParam { get; set; }
        public nint LParam { get; set; }
    }
}


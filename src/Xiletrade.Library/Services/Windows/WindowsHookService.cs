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
public sealed class WindowsHookService : IHookService
{
    private const string WindowClassName = "SpongeWindowClass";

    public nint Hwnd { get; private set; }

    // Kept in a field so the GC never collects the delegate while Windows holds its function pointer.
    private readonly Native.WndProcDelegate _wndProcDelegate;

    private event EventHandler<NativeMessage> WndProcCalled;

    public WindowsHookService(Action<int, nint> action)
    {
        _wndProcDelegate = WndProc;

        nint classNamePtr = Marshal.StringToHGlobalUni(WindowClassName);
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

    ~WindowsHookService()
    {
        //Native.RemoveClipboardFormatListener(Hwnd);
        Native.DestroyWindow(Hwnd);
    }

    // Message struct
    internal class NativeMessage : EventArgs
    {
        public IntPtr HWnd { get; set; }
        public int Msg { get; set; }
        public IntPtr WParam { get; set; }
        public IntPtr LParam { get; set; }
    }
}


using System;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services.Windows;

public sealed class WindowsHookService : IHookService
{
    public nint Hwnd { get; private set; }

    private readonly Native.WndProcDelegate _wndProcDelegate;

    private event EventHandler<NativeMessage> WndProcCalled;

    public WindowsHookService(Action<int, nint> action)
    {
        _wndProcDelegate = WndProc;

        var wc = new Native.WNDCLASS
        {
            lpfnWndProc = _wndProcDelegate,
            lpszClassName = "SpongeWindowClass"
        };

        Native.RegisterClass(ref wc);

        Hwnd = Native.CreateWindowEx(0, "SpongeWindowClass", "", 0, 0, 0, 0, 0,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero
        );

        // Hook the message handler
        WndProcCalled += (s, e) => action(e.Msg, e.WParam);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
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


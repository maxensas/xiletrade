using System;
using System.Windows.Forms;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Util.Hook;

/// <summary>Low-level encapsulation of a window handle and a window procedure used to catch hotkeys pressed.</summary>
internal sealed class SpongeWindow : NativeWindow, IHookService
{
    public nint Hwnd { get; private set; }

    internal event EventHandler<Message> WndProcCalled;

    internal SpongeWindow(Action<int, nint> action)
    {
        CreateHandle(new CreateParams());
        WndProcCalled += (s, e) => action(e.Msg, e.WParam);
        Hwnd = Handle;
    }

    protected override void WndProc(ref Message m)
    {
        WndProcCalled?.Invoke(this, m);
        base.WndProc(ref m);
    }
}

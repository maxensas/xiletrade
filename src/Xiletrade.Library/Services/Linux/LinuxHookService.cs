using System;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services.Linux;

//TODO
public sealed class LinuxHookService : IHookService
{
    public nint Hwnd => IntPtr.Zero;

    public LinuxHookService(Action<int, nint> action)
    {
        // Adapt to your needs:
        // - listen to the keyboard via X11
        // - read events via /dev/input
        // - use D-Bus
        // - or simply do nothing if you don't need it on Linux
    }
}
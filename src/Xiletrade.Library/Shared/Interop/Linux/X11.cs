using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Xiletrade.Library.Shared.Interop.Linux;

/// <summary>Xlib and XTest functions (with their constants) used by the Linux services.</summary>
/// <remarks>
/// Works with X11 sessions and XWayland (Path of Exile runs through Wine/Proton as an X11 client).
/// <br/>Structure sizes and offsets assume an LP64 platform (x86_64, arm64):
/// ids, atoms and keysyms are 64-bit "unsigned long".
/// </remarks>
[SupportedOSPlatform("linux")]
internal static unsafe partial class X11
{
    private const string LibX11 = "libX11.so.6";
    private const string LibXtst = "libXtst.so.6";

    // -------- Codes (X.h) --------
    internal const int KeyPress = 2;
    internal const int ClientMessage = 33;
    internal const int GrabModeAsync = 1;
    internal const int XEventSize = 192; // sizeof(XEvent) on LP64

    internal const long KeyPressMask = 1L << 0;
    internal const long SubstructureNotifyMask = 1L << 19;
    internal const long SubstructureRedirectMask = 1L << 20;

    internal const uint ShiftMask = 1u << 0;
    internal const uint LockMask = 1u << 1;    // CapsLock
    internal const uint ControlMask = 1u << 2;
    internal const uint Mod1Mask = 1u << 3;    // Alt
    internal const uint Mod2Mask = 1u << 4;    // NumLock
    internal const uint Mod4Mask = 1u << 6;    // Super / Windows

    internal const nuint AnyPropertyType = 0;
    internal const nuint XA_CARDINAL = 6;
    internal const nuint XA_STRING = 31;
    internal const nuint XA_WINDOW = 33;

    // -------- XKeyEvent / XClientMessageEvent layout (LP64) --------
    internal const int OffsetType = 0;
    internal const int OffsetSendEvent = 16;
    internal const int OffsetWindow = 32;
    internal const int OffsetKeyState = 80;
    internal const int OffsetKeyCode = 84;
    internal const int OffsetMessageType = 40;
    internal const int OffsetFormat = 48;
    internal const int OffsetData = 56;

    // -------- XErrorEvent layout (LP64) --------
    internal const int OffsetErrorCode = 32;

    // -------- p/invoke : Xlib --------
    [LibraryImport(LibX11)]
    internal static partial nint XOpenDisplay(nint displayName);

    [LibraryImport(LibX11)]
    internal static partial int XCloseDisplay(nint display);

    [LibraryImport(LibX11)]
    internal static partial nuint XDefaultRootWindow(nint display);

    [LibraryImport(LibX11, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint XInternAtom(nint display, string atomName, int onlyIfExists);

    [LibraryImport(LibX11)]
    internal static partial byte XKeysymToKeycode(nint display, nuint keysym);

    [LibraryImport(LibX11)]
    internal static partial int XGrabKey(nint display, int keycode, uint modifiers, nuint grabWindow,
        int ownerEvents, int pointerMode, int keyboardMode);

    [LibraryImport(LibX11)]
    internal static partial int XUngrabKey(nint display, int keycode, uint modifiers, nuint grabWindow);

    [LibraryImport(LibX11)]
    internal static partial int XUngrabKeyboard(nint display, nuint time);

    [LibraryImport(LibX11)]
    internal static partial int XSelectInput(nint display, nuint window, nint eventMask);

    [LibraryImport(LibX11)]
    internal static partial int XPending(nint display);

    [LibraryImport(LibX11)]
    internal static partial int XNextEvent(nint display, byte* eventReturn);

    [LibraryImport(LibX11)]
    internal static partial int XFlush(nint display);

    [LibraryImport(LibX11)]
    internal static partial int XSync(nint display, int discard);

    [LibraryImport(LibX11)]
    internal static partial int XSendEvent(nint display, nuint window, int propagate, nint eventMask, byte* eventSend);

    [LibraryImport(LibX11)]
    internal static partial int XQueryKeymap(nint display, byte* keys);

    [LibraryImport(LibX11)]
    internal static partial int XGetWindowProperty(nint display, nuint window, nuint property,
        nint longOffset, nint longLength, int delete, nuint requestedType,
        out nuint actualType, out int actualFormat, out nuint itemCount, out nuint bytesAfter, out byte* data);

    [LibraryImport(LibX11)]
    internal static partial int XFree(void* data);

    [LibraryImport(LibX11)]
    internal static partial nint XSetErrorHandler(delegate* unmanaged<nint, nint, int> handler);

    // -------- p/invoke : XTest --------
    [LibraryImport(LibXtst)]
    internal static partial int XTestFakeKeyEvent(nint display, uint keycode, int isPress, nuint delay);
}

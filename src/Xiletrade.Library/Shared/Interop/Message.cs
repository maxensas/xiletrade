using System;
using System.Runtime.InteropServices;

namespace Xiletrade.Library.Shared.Interop;

public static class Message
{
#if Windows
    // -------- p/invoke --------
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    // -------- Codes --------
    public const uint MB_OK = 0x00000000;
    public const uint MB_OKCANCEL = 0x00000001;
    public const uint MB_ABORTRETRYIGNORE = 0x00000002;
    public const uint MB_YESNOCANCEL = 0x00000003;
    public const uint MB_YESNO = 0x00000004;
    public const uint MB_RETRYCANCEL = 0x00000005;
    public const uint MB_ICONERROR = 0x00000010;
    public const uint MB_ICONQUESTION = 0x00000020;
    public const uint MB_ICONWARNING = 0x00000030;
    public const uint MB_ICONINFORMATION = 0x00000040;
    public const uint MB_TOPMOST = 0x00040000;
    public const int IDOK = 1;
    public const int IDCANCEL = 2;
    public const int IDABORT = 3;
    public const int IDRETRY = 4;
    public const int IDIGNORE = 5;
    public const int IDYES = 6;
    public const int IDNO = 7;
#endif
#if Linux
    // TODO
#endif
#if OSX
    // TODO
#endif
}

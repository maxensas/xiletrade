using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Shared.Interop.Windows;

[SupportedOSPlatform("windows")]
public static partial class Message
{
    // -------- p/invoke --------
    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int MessageBox(nint hWnd, string lpText, string lpCaption, uint uType);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool MessageBeep(uint uType);

    // -------- Codes --------
    internal const uint MB_OK = 0x00000000;
    //internal const uint MB_OKCANCEL = 0x00000001;
    //internal const uint MB_ABORTRETRYIGNORE = 0x00000002;
    //internal const uint MB_YESNOCANCEL = 0x00000003;
    internal const uint MB_YESNO = 0x00000004;
    //internal const uint MB_RETRYCANCEL = 0x00000005;
    internal const uint MB_ICONERROR = 0x00000010;
    //internal const uint MB_ICONQUESTION = 0x00000020;
    internal const uint MB_ICONWARNING = 0x00000030;
    internal const uint MB_ICONINFORMATION = 0x00000040;
    internal const uint MB_TOPMOST = 0x00040000;
    internal const int IDOK = 1;
    //internal const int IDCANCEL = 2;
    //internal const int IDABORT = 3;
    //internal const int IDRETRY = 4;
    //internal const int IDIGNORE = 5;
    internal const int IDYES = 6;
    //internal const int IDNO = 7;

    public static void MessageBeep(MessageStatus status) 
    {
        MessageBeep(status switch
        {
            MessageStatus.Information => MB_ICONINFORMATION,
            MessageStatus.Exclamation => MB_ICONWARNING,
            MessageStatus.Warning => MB_ICONWARNING,
            _ => MB_ICONERROR
        });
    }
}

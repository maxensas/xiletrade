using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Xiletrade.Library.Shared.Interop;

/// <summary>Static class used to call native functions related to Mouse and Keyboard inputs behaviors.</summary>
/// <remarks></remarks>
public static class Input
{
#if Windows
    // -------- p/invoke --------
    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    // -------- Codes --------
    public const ushort VK_CONTROL = 0x11;
    public const ushort VK_LCONTROL = 0xA2;
    public const ushort VK_RCONTROL = 0xA3;
    public const ushort VK_MENU = 0x12;   // ALT
    public const ushort VK_RMENU = 0xA5;
    public const ushort VK_SHIFT = 0x10;
    public const ushort VK_LSHIFT = 0xA0;
    public const ushort VK_RSHIFT = 0xA1;
    public const ushort VK_C = 0x43;
    public const ushort VK_V = 0x56;
    public const ushort VK_X = 0x58;
    public const ushort VK_RETURN = 0x0D;
    public const ushort VK_DELETE = 0x2E;
    public const ushort VK_HOME = 0x24;
    public const ushort VK_F = 0x46;
    public const ushort VK_BACK = 0x08;
    public const ushort VK_INSERT = 0x2D;
    public const ushort VK_END = 0x23;
    public const ushort VK_PRIOR = 0x21; // Page Up
    public const ushort VK_NEXT = 0x22; // Page Down
    public const ushort VK_UP = 0x26;
    public const ushort VK_LEFT = 0x25;
    public const ushort VK_RIGHT = 0x27;
    public const ushort VK_DOWN = 0x28;

    private const int INPUT_MOUSE = 0;
    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const uint KEYEVENTF_SCANCODE = 0x0008;

    /// <summary>Class used to bind mouse inputs entry conditionally.</summary>
    public static class MouseHook
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] private static extern short GetKeyState(int nVirtKey);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_MOUSE_LL = 14;
        private static event EventHandler MouseAction = delegate { };

        private static bool _mHotkeyProcBlock = false;
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool _initialized = false;
        private static DateTime _mouseHookCallbackTime;

        public static void Start()
        {
            if (!_initialized)
            {
                _mouseHookCallbackTime = Convert.ToDateTime(DateTime.Now);
                MouseAction += new EventHandler(MouseEvent);
                _initialized = true;
            }

            if (_hookID != IntPtr.Zero)
            {
                Stop();
            }

            var dateDiff = Convert.ToDateTime(DateTime.Now) - _mouseHookCallbackTime;
            if (dateDiff.Ticks > 3000000000) // 5 min
            {
                _mouseHookCallbackTime = Convert.ToDateTime(DateTime.Now);
            }

            _hookID = SetHook(_proc);
        }

        public static void Stop()
        {
            try
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
            catch (Exception)
            {
            }
        }

        private static void MouseEvent(object sender, EventArgs e)
        {
            if (!_mHotkeyProcBlock)
            {
                _mHotkeyProcBlock = true;
                try
                {
                    if (((MouseEventArgs)e).ZDelta is not 0)
                    {
                        Send.StartNewLeftClickTask();
                    }
                }
                catch (Exception)
                {
                }
                _mHotkeyProcBlock = false;
            }
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (MouseMessages.WM_MOUSEWHEEL == (MouseMessages)wParam && (GetKeyState(VK_CONTROL) & 0x100) != 0)
                {
                    try
                    {
                        MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                        int GET_WHEEL_DELTA_WPARAM = (short)(hookStruct.mouseData >> 0x10); // HIWORD
                        MouseEventArgs mouseEventArgs = new();
                        mouseEventArgs.ZDelta = GET_WHEEL_DELTA_WPARAM;
                        mouseEventArgs.X = hookStruct.pt.x;
                        mouseEventArgs.Y = hookStruct.pt.y;
                        MouseAction(null, mouseEventArgs);
                    }
                    catch { }
                    return new IntPtr(1);
                }
                _mouseHookCallbackTime = Convert.ToDateTime(DateTime.Now);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int VK_CONTROL = 0x11;

        private class MouseEventArgs : EventArgs
        {
            public int ZDelta { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }

    /// <remarks>Class used to send inputs asynchronously without delay.</remarks>
    internal static class Send
    {
        private static Task _leftClickTask ;

        // invoke
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        // structs
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            internal uint type;
            internal InputUnion U;
            internal static int Size
            {
                get { return Marshal.SizeOf<INPUT>(); }
            }

            internal INPUT(ushort scanCode, uint flags)
            {
                type = INPUT_KEYBOARD;
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = scanCode,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                };
            }

            internal INPUT(MOUSEEVENTF flags)
            {
                type = INPUT_MOUSE;
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        time = 0,
                        dwFlags = flags,
                        dwExtraInfo = UIntPtr.Zero,
                        dx = 1,
                        dy = 1,
                    }
                };
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal int mouseData;
            internal MOUSEEVENTF dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            internal ushort wVk;
            internal ushort wScan;
            internal uint dwFlags;
            internal uint time;
            internal IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            internal uint uMsg;
            internal ushort wParamL;
            internal ushort wParamH;
        }

        [Flags]
        private enum MOUSEEVENTF : uint
        {
            ABSOLUTE = 0x8000,
            HWHEEL = 0x01000,
            MOVE = 0x0001,
            MOVE_NOCOALESCE = 0x2000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            VIRTUALDESK = 0x4000,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100
        }

        //helper
        internal static void StartNewLeftClickTask()
        {
            if (_leftClickTask is not null && !_leftClickTask.IsCompleted)
            {
                return;
            }
            _leftClickTask = Task.Run(static () => LeftClick());
        }

        private static void LeftClick()
        {
            INPUT[] inputs = [new(MOUSEEVENTF.LEFTDOWN | MOUSEEVENTF.ABSOLUTE)
                ,new(MOUSEEVENTF.LEFTUP | MOUSEEVENTF.ABSOLUTE)];
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }

        internal static void SendKeyDown(ushort vk) => SendKey(vk, KEYEVENTF_SCANCODE);

        internal static void SendKeyUp(ushort vk) => SendKey(vk, KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP);

        private static void SendKey(ushort vk, uint flags)
        {
            ushort scanCode = (ushort)MapVirtualKey(vk, 0);

            if (IsExtendedKey(vk))
            {
                flags |= KEYEVENTF_EXTENDEDKEY;
            }

            SendInput(1, [new(scanCode, flags)], INPUT.Size);
        }

        internal static void SendUnicodeChar(char character)
        {
            INPUT[] inputs = [new(character, KEYEVENTF_UNICODE)
                , new(character, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP)];
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }

        private static bool IsExtendedKey(ushort vk)
        {
            return vk is VK_RCONTROL or VK_RMENU or VK_INSERT
                or VK_DELETE or VK_HOME or VK_END
                or VK_PRIOR or VK_NEXT or VK_UP
                or VK_LEFT or VK_RIGHT or VK_DOWN;
        }
    }

    /*
    internal static class Keyboard
    {
        private const uint KLF_ACTIVATE = 0x00000001;
        private const uint MAPVK_VK_TO_VSC = 0;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint virtualKeyCode, uint scanCode, byte[] keyboardState,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder receivingBuffer, int bufferSize,
        uint flags, IntPtr keyboardLayout);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);
        
        internal static string GetLocalizedKeyName(uint virtualKeyCode, CultureInfo culture)
        {
            uint scanCode = MapVirtualKey(virtualKeyCode, MAPVK_VK_TO_VSC);

            // Load the culture's keyboard layout
            IntPtr keyboardLayout = LoadKeyboardLayout(culture.Name, KLF_ACTIVATE);

            var keyboardState = new byte[256]; // keyboard state (eg shift, ctrl keys)
            var sb = new StringBuilder(10);

            int result = ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, sb, sb.Capacity, 0, keyboardLayout);

            if (result > 0)
            {
                return sb.ToString();
            }
            else
            {
                // Fallback: returns the standard name
                return $"VK_{virtualKeyCode:X2}";
            }
        }
    }*/
#endif
#if Linux
    // TODO
#endif
#if OSX
    // TODO
#endif
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Xiletrade.Library.Shared.Interop;

/// <summary>Static class used to call native functions related to Mouse inputs behaviors.</summary>
/// <remarks></remarks>
public static class Mouse
{
#if Windows
    /// <summary>Class used to bind mouse inputs entry conditionally.</summary>
    /// <remarks>Inspired by MouseHook class in https://github.com/phiDelPark/PoeTradeSearch/blob/master/Functions.cs</remarks>
    public static class Hook
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

    /// <remarks>Class used to send mouse input asynchronously without delay.</remarks>
    internal static class Send
    {
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        private static Task _leftClickTask ;

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
            INPUT[] i = new INPUT[1];

            i[0].type = 0;
            i[0].U.mi.time = 0;
            i[0].U.mi.dwFlags = MOUSEEVENTF.LEFTDOWN | MOUSEEVENTF.ABSOLUTE;
            i[0].U.mi.dwExtraInfo = UIntPtr.Zero;
            i[0].U.mi.dx = 1;
            i[0].U.mi.dy = 1;

            SendInput(1, i, INPUT.Size);

            i[0].type = 0;
            i[0].U.mi.time = 0;
            i[0].U.mi.dwFlags = MOUSEEVENTF.LEFTUP | MOUSEEVENTF.ABSOLUTE;
            i[0].U.mi.dwExtraInfo = UIntPtr.Zero;
            i[0].U.mi.dx = 1;
            i[0].U.mi.dy = 1;

            SendInput(1, i, INPUT.Size);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            internal uint type;
            internal InputUnion U;
            internal static int Size
            {
                get { return Marshal.SizeOf<INPUT>(); }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal int mouseData;
            internal MOUSEEVENTF dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            internal ushort wVk;
            internal ushort wScan;
            internal uint dwFlags;
            internal uint time;
            internal IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            internal uint uMsg;
            internal ushort wParamL;
            internal ushort wParamH;
        }

        [Flags]
        internal enum MOUSEEVENTF : uint
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
    }
#endif
#if Linux
    // TODO
#endif
#if OSX
    // TODO
#endif
}

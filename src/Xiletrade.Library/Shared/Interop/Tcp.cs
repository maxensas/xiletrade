using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xiletrade.Library.Shared.Interop;

/// <summary>Static class used to call win32 functions.</summary>
/// <remarks>
/// Made by /u/Umocrajen. Kills TCP connections based on PID
/// </remarks>
public sealed class Tcp
{
    private enum TcpTableClass
    {
        TcpTableBasicListener,
        TcpTableBasicConnections,
        TcpTableBasicAll,
        TcpTableOwnerPidListener,
        TcpTableOwnerPidConnections,
        TcpTableOwnerPidAll,
        TcpTableOwnerModuleListener,
        TcpTableOwnerModuleConnections,
        TcpTableOwnerModuleAll
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MibTcprowOwnerPid
    {
        public uint state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
        public uint owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MibTcptableOwnerPid
    {
        public uint dwNumEntries;
        private readonly MibTcprowOwnerPid table;
    }

#if Windows
    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(nint pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0);

    [DllImport("iphlpapi.dll")]
    private static extern int SetTcpEntry(nint pTcprow); // Run as administrator only

    public static long KillTCPConnectionForProcess()
    {
        long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        MibTcprowOwnerPid[] table = null;
        var afInet = 2;
        var buffSize = 0;
        var ret = GetExtendedTcpTable(nint.Zero, ref buffSize, true, afInet, TcpTableClass.TcpTableOwnerPidAll);
        var buffTable = Marshal.AllocHGlobal(buffSize);

        try
        {
            uint statusCode = GetExtendedTcpTable(buffTable, ref buffSize, true, afInet, TcpTableClass.TcpTableOwnerPidAll);
            if (statusCode != 0) return -1;

            var tab = (MibTcptableOwnerPid)Marshal.PtrToStructure(buffTable, typeof(MibTcptableOwnerPid));
            var rowPtr = (nint)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
            table = new MibTcprowOwnerPid[tab.dwNumEntries];

            for (var i = 0; i < tab.dwNumEntries; i++)
            {
                var tcpRow = (MibTcprowOwnerPid)Marshal.PtrToStructure(rowPtr, typeof(MibTcprowOwnerPid));
                table[i] = tcpRow;
                rowPtr = (nint)((long)rowPtr + Marshal.SizeOf(tcpRow));
            }

        }
        catch (Exception)
        {
            //Debug.Trace("Exception TCP Logout : " + ex.Message);
        }
        finally
        {
            Marshal.FreeHGlobal(buffTable);
        }

        // Kill Path Connection
        // Get window PID from handler
        var client_hWnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
        uint threadId = Native.GetWindowThreadProcessId(client_hWnd, out uint processId); // return not used

        var PathConnection = table.FirstOrDefault(t => t.owningPid == processId);
        PathConnection.state = 12;
        var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(PathConnection));
        Marshal.StructureToPtr(PathConnection, ptr, false);
        int errorCode = SetTcpEntry(ptr);
        if (errorCode > 0)
        {
            //Debug.Trace("Error while killing TCP connection (using SetTcpEntry) : " + errorCode);
        }
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime;
    }
#endif
#if Linux
    // TODO
#endif
#if OSX
    // TODO
#endif
}

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Xiletrade.Library.Shared.Interop.Windows;

/// <summary>Static class used to call win32 functions.</summary>
/// <remarks>
/// Made by /u/Umocrajen. Kills TCP connections based on PID
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed partial class Tcp
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
    private struct MibTcprowOwnerPid
    {
        public uint state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
        public uint owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MibTcptableOwnerPid
    {
        public uint dwNumEntries;
        private readonly MibTcprowOwnerPid table;
    }

    [LibraryImport("iphlpapi.dll", SetLastError = true)]
    private static partial uint GetExtendedTcpTable(nint pTcpTable, ref int dwOutBufLen, [MarshalAs(UnmanagedType.Bool)] bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0);

    [LibraryImport("iphlpapi.dll")]
    private static partial int SetTcpEntry(nint pTcprow); // Run as administrator only

    internal static long KillTCPConnectionForProcess()
    {
        using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
        {
            bool isAdmin = new System.Security.Principal.WindowsPrincipal(identity)
                .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                return -2;
            }
        }

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

            var tab = Marshal.PtrToStructure<MibTcptableOwnerPid>(buffTable);
            var rowPtr = (nint)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
            table = new MibTcprowOwnerPid[tab.dwNumEntries];

            for (var i = 0; i < tab.dwNumEntries; i++)
            {
                var tcpRow = Marshal.PtrToStructure<MibTcprowOwnerPid>(rowPtr);
                table[i] = tcpRow;
                rowPtr = (nint)((long)rowPtr + Marshal.SizeOf(tcpRow));
            }

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Tcp.KillTCPConnectionForProcess() : \n\n" + ex.Message);
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
            return -errorCode;
        }
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime;
    }
}

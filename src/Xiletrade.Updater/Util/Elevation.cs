using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Xiletrade.Updater.Util;

/// <summary>
/// The updater runs with the rights of the user : administrator rights are only requested
/// when the installation folder needs them (detected while moving the current version aside).
/// </summary>
internal static class Elevation
{
    /// <summary>
    /// Runs this updater again as administrator, with the same arguments and current directory.
    /// </summary>
    /// <returns>True when started : this process must stop. False : declined by the user (UAC) or not started.</returns>
    internal static bool TryRelaunchElevated()
    {
        if (Environment.ProcessPath is not { } exe)
        {
            return false;
        }
        var args = Environment.GetCommandLineArgs();
        var startInfo = new ProcessStartInfo(exe)
        {
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = Environment.CurrentDirectory
        };
        for (int i = 1; i < args.Length; i++)
        {
            startInfo.ArgumentList.Add(args[i]);
        }
        try
        {
            Process.Start(startInfo)?.Dispose();
            return true;
        }
        catch (Win32Exception) // 1223 : UAC declined
        {
            return false;
        }
    }
}

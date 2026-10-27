using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Linux.Helper;

/// <summary>
/// Result of a command line tool execution.
/// </summary>
/// <param name="ExitCode">Exit code, or -1 if the tool could not be started or timed out.</param>
/// <param name="Output">Standard output (only when captured).</param>
/// <param name="TimedOut">True when the tool was killed because it did not exit in time.</param>
internal readonly record struct ProcessResult(int ExitCode, string Output, bool TimedOut)
{
    internal bool Success => ExitCode is 0 && !TimedOut;

    /// <summary>The result of a tool which could not be run.</summary>
    internal static ProcessResult Failed => new(-1, string.Empty, false);
}

/// <summary>
/// Runs short-lived command line tools (xclip, wl-copy, ss...) without any shell.
/// </summary>
internal static class ProcessRunner
{
    private static readonly UTF8Encoding Utf8NoBom = new(false);

    /// <param name="fileName">Executable full path or name.</param>
    /// <param name="arguments">Arguments, passed one by one (no quoting issue).</param>
    /// <param name="stdin">Text written to the standard input, null for none.</param>
    /// <param name="captureOutput">
    /// True to read the standard output. Must be false for tools that fork a background
    /// process keeping the pipes open (xclip and wl-copy when they own the selection).
    /// </param>
    /// <param name="timeoutMs">Maximum time to wait for the tool to exit.</param>
    internal static ProcessResult Run(string fileName, IEnumerable<string> arguments, string stdin = null,
        bool captureOutput = true, int timeoutMs = 2000)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = stdin is not null,
                RedirectStandardOutput = captureOutput,
                RedirectStandardError = captureOutput,
                StandardInputEncoding = stdin is not null ? Utf8NoBom : null,
                StandardOutputEncoding = captureOutput ? Utf8NoBom : null,
                StandardErrorEncoding = captureOutput ? Utf8NoBom : null
            };
            foreach (var argument in arguments)
            {
                psi.ArgumentList.Add(argument);
            }

            using var process = Process.Start(psi);
            if (process is null)
            {
                return ProcessResult.Failed;
            }

            // read to the end : the output is returned exactly as written by the tool
            Task<string> outputTask = null;
            if (captureOutput)
            {
                outputTask = process.StandardOutput.ReadToEndAsync();
                _ = process.StandardError.ReadToEndAsync(); // drained so that the tool never blocks on a full pipe
            }

            if (stdin is not null)
            {
                process.StandardInput.Write(stdin);
                process.StandardInput.Close();
            }

            if (!process.WaitForExit(timeoutMs))
            {
                TryKill(process);
                return new ProcessResult(-1, string.Empty, true);
            }

            // the pipe is closed once the tool has exited
            string output = outputTask is not null && outputTask.Wait(timeoutMs) ? outputTask.Result : string.Empty;
            return new ProcessResult(process.ExitCode, output, false);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException
            or System.IO.IOException or NotSupportedException)
        {
            return ProcessResult.Failed;
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            // already exited
        }
    }
}

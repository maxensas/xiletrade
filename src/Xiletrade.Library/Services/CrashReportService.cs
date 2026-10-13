using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>
/// Opt-in crash reporting: nothing is sent automatically.
/// The crash is stored locally, then at the next launch the user can open a pre-filled GitHub issue
/// that he reviews and submits himself.
/// </summary>
public sealed partial class CrashReportService(IMessageAdapterService message)
{
    private const string Template = "crash_report.yml";
    private const int MaxUrlLength = 7000; // browsers/GitHub reject very long URLs
    private static readonly string PendingFile = Path.GetFullPath("Xiletrade.crash");

    /// <summary>
    /// To call from the global exception handler: no UI here, the process is going down.
    /// </summary>
    public void SaveReport(Exception exception)
    {
        File.WriteAllText(PendingFile, Sanitize(exception.ToString()), Encoding.UTF8);
    }

    /// <summary>
    /// To call once at startup, when the UI/message service is ready.
    /// </summary>
    public void OfferPendingReport()
    {
        if (!File.Exists(PendingFile))
        {
            return;
        }

        var report = File.ReadAllText(PendingFile, Encoding.UTF8);
        File.Delete(PendingFile); // deleted first: never ask twice, even if the user declines

        var accepted = message.ShowResult(Resources.Resources.Error035_ReportOnGithub,
            Resources.Resources.Error034_XCrashReport, MessageStatus.Warning, yesNo: true);

        if (accepted)
        {
            Process.Start(new ProcessStartInfo { FileName = BuildUrl(report), UseShellExecute = true });
        }
    }

    private static string BuildUrl(string report)
    {
        var title = "[Crash] " + FirstLine(report);
        var version = Common.GetFileVersion();
        var os = RuntimeInformation.OSDescription;

        string Build(string trace) => $"{Strings.Url.Issues}?template={Template}" +
            $"&title={Uri.EscapeDataString(title)}" +
            $"&version={Uri.EscapeDataString(version)}" +
            $"&os={Uri.EscapeDataString(os)}" +
            $"&stacktrace={Uri.EscapeDataString(trace)}";

        var url = Build(report);
        while (url.Length > MaxUrlLength && report.Length > 200)
        {
            report = report[..(report.Length * 3 / 4)];
            url = Build(report + "\n[truncated - please attach Xiletrade.log]");
        }
        return url;
    }

    private static string FirstLine(string text)
    {
        var line = text.Split('\n', 2)[0].Trim();
        return line.Length > 100 ? line[..100] : line;
    }

    /// <summary>
    /// Removes private data before the report leaves the machine.
    /// </summary>
    private static string Sanitize(string text)
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(profile))
        {
            text = text.Replace(profile, "%USERPROFILE%", StringComparison.OrdinalIgnoreCase);
        }
        return SessionIdRegex().Replace(text, "POESESSID=<removed>");
    }

    [GeneratedRegex(@"POESESSID\s*[=:]\s*\w+", RegexOptions.IgnoreCase)]
    private static partial Regex SessionIdRegex();
}

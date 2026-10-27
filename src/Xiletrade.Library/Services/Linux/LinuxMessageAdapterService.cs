using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux.Helper;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Provides message dialogs through the dialog tools of the desktop : zenity (GTK), kdialog (KDE) or xmessage (X11).
/// </summary>
/// <remarks>
/// Default implementation of the library : a UI framework provides its own implementation, which then replaces this one.
/// <br/>The dialogs are separate processes : they never block the UI thread of the application.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxMessageAdapterService : IMessageAdapterService
{
    // A dialog stays on screen until the user closes it : no timeout.
    private const int NoTimeout = System.Threading.Timeout.Infinite;

    private enum DialogTool
    {
        None,
        Zenity,
        Kdialog,
        Xmessage
    }

    private readonly Lazy<(DialogTool Tool, string Path)> _tool = new(DetectTool);

    public LinuxMessageAdapterService(ILogger<LinuxMessageAdapterService> logger)
    {
#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void Show(string message, string caption, MessageStatus status)
    {
        _ = Task.Run(() => ShowResult(message, caption, status));
    }

    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        var (tool, path) = _tool.Value;
        if (tool is DialogTool.None)
        {
            Console.Error.WriteLine($"{caption} : {message}");
            return false;
        }

        var result = ProcessRunner.Run(path, BuildArguments(tool, message, caption, status, yesNo),
            captureOutput: false, timeoutMs: NoTimeout);
        // zenity and kdialog : 0 = OK or Yes, 1 = No or closed. xmessage : the exit code is the one of the button.
        return result.ExitCode is 0;
    }

    public Task<bool> ShowResultAsync(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        return Task.Run(() => ShowResult(message, caption, status, yesNo));
    }

    private static List<string> BuildArguments(DialogTool tool, string message, string caption,
        MessageStatus status, bool yesNo)
    {
        switch (tool)
        {
            case DialogTool.Zenity:
                return
                [
                    yesNo ? "--question" : status switch
                    {
                        MessageStatus.Information => "--info",
                        MessageStatus.Error => "--error",
                        _ => "--warning"
                    },
                    "--width=400", "--title", caption, "--text", EscapeMarkup(message)
                ];

            case DialogTool.Kdialog:
                return
                [
                    yesNo ? "--yesno" : status switch
                    {
                        MessageStatus.Information => "--msgbox",
                        MessageStatus.Error => "--error",
                        _ => "--sorry"
                    },
                    message, "--title", caption
                ];

            default: // xmessage
                return
                [
                    "-center", "-title", caption, "-buttons", yesNo ? "Yes:0,No:1" : "OK:0", message
                ];
        }
    }

    /// <summary>
    /// zenity reads Pango markup : the characters of the markup are escaped so the message is displayed as is.
    /// </summary>
    private static string EscapeMarkup(string text)
        => (text ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static (DialogTool Tool, string Path) DetectTool()
    {
        var zenity = ExecutableLocator.Find("zenity");
        if (zenity is not null)
        {
            return (DialogTool.Zenity, zenity);
        }
        var kdialog = ExecutableLocator.Find("kdialog");
        if (kdialog is not null)
        {
            return (DialogTool.Kdialog, kdialog);
        }
        var xmessage = ExecutableLocator.Find("xmessage");
        return xmessage is not null ? (DialogTool.Xmessage, xmessage) : (DialogTool.None, null);
    }
}

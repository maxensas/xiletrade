using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using Xiletrade.Library.Models.Application.Adapter;
using Xiletrade.Library.Services.Linux.Helper;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Provides access to the system clipboard through command line tools :
/// wl-clipboard (Wayland session), xclip or xsel (X11 session).
/// </summary>
/// <remarks>
/// Works from any thread and without any UI framework, unlike the clipboard of the UI toolkit
/// which is bound to the UI thread and to the focus of its windows.
/// <br/>Only plain text is supported.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxClipboardAdapter : IClipboardAdapter
{
    private static readonly Lock _clipboardLock = new();

    private readonly Lazy<ClipboardTool> _tool = new(DetectTool);

    private enum ToolKind
    {
        None,
        WlClipboard,
        Xclip,
        Xsel
    }

    private sealed record ClipboardTool(ToolKind Kind, string Copy, string Paste);

    /// <summary>
    /// Name of the missing package to suggest to the user, null when a tool is available.
    /// </summary>
    public string MissingToolHint => _tool.Value.Kind is ToolKind.None ? IsWayland ? "wl-clipboard" : "xclip" : null;

    public bool ContainsTextData() => GetClipboard(false).Length > 0;

    public bool ContainsUnicodeTextData() => ContainsTextData();

    public void Clear()
    {
        lock (_clipboardLock)
        {
            ClearCore(_tool.Value);
        }
    }

    public void SetClipboard(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        lock (_clipboardLock)
        {
            var tool = _tool.Value;
            // The tool forks and keeps serving the selection : its output must not be waited for.
            var result = tool.Kind switch
            {
                ToolKind.WlClipboard => ProcessRunner.Run(tool.Copy, ["--type", "text/plain;charset=utf-8"], data, captureOutput: false),
                ToolKind.Xclip => ProcessRunner.Run(tool.Copy, ["-selection", "clipboard", "-in"], data, captureOutput: false),
                ToolKind.Xsel => ProcessRunner.Run(tool.Copy, ["--clipboard", "--input"], data, captureOutput: false),
                _ => ProcessResult.Failed
            };
            if (tool.Kind is not ToolKind.None && !result.Success)
            {
                throw new InvalidOperationException($"{System.IO.Path.GetFileName(tool.Copy)} failed to set the clipboard.");
            }
        }
    }

    public string GetClipboard(bool clear)
    {
        lock (_clipboardLock)
        {
            var tool = _tool.Value;
            var result = tool.Kind switch
            {
                ToolKind.WlClipboard => ProcessRunner.Run(tool.Paste, ["--no-newline", "--type", "text"]),
                ToolKind.Xclip => ReadWithXclip(tool),
                ToolKind.Xsel => ProcessRunner.Run(tool.Paste, ["--clipboard", "--output"]),
                _ => ProcessResult.Failed
            };

            // An empty clipboard, or one without text, makes these tools exit with an error.
            string text = result.Success ? result.Output : string.Empty;

            if (clear && text.Length > 0)
            {
                ClearCore(tool);
            }
            return text;
        }
    }

    // -------- private --------

    private static bool IsWayland => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

    // UTF8_STRING first, the owner of the selection may only offer the legacy STRING target
    private static ProcessResult ReadWithXclip(ClipboardTool tool)
    {
        var result = ProcessRunner.Run(tool.Paste, ["-selection", "clipboard", "-out", "-target", "UTF8_STRING"]);
        return result.Success ? result : ProcessRunner.Run(tool.Paste, ["-selection", "clipboard", "-out"]);
    }

    // caller holds _clipboardLock
    private static void ClearCore(ClipboardTool tool)
    {
        switch (tool.Kind)
        {
            case ToolKind.WlClipboard:
                ProcessRunner.Run(tool.Copy, ["--clear"], captureOutput: false);
                break;
            case ToolKind.Xclip:
                ProcessRunner.Run(tool.Copy, ["-selection", "clipboard", "-in"], stdin: string.Empty, captureOutput: false);
                break;
            case ToolKind.Xsel:
                ProcessRunner.Run(tool.Copy, ["--clipboard", "--clear"], captureOutput: false);
                break;
        }
    }

    private static ClipboardTool DetectTool()
    {
        var candidates = new List<ClipboardTool>();
        var wlCopy = ExecutableLocator.Find("wl-copy");
        var wlPaste = ExecutableLocator.Find("wl-paste");
        var xclip = ExecutableLocator.Find("xclip");
        var xsel = ExecutableLocator.Find("xsel");

        if (wlCopy is not null && wlPaste is not null)
        {
            candidates.Add(new ClipboardTool(ToolKind.WlClipboard, wlCopy, wlPaste));
        }
        if (xclip is not null)
        {
            candidates.Add(new ClipboardTool(ToolKind.Xclip, xclip, xclip));
        }
        if (xsel is not null)
        {
            candidates.Add(new ClipboardTool(ToolKind.Xsel, xsel, xsel));
        }

        // In a Wayland session the Wayland tools are preferred, in an X11 session the X11 ones.
        foreach (var candidate in candidates)
        {
            bool isWaylandTool = candidate.Kind is ToolKind.WlClipboard;
            if (isWaylandTool == IsWayland)
            {
                return candidate;
            }
        }
        return candidates.Count > 0 ? candidates[0] : new ClipboardTool(ToolKind.None, null, null);
    }
}

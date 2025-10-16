using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services.Linux;

//TOTEST
public sealed class LinuxSendInputService : ISendInputService
{
    private static IServiceProvider _serviceProvider;

    private readonly string? _xdotoolPath;
    private readonly string? _wtypePath;
    private readonly bool _isWayland;

    public LinuxSendInputService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _isWayland = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

        _xdotoolPath = FindTool("xdotool");
        _wtypePath = FindTool("wtype");

        if (_isWayland && _wtypePath is null)
        {
            WarnMissingTool("wtype", "sudo apt install wtype");
        }
        else if (!_isWayland && _xdotoolPath is null)
        {
            WarnMissingTool("xdotool", "sudo apt install xdotool");
        }
    }

    public void PasteClipboard()
    {
        // Ctrl+V + Enter
        SendKeys("ctrl+v", "Return");
    }

    public void CleanChatAndPasteClipboard()
    {
        string chatKey = GetChatKey();
        SendKeys($"{chatKey}+Home", "Delete");
        PasteClipboard();
    }

    public void ReplyLastWhisper()
    {
        string chatKey = GetChatKey();
        SendKeys($"ctrl+{chatKey}");
        PasteClipboard();
    }

    public void CopyItemDetailAdvanced()
    {
        SendKeys("ctrl+alt+c");
    }

    public void CopyItemDetail()
    {
        SendKeys("ctrl+c");
    }

    public void CutLastWhisperToClipboard()
    {
        string chatKey = GetChatKey();
        SendKeys($"ctrl+{chatKey}", "Shift+Home", "ctrl+x");
    }

    public void StartMouseWheelCapture()
    {
        Shared.Interop.Input.MouseHook.Start();
    }

    public void StopMouseWheelCapture()
    {
        Shared.Interop.Input.MouseHook.Stop();
    }

    public void CleanPoeSearchBarAndPasteClipboard()
    {
        SendKeys("ctrl+f", "Delete");
        PasteClipboard();
    }

    // ---------------- Helpers ----------------

    private string GetChatKey()
    {
        return _serviceProvider.GetRequiredService<HotKeyService>().ChatKey;
    }

    private void SendKeys(params string[] sequences)
    {
        foreach (var key in sequences)
        {
            if (_isWayland && _wtypePath != null)
            {
                RunProcess(_wtypePath, $"--text \"{KeyToString(key)}\"");
            }
            else if (!_isWayland && _xdotoolPath != null)
            {
                RunProcess(_xdotoolPath, $"key --clearmodifiers {key}");
            }
        }
    }

    private static string? FindTool(string toolName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = toolName,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string? path = process?.StandardOutput.ReadLine();
            process?.WaitForExit();

            return File.Exists(path ?? "") ? path : null;
        }
        catch
        {
            return null;
        }
    }

    private void RunProcess(string command, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error running command: {command} {args}\n{ex.Message}");
        }
    }

    private void WarnMissingTool(string tool, string suggestion)
    {
        Console.Error.WriteLine($"❌ Required tool '{tool}' is not installed.");
        Console.Error.WriteLine($"👉 You can install it with: {suggestion}");
    }

    private static string KeyToString(string key)
    {
        return key switch
        {
            "ctrl+c" => "\u0003", // ETX
            "ctrl+v" => "\u0016", // SYN
            "Return" => "\n",
            "Delete" => "\u007F",
            _ => key
        };
    }
}

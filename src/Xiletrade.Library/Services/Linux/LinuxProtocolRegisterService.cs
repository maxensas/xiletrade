using System;
using System.Diagnostics;
using System.IO;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services.Linux;

//TOTEST
public class LinuxProtocolRegisterService : IProtocolRegisterService
{
    private static IServiceProvider _serviceProvider;

    private const string ProtocolName = "Xiletrade";
    private readonly string _executablePath;

    public LinuxProtocolRegisterService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _executablePath = Environment.ProcessPath ?? throw new InvalidOperationException("Executable path not found.");
    }

    public void RegisterOrUpdateProtocol()
    {
        if (!IsXdgMimeAvailable())
        {
            Console.WriteLine("⚠️ xdg-mime is not installed. Trying to install with apt...");

            if (!TryInstallXdgMime())
            {
                Console.WriteLine("❌ Automatic installation failed. Please install xdg-utils manually :");
                Console.WriteLine("sudo apt install xdg-utils");
                return;
            }
        }

        string desktopFileName = $"{ProtocolName}-protocol.desktop";
        string localAppPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "share", "applications"
        );
        string desktopFilePath = Path.Combine(localAppPath, desktopFileName);

        Directory.CreateDirectory(localAppPath);

        string desktopEntry = $"""
            [Desktop Entry]
            Name={ProtocolName} Protocol Handler
            Exec="{_executablePath}" %u
            Type=Application
            NoDisplay=true
            MimeType=x-scheme-handler/{ProtocolName};
            """;

        File.WriteAllText(desktopFilePath, desktopEntry);
        Console.WriteLine($"✅ .desktop file written : {desktopFilePath}");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "xdg-mime",
            ArgumentList = { "default", desktopFileName, $"x-scheme-handler/{ProtocolName}" },
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        process?.WaitForExit();

        Console.WriteLine("✅ Custom protocol recorded !");
    }

    private static bool IsXdgMimeAvailable()
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                ArgumentList = { "xdg-mime" },
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryInstallXdgMime()
    {
        try
        {
            var install = Process.Start(new ProcessStartInfo
            {
                FileName = "sudo",
                ArgumentList = { "apt", "install", "-y", "xdg-utils" },
                UseShellExecute = false
            });

            install?.WaitForExit();
            return install?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services.Windows;

public class WindowsProtocolRegisterService : IProtocolRegisterService
{
    public WindowsProtocolRegisterService(ILogger<WindowsProtocolRegisterService> logger)
    {
        RegisterOrUpdateProtocol();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }
    /// <summary>
    /// Automatically register or update the custom protocol handler in the registry
    /// </summary>
    private static void RegisterOrUpdateProtocol()
    {
#if Windows
#pragma warning disable CA1416 // Validate platform compatibility
        string registryPath = $@"Software\Classes\{IProtocolRegisterService.ProtocolName}";
        string currentExePath = Environment.ProcessPath;

        // Create or open the protocol registry key
        using Microsoft.Win32.RegistryKey protocolKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryPath);
        protocolKey.SetValue("", $"URL:{IProtocolRegisterService.ProtocolName} Protocol");
        protocolKey.SetValue("URL Protocol", "");

        // Set or update the icon path
        using (Microsoft.Win32.RegistryKey iconKey = protocolKey.CreateSubKey("DefaultIcon"))
        {
            object existingIcon = iconKey.GetValue("");
            if (existingIcon is null || existingIcon.ToString() != currentExePath)
            {
                iconKey.SetValue("", currentExePath);
            }
        }

        // Set or update the command used when launching the app

        using Microsoft.Win32.RegistryKey commandKey = protocolKey.CreateSubKey(@"shell\open\command");

        string expectedCommand = $"\"{currentExePath}\" \"%1\"";
        object existingCommand = commandKey.GetValue("");

        if (existingCommand is null || existingCommand.ToString() != expectedCommand)
        {
            commandKey.SetValue("", expectedCommand);
        }
    }
#pragma warning restore CA1416 // Validate platform compatibility
#endif
}

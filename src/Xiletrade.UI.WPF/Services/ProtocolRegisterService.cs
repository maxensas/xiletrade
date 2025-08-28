using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Services;

// windows implementation
public class ProtocolRegisterService : IProtocolRegisterService
{
    private static IServiceProvider _serviceProvider;

    private const string ProtocolName = "Xiletrade";

    public ProtocolRegisterService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void RegisterOrUpdateProtocol()
    {
        string registryPath = $@"Software\Classes\{ProtocolName}";
        string currentExePath = Environment.ProcessPath;

        // Create or open the protocol registry key
        using RegistryKey protocolKey = Registry.CurrentUser.CreateSubKey(registryPath);
        protocolKey.SetValue("", $"URL:{ProtocolName} Protocol");
        protocolKey.SetValue("URL Protocol", "");

        // Set or update the icon path
        using (RegistryKey iconKey = protocolKey.CreateSubKey("DefaultIcon"))
        {
            object existingIcon = iconKey.GetValue("");
            if (existingIcon is null || existingIcon.ToString() != currentExePath)
            {
                iconKey.SetValue("", currentExePath);
            }
        }

        // Set or update the command used when launching the app
        using RegistryKey commandKey = protocolKey.CreateSubKey(@"shell\open\command");
        string expectedCommand = $"\"{currentExePath}\" \"%1\"";
        object existingCommand = commandKey.GetValue("");

        if (existingCommand is null || existingCommand.ToString() != expectedCommand)
        {
            commandKey.SetValue("", expectedCommand);
        }
    }
}

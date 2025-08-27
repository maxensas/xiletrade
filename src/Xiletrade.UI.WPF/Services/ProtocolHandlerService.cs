using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Services;

public class ProtocolHandlerService : IProtocolHandlerService, IDisposable
{
    private static IServiceProvider _serviceProvider;

    private const string PipeName = "XiletradePipe";
    private const string ProtocolName = "Xiletrade";

    private CancellationTokenSource _cts;
    private Task _listeningTask;

    public ProtocolHandlerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void HandleUrl(string url)
    {
        string urlPrefix = "://open/";
        var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        if (url.StartsWith(ProtocolName + urlPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            string[] items = url.Split(urlPrefix);
            var item = items.Length > 1 ? items[1] : string.Empty;
            service.Show($"Opening item: {item}", "Protocol Handler", Library.Models.Enums.MessageStatus.Information);
            // TODO: Handle item
        }
        else
        {
            service.Show($"Unknown protocol URL: {url}", "Protocol Handler", Library.Models.Enums.MessageStatus.Error);
        }
    }

    public void StartListening()
    {
        _cts = new CancellationTokenSource();
        _listeningTask = Task.Run(() => ListenLoop(_cts.Token), _cts.Token);
    }

    public void StopListening()
    {
        _cts?.Cancel();
        try
        {
            _listeningTask?.Wait(1000);
        }
        catch { /* ignore */ }
        _cts?.Dispose();
    }

    private void ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                var connectTask = server.WaitForConnectionAsync(token);

                connectTask.Wait(token); // Will throw on cancellation

                using var reader = new StreamReader(server);
                string message = reader.ReadLine();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    _serviceProvider.GetRequiredService<INavigationService>()
                        .DelegateActionToUiThread(new (() => { HandleUrl(message); }));
                }

                server.Disconnect();
            }
            catch (OperationCanceledException)
            {
                break; // graceful shutdown
            }
            catch
            {
                // Optional: log or ignore errors during listen loop
            }
        }
    }

    public void SendToRunningInstance(string url)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(500); // ms

            using var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine(url);
        }
        catch
        {
            // Optional: log or ignore
        }
    }

    public void RegisterOrUpdateProtocol()
    {
        string registryPath = $@"Software\Classes\{ProtocolName}";
        string currentExePath = Environment.ProcessPath;

        // Create or open the protocol registry key
        using RegistryKey protocolKey = Registry.CurrentUser.CreateSubKey(registryPath);
        protocolKey.SetValue("", "URL:MonApp Protocol");
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

    public void Dispose()
    {
        StopListening();
    }
}

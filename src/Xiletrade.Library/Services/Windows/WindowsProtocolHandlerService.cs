using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Windows;

/// <summary>
/// Start a named pipe server that listens for protocol URLs sent by secondary instances.
/// </summary>
/// <remarks>
/// Registers the protocol and starts listening upon instantiation.
/// </remarks>
[SupportedOSPlatform("windows")]
public class WindowsProtocolHandlerService : IProtocolHandlerService, IDisposable
{
    private readonly IMessageAdapterService _message;
    private readonly ITokenService _token;
    private readonly IFileLoggerService _fileLogger;
    private readonly ILogger<WindowsProtocolHandlerService> _logger;
    private readonly StartupArguments _startup;
    private readonly IUIService _ui;

    private const string PipeName = "XiletradePipe";
    private CancellationTokenSource _cts;
    private Task _listeningTask;
    private bool _init;

    public WindowsProtocolHandlerService(IMessageAdapterService message, ITokenService token, 
        IFileLoggerService fileLogger, ILogger<WindowsProtocolHandlerService> logger,
        StartupArguments startup, IUIService ui)
    {
        _message = message;
        _token = token;
        _fileLogger = fileLogger;
        _logger = logger;
        _startup = startup;
        _ui = ui;

        RegisterOrUpdateProtocol();
        StartListening();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    /// <summary>
    /// Automatically register or update the custom protocol handler in the registry
    /// </summary>
    private static void RegisterOrUpdateProtocol()
    {
        string registryPath = $@"Software\Classes\{IProtocolHandlerService.ProtocolName}";
        string currentExePath = Environment.ProcessPath;

        // Create or open the protocol registry key
        using Microsoft.Win32.RegistryKey protocolKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryPath);
        protocolKey.SetValue("", $"URL:{IProtocolHandlerService.ProtocolName} Protocol");
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

    /// <summary>
    /// Starts the pipe server
    /// </summary>
    private void StartListening()
    {
        _cts = new CancellationTokenSource();
        _listeningTask = Task.Run(() => ListenLoop(_cts.Token), _cts.Token);

        // If a protocol URL was passed on first launch, handle it now
        if (!_init && _startup.HasArgs)
        {
            HandleUrl(_startup.Args);
            _init = true;
        }
    }

    /// <summary>
    /// Stop listening server Task / Break infinite loop.
    /// </summary>
    private void StopListening()
    {
        _cts?.Cancel();
        try
        {
            _listeningTask?.Wait(1000);
        }
        catch { /* ignore */ }
        _cts?.Dispose();
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
        catch (Exception ex)
        {
            _fileLogger.Log(ex);
        }
    }

    public void Dispose()
    {
        StopListening();
        GC.SuppressFinalize(this);
    }

    //private
    private void HandleUrl(string url)
    {
        var uri = new Uri(url);
        if (uri.Host is "oauth")
        {
            _token.TryInitToken(uri.Query);
            return;
        }
        _message.Show($"Unknown protocol URL: {url}", "Protocol Handler", MessageStatus.Error);
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
                    _ui.Invoke(new(() => { HandleUrl(message); }));
                }

                server.Disconnect();
            }
            catch (OperationCanceledException)
            {
                break; // graceful shutdown
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing ListenLoop()");
            }
        }
    }
}

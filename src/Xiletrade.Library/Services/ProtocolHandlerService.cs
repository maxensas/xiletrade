using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.TaskBar;

namespace Xiletrade.Library.Services;

/// <summary>
/// Named pipe server that listens for protocol URLs sent by secondary instances.
/// </summary>
public class ProtocolHandlerService : IProtocolHandlerService, IDisposable
{
    private readonly IMessageAdapterService _message;
    private readonly ITokenService _token;
    private readonly IFileLoggerService _fileLogger;
    private readonly ILogger<ProtocolHandlerService> _logger;
    private readonly StartupArguments _startup;
    private readonly UIService _ui;
    private readonly TaskBarViewModel _taskBarVm;

    private const string PipeName = "XiletradePipe";
    private CancellationTokenSource _cts;
    private Task _listeningTask;
    private bool _init;

    public ProtocolHandlerService(IMessageAdapterService message, ITokenService token, 
        IFileLoggerService fileLogger, ILogger<ProtocolHandlerService> logger,
        StartupArguments startup, UIService ui, TaskBarViewModel taskBarVm)
    {
        _message = message;
        _token = token;
        _fileLogger = fileLogger;
        _logger = logger;
        _startup = startup;
        _ui = ui;
        _taskBarVm = taskBarVm;

        StartListening();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
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
    }

    //private
    private void HandleUrl(string url)
    {
        var uri = new Uri(url);
        if (uri.Host is "oauth")
        {
            _token.TryInitToken(uri.Query);
            _taskBarVm.RefreshAuthenticationState();
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
                    _ui.DelegateActionToUiThread(new(() => { HandleUrl(message); }));
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

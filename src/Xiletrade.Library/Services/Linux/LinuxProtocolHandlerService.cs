using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux.Helper;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Start a local socket server that listens for protocol URLs sent by secondary instances.
/// </summary>
/// <remarks>
/// Registers the protocol (freedesktop.org desktop entry) and starts listening upon instantiation.
/// <br/>On Linux, a named pipe is a Unix domain socket restricted to the current user.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxProtocolHandlerService : IProtocolHandlerService, IDisposable
{
    private readonly IMessageAdapterService _message;
    private readonly ITokenService _token;
    private readonly IFileLoggerService _fileLogger;
    private readonly ILogger<LinuxProtocolHandlerService> _logger;
    private readonly StartupArguments _startup;
    private readonly IUIService _ui;

    private const string PipeName = "XiletradePipe";
    private const string DesktopFileName = "xiletrade-url-handler.desktop";
    private CancellationTokenSource _cts;
    private Task _listeningTask;
    private bool _init;

    public LinuxProtocolHandlerService(IMessageAdapterService message, ITokenService token,
        IFileLoggerService fileLogger, ILogger<LinuxProtocolHandlerService> logger,
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
    /// Registers or updates the handler of the custom protocol for the current user :
    /// a desktop entry in ~/.local/share/applications associated to x-scheme-handler/xiletrade.
    /// </summary>
    private void RegisterOrUpdateProtocol()
    {
        try
        {
            var command = GetCommand();
            if (command is null)
            {
                return;
            }

            string scheme = IProtocolHandlerService.ProtocolName.ToLowerInvariant();
            string content = DesktopEntry.Build(IProtocolHandlerService.ProtocolName, scheme, command);

            string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (string.IsNullOrEmpty(dataHome))
            {
                dataHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
            }
            string directory = Path.Combine(dataHome, "applications");
            string file = Path.Combine(directory, DesktopFileName);

            if (File.Exists(file) && File.ReadAllText(file) == content)
            {
                return; // already registered
            }

            Directory.CreateDirectory(directory);
            File.WriteAllText(file, content);

            // Both tools are optional : without them the desktop environment finds the entry on its own at its next scan.
            var xdgMime = ExecutableLocator.Find("xdg-mime");
            if (xdgMime is not null)
            {
                ProcessRunner.Run(xdgMime, ["default", DesktopFileName, "x-scheme-handler/" + scheme], captureOutput: false);
            }
            var updateDatabase = ExecutableLocator.Find("update-desktop-database");
            if (updateDatabase is not null)
            {
                ProcessRunner.Run(updateDatabase, [directory], captureOutput: false);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _fileLogger.Log(ex);
        }
    }

    /// <summary>
    /// Command line of the application : the executable, or "dotnet" followed by the assembly.
    /// </summary>
    private static string[] GetCommand()
    {
        string processPath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(processPath))
        {
            return null;
        }
        bool isDotnetHost = string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.Ordinal);
        if (!isDotnetHost)
        {
            return new[] { processPath };
        }

        string assembly = System.Reflection.Assembly.GetEntryAssembly()?.Location;
        return string.IsNullOrEmpty(assembly) ? null : new[] { processPath, assembly };
    }

    /// <summary>
    /// Starts the socket server
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
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.CurrentUserOnly);
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
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _message.Show($"Unknown protocol URL: {url}", "Protocol Handler", MessageStatus.Error);
            return;
        }
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
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                var connectTask = server.WaitForConnectionAsync(token);

                connectTask.Wait(token); // Will throw on cancellation

                using var reader = new StreamReader(server);
                string message = reader.ReadLine();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    _ui.Invoke(new(() => { HandleUrl(message); }));
                }
            }
            catch (OperationCanceledException)
            {
                break; // graceful shutdown
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                break; // Wait(token) wraps the cancellation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing ListenLoop()");
                if (token.WaitHandle.WaitOne(100))
                {
                    break; // avoids a busy loop if the socket cannot be created
                }
            }
        }
    }
}

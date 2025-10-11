using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

public class ProtocolHandlerService : IProtocolHandlerService, IDisposable
{
    private static IServiceProvider _serviceProvider;
    private static IMessageAdapterService _message;

    private const string PipeName = "XiletradePipe";

    private CancellationTokenSource _cts;
    private Task _listeningTask;

    public ProtocolHandlerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _message = _serviceProvider.GetRequiredService<IMessageAdapterService>();
    }

    public void HandleUrl(string url)
    {
        var uri = new Uri(url);
        if (uri.Host is "oauth")
        {
            _serviceProvider.GetRequiredService<ITokenService>().TryParseQuery(uri.Query);
            _serviceProvider.GetRequiredService<XiletradeService>().RefreshTokenState();
            return;
        }
        _message.Show($"Unknown protocol URL: {url}", "Protocol Handler", MessageStatus.Error);
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
                        .DelegateActionToUiThread(new(() => { HandleUrl(message); }));
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

    public void Dispose()
    {
        StopListening();
    }
}

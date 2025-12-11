using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Services;

/// <summary>Main service used to launch Xiletrade application.</summary>
/// <remarks>Initialize all services.</remarks>
public sealed class XiletradeService
{
    private static IServiceProvider _serviceProvider;
    private static string _args;
    private bool _started;

    // public members
    public static SynchronizationContext UiThreadContext { get; } = SynchronizationContext.Current;
    public nint MainHwnd { get; set; }

    public XiletradeService(IServiceProvider serviceProvider, string args)
    {
        _serviceProvider = serviceProvider;
        _args = args;
    }

    /// <summary>
    /// Start Xiletrade app.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Start()
    {
        if (_started)
        {
            throw new Exception(Resources.Resources.Main188_Alreadystarted);
        }
        try
        {
#if DEBUG
            var logger = _serviceProvider.GetRequiredService<ILogger<XiletradeService>>();
            logger.LogInformation("Launching Xiletrade service");
#endif
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            dm.TryInit();
            
            // MainWindow need to be instantiated before StartWindow.
            var nav = _serviceProvider.GetRequiredService<INavigationService>();
            nav.InstantiateMainView();
            if (!dm.Config.Options.DisableStartupMessage)
            {
                await nav.ShowStartView();
            }

            _ = _serviceProvider.GetRequiredService<PoeNinjaService>().LoadStateAsync();
            if (dm.Config.Options.CheckFilters)
            {
                _ = _serviceProvider.GetRequiredService<DataUpdaterService>().UpdateAsync();
            }
            if (dm.Config.Options.CheckUpdates)
            {
                _ = _serviceProvider.GetRequiredService<IAutoUpdaterService>().CheckUpdateAsync();
            }

            _serviceProvider.GetRequiredService<HotKeyService>().StartAutoRegister();
            _serviceProvider.GetRequiredService<ClipboardService>();

            // Automatically register or update the custom protocol handler in the registry
            _serviceProvider.GetRequiredService<IProtocolRegisterService>().RegisterOrUpdateProtocol();

            // Starts pipe server.
            var phs = _serviceProvider.GetRequiredService<IProtocolHandlerService>();
            phs.StartListening();

            // Token initialization on first call.
            RefreshAuthenticationState();

            // If a protocol URL was passed on first launch, handle it now
            if (!string.IsNullOrEmpty(_args))
            {
                phs.HandleUrl(_args);
            }
            Shared.Common.CollectGarbage();
#if DEBUG
            logger.LogInformation("Xiletrade launched");
#endif
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            var message = $"Xiletrade will shutdown shortly.\n\n{ex.Message}";
            if (ex.InnerException?.Message.Length > 0)
            {
                message += $"\n\n{ex.InnerException.Message}";
            }
            await ms.ShowResultAsync(message, "Failed to launch Xiletrade", MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade(1);
        }
        finally
        {
            _started = true;
        }
    }

    public void RefreshAuthenticationState()
    {
        var token = _serviceProvider.GetRequiredService<ITokenService>();
        token.Load();

        var mvm = _serviceProvider.GetRequiredService<MainViewModel>();
        mvm.Authenticated = token.CacheToken is not null;

        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        mvm.Authentication = !string.IsNullOrEmpty(dm.Config.Options.Secret);
    }

    // Not used for now
    public void DelegateToUi(Action action) => UiThreadContext.Send(_ => action(), null);
    public void DelegateToUiASync(Action action) => UiThreadContext.Post(_ => action(), null);
}

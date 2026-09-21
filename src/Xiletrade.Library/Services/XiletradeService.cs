using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.TaskBar;

namespace Xiletrade.Library.Services;

/// <summary>Main service used to launch Xiletrade application.</summary>
/// <remarks>Initialize all services.</remarks>
public sealed class XiletradeService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<XiletradeService> _logger;
    private readonly DataManagerService _dm;
    private readonly INavigationService _navigation;
    private readonly IMessageAdapterService _message;
    private readonly UIService _ui;

    private bool _started;

    public XiletradeService(IServiceProvider serviceProvider, ILogger<XiletradeService> logger, 
        DataManagerService dm, INavigationService navigation, IMessageAdapterService message, 
        UIService ui)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _dm = dm;
        _navigation = navigation;
        _message = message;
        _ui = ui;
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
            _logger.LogInformation("Launching Xiletrade service");
#endif           
            if (!_dm.Config.Options.DisableStartupMessage)
            {
                await _navigation.ShowStartView();
            }

            // Tokens initialization on first call.
            RefreshAuthenticationState();

            _ = _serviceProvider.GetRequiredService<PoeNinjaService>().InitLeaguesAsync();
            if (_dm.Config.Options.CheckFilters)
            {
                _ = _serviceProvider.GetRequiredService<DataUpdaterService>().UpdateAsync();
            }
            if (_dm.Config.Options.CheckUpdates)
            {
                _ = _serviceProvider.GetRequiredService<IAutoUpdaterService>().CheckUpdateAsync();
            }

            _serviceProvider.GetRequiredService<HotKeyService>().StartAutoRegister();
            _serviceProvider.GetRequiredService<ClipboardService>();

            // Automatically register or update the custom protocol handler in the registry
            _serviceProvider.GetRequiredService<IProtocolRegisterService>().RegisterOrUpdateProtocol();

            // Starts pipe server.
            _serviceProvider.GetRequiredService<IProtocolHandlerService>().StartListening();

            Shared.Common.CollectGarbage();
#if DEBUG
            _logger.LogInformation("Xiletrade launched");
#endif
        }
        catch (Exception ex)
        {
            var message = $"Xiletrade will shutdown shortly.\n\n{ex.Message}";
            if (ex.InnerException?.Message.Length > 0)
            {
                message += $"\n\n{ex.InnerException.Message}";
            }
            await _message.ShowResultAsync(message, "Failed to launch Xiletrade", MessageStatus.Exclamation);
            _ui.ShutDownXiletrade(1);
        }
        finally
        {
            _started = true;
        }
    }

    // TO REDO
    public void RefreshAuthenticationState()
    {
        var token = _serviceProvider.GetRequiredService<ITokenService>();
        token.LoadTokens();

        var vm = _serviceProvider.GetRequiredService<TaskBarViewModel>();
        vm.Authenticated = token.CacheToken is not null;
        vm.Authentication = !string.IsNullOrEmpty(_dm.Config.Options.Secret);
    }
}

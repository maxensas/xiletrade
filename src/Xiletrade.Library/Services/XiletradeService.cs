using Microsoft.Extensions.DependencyInjection;
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
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            HandleDataManagerException(dm.InitializeException);

            // MainWindow need to be instantiated before StartWindow.
            var nav = _serviceProvider.GetRequiredService<INavigationService>();
            nav.InstantiateMainView();
            if (!dm.Config.Options.DisableStartupMessage)
            {
                await nav.ShowStartView();
            }
            await dm.LoadNinjaStateAsync();
            if (dm.Config.Options.CheckFilters)
            {
                await _serviceProvider.GetRequiredService<DataUpdaterService>().UpdateAsync();
            }
            if (dm.Config.Options.CheckUpdates)
            {
                await _serviceProvider.GetRequiredService<IAutoUpdaterService>().CheckUpdateAsync();
            }

            _serviceProvider.GetRequiredService<HotKeyService>();
            _serviceProvider.GetRequiredService<ClipboardService>();

            // Automatically register or update the custom protocol handler in the registry
            _serviceProvider.GetRequiredService<IProtocolRegisterService>().RegisterOrUpdateProtocol();

            // Starts pipe server.
            _serviceProvider.GetRequiredService<IProtocolHandlerService>().StartListening();

            // Token initialization on first call.
            RefreshAuthenticationState();

            // If a protocol URL was passed on first launch, handle it now
            if (!string.IsNullOrEmpty(_args))
            {
                _serviceProvider.GetRequiredService<IProtocolHandlerService>().HandleUrl(_args);
            }
            Shared.Common.CollectGarbage();
        }
        catch (Exception ex)
        {
            throw new Exception(Resources.Resources.Main187_Fatalerror, ex);
        }
        finally
        {
            _started = true;
        }
    }

    public void RefreshAuthenticationState()
    {
        var token = _serviceProvider.GetRequiredService<ITokenService>();
        if (!token.IsInitialized)
        {
            token.Load();
        }
        var mvm = _serviceProvider.GetRequiredService<MainViewModel>();
        mvm.Authenticated = token.CacheToken is not null;
    }

    // Not used for now
    public void DelegateToUi(Action action) => UiThreadContext.Send(_ => action(), null);
    public void DelegateToUiASync(Action action) => UiThreadContext.Post(_ => action(), null);

    private static void HandleDataManagerException(Exception ex)
    {
        if (ex is null)
        {
            return;
        }
        var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        ms.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "UTF8 Deserialize error", MessageStatus.Error);
        ms.Show(Resources.Resources.Main118_Closing, Resources.Resources.Main187_Fatalerror, MessageStatus.Exclamation);
        _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade(1);
    }
}

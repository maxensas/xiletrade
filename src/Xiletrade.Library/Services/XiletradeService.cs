using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Services;

/// <summary>Main service used to launch Xiletrade application.</summary>
/// <remarks>Initialize all services and helper class.</remarks>
public sealed class XiletradeService
{
    private static IServiceProvider _serviceProvider;

    // public members
    public static SynchronizationContext UiThreadContext { get; } = SynchronizationContext.Current;
    public nint MainHwnd { get; set; }

    // constructor
    public XiletradeService(IServiceProvider serviceProvider) // WIP services
    {
        _serviceProvider = serviceProvider;
        try
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();

            // MainWindow need to be instantiated before StartWindow.
            _serviceProvider.GetRequiredService<INavigationService>().InstantiateMainView();
            if (!dm.Config.Options.DisableStartupMessage)
            {
                _serviceProvider.GetRequiredService<INavigationService>().ShowStartView();
            }
            DataFilters.Initialize(_serviceProvider);
            if (dm.Config.Options.CheckFilters)
            {
                DataFilters.Update(); //updateGenerated: false
            }
            if (dm.Config.Options.CheckUpdates)
            {
                _serviceProvider.GetRequiredService<IAutoUpdaterService>().CheckUpdate();
            }

            _serviceProvider.GetRequiredService<HotKeyService>();
            _serviceProvider.GetRequiredService<ClipboardService>();
            // using static helper class atm
            Json.Initialize(_serviceProvider);
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show("Failed to launch Xiletrade :\n" + ex.Message, Resources.Resources.Main187_Fatalerror, MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade();
        }
    }

    // Not used for now
    public void DelegateToUi(Action action) => UiThreadContext.Send(_ => action(), null);
    public void DelegateToUiASync(Action action) => UiThreadContext.Post(_ => action(), null);
}

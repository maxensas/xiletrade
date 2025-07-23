using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

/// <summary>Main service used to launch Xiletrade application.</summary>
/// <remarks>Initialize all services.</remarks>
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
            var nav = _serviceProvider.GetRequiredService<INavigationService>();
            nav.InstantiateMainView();
            if (!dm.Config.Options.DisableStartupMessage)
            {
                nav.ShowStartView();
            }
            if (dm.Config.Options.CheckFilters)
            {
                _serviceProvider.GetRequiredService<DataUpdaterService>().Update();
            }
            if (dm.Config.Options.CheckUpdates)
            {
                _serviceProvider.GetRequiredService<IAutoUpdaterService>().CheckUpdate();
            }
            _ = dm.LoadNinjaStateAsync();

            _serviceProvider.GetRequiredService<HotKeyService>();
            _serviceProvider.GetRequiredService<ClipboardService>();
        }
        catch (Exception ex)
        {
            var message = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            message.Show("Failed to launch Xiletrade :\n" + ex.Message, Resources.Resources.Main187_Fatalerror, MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade();
        }
    }

    // Not used for now
    public void DelegateToUi(Action action) => UiThreadContext.Send(_ => action(), null);
    public void DelegateToUiASync(Action action) => UiThreadContext.Post(_ => action(), null);
}

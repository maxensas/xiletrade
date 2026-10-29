using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Views;

namespace Xiletrade.Library.Services;

/// <summary>Main service used to launch Xiletrade application.</summary>
/// <remarks>Bootstraps the application and initializes its services.</remarks>
public sealed class XiletradeService
{
    public XiletradeService(ILogger<XiletradeService> logger, 
        DataManagerService dm, CrashReportService report,
        IDataUpdaterService dataUpdater, IViewManager view, 
        IMessageAdapterService message, IAutoUpdaterService updater,
        // Instantiate singletons : 
        IInputService input, IProtocolHandlerService protocol, IMainView main, ITaskbar taskbar)
    {
#if DEBUG
        logger.LogInformation("Launching Xiletrade service");
#endif

        _ = StartAsync(logger, dm, report, dataUpdater, view, message, updater);
    }

    /// <summary>
    /// Start Xiletrade app.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task StartAsync(ILogger<XiletradeService> logger, 
        DataManagerService dm, CrashReportService report,
        IDataUpdaterService dataUpdater, IViewManager view, 
        IMessageAdapterService message, IAutoUpdaterService appUpdater)
    {
        try
        {
            _ = appUpdater.DeleteUpdaterFolderAsync();
            report.OfferPendingReport();

            var options = dm.Config.Options;
            if (!options.DisableStartupMessage)
            {
                await view.ShowStartView();
            }
            if (options.CheckFilters)
            {
                _ = dataUpdater.UpdateAsync();
            }
            if (options.CheckUpdates)
            {
                _ = appUpdater.CheckForUpdateAsync();
            }

            Shared.Common.CollectGarbage();
#if DEBUG
            logger.LogInformation("Xiletrade launched");
#endif
        }
        catch (Exception ex)
        {
            var strMessage = $"{Resources.Resources.Error031_ShutdownShortly}\n\n{ex.Message}";
            if (ex.InnerException?.Message.Length > 0)
            {
                strMessage += $"\n\n{ex.InnerException.Message}";
            }
            await message.ShowResultAsync(strMessage, Resources.Resources.Error032_XFailedLaunch, MessageStatus.Exclamation);
            view.ShutDownNativeApp(1);
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>Main service used to launch Xiletrade application.</summary>
/// <remarks>Initialize all services.</remarks>
public sealed class XiletradeService
{
    public XiletradeService(ILogger<XiletradeService> logger, DataManagerService dm, 
        DataUpdaterService dataUpdater, UIService ui, INavigationService navigation, 
        IMessageAdapterService message, IAutoUpdaterService updater, 
        // Instantiate singletons :
        IProtocolHandlerService handler, IProtocolRegisterService reg, HotKeyService hk)
    {
        _ = Start(logger, dm, dataUpdater, ui, navigation, message, updater);
    }

    /// <summary>
    /// Start Xiletrade app.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task Start(ILogger<XiletradeService> logger, DataManagerService dm,
        DataUpdaterService dataUpdater, UIService ui, INavigationService navigation, 
        IMessageAdapterService message, IAutoUpdaterService updater)
    {
        try
        {
#if DEBUG
            logger.LogInformation("Launching Xiletrade service");
#endif           
            var options = dm.Config.Options;
            if (!options.DisableStartupMessage)
            {
                await navigation.ShowStartView();
            }
            if (options.CheckFilters)
            {
                _ = dataUpdater.UpdateAsync();
            }
            if (options.CheckUpdates)
            {
                _ = updater.CheckUpdateAsync();
            }

            Shared.Common.CollectGarbage();
#if DEBUG
            logger.LogInformation("Xiletrade launched");
#endif
        }
        catch (Exception ex)
        {
            var strMessage = $"Xiletrade will shutdown shortly.\n\n{ex.Message}";
            if (ex.InnerException?.Message.Length > 0)
            {
                strMessage += $"\n\n{ex.InnerException.Message}";
            }
            await message.ShowResultAsync(strMessage, "Failed to launch Xiletrade", MessageStatus.Exclamation);
            ui.ShutDownXiletrade(1);
        }
    }
}

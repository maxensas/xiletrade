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
        PoeNinjaService ninja, HotKeyService hotkey, DataUpdaterService dataUpdater,
        UIService ui, INavigationService navigation, IMessageAdapterService message, 
        IAutoUpdaterService updater, IProtocolRegisterService protocolRegister, 
        IProtocolHandlerService protocolHandler)
    {
        _ = Start(logger, dm, ninja, hotkey, dataUpdater, ui, navigation,  
            message, updater, protocolRegister, protocolHandler);
    }

    /// <summary>
    /// Start Xiletrade app.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task Start(ILogger<XiletradeService> logger, DataManagerService dm,
        PoeNinjaService ninja, HotKeyService hotkey, DataUpdaterService dataUpdater,
        UIService ui, INavigationService navigation, IMessageAdapterService message, IAutoUpdaterService updater,
        IProtocolRegisterService protocolRegister, IProtocolHandlerService protocolHandler)
    {
        try
        {
#if DEBUG
            logger.LogInformation("Launching Xiletrade service");
#endif           
            if (!dm.Config.Options.DisableStartupMessage)
            {
                await navigation.ShowStartView();
            }

            _ = ninja.InitLeaguesAsync();
            if (dm.Config.Options.CheckFilters)
            {
                _ = dataUpdater.UpdateAsync();
            }
            if (dm.Config.Options.CheckUpdates)
            {
                _ = updater.CheckUpdateAsync();
            }

            hotkey.StartAutoRegister();

            // Automatically register or update the custom protocol handler in the registry
            protocolRegister.RegisterOrUpdateProtocol();

            // Starts pipe server.
            protocolHandler.StartListening();

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

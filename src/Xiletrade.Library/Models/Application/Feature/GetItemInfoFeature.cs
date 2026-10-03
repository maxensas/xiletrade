using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Application.Feature;

internal sealed class GetItemInfoFeature(ILogger<GetItemInfoFeature> logger, INetService net, 
    IViewManager view, IPoeActionService poe, ClipboardService clipboard,
    MainViewModel vm, ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        try
        {
            if (net.TradeCooldown is not null && net.TradeCooldown.IsEnabled)
            {
                if (_shortcut.Fonction is Strings.Feature.run)
                {
                    view.ShowMainView();
                }
                return;
            }

            vm.StopWatch.Restart();

            poe.CopyItemDetail();
            
            if (!clipboard.ContainsAnyTextData())
            {
                vm.StopWatch.StopAndGetTimeString();
                return;
            }
            vm.InitViewModels();

            var clip = clipboard.GetClipboard(true);
            if (!string.IsNullOrEmpty(clip))
            {
                vm.ClipboardText = clip;
                _ = vm.RunMainTaskAsync(_shortcut.Fonction);
            }
        }
        catch (COMException ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("COMException raised : {Message}", ex.Message);
            if (ex.Message.Contain("0x800401D0")) // CLIPBRD_E_CANT_OPEN 
            {
                return;
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Exception raised : {Message}", ex.Message);
        }
    }
}

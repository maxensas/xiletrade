using System;
using System.Runtime.InteropServices;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal sealed class GetItemInfoFeature(INetService net, INavigationService navigation, 
    ISendInputService sendInput,ClipboardService clipboard,
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
                    navigation.ShowMainView();
                }
                return;
            }

            vm.StopWatch.Restart();

            sendInput.CopyItemDetail();
            
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
                _ = vm.RunMainUpdaterTaskAsync(_shortcut.Fonction);
            }
        }
        catch (COMException ex) // for now : do not re-throw exception
        {
            if (ex.Message.Contain("0x800401D0")) // CLIPBRD_E_CANT_OPEN 
            {
                return;
            }
        }
        catch (Exception) // do not re-throw exception
        {
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal sealed class GetItemInfoFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        try
        {
            if (ServiceProvider.GetRequiredService<PoeApiService>().IsCooldownEnabled)
            {
                if (Shortcut.Fonction is Strings.Feature.run)
                {
                    ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
                }
                return;
            }

            var vm = ServiceProvider.GetRequiredService<MainViewModel>();

            vm.StopWatch.Restart();
            ServiceProvider.GetRequiredService<ISendInputService>().CopyItemDetailAdvanced();

            var clipService = ServiceProvider.GetRequiredService<ClipboardService>();
            if (!clipService.ContainsAnyTextData())
            {
                vm.StopWatch.StopAndGetTimeString();
                return;
            }
            vm.InitViewModels();

            var clipAdvanced = clipService.GetClipboard(true);
            if (clipAdvanced is not null && clipAdvanced.Length > 0)
            {
                vm.ClipboardText = clipAdvanced;
                _ = vm.RunMainUpdaterTaskAsync(Shortcut.Fonction);
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

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Library.Models.Feature;

internal sealed class GetItemInfoFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var vm = ServiceProvider.GetRequiredService<MainViewModel>();
        var service = ServiceProvider.GetRequiredService<PoeApiService>();
        if (service.IsCooldownEnabled)
        {
            if (Shortcut.Fonction is Strings.Feature.run)
            {
                ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
            }
            return;
        }
        var inputService = ServiceProvider.GetRequiredService<ISendInputService>();
        var isEnglish = DataManager.Config.Options.Language is 0;
        if (isEnglish)
        {
            inputService.CopyItemDetailAdvanced();
        }
        else
        {
            inputService.CopyItemDetail();
        }

        try
        {
            var clipService = ServiceProvider.GetRequiredService<ClipboardService>();
            if (!clipService.ContainsAnyTextData())
            {
                return;
            }
            bool openWikiOnly = Shortcut.Fonction is Strings.Feature.wiki;
            bool openNinjaOnly = Shortcut.Fonction is Strings.Feature.ninja;
            bool openMainWindow = !openWikiOnly && !openNinjaOnly;

            vm.StopWatch.Restart();
            vm.TaskManager.CancelPreviousTasks();
            vm.InitViewModels();

            string clipText = clipService.GetClipboard(true);
            if (!isEnglish) // Handle item name/type in non-english, not translated anymore in advanced desc.
            {
                inputService.CopyItemDetailAdvanced();
                if (clipService.ContainsAnyTextData())
                {
                    string clipTextAdvanced = clipService.GetClipboard(true);
                    var firstIdx = clipText.IdxOf(Strings.ItemInfoDelimiterCRLF);
                    var secondIdx = clipTextAdvanced.IdxOf(Strings.ItemInfoDelimiterCRLF);
                    clipText = string.Concat(clipText.AsSpan(0, firstIdx), clipTextAdvanced.AsSpan(secondIdx, clipTextAdvanced.Length - secondIdx));
                }
            }
            if (clipText is not null && clipText.Length > 0)
            {
                vm.RunMainUpdaterTask(clipText, openMainWindow);
            }

            if (openWikiOnly)
            {
                var poeWiki = new PoeWiki(vm.Item);
                vm.OpenUrlTask(poeWiki.Link, UrlType.PoeWiki);
            }
            if (openNinjaOnly)
            {
                vm.OpenUrlTask(vm.Ninja.GetFullUrl(), UrlType.Ninja);
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

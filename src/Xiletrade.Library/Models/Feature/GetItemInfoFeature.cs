using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Library.Models.Feature;

internal sealed class GetItemInfoFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var vm = ServiceProvider.GetRequiredService<MainViewModel>();
        if (vm.Logic.Task.Price.CoolDown.IsEnabled)
        {
            if (Shortcut.Fonction is Strings.Feature.run)
            {
                ServiceProvider.GetRequiredService<INavigationService>().ShowMainView();
            }
            return;
        }

        var inputService = ServiceProvider.GetRequiredService<ISendInputService>();
        vm.Logic.Task.Price.Watch.Restart();
        inputService.CopyItemDetailAdvanced();
        vm.Logic.Task.HandlePriceCheckSpam(); // avoid price check spam, previous threads need to end properly

        try
        {
            bool openWikiOnly = Shortcut.Fonction is Strings.Feature.wiki;
            bool openNinjaOnly = Shortcut.Fonction is Strings.Feature.ninja;
            bool openMainWindow = !openWikiOnly && !openNinjaOnly;

            if (ClipboardHelper.ContainsUnicodeTextData() || ClipboardHelper.ContainsTextData())
            {
                string clipText = ClipboardHelper.GetClipboard(true);
                // TO RECHECK patch to handle item name/type in japanese, not translated atm in advanced desc.
                if (DataManager.Config.Options.Language is 10) //"ja-JP"
                {
                    inputService.CopyItemDetail();
                    string delimiter = "--------\r\n";
                    string clipTextTmp = ClipboardHelper.GetClipboard(true);
                    var sub = clipTextTmp[..clipTextTmp.IndexOf(delimiter)];
                    clipText = sub + clipText.Remove(0, clipText.IndexOf(delimiter));
                }
                vm.Logic.Task.UpdateMainViewModel(clipText, openMainWindow);
                if (openWikiOnly && vm.Commands.OpenWiki.CanExecute(null))
                {
                    vm.Commands.OpenWiki.Execute(null);
                }
                if (openNinjaOnly && vm.Commands.OpenNinja.CanExecute(null))
                {
                    vm.Commands.OpenNinja.Execute(null);
                }
            }
        }
        catch (COMException ex) // for now : do not re-throw exception
        {
            if (ex.Message.Contains("0x800401D0", StringComparison.Ordinal)) // CLIPBRD_E_CANT_OPEN 
            {
                //Shared.Util.Helper.Debug.Trace("Can not access clipboard : " + ex.Message);
                return;
            }
            //Shared.Util.Helper.Debug.Trace("COMException catched : " + ex.Message);
        }
        catch (Exception ex) // do not re-throw exception
        {
            //Shared.Util.Helper.Debug.Trace("Exception while parsing data : " + ex.Message);
        }
    }
}

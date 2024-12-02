using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Main;

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
        var isEnglish = DataManager.Config.Options.Language is 0;

        vm.Logic.Task.Price.Watch.Restart();
        if (isEnglish)
        {
            inputService.CopyItemDetailAdvanced();
        }
        else
        {
            inputService.CopyItemDetail();
        }
        vm.Logic.Task.HandlePriceCheckSpam();

        try
        {
            bool openWikiOnly = Shortcut.Fonction is Strings.Feature.wiki;
            bool openNinjaOnly = Shortcut.Fonction is Strings.Feature.ninja;
            bool openMainWindow = !openWikiOnly && !openNinjaOnly;

            if (ClipboardHelper.ContainsAnyTextData())
            {
                string clipText = ClipboardHelper.GetClipboard(true);
                if (!isEnglish) // Handle item name/type in non-english, not translated anymore in advanced desc.
                {
                    inputService.CopyItemDetailAdvanced();
                    if (ClipboardHelper.ContainsAnyTextData())
                    {
                        string clipTextAdvanced = ClipboardHelper.GetClipboard(true);
                        var sub = clipText[..clipText.IndexOf(Strings.ItemInfoDelimiterCRLF)];
                        clipText = sub + clipTextAdvanced.Remove(0, clipTextAdvanced.IndexOf(Strings.ItemInfoDelimiterCRLF));
                    }
                }
                vm.Logic.Task.UpdateMainViewModel(clipText, openMainWindow);
                if (openWikiOnly)
                {
                    vm.Logic.Task.OpenWiki();
                }
                if (openNinjaOnly)
                {
                    vm.Logic.Task.OpenNinja();
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
        catch (Exception) // do not re-throw exception
        {
            //Shared.Util.Helper.Debug.Trace("Exception while parsing data : " + ex.Message);
        }
    }
}

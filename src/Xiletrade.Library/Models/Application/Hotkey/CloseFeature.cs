using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal sealed class CloseFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        nint findHwnd = 0;
        foreach (var win in Strings.WindowName.XiletradeWindowList)
        {
            findHwnd = Native.FindWindow(null, win);
            if (findHwnd is not 0)
            {
                break;
            }
        }
        var isVisibleMain = ServiceProvider.GetRequiredService<INavigationService>().IsVisibleMainView();
        if (findHwnd.ToInt32() is 0 && !isVisibleMain)
        {
            nint findPoeHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
            bool poeLaunched = findPoeHwnd.ToInt32() > 0;
            if (poeLaunched)
            {
                Native.SendMessage(findPoeHwnd, Native.WM_KEYUP, new nint(Shortcut.Keycode), nint.Zero);
            }
            return;
        }

        if (findHwnd.ToInt32() is not 0)
        {
            Native.SendMessage(findHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        ServiceProvider.GetRequiredService<INavigationService>().CloseMainView();
    }
}
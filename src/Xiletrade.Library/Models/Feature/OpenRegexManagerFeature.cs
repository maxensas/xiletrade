using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Feature;

internal class OpenRegexManagerFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        nint pHwnd = Native.FindWindow(null, Strings.WindowName.Regex);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        var service = ServiceProvider.GetRequiredService<INavigationService>();
        service.CloseMainView();
        service.ShowRegexView();
    }
}

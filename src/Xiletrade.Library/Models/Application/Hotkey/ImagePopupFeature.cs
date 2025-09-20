using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class ImagePopupFeature(IServiceProvider service, ConfigShortcut shortcut): BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var service = ServiceProvider.GetRequiredService<INavigationService>();
        service.CloseMainView();
        nint pHwnd = Native.FindWindow(null, Strings.WindowName.Popup);
        if (pHwnd.ToInt32() is not 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        service.ShowPopupView(Shortcut.Value);
    }
}

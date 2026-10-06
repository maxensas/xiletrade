using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.Library.Models.Application.Feature;

internal class ImagePopupFeature(IViewManager view,
    ConfigShortcut shortcut): BaseFeature(shortcut)
{
    internal override void Launch()
    {
        view.CloseMainView();
        nint pHwnd = Native.FindWindow(null, Strings.WindowName.Popup);
        if (pHwnd.ToInt32() is not 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        view.ShowPopupView(_shortcut.Value);
    }
}

using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class ImagePopupFeature(INavigationService navigation,
    ConfigShortcut shortcut): BaseFeature(shortcut)
{
    internal override void Launch()
    {
        navigation.CloseMainView();
        nint pHwnd = Native.FindWindow(null, Strings.WindowName.Popup);
        if (pHwnd.ToInt32() is not 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        navigation.ShowPopupView(_shortcut.Value);
    }
}

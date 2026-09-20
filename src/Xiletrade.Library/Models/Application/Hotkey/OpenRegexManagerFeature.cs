using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class OpenRegexManagerFeature(INavigationService navigation,
    ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        nint pHwnd = Native.FindWindow(null, Strings.WindowName.Regex);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        navigation.CloseMainView();
        navigation.ShowRegexView();
    }
}

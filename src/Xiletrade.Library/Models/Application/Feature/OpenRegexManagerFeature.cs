using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.Library.Models.Application.Feature;

internal class OpenRegexManagerFeature(IViewManager view,
    ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        nint pHwnd = Native.FindWindow(null, Strings.WindowName.Regex);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        view.CloseMainView();
        view.ShowRegexView();
    }
}

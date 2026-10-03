using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Application.Feature;

internal sealed class CloseFeature(IViewManager view, 
    ConfigShortcut shortcut) : BaseFeature(shortcut)
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
        var isVisibleMain = view.IsVisibleMainView();
        if (findHwnd.ToInt32() is 0 && !isVisibleMain)
        {
            nint findPoeHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
            bool poeLaunched = findPoeHwnd.ToInt32() > 0;
            if (poeLaunched)
            {
                Native.SendMessage(findPoeHwnd, Native.WM_KEYUP, new nint(_shortcut.Keycode), nint.Zero);
            }
            return;
        }

        if (findHwnd.ToInt32() is not 0)
        {
            Native.SendMessage(findHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
        }
        view.CloseMainView();
    }
}
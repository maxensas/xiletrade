using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.Library.Models.Application.Feature;

internal class KillTcpFeature(ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        var delay = Tcp.KillTCPConnectionForProcess();
        //Shared.Util.Helper.Debug.Trace("Closed connections (took " + delay + " ms)");
    }
}

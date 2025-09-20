using System;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class KillTcpFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var delay = Tcp.KillTCPConnectionForProcess();
        //Shared.Util.Helper.Debug.Trace("Closed connections (took " + delay + " ms)");
    }
}

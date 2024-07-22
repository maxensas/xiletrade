using System;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Models.Feature;

internal class KillTcpFeature(IServiceProvider service, ConfigShortcut shortcut) : BaseFeature(service, shortcut)
{
    internal override void Launch()
    {
        var delay = Tcp.KillTCPConnectionForProcess();
        //Shared.Util.Helper.Debug.Trace("Closed connections (took " + delay + " ms)");
    }
}

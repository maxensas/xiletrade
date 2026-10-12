using System;
using System.Runtime.Versioning;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.Library.Models.Application.Feature;

internal class KillTcpFeature(IMessageAdapterService message, ConfigShortcut shortcut) : BaseFeature(shortcut)
{
    internal override void Launch()
    {
        if (OperatingSystem.IsWindows())
        {
            KillWinTcp();
        }
        if (OperatingSystem.IsLinux())
        {
            //TODO
        }
    }

    [SupportedOSPlatform("windows")]
    private void KillWinTcp()
    {
        var delayOrError = Tcp.KillTCPConnectionForProcess();
        if (delayOrError > 0)
        {
            message.ShowResult("Closing connection took " + delayOrError + " ms",
                "Fast enough ?", MessageStatus.Information);
            return;
        }
        if (delayOrError is -1)
        {
            message.ShowResult("KillTCPConnection : " + Resources.Resources.Error004_XError,
                Resources.Resources.Error004_XError, MessageStatus.Exclamation);
            return;
        }
        if (delayOrError is -2)
        {
            message.ShowResult(Resources.Resources.Error033_NotAdmin, 
                Resources.Resources.Error019_XInvalid, MessageStatus.Exclamation);
            return;
        }
        delayOrError = -delayOrError;
        message.ShowResult("KillTCPConnection error code : " + delayOrError,
                Resources.Resources.Error004_XError, MessageStatus.Exclamation);
    }
}

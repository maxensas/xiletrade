using System;
using System.Diagnostics;
using Xiletrade.Library.Models.Application.Configuration.DTO;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class StartProcessFeature(IServiceProvider service, ConfigShortcut shortcut, string stringValue) 
    : BaseFeature(service, shortcut, stringValue)
{
    internal override void Launch()
    {
        if (StringValue is not null && Uri.IsWellFormedUriString(StringValue, UriKind.Absolute))
        {
            Process.Start(new ProcessStartInfo { FileName = StringValue, UseShellExecute = true });
        }
    }
}

using System;
using System.Diagnostics;
using Xiletrade.Library.Models.Application.Configuration.DTO;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal class StartProcessFeature(ConfigShortcut shortcut, string stringValue) 
    : BaseFeature(shortcut, stringValue)
{
    internal override void Launch()
    {
        if (_stringValue is not null && Uri.IsWellFormedUriString(_stringValue, UriKind.Absolute))
        {
            Process.Start(new ProcessStartInfo { FileName = _stringValue, UseShellExecute = true });
        }
    }
}

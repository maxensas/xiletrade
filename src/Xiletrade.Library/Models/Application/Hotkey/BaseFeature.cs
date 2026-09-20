using Xiletrade.Library.Models.Application.Configuration.DTO;

namespace Xiletrade.Library.Models.Application.Hotkey;

internal abstract class BaseFeature
{  
    protected readonly ConfigShortcut _shortcut;
    protected readonly string _stringValue;

    internal BaseFeature(ConfigShortcut shortcut, string stringValue = null)
    {
        _shortcut = shortcut;
        _stringValue = stringValue;
    }

    internal abstract void Launch();
}
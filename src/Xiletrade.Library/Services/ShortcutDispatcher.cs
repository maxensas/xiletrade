using Microsoft.Extensions.Logging;
using System;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary>
/// Service responsible for launching features when a registered hotkey is pressed.
/// </summary>
public sealed class ShortcutDispatcher
{
    private static bool _runingProcess = false;

    public readonly Action<int, nint> HandleMessage;

    public ShortcutDispatcher(ILogger<ShortcutDispatcher> logger, IMessageAdapterService message, 
        DataManagerService dm, FeatureProvider featureProvider)
    {
        // Here we process incoming messages
        HandleMessage = new((Msg, WParam) =>
        {
            if (_runingProcess || Msg is not Native.WM_HOTKEY) // Native.WM_DRAWITEM, Native.WM_CLIPBOARDUPDATE
            {
                return;
            }
            _runingProcess = true;
            try
            {
                var shortcut = dm.Config.Shortcuts[WParam.ToInt32() - InputService.SHIFTHOTKEYID];
                if (shortcut is null || shortcut.Fonction is null)
                {
                    return;
                }
                var feature = featureProvider.GetFeature(shortcut);
                feature?.Launch();
            }
            catch (Exception ex)
            {
                message.Show(ex.GetFormated(), Resources.Resources.Error015_XMainCommand, MessageStatus.Error);
            }
            finally
            {
                _runingProcess = false;
            }
        });

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }
}

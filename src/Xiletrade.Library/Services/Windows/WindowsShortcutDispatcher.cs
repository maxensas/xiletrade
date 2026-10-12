using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Versioning;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.Library.Services.Windows;

/// <summary>
/// Service responsible for launching features when a registered hotkey is pressed.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsShortcutDispatcher : IShortcutDispatcher
{
    private static bool _runingProcess = false;

    public Action<int, nint> HandleMessage { get; }

    public WindowsShortcutDispatcher(ILogger<WindowsShortcutDispatcher> logger, IMessageAdapterService message, 
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
                var shortcut = dm.Config.Shortcuts[WParam.ToInt32() - IInputService.SHIFTHOTKEYID];
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

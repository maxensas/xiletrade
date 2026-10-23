using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Versioning;
using System.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Linux;

/// <summary>
/// Service responsible for launching features when a registered hotkey is pressed.
/// </summary>
/// <remarks>
/// The messages come from the hotkey thread of <see cref="LinuxHookService"/> :
/// the features are launched on the UI thread, like the Windows message loop does.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class LinuxShortcutDispatcher : IShortcutDispatcher
{
    // 0 : idle, 1 : a feature is waiting to be launched or running.
    // A shortcut pressed meanwhile (key repeat, double press) is ignored.
    private int _busy;

    public Action<int, nint> HandleMessage { get; }

    public LinuxShortcutDispatcher(ILogger<LinuxShortcutDispatcher> logger, IMessageAdapterService message,
        DataManagerService dm, FeatureProvider featureProvider, IUIService ui)
    {
        HandleMessage = new((msg, wParam) =>
        {
            if (msg is not LinuxHookService.HotkeyMessage)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref _busy, 1, 0) is not 0)
            {
                return;
            }

            try
            {
                ui.Invoke(() => Launch(wParam.ToInt32(), message, dm, featureProvider));
            }
            catch (Exception ex)
            {
                Volatile.Write(ref _busy, 0);
                logger.LogError(ex, "Unable to post the shortcut to the UI thread");
            }
        });

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private void Launch(int id, IMessageAdapterService message, DataManagerService dm, FeatureProvider featureProvider)
    {
        try
        {
            int index = id - IInputService.SHIFTHOTKEYID;
            var shortcuts = dm.Config.Shortcuts;
            if (index < 0 || index >= shortcuts.Length)
            {
                return;
            }
            var shortcut = shortcuts[index];
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
            Volatile.Write(ref _busy, 0);
        }
    }
}

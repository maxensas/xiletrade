using System;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.Services;

/// <summary>
/// Service class used to launch Xiletrade feature after pressing registered hotkey.
/// </summary>
public sealed class WndProcService
{
    private static bool _runingProcess = false;

    public readonly Action<int, nint> ProcessMessageAsync;

    public WndProcService(IMessageAdapterService message, DataManagerService dm, 
        FeatureProviderService featureProvider)
    {
        // Here we process incoming messages
        ProcessMessageAsync = new((Msg, WParam) =>
        {
            if (_runingProcess || Msg is not Native.WM_HOTKEY) // Native.WM_DRAWITEM, Native.WM_CLIPBOARDUPDATE
            {
                return;
            }
            _runingProcess = true;
            try
            {
                var shortcut = dm.Config.Shortcuts[WParam.ToInt32() - HotKeyService.SHIFTHOTKEYID];
                if (shortcut is null || shortcut.Fonction is null)
                {
                    return;
                }
                var feature = featureProvider.GetFeature(shortcut);
                feature?.Launch();
            }
            catch (Exception ex)
            {
                message.Show(ex.GetFormated(), "Main commands error", MessageStatus.Error);
            }
            finally
            {
                _runingProcess = false;
            }
        });
    }
}

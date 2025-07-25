﻿using System;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.Models.Feature;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

/// <summary>
/// Service class used to launch Xiletrade feature after pressing registered hotkey.
/// </summary>
public sealed class WndProcService
{
    private static IServiceProvider _serviceProvider;
    private static bool _runingProcess = false;

    public WndProcService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // Here we process incoming messages
    public readonly Action<int, nint> ProcessMessageAsync = new((Msg, WParam) =>
    {
        if (_runingProcess || Msg is not Native.WM_HOTKEY)
        {
            return;
        }
        _runingProcess = true;
        try
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            var shortcut = dm.Config.Shortcuts[WParam.ToInt32()
                - _serviceProvider.GetRequiredService<HotKeyService>().ShiftHotkeyId];
            if (shortcut is null || shortcut.Fonction is null)
            {
                return;
            }
            var feature = FeatureProvider.GetFeature(_serviceProvider, shortcut);
            feature?.Launch();
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "Main commands error", MessageStatus.Error);
        }
        finally
        {
            _runingProcess = false;
        }
    });
}

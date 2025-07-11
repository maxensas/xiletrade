using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class HotkeyViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private bool isEnable;

    [ObservableProperty]
    private bool isInConflict;

    [ObservableProperty]
    private bool useCheckBox;

    [ObservableProperty]
    private string hotkey = string.Empty;

    [ObservableProperty]
    private string val = string.Empty;

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private string textToolTip;

    [ObservableProperty]
    private AsyncObservableCollection<string> list;

    [ObservableProperty]
    private int listIndex;

    public HotkeyViewModel(IServiceProvider serviceProvider, string txt, string tip, bool useCb = true, bool initList = false)
    {
        _serviceProvider = serviceProvider;
        text = txt;
        textToolTip = tip;
        useCheckBox = useCb;
        if (!useCb)
        {
            isEnable = true;
        }
        if (initList)
        {
            list = new();
        }
    }

    [RelayCommand]
    private void CheckHotkey(object commandParameter)
    {
        if (commandParameter is CompositeCommandParameter composite)
        {
            var keyPressed = _serviceProvider.GetRequiredService<INavigationService>().GetKeyPressed(composite.EventArgs);
            if (keyPressed.Length > 0)
            {
                Hotkey = keyPressed;
                UpdateHotkeysConflictStates(Hotkey);
            }
        }
    }

    private static void UpdateHotkeysConflictStates(string hotkey)
    {
        var cfgVm = _serviceProvider.GetRequiredService<ConfigViewModel>();
        var fullList = cfgVm.CommonKeys.GetListHotkey()
            .Concat(cfgVm.AdditionalKeys.GetListHotkey());
        var hkConflict = fullList.Where(x => x.Hotkey == hotkey);
        if (hkConflict.Any() && hkConflict.Count() > 1)
        {
            foreach (var hk in hkConflict)
            {
                hk.IsInConflict = true;
            }
        }
        foreach (var hk in fullList)
        {
            if (!hk.IsInConflict)
            {
                continue;
            }
            var hkUpdate = fullList.Count(x => x.Hotkey == hk.Hotkey);
            if (hkUpdate is 1)
            {
                hk.IsInConflict = false;
            }
        }
        var conflict = fullList.Count(x => x.IsInConflict);
        cfgVm.CanSave = conflict is 0;
    }
}

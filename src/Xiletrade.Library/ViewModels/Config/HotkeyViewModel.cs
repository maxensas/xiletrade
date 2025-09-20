using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
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
                UpdateHotkeysConflictStates(this);
            }
        }
    }

    private static void UpdateHotkeysConflictStates(HotkeyViewModel vm)
    {
        var cfgVm = _serviceProvider.GetRequiredService<ConfigViewModel>();
        var fullList = cfgVm.CommonKeys.GetListHotkey()
            .Concat(cfgVm.AdditionalKeys.GetListHotkey());
        var hkConflict = fullList.Where(x => x.Hotkey == vm.Hotkey);
        if (hkConflict.Any() && hkConflict.Count() > 1)
        {
            foreach (var hk in hkConflict)
            {
                hk.IsInConflict = true;
            }
            bool displayMessage = !hkConflict.Contains(cfgVm.AdditionalKeys.ChatKey) 
                || vm == cfgVm.AdditionalKeys.ChatKey;
            if (displayMessage)
            {
                bool overwrite = _serviceProvider.GetRequiredService<IMessageAdapterService>()
                    .ShowResult(Resources.Resources.Config174_hkConflictMessage
                    , Resources.Resources.Config175_hkConflictCaption
                    , MessageStatus.Exclamation, yesNo: true);
                if (overwrite)
                {
                    foreach (var hk in hkConflict)
                    {
                        if (hk != vm)
                        {
                            hk.Hotkey = string.Empty;
                            hk.IsEnable = false;
                        }
                        hk.IsInConflict = false;
                    }
                }
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

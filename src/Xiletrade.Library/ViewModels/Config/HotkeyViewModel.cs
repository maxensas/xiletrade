using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class HotkeyViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IMessageAdapterService _message;
    private readonly ConfigViewModel _cfgVm;

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

    public HotkeyViewModel(INavigationService navigation, IMessageAdapterService message, ConfigViewModel cfgVm,
        string txt, string tip, bool useCb = true, bool initList = false)
    {
        _navigation = navigation;
        _message = message;
        _cfgVm = cfgVm;

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
            var keyPressed = _navigation.GetKeyPressed(composite.EventArgs);
            if (keyPressed.Length > 0)
            {
                Hotkey = keyPressed;
                UpdateHotkeysConflictStates(this);
            }
        }
    }

    private void UpdateHotkeysConflictStates(HotkeyViewModel vm)
    {
        var fullList = _cfgVm.CommonKeys.GetListHotkey()
            .Concat(_cfgVm.AdditionalKeys.GetListHotkey());
        var hkConflict = fullList.Where(x => x.Hotkey == vm.Hotkey);
        if (hkConflict.Any() && hkConflict.Count() > 1)
        {
            foreach (var hk in hkConflict)
            {
                hk.IsInConflict = true;
            }
            bool displayMessage = !hkConflict.Contains(_cfgVm.AdditionalKeys.ChatKey) 
                || vm == _cfgVm.AdditionalKeys.ChatKey;
            if (displayMessage)
            {
                bool overwrite = _message.ShowResult(Resources.Resources.Config174_hkConflictMessage
                    , Resources.Resources.Config175_hkConflictCaption, MessageStatus.Exclamation, yesNo: true);
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
        _cfgVm.CanSave = conflict is 0;
    }
}

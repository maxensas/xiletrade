using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Logic;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;
using Xiletrade.Library.ViewModels.Main.Form;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private FormViewModel form;

    [ObservableProperty]
    private ResultViewModel result;

    [ObservableProperty]
    private NinjaButtonViewModel ninjaButton = new();

    [ObservableProperty]
    private string notifyName;

    internal ItemBaseName CurrentItem { get; set; }
    internal MainLogic Logic { get; private set; }

    public MainCommand Commands { get; private set; }
    public TrayMenuCommand TrayCommands { get; private set; }

    public List<MouseGestureCom> GestureList { get; set; } = new();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        Logic = new(this, serviceProvider);
        Commands = new(this, serviceProvider);
        TrayCommands = new(this, serviceProvider);
        NotifyName = "Xiletrade " + Common.GetFileVersion();
        GestureList.Add(new (Commands.WheelIncrementCommand, ModifierKey.None, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelDecrementCommand, ModifierKey.None, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Down));
    }

    internal void InitViewModel(bool useBulk = false)
    {
        Form = new(useBulk);
        Result = new();
    }

    internal void SearchCurrency(string str)
    {
        var exVm = str is "get" ? Form.Bulk.Get :
            str is "pay" ? Form.Bulk.Pay :
            str is "shop" ? Form.Shop.Exchange : null;
        if (exVm is not null)
        {
            if (exVm.Search.Length >= 1)
            {
                Logic.SelectViewModelExchangeCurrency(str + "/contains", exVm.Search);
            }
            else
            {
                exVm.CategoryIndex = 0;
                exVm.CurrencyIndex = 0;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Logic;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private FormViewModel form = new();
    private ResultViewModel result = new();
    private NinjaButtonViewModel ninjaButton = new();
    //private TranslationViewModel translate = new();
    private bool sameUser;
    private bool chaosDiv;
    private bool autoClose;
    private string notifyName;

    public FormViewModel Form { get => form; set => SetProperty(ref form, value); }
    public ResultViewModel Result { get => result; set => SetProperty(ref result, value); }
    public NinjaButtonViewModel NinjaButton { get => ninjaButton; set => SetProperty(ref ninjaButton, value); }
    //public TranslationViewModel Translate { get => translate; set => SetProperty(ref translate, value); }
    public bool SameUser { get => sameUser; set => SetProperty(ref sameUser, value); }
    public bool ChaosDiv { get => chaosDiv; set => SetProperty(ref chaosDiv, value); }
    public bool AutoClose { get => autoClose; set => SetProperty(ref autoClose, value); }
    public string NotifyName { get => notifyName; set => SetProperty(ref notifyName, value); }

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
        GestureList.Add(new MouseGestureCom(Commands.WheelIncrement, ModifierKey.None, MouseWheelDirection.Up));
        GestureList.Add(new MouseGestureCom(Commands.WheelIncrementTenth, ModifierKey.Control, MouseWheelDirection.Up));
        GestureList.Add(new MouseGestureCom(Commands.WheelIncrementHundredth, ModifierKey.Shift, MouseWheelDirection.Up));
        GestureList.Add(new MouseGestureCom(Commands.WheelDecrement, ModifierKey.None, MouseWheelDirection.Down));
        GestureList.Add(new MouseGestureCom(Commands.WheelDecrementTenth, ModifierKey.Control, MouseWheelDirection.Down));
        GestureList.Add(new MouseGestureCom(Commands.WheelDecrementHundredth, ModifierKey.Shift, MouseWheelDirection.Down));
    }
}

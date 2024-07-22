using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using System.Windows.Input;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.ViewModels.Command;

public sealed class TrayMenuCommand
{
    private static MainViewModel Vm { get; set; }
    private static IServiceProvider _serviceProvider;

    private readonly DelegateCommand openAbout;
    private readonly DelegateCommand checkUpdate;
    private readonly DelegateCommand openConfig;
    private readonly DelegateCommand closeApplication;

    public ICommand OpenAbout => openAbout;
    public ICommand CheckUpdate => checkUpdate;
    public ICommand OpenConfig => openConfig;
    public ICommand CloseApplication => closeApplication;

    public TrayMenuCommand(MainViewModel vm, IServiceProvider serviceProvider)
    {
        Vm = vm;
        _serviceProvider = serviceProvider;
        openAbout = new(OnOpenAbout, CanOpenAbout);
        checkUpdate = new(OnCheckUpdate, CanCheckUpdate);
        openConfig = new(OnOpenConfig, CanOpenConfig);
        closeApplication = new(OnCloseApplication, CanCloseApplication);
    }

    private static bool CanOpenAbout(object commandParameter)
    {
        return true;
    }

    private static void OnOpenAbout(object commandParameter)
    {
        StringBuilder message = new("Version: ");
        message.Append(Common.GetFileVersion()).AppendLine().AppendLine()
            .Append(Resources.Resources.Main120_About1).AppendLine().AppendLine()
            .Append(Resources.Resources.Main121_About2).AppendLine().AppendLine()
            .Append(Resources.Resources.Main122_About3);
        var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        service.Show(message.ToString(), "Xiletrade by maxensas", MessageStatus.Information);
    }

    private static bool CanCheckUpdate(object commandParameter)
    {
        return true;
    }

    private static void OnCheckUpdate(object commandParameter)
    {
        _serviceProvider.GetRequiredService<IAutoUpdaterService>().CheckUpdate();
    }

    private static bool CanOpenConfig(object commandParameter)
    {
        return true;
    }

    private static void OnOpenConfig(object commandParameter)
    {
        IntPtr pHwnd = Native.FindWindow(null, Strings.WindowName.Config);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        var service = _serviceProvider.GetRequiredService<INavigationService>();
        service.CloseMainView();
        service.ShowConfigView();
    }

    private static bool CanCloseApplication(object commandParameter)
    {
        return true;
    }

    private static void OnCloseApplication(object commandParameter)
    {
        var service = _serviceProvider.GetRequiredService<INavigationService>();
        if (commandParameter is string str && str is "terminate")
        {
            service.ShutDownXiletrade();
        }
        service.CloseMainView();
    }
}

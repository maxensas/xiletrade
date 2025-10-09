using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.ViewModels.Command;

public sealed partial class TrayMenuCommand : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    public TrayMenuCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private static void OpenAbout(object commandParameter)
    {
        StringBuilder message = new("Version: ");
        message.Append(Common.GetFileVersion()).AppendLine().AppendLine()
            .Append(Resources.Resources.Main120_About1).AppendLine().AppendLine()
            .Append(Resources.Resources.Main121_About2).AppendLine().AppendLine()
            .Append(Resources.Resources.Main122_About3);
        var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        service.Show(message.ToString(), "Xiletrade by maxensas", MessageStatus.Information);
    }

    [RelayCommand]
    private static void CheckUpdate(object commandParameter)
    {
        _serviceProvider.GetRequiredService<IAutoUpdaterService>().CheckUpdate(manualCheck: true);
    }

    [RelayCommand]
    private static void OpenConfig(object commandParameter)
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

    [RelayCommand]
    private static void CloseApplication(object commandParameter)
    {
        var service = _serviceProvider.GetRequiredService<INavigationService>();
        if (commandParameter is string str && str is "terminate")
        {
            service.ShutDownXiletrade();
        }
        service.CloseMainView();
        _serviceProvider.GetRequiredService<MainViewModel>().ClearContentViewModels();
    }
}

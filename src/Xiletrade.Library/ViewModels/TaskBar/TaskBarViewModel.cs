using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.ViewModels.TaskBar;

public sealed partial class TaskBarViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageAdapterService _message;
    private readonly MainViewModel _vm;
    private IAutoUpdaterService AutoUpdater => _serviceProvider.GetRequiredService<IAutoUpdaterService>();
    private INavigationService Navigation => _serviceProvider.GetRequiredService<INavigationService>();
    private DataManagerService Dm => _serviceProvider.GetRequiredService<DataManagerService>();

    

    [ObservableProperty]
    private bool authenticated;

    [ObservableProperty]
    private bool authentication;

    [ObservableProperty]
    private string notifyName;

    public TaskBarViewModel(IServiceProvider serviceProvider
        , IMessageAdapterService message, MainViewModel vm)
    {
        _serviceProvider = serviceProvider;
        _message = message;
        _vm = vm;

        notifyName = "Xiletrade " + Common.GetFileVersion();
    }

    [RelayCommand]
    private void OpenAbout(object commandParameter)
    {
        StringBuilder sb = new("Version: ");
        sb.Append(Common.GetFileVersion()).AppendLine().AppendLine()
            .Append(Resources.Resources.Main120_About1).AppendLine().AppendLine()
            .Append(Resources.Resources.Main121_About2).AppendLine().AppendLine()
            .Append(Resources.Resources.Main122_About3);
        _message.Show(sb.ToString(), "Xiletrade by maxensas", MessageStatus.Information);
    }

    [RelayCommand]
    private void CheckUpdate(object commandParameter) 
        => AutoUpdater.CheckUpdateAsync(manualCheck: true);

    [RelayCommand]
    private void OpenConfig(object commandParameter)
    {
        IntPtr pHwnd = Native.FindWindow(null, Strings.WindowName.Config);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        var service = Navigation;
        service.CloseMainView();
        service.ShowConfigView();
    }

    [RelayCommand]
    private void CloseApplication(object commandParameter)
    {
        var service = Navigation;
        if (commandParameter is string str && str is "terminate")
        {
            service.ShutDownXiletrade();
        }
        service.CloseMainView();
        _vm.ClearContentViewModels();
    }

    [RelayCommand]
    private void AuthenticateApplication(object commandParameter)
    {
        if (commandParameter is string str && str is "authenticate")
        {
            var dm = Dm;
            var useSecret = !string.IsNullOrEmpty(dm.Config.Options.Secret);
            var url = Strings.UrlXiletradeAuth + (useSecret ? $"?secret={dm.Config.Options.Secret}" : string.Empty);
            _vm.OpenUrlTask(url, UrlType.Xiletrade);
        }
    }
}

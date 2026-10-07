using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.ViewModels.TaskBar;

[ViewModelCreation(ViewModelCreation.Container)]
public sealed partial class TaskBarViewModel : ViewModelBase
{
    private readonly IMessageAdapterService _message;
    private readonly ITokenService _token;
    private readonly DataManagerService _dm;
    private readonly IAutoUpdaterService _updater;
    private readonly IViewManager _view;
    private readonly MainViewModel _vm;

    [ObservableProperty]
    private bool authenticated;

    [ObservableProperty]
    private bool authentication;

    [ObservableProperty]
    private string notifyName;

    public TaskBarViewModel(ILogger<TaskBarViewModel> logger, ITokenService token,
        IMessageAdapterService message, IViewManager view, 
        IAutoUpdaterService updater, DataManagerService dm, MainViewModel vm)
    {
        _message = message;
        _token = token;
        _view = view;
        _updater = updater;
        _dm = dm;
        _vm = vm;

        notifyName = "Xiletrade " + Common.GetFileVersion();

        RefreshAuthenticationState();

#if DEBUG
        logger.LogInformation("ViewModel initialized");
#endif
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
    private void CheckUpdate(object commandParameter) => _updater.CheckForUpdateAsync(manualCheck: true);

    [RelayCommand]
    private void OpenConfig(object commandParameter) => _view.ShowConfigView();

    [RelayCommand]
    private void CloseApplication(object commandParameter)
    {
        if (commandParameter is string str && str is "terminate")
        {
            _view.ShutDownNativeApp();
        }
        _view.CloseMainView();
        _vm.ClearContentViewModels();
    }

    [RelayCommand]
    private void AuthenticateApplication(object commandParameter)
    {
        if (commandParameter is string str && str is "authenticate")
        {
            var useSecret = !string.IsNullOrEmpty(_dm.Config.Options.Secret);
            var url = Strings.Url.XiletradeAuth + (useSecret ? $"?secret={_dm.Config.Options.Secret}" : string.Empty);
            _vm.OpenUrlTask(url, UrlType.Xiletrade);
        }
    }

    public void RefreshAuthenticationState()
    {
        _token.LoadTokens();

        Authenticated = _token.CacheToken is not null;
        Authentication = !string.IsNullOrEmpty(_dm.Config.Options.Secret);
    }
}

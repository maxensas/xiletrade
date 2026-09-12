using System;
using System.Threading.Tasks;
using System.Windows;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

public class MessageAdapterService : IMessageAdapterService
{
    private readonly INavigationService _navigation;
    private readonly MainView _main;

    public MessageAdapterService(INavigationService navigation, MainView main)
    {
        _navigation = navigation;
        _main = main;
    }

    public void Show(string message, string caption, MessageStatus status)
    {
        var icon = status is MessageStatus.Exclamation ? MessageBoxImage.Exclamation
            : status is MessageStatus.Information ? MessageBoxImage.Information
            : status is MessageStatus.Warning ? MessageBoxImage.Warning
            : MessageBoxImage.Error;
        var action = new Action(() =>
        {
            MessageBox.Show(_main, message, caption, MessageBoxButton.OK, icon);
        });
        _navigation.DelegateActionToUiThread(action);
    }

    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        var icon = status is MessageStatus.Exclamation ? MessageBoxImage.Exclamation
            : status is MessageStatus.Information ? MessageBoxImage.Information
            : status is MessageStatus.Warning ? MessageBoxImage.Warning
            : MessageBoxImage.Error;
        var func = new Func<MessageBoxResult>(() =>
        {
            return MessageBox.Show(_main, message, caption, yesNo ? MessageBoxButton.YesNo : MessageBoxButton.OK, icon);
        });

        var boxResult = _navigation.DelegateFuncToUiThread(func);         
        return boxResult.Equals(MessageBoxResult.Yes) || boxResult.Equals(MessageBoxResult.OK);
    }

    // not async
    public Task<bool> ShowResultAsync(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        return Task.Run(() => ShowResult(message, caption, status, yesNo));
    }
}

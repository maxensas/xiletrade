using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.UI.Avalonia.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Dto;

namespace Xiletrade.UI.Avalonia.Services;

public class MessageAdapterService : IMessageAdapterService
{
    private static IServiceProvider _serviceProvider;

    public MessageAdapterService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Show(string message, string caption, MessageStatus status)
    {
        var icon = status switch
        {
            MessageStatus.Exclamation => Icon.Warning,
            MessageStatus.Information => Icon.Info,
            MessageStatus.Warning => Icon.Warning,
            _ => Icon.Error
        };

        var mainWindow = _serviceProvider.GetRequiredService<MainView>();

        void Action()
        {
            var msgBox = MessageBoxManager
                .GetMessageBoxStandard(new MessageBoxStandardParams
                {
                    ContentTitle = caption,
                    ContentMessage = message,
                    Icon = icon,
                    ButtonDefinitions = ButtonEnum.Ok,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });

            msgBox.ShowAsPopupAsync(mainWindow);
        }

        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(Action);
    }

    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        var mainWindow = _serviceProvider.GetRequiredService<Window>();

        var icon = GetMessageBoxIcon(status);

        var buttons = yesNo ? ButtonEnum.YesNo : ButtonEnum.Ok;

        // Synchronously wait for result (blocking call)
        var result = MessageBoxManager
            .GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = caption,
                ContentMessage = message,
                Icon = icon,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            })
            .ShowAsPopupAsync(mainWindow)
            .GetAwaiter()
            .GetResult();

        return result == ButtonResult.Yes || result == ButtonResult.Ok;
    }

    private static Icon GetMessageBoxIcon(MessageStatus status)
    {
        return status switch
        {
            MessageStatus.Exclamation => Icon.Warning,
            MessageStatus.Information => Icon.Info,
            MessageStatus.Warning => Icon.Warning,
            _ => Icon.Error,
        };
    }
}

using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

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
        var icon = GetMessageBoxIcon(status);

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
            _ = msgBox.ShowAsync();
        }

        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(Action);
    }

    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ShowResultAsync(string message, string caption, MessageStatus status, bool yesNo = false)
    {
        var icon = GetMessageBoxIcon(status);
        var buttons = yesNo ? ButtonEnum.YesNo : ButtonEnum.Ok;

        var navService = _serviceProvider.GetRequiredService<INavigationService>();

        var result = await navService.DelegateActionToUiThreadAsync(async () =>
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = caption,
                ContentMessage = message,
                Icon = icon,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            });

            return await msgBox.ShowAsync();
        });

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

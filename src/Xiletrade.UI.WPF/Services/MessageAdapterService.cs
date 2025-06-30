using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services
{
    public class MessageAdapterService : IMessageAdapterService
    {
        private static IServiceProvider _serviceProvider;

        public MessageAdapterService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Show(string message, string caption, MessageStatus status)
        {
            var icon = status is MessageStatus.Exclamation ? MessageBoxImage.Exclamation
                : status is MessageStatus.Information ? MessageBoxImage.Information
                : status is MessageStatus.Warning ? MessageBoxImage.Warning
                : MessageBoxImage.Error;
            var main = _serviceProvider.GetRequiredService<MainView>();
            var action = new Action(() =>
            {
                MessageBox.Show(main, message, caption, MessageBoxButton.OK, icon);
            });
            _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(action);
        }

        public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
        {
            var icon = status is MessageStatus.Exclamation ? MessageBoxImage.Exclamation
                : status is MessageStatus.Information ? MessageBoxImage.Information
                : status is MessageStatus.Warning ? MessageBoxImage.Warning
                : MessageBoxImage.Error;
            var main = _serviceProvider.GetRequiredService<MainView>();
            var func = new Func<MessageBoxResult>(() =>
            {
                return MessageBox.Show(main, message, caption, yesNo ? MessageBoxButton.YesNo : MessageBoxButton.OK, icon);
            });

            var boxResult = _serviceProvider.GetRequiredService<INavigationService>().DelegateFuncToUiThread(func);         
            return boxResult.Equals(MessageBoxResult.Yes) || boxResult.Equals(MessageBoxResult.OK);
        }
    }
}

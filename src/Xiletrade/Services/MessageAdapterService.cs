using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Threading;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Views;

namespace Xiletrade.Services
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
    }
}

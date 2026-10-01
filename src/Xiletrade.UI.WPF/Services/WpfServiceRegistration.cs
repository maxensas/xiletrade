using Microsoft.Extensions.DependencyInjection;
using Notification.Core;
using Notification.Wpf.DependencyInjection;
using System;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Regex;
using Xiletrade.Library.ViewModels.TaskBar;
using Xiletrade.UI.WPF.UserControls.Main;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

public static class WpfServiceRegistration
{
    /// <summary>
    /// Windows WPF platform implementation
    /// </summary>
    /// <param name="sc"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IServiceCollection AddWpfPlatform(this IServiceCollection sc, string args)
    {
        return sc.AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IKeyboardAdapterService, KeyboardAdapterService>()
            .AddSingleton<IClipboardAdapterService, ClipboardAdapterService>()
            .AddWpfNotifications(cfg =>
            {
                cfg.SuccessBackgroundColor = NotificationColor.FromHex("#FF252525");
                cfg.ErrorBackgroundColor = NotificationColor.FromHex("#FF252525");
                cfg.SuccessIconColor = NotificationColor.LimeGreen;
                cfg.ErrorIconColor = NotificationColor.OrangeRed;
            })
            // views
            .AddSingleton<IMainView>(sp => new MainView(sp.GetRequiredService<MainViewModel>()))
            .AddSingleton<ITaskbar>(sp => new TaskbarIcon(sp.GetRequiredService<TaskBarViewModel>()))
            .AddTransient<IConfigView>(sp =>
            {
                var scope = sp.CreateScope();
                var vm = scope.ServiceProvider.GetRequiredService<ConfigViewModel>();
                return new ConfigView(vm, scope);
            })
            .AddTransient<IEditorView>(sp => new EditorView(sp.GetRequiredService<EditorViewModel>()))
            .AddTransient<IRegexView>(sp => new RegexView(sp.GetRequiredService<RegexManagerViewModel>()))
            .AddTransient<IUpdateView, UpdateView>()
            // library
            .AddLibraryServices(args);
    }
}
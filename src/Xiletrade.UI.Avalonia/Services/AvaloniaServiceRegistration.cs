using Microsoft.Extensions.DependencyInjection;
using Notification.Avalonia;
using Notification.Core;
using System;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Regex;
using Xiletrade.UI.Avalonia.Views;

namespace Xiletrade.UI.Avalonia.Services;

public static class AvaloniaServiceRegistration
{
    /// <summary>
    /// Avalonia platform implementation
    /// </summary>
    /// <param name="sc"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IServiceCollection AddAvaloniaPlatform(this IServiceCollection sc, string args)
    {
        // Avalonia imp
        sc.AddSingleton<IViewManager, ViewManager>()
            .AddAvaloniaNotifications(cfg => // TO TEST
            {
                cfg.SuccessBackgroundColor = NotificationColor.FromHex("#FF252525");
                cfg.ErrorBackgroundColor = NotificationColor.FromHex("#FF252525");
                cfg.SuccessIconColor = NotificationColor.LimeGreen;
                cfg.ErrorIconColor = NotificationColor.OrangeRed;
                cfg.DefaultExpirationTime = TimeSpan.FromSeconds(5);
            })
            // views
            .AddSingleton<IMainView>(sp => new MainView(sp, sp.GetRequiredService<MainViewModel>()))
            /* TODO
            .AddSingleton<ITaskbar>(sp => new TaskbarIcon(sp.GetRequiredService<TaskBarViewModel>()))
            */
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

        return sc;
    }
}

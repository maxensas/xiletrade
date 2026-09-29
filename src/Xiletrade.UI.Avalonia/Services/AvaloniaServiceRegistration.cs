using Microsoft.Extensions.DependencyInjection;
using Notification.Avalonia;
using Notification.Core;
using System;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux;
using Xiletrade.Library.Services.Windows;
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
        //services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

        // Platform-specific library services
        if (OperatingSystem.IsWindows())
        {
            sc.AddSingleton<IHookService>(sp => new WindowsHookService(sp.GetRequiredService<WndProcService>().ProcessMessageAsync))
                .AddSingleton<IProtocolRegisterService, WindowsProtocolRegisterService>()
                .AddSingleton<ISendInputService, WindowsSendInputService>();
        }
        else if (OperatingSystem.IsLinux())
        {
            //TOTEST
            sc.AddSingleton<IHookService>(sp => new LinuxHookService(sp.GetRequiredService<WndProcService>().ProcessMessageAsync))
                .AddSingleton<IProtocolRegisterService, LinuxProtocolRegisterService>()
                .AddSingleton<ISendInputService, LinuxSendInputService>();
        }
        else
        {
            return sc;
        }
        // Avalonia imp
        sc.AddSingleton<IClipboardAdapterService, ClipboardAdapterService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IMessageAdapterService, MessageAdapterService>()
            .AddAvaloniaNotifications(cfg => // TO TEST
            {
                cfg.SuccessBackgroundColor = NotificationColor.FromHex("#FF252525");
                cfg.ErrorBackgroundColor = NotificationColor.FromHex("#FF252525");
                cfg.SuccessIconColor = NotificationColor.LimeGreen;
                cfg.ErrorIconColor = NotificationColor.OrangeRed;
                cfg.DefaultExpirationTime = TimeSpan.FromSeconds(5);
            })
            // views
            .AddSingleton(sp => new MainView(sp, sp.GetRequiredService<MainViewModel>()))
            .AddTransient(sp => new ConfigView(sp.CreateScope().ServiceProvider.GetRequiredService<ConfigViewModel>()))
            .AddTransient(sp => new EditorView(sp.GetRequiredService<EditorViewModel>()))
            .AddTransient(sp => new RegexView(sp.GetRequiredService<RegexManagerViewModel>()))
            .AddTransient<UpdateView>()
            // library
            .AddLibraryServices(args);

        return sc;
    }
}

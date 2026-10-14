using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Regex;
using Xiletrade.Library.ViewModels.Start;
using Xiletrade.Library.ViewModels.TaskBar;
using Xiletrade.Library.Views;
using Xiletrade.UI.Avalonia.UserControls;
using Xiletrade.UI.Avalonia.Util;
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
            .AddSingleton<IMessageAdapterService, MessageAdapterService>()
            .AddSingleton<INotificationService>(sp => new NotificationService(dismiss 
                => new NotificationHost(sp.GetRequiredService<IUIService>(), dismiss)))
            // views
            .AddSingleton<IMainView>(sp => new MainView(sp, sp.GetRequiredService<MainViewModel>()))
            .AddSingleton<ITaskbar>(sp => new TaskbarIcon(sp.GetRequiredService<TaskBarViewModel>()))
            .AddTransient<IConfigView>(sp =>
            {
                var scope = sp.CreateScope();
                var vm = scope.ServiceProvider.GetRequiredService<ConfigViewModel>();
                return new ConfigView(vm, scope);
            })
            .AddTransient<IEditorView>(sp => new EditorView(sp.GetRequiredService<EditorViewModel>()))
            .AddTransient<IRegexView>(sp => new RegexView(sp.GetRequiredService<RegexManagerViewModel>()))
            .AddTransient<IStartView>(sp => new StartView(sp.GetRequiredService<StartViewModel>()))
            // library
            .AddLibraryServices(args);

        return sc;
    }
}

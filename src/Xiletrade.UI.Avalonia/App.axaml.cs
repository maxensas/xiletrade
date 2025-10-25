using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.UI.Avalonia.Views;

namespace Xiletrade.UI.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services => Program.AppHost.Services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            // Starts Xiletrade application.
#if DEBUG
            var logger = Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation("Launching Xiletrade service");
#endif
            try
            {
                
                await Services.GetRequiredService<XiletradeService>().Start();
                desktop.MainWindow = Services.GetRequiredService<MainView>();
            }
            catch (Exception ex)
            {
                var message = Services.GetRequiredService<IMessageAdapterService>();
                message.Show("Failed to launch Xiletrade :\n" + ex.InnerException.Message, ex.Message, MessageStatus.Exclamation);
                Environment.Exit(1);
            }
#if DEBUG
            logger.LogInformation("Xiletrade launched");
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Xiletrade.Library.Services;
using Xiletrade.UI.Avalonia.Views;

namespace Xiletrade.UI.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services => Program.AppHost.Services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        /*
        var dict = new ResourceInclude(new Uri("avares://Xiletrade/Styles/Media.axaml"));
        this.Resources.MergedDictionaries.Add(dict);*/
    }

    public override void OnFrameworkInitializationCompleted()
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
            Services.GetRequiredService<XiletradeService>();
            desktop.MainWindow = Services.GetRequiredService<MainView>();
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
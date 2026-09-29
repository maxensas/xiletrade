using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.UI.Avalonia.Services;
using Xiletrade.UI.Avalonia.Util;

namespace Xiletrade.UI.Avalonia;

internal sealed class Program
{
    private static Mutex _mutex = null;

    public static IHost AppHost { get; private set; }
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        StringBuilder sbArgs = new();
        foreach (var arg in args)
        {
            sbArgs.AppendLine(arg);
        }

        AppHost = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) 
            => services.AddAvaloniaPlatform(sbArgs.ToString())).UseDefaultServiceProvider(o =>
            {
                // o.ValidateOnBuild = true;
                o.ValidateScopes = true;
            }).Build();
        AppHost.Start();
#if DEBUG
        var logger = AppHost.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application services initialized");
#endif

        if (!TryInitMutex())
        {
            if (string.IsNullOrEmpty(sbArgs.ToString()))
            {
                // App is already running and no argument provided: just show info message
                AppHost.Services.GetRequiredService<IMessageAdapterService>().Show(
                    Library.Resources.Resources.Main188_Alreadystarted,
                    Library.Resources.Resources.Main189_Duplicateexecute,
                    MessageStatus.Information
                );
            }
            else
            {
                // App is already running and a protocol URL was passed: forward it to the running instance
                AppHost.Services.GetRequiredService<IProtocolHandlerService>().SendToRunningInstance(sbArgs.ToString());
            }

            // Exit the current (secondary) instance
            Environment.Exit(0);
            return;
        }
        AppHost.Services.GetRequiredService<IFileLoggerService>().Reset();

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            RunException(ex);
        };

        try
        {
            Localization.Initialize(AppHost.Services);
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            AppHost.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static bool TryInitMutex()
    {
        // Create a unique mutex name based on the assembly
        var assembly = Assembly.GetExecutingAssembly();
        var appName = AppDomain.CurrentDomain.FriendlyName;
        var guidAttr = (GuidAttribute)Attribute.GetCustomAttribute(assembly, typeof(GuidAttribute));
        var mutexName = string.Format(CultureInfo.InvariantCulture,
            "Local\\{{{0}}}{{{1}}}", guidAttr.Value, appName);

        // Try to create a mutex to check if another instance is already running
        _mutex = new(true, mutexName, out bool createdNew);

        return createdNew;
    }

    private static void RunException(Exception ex)
    {
        AppHost.Services.GetRequiredService<IFileLoggerService>().Log(ex);
        if (ex.InnerException is not null)
        {
            RunException(ex.InnerException);
            return;
        }
        Environment.Exit(2);
    }
}

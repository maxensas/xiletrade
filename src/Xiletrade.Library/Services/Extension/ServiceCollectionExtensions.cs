using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Versioning;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Models.Application.Adapter;
using Xiletrade.Library.Models.Application.Diagnostic;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux;
using Xiletrade.Library.Services.Windows;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Result;
using Xiletrade.Library.ViewModels.Regex;
using Xiletrade.Library.ViewModels.Start;
using Xiletrade.Library.ViewModels.TaskBar;

#if MOCK_API
using Xiletrade.Library.Services.Adapter;
#endif
#if !DEBUG
using Microsoft.Extensions.Logging.Abstractions;
#endif

namespace Xiletrade.Library.Services.Extension;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all library services.
    /// </summary>
    /// <remarks>
    /// Here we pass all cross platform related implementations
    /// </remarks>
    /// <param name="sc"></param>
    /// <returns></returns>
    public static IServiceCollection AddLibraryServices(this IServiceCollection sc, string args)
    {
        sc.AddCommonLibraryServices(args);

        if (OperatingSystem.IsWindows())
        {
            sc.AddWindowsServices();
            return sc;
        }

        if (OperatingSystem.IsLinux())
        {
            sc.AddLinuxServices();
            return sc;
        }
        throw new PlatformNotSupportedException("Xiletrade is only supported on Windows and Linux.");
    }

    /// <summary>
    /// Add the library services which do not depend on the platform.
    /// </summary>
    internal static IServiceCollection AddCommonLibraryServices(this IServiceCollection sc, string args)
    {
        sc.AddSingleton(new StartupArguments(args))
            .AddSingleton<XiletradeService>()
            .AddSingleton<DataManagerService>()
            .AddSingleton<PoeApiService>()
            .AddSingleton<PoeNinjaService>()
            .AddSingleton<LocalizationService>()
            .AddSingleton<FeatureProvider>()
            .AddSingleton<CrashReportService>()
            .AddSingleton<IUIService, UIService>()
            .AddSingleton<IKeyboardAdapterService, KeyboardAdapterService>()
            .AddSingleton<IDataUpdaterService, DataUpdaterService>()
            .AddSingleton<IAutoUpdaterService, AutoUpdaterService>()
            .AddSingleton<ITokenService, TokenService>()
            .AddSingleton<IUpdateDownloader, UpdateDownloader>()
            // logs
            .AddSingleton<IFileLoggerService, FileLoggerService>()
#if DEBUG
            .AddLogging(builder => builder.ClearProviders().AddDebug().SetMinimumLevel(LogLevel.Debug))
            .AddTransient(typeof(ILogger<>), typeof(TimestampedLoggerFactory<>))
#else
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
#endif
#if MOCK_API
            .AddSingleton<INetService, NetServiceAdapter>()
#else
            .AddSingleton<INetService, NetService>()
#endif
            // viewmodels :
            .AddSingleton<IViewModelProvider, ViewModelProvider>()
            // with attribute [ViewModelCreation(ViewModelCreation.Container)]
            .AddSingleton<MainViewModel>()
            .AddSingleton<TaskBarViewModel>()
            .AddScoped<ConfigViewModel>()
            .AddTransient<EditorViewModel>()
            .AddTransient<RegexViewModel>()
            .AddTransient<RegexManagerViewModel>()
            .AddTransient<NinjaViewModel>()
            .AddTransient<ResultViewModel>()
            .AddTransient<StartViewModel>();

        return sc;
    }

    [SupportedOSPlatform("windows")]
    private static IServiceCollection AddWindowsServices(this IServiceCollection sc)
    {
        sc.TryAddSingleton<IMessageAdapterService, WindowsMessageAdapterService>();
        sc.TryAddSingleton<IClipboardService, WindowsClipboardService>();
        sc.TryAddSingleton<IInputService, WindowsInputService>();
        sc.TryAddSingleton<IProtocolHandlerService, WindowsProtocolHandlerService>();
        sc.TryAddSingleton<IPoeActionService, WindowsPoeActionService>();
        sc.TryAddSingleton<IShortcutDispatcher, WindowsShortcutDispatcher>();
        sc.TryAddSingleton<IHookService>(sp =>
            new WindowsHookService(sp.GetRequiredService<IShortcutDispatcher>().HandleMessage));
        return sc;
    }

    /// <remarks>
    /// Needs an X11 session or XWayland. Optional tools : xclip, xsel or wl-clipboard (clipboard),
    /// zenity, kdialog or xmessage (message boxes, when the UI framework does not provide its own).
    /// </remarks>
    [SupportedOSPlatform("linux")]
    internal static IServiceCollection AddLinuxServices(this IServiceCollection sc)
    {
        sc.TryAddSingleton<LinuxDesktopService>();
        sc.TryAddSingleton<IMessageAdapterService, LinuxMessageAdapterService>();
        sc.TryAddSingleton<IClipboardService, LinuxClipboardService>();
        sc.TryAddSingleton<IInputService, LinuxInputService>();
        sc.TryAddSingleton<IProtocolHandlerService, LinuxProtocolHandlerService>();
        sc.TryAddSingleton<IPoeActionService, LinuxPoeActionService>();
        sc.TryAddSingleton<IShortcutDispatcher, LinuxShortcutDispatcher>();
        sc.TryAddSingleton<IHookService>(sp =>
            new LinuxHookService(sp.GetRequiredService<IShortcutDispatcher>().HandleMessage));
        return sc;
    }
}

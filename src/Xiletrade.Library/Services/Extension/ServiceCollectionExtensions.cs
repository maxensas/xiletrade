using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xiletrade.Library.Models.Application;
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
        sc.AddSingleton(new StartupArguments(args))
            .AddSingleton<XiletradeService>()
            .AddSingleton<DataManagerService>()
            .AddSingleton<ShortcutDispatcher>()
            .AddSingleton<PoeApiService>()
            .AddSingleton<PoeNinjaService>()
            .AddSingleton<InputService>()
            .AddSingleton<ClipboardService>()
            .AddSingleton<LocalizationService>()
            .AddSingleton<FeatureProvider>()
            .AddSingleton<IUIService, UIService>()
            .AddSingleton<IKeyboardAdapterService, KeyboardAdapterService>()
            .AddSingleton<IDataUpdaterService, DataUpdaterService>()
            .AddSingleton<IAutoUpdaterService, AutoUpdaterService>()
            .AddSingleton<ITokenService, TokenService>()
            .AddSingleton<IUpdateDownloader, UpdateDownloader>()
            .AddSingleton<IProtocolHandlerService, ProtocolHandlerService>()
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

        if (OperatingSystem.IsWindows())
        {
            return sc.AddSingleton<IMessageAdapterService, WindowsMessageAdapterService>()
                .AddSingleton<IClipboardAdapterService, WindowsClipboardAdapterService>()
                .AddSingleton<IProtocolRegisterService, WindowsProtocolRegisterService>()
                .AddSingleton<IPoeActionService, WindowsPoeActionService>()
                .AddSingleton<IHookService>(sp => new WindowsHookService(sp.GetRequiredService<ShortcutDispatcher>().HandleMessage));
        }

        // TODO
        if (OperatingSystem.IsLinux())
        {
            return sc//.AddSingleton<IMessageAdapterService, LinuxMessageAdapterService>()
                .AddSingleton<IProtocolRegisterService, LinuxProtocolRegisterService>()
                .AddSingleton<IPoeActionService, LinuxPoeActionService>()
                .AddSingleton<IHookService>(sp => new LinuxHookService(sp.GetRequiredService<ShortcutDispatcher>().HandleMessage));
        }

        throw new PlatformNotSupportedException("Xiletrade is only supported on Windows.");
    }
}

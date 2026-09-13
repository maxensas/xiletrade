using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Models.Application.Diagnostic;
using Xiletrade.Library.Models.Application.Hotkey.Converter;
using Xiletrade.Library.Services.Adapter;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Regex;

namespace Xiletrade.Library.Services;

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
            .AddSingleton<DataUpdaterService>()
            .AddSingleton<WndProcService>()
            .AddSingleton<PoeApiService>()
            .AddSingleton<PoeNinjaService>()
            .AddSingleton<HotKeyService>()
            .AddSingleton<ClipboardService>()
            .AddSingleton<LocalizationService>()
            .AddSingleton<IAutoUpdaterService, AutoUpdaterService>()
            .AddSingleton<ITokenService, TokenService>()
            .AddSingleton<IUpdateDownloader, UpdateDownloader>()
            .AddSingleton<IProtocolHandlerService, ProtocolHandlerService>()
            .AddSingleton<IKeysConverter, KeysConverter>()
            // logs
            .AddSingleton<IFileLoggerService, FileLoggerService>()
#if DEBUG
            .AddLogging(builder => builder.ClearProviders().AddDebug().SetMinimumLevel(LogLevel.Debug))
            .AddTransient(typeof(ILogger<>), typeof(TimestampedLoggerFactory<>))
#else
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
#endif
#if MOCK_API
            .AddSingleton<NetService>(sp => new NetServiceAdapter(sp)) // mock without using interface
#else
            .AddSingleton<NetService>()
#endif
            // viewmodels
            .AddSingleton<MainViewModel>()
            .AddScoped<ConfigViewModel>()
            .AddTransient<EditorViewModel>()
            .AddTransient<RegexManagerViewModel>();
        return sc;
    }
}

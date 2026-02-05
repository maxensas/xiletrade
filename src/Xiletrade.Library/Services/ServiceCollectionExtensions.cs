using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xiletrade.Library.Models.Application.Diagnostic;
using Xiletrade.Library.Models.Application.Hotkey.Converter;
using Xiletrade.Library.Services.Adapter;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Main;

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
        bool mockApi = false;
#if MOCK_API
        mockApi = true;
#endif
        sc.AddSingleton(sp => new XiletradeService(sp, args))
            .AddSingleton<DataManagerService>()
            .AddSingleton<DataUpdaterService>()
            .AddSingleton<WndProcService>()
            .AddSingleton<NetService>(sp => new NetServiceAdapter(sp, mockApi)) // mock without using interface
            .AddSingleton<PoeApiService>()
            .AddSingleton<PoeNinjaService>()
            .AddSingleton<HotKeyService>()
            .AddSingleton<ClipboardService>()
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
#endif
            // viewmodels
            .AddSingleton<MainViewModel>()
            .AddScoped<ConfigViewModel>()
            .AddTransient<EditorViewModel>()
            .AddTransient<RegexManagerViewModel>();
        return sc;
    }
}

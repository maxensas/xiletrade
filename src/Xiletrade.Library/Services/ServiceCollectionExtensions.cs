using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xiletrade.Library.Models.Application.Diagnostic;
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
        sc.AddSingleton(s => new XiletradeService(s, args))
            .AddSingleton<DataManagerService>()
            .AddSingleton<DataUpdaterService>()
            .AddSingleton<WndProcService>()
            .AddSingleton<NetService>()
            .AddSingleton<PoeApiService>()
            .AddSingleton<PoeNinjaService>()
            .AddSingleton<HotKeyService>()
            .AddSingleton<ClipboardService>()
            .AddSingleton<ITokenService, TokenService>()
            .AddSingleton<IUpdateDownloader, UpdateDownloader>()
            .AddSingleton<IProtocolHandlerService, ProtocolHandlerService>()
            // logs
            .AddSingleton<IFileLoggerService, FileLoggerService>()
            .AddLogging(builder => builder.ClearProviders().AddDebug().SetMinimumLevel(LogLevel.Debug))
            .AddTransient(typeof(ILogger<>), typeof(TimestampedLoggerFactory<>))
            // viewmodels
            .AddSingleton<MainViewModel>()
            .AddScoped<ConfigViewModel>()
            .AddTransient<EditorViewModel>()
            .AddTransient<RegexManagerViewModel>();
        return sc;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all library services.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLibraryServices(this IServiceCollection services)
    {
        services.AddSingleton<XiletradeService>()
            .AddSingleton<DataManagerService>()
            .AddSingleton<DataUpdaterService>()
            .AddSingleton<WndProcService>()
            .AddSingleton<NetService>()
            .AddSingleton<PoeApiService>()
            .AddSingleton<HotKeyService>()
            .AddSingleton<ClipboardService>()
            .AddSingleton<IUpdateDownloader, UpdateDownloader>();
        return services;
    }
}

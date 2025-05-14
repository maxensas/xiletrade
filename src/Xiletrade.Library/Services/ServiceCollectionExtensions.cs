using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Main;

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
            .AddSingleton<WndProcService>()
            .AddSingleton<NetService>()
            .AddSingleton<PoeApiService>()
            .AddSingleton<HotKeyService>()
            .AddSingleton<ClipboardService>();

        return services;
    }
}

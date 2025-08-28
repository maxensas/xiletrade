using Microsoft.Extensions.DependencyInjection;
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
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLibraryServices(this IServiceCollection services, string args)
    {
        services.AddSingleton(s => new XiletradeService(s, args))
            .AddSingleton<DataManagerService>()
            .AddSingleton<DataUpdaterService>()
            .AddSingleton<WndProcService>()
            .AddSingleton<NetService>()
            .AddSingleton<PoeApiService>()
            .AddSingleton<HotKeyService>()
            .AddSingleton<ClipboardService>()
            .AddSingleton<IUpdateDownloader, UpdateDownloader>()
            .AddSingleton<IProtocolHandlerService, ProtocolHandlerService>()
            // viewmodels
            .AddSingleton<MainViewModel>()
            .AddScoped<ConfigViewModel>()
            .AddTransient<EditorViewModel>()
            .AddTransient<RegexManagerViewModel>();
        return services;
    }
}

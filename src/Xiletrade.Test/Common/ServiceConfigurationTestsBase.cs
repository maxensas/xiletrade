using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Form;
using Xiletrade.Library.ViewModels.Main.Result;
using Xiletrade.Library.ViewModels.Regex;
using Xiletrade.Library.ViewModels.TaskBar;

namespace Xiletrade.Test.Common;

public abstract class ServiceConfigurationTestsBase
{
    /// Actual composition of the tested platform.
    protected abstract IServiceCollection CreateServices();

    /// Replaces components with side effects (windows, threads, network...).
    protected virtual void ReplaceSideEffects(IServiceCollection services) { }

    /// Services that the platform MUST provide to the bookstore.
    protected virtual IEnumerable<Type> RequiredServices =>
    [
        // framework imp
        typeof(INavigationService),
        typeof(IClipboardAdapterService),
        //INotifications
        typeof(IMainView),
        typeof(ITaskbar),
        typeof(IConfigView),
        typeof(IEditorView),
        typeof(IRegexView),
        typeof(IUpdateView),
        // platform imp
        typeof(IMessageAdapterService),
        typeof(IProtocolRegisterService),
        typeof(ISendInputService),
        typeof(IHookService),
        // lib imp
        typeof(StartupArguments),
        typeof(XiletradeService),
        typeof(DataManagerService),
        typeof(DataUpdaterService),
        typeof(WndProcService),
        typeof(PoeApiService),
        typeof(PoeNinjaService),
        typeof(HotKeyService),
        typeof(ClipboardService),
        typeof(LocalizationService),
        typeof(FeatureProviderService),
        typeof(IUIService),
        typeof(IAutoUpdaterService),
        typeof(ITokenService),
        typeof(IUpdateDownloader),
        typeof(IProtocolHandlerService),
        typeof(IKeysConverter),
        typeof(IFileLoggerService),
        //typeof(ILogger<>),
        typeof(INetService),
        //vm
        typeof(MainViewModel),
        typeof(TaskBarViewModel),
        typeof(ResultViewModel),
        typeof(NinjaViewModel),
        typeof(RegexViewModel),
        typeof(RegexManagerViewModel),

        //typeof(FormViewModel),
        //typeof(ConfigViewModel),
        //typeof(EditorViewModel),
    ];

    protected ServiceProvider BuildProvider()
    {
        var services = CreateServices();
        ReplaceSideEffects(services);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    [Fact]
    public void Container_Should_Build_And_Validate()
    {
        var act = () => BuildProvider();
        act.Should().NotThrow();
    }

    [Fact]
    //[STAThread]
    public void Required_Services_Should_Be_Registered()
    {
        //SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

        using var provider = BuildProvider();
        foreach (var type in RequiredServices)
            provider.GetService(type).Should().NotBeNull($"{type.Name} is required");
    }

    /*
    [Fact]
    public void All_Closed_Registrations_Should_Resolve()
    {
        var services = CreateServices();
        ReplaceSideEffects(services);
        using var provider = services.BuildServiceProvider(validateScopes: true);

        var failures = services
            .Where(d => !d.ServiceType.ContainsGenericParameters)   // ignore open generics
            .Where(d => d.ServiceKey is null)
            .Select(d =>
            {
                try { provider.GetRequiredService(d.ServiceType); return null; }
                catch (Exception ex) { return $"{d.ServiceType.Name}: {ex.Message}"; }
            })
            .Where(m => m is not null);

        failures.Should().BeEmpty();
    }
    */


    /// Assemblies à scanner. Par défaut, celle de ViewModelBase (la librairie).
    protected virtual IEnumerable<Assembly> ViewModelAssemblies =>
        new[] { typeof(ViewModelBase).Assembly };

    /// ViewModels créés à la main : type -> arguments runtime (hors dépendances DI).
    protected virtual IReadOnlyDictionary<Type, object[]> ManuallyCreatedViewModels =>
        new Dictionary<Type, object[]>();

    private IEnumerable<Type> DiscoverViewModels() => ViewModelAssemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => t.IsClass
                    && !t.IsAbstract
                    && !t.ContainsGenericParameters
                    && typeof(ViewModelBase).IsAssignableFrom(t))
        .Distinct();
    /*
    [Fact]
    public void Registered_ViewModels_Should_Be_Resolvable()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var viewModelTypes = DiscoverViewModels()
            .Where(t => !ManuallyCreatedViewModels.ContainsKey(t))
            .ToList();

        Assert.NotEmpty(viewModelTypes);

        var failures = viewModelTypes
            .Select(vm =>
            {
                try { scope.ServiceProvider.GetRequiredService(vm); return null; }
                catch (Exception ex) { return $"{vm.Name}: {ex.GetBaseException().Message}"; }
            })
            .Where(m => m is not null)
            .ToList();

        Assert.True(failures.Count == 0,
            "ViewModels non résolubles (à enregistrer, ou à déclarer dans ManuallyCreatedViewModels) :"
            + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }
    */
    [Fact]
    public void Manually_Created_ViewModels_Should_Be_Constructible_With_Container_Dependencies()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var failures = ManuallyCreatedViewModels
            .Select(entry =>
            {
                try { ActivatorUtilities.CreateInstance(scope.ServiceProvider, entry.Key, entry.Value); return null; }
                catch (Exception ex) { return $"{entry.Key.Name}: {ex.GetBaseException().Message}"; }
            })
            .Where(m => m is not null)
            .ToList();

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
}
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
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
        typeof(IKeyboardAdapterService),
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
        typeof(IPoeActionService),
        typeof(IHookService),
        // lib imp
        typeof(StartupArguments),
        typeof(XiletradeService),
        typeof(DataManagerService),
        typeof(ShortcutDispatcher),
        typeof(PoeApiService),
        typeof(PoeNinjaService),
        typeof(InputService),
        typeof(ClipboardService),
        typeof(LocalizationService),
        typeof(FeatureProvider),
        typeof(IUIService),
        typeof(IDataUpdaterService),
        typeof(IAutoUpdaterService),
        typeof(ITokenService),
        typeof(IUpdateDownloader),
        typeof(IProtocolHandlerService),
        typeof(IFileLoggerService),
        typeof(INetService),
        typeof(IViewModelProvider),
        // registered vm
        typeof(MainViewModel),
        typeof(TaskBarViewModel),
        typeof(ResultViewModel),
        typeof(NinjaViewModel),
        typeof(RegexViewModel),
        typeof(RegexManagerViewModel),
        typeof(EditorViewModel),
        typeof(ConfigViewModel)
    ];

    private static IEnumerable<Type> GetViewModelsFromAssembly(ViewModelCreation mode) =>
        typeof(ViewModelBase).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel"))
        .Where(t => t.GetCustomAttribute<ViewModelCreationAttribute>()?.Mode == mode
            || (mode == ViewModelCreation.Manual && t.GetCustomAttribute<ViewModelCreationAttribute>() is null));

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
    public void _01_Container_Should_Build_And_Validate()
    {
        var act = () => BuildProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void _02_Required_Services_Should_Be_Registered()
    {
        //SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        foreach (var type in RequiredServices)
            scope.ServiceProvider.GetService(type).Should().NotBeNull($"{type.Name} is required");
    }

    [Fact]
    public void _03_All_Closed_Registrations_Should_Resolve()
    {
        var services = CreateServices();
        ReplaceSideEffects(services);
        using var provider = services.BuildServiceProvider(validateScopes: true);

        var failures = new List<string>();

        var desc = services.Where(d => !d.ServiceType.ContainsGenericParameters).Where(d => d.ServiceKey is null);
        foreach (var d in desc)
        {
            try
            {
                if (d.Lifetime == ServiceLifetime.Scoped)
                {
                    using var scope = provider.CreateScope();
                    scope.ServiceProvider.GetRequiredService(d.ServiceType);
                }
                else
                {
                    provider.GetRequiredService(d.ServiceType);
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{d.ServiceType.Name} [{d.Lifetime}]:{Environment.NewLine}{ex.GetBaseException()}");
            }
        }

        failures.Should().BeEmpty();
    }

    [Fact]
    public void _04_Container_ViewModels_Should_Be_Resolvable()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var vms = GetViewModelsFromAssembly(ViewModelCreation.Container);

        Assert.NotEmpty(vms);

        var failures = vms.Select(vm =>
            {
                try { scope.ServiceProvider.GetRequiredService(vm); return null; }
                catch (Exception ex) { return $"{vm.Name}:{Environment.NewLine}{ex.GetBaseException()}"; }
            }).Where(m => m is not null).ToList();

        Assert.True(failures.Count is 0,
            "ViewModels marked as Container but not resolvable (need to be registered or have their mode changed) :"
            + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void _05_Non_Container_ViewModels_Should_Not_Be_Registered_Directly()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var isService = scope.ServiceProvider.GetService<IServiceProviderIsService>()!;

        var vms = GetViewModelsFromAssembly(ViewModelCreation.Manual)
            .Concat(GetViewModelsFromAssembly(ViewModelCreation.Activator))
            .Concat(GetViewModelsFromAssembly(ViewModelCreation.Factory));

        var wronglyRegistered = vms.Where(t => isService.IsService(t))
            .Select(t => $"{t.Name} ({t.GetCreationMode()})").ToList();

        Assert.True(wronglyRegistered.Count == 0,
            "ViewModels Manual/Activator registered in the container (fix the attribute or the registration):"
            + Environment.NewLine+ string.Join(Environment.NewLine, wronglyRegistered));
    }

    [Fact]
    public void _06_Activator_ViewModels_Should_Be_Constructible_With_Utilities()
    {
        using var sp = BuildProvider();
        using var scope = sp.CreateScope();

        List<string> failures = new();

        var vms = GetViewModelsFromAssembly(ViewModelCreation.Activator);
        foreach (var vm in vms)
        {
            try
            {
                var instance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, vm);
            }
            catch (Exception ex)
            {
                failures.Add($"{vm.Name}:{Environment.NewLine}{ex.GetBaseException()}");
            }
        }

        Assert.True(failures.Count is 0,
            "Non-instantiable Activator ViewModels (unregistered dependency or missing runtime argument):"
            + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }
}
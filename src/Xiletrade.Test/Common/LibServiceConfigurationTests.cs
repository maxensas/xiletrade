using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Test.Common;

public class LibServiceConfigurationTests : LibServiceConfiguration
{
    [Fact]
    public void _01_Container_Should_Build_And_Validate()
    {
        var act = () => BuildProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void _02_Library_Required_Services_Should_Be_Registered()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        foreach (var type in LibRequiredServices)
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
            + Environment.NewLine + string.Join(Environment.NewLine, wronglyRegistered));
    }

    [Fact]
    public void _06_Activator_ViewModels_Should_Be_Constructible_With_Utilities()
    {
        using var sp = BuildProvider();
        using var scope = sp.CreateScope();
        var isService = scope.ServiceProvider.GetRequiredService<IServiceProviderIsService>();

        List<string> failures = new();

        var vms = GetViewModelsFromAssembly(ViewModelCreation.Activator);
        foreach (var vm in vms)
        {
            var ctors = vm.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            foreach (var ctor in ctors)
            {
                // Parameters not provided by the container = runtime arguments
                var runtimeParams = ctor.GetParameters().Where(p => !isService.IsService(p.ParameterType));

                try
                {
                    var argTypes = runtimeParams.Select(p => p.ParameterType).ToArray();
                    var args = runtimeParams.Select(CreateRuntimeArgument).ToArray();

                    var factory = ActivatorUtilities.CreateFactory(vm, argTypes);
                    var instance = factory(scope.ServiceProvider, args);

                    (instance as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    var signature = string.Join(", ", ctor.GetParameters()
                        .Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    failures.Add($"{vm.Name}({signature}):{Environment.NewLine}{ex.GetBaseException()}");
                }
            }
        }

        Assert.True(failures.Count is 0,
            "Non-instantiable Activator ViewModels (unregistered dependency or invalid runtime argument):"
            + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }
}

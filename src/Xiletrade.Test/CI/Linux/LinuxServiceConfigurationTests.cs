using System.Runtime.Versioning;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux;
using Xiletrade.Test.Configuration;

namespace Xiletrade.Test.CI.Linux;

/// <summary>
/// Composition of the library on Linux : every service of the platform is registered and resolvable.
/// </summary>
[SupportedOSPlatform("linux")]
public class LinuxServiceConfigurationTests : LibServiceConfiguration
{
    protected override IServiceCollection CreateServices() =>
        new ServiceCollection().AddCommonLibraryServices(args: null).AddLinuxServices();

    protected override void ReplaceSideEffects(IServiceCollection services)
    {
        base.ReplaceSideEffects(services);
        services
            // writes a desktop entry in the profile of the current user
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IProtocolHandlerService>()))
            // would open dialogs
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IMessageAdapterService>()));
    }

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
    public void _03_Hook_And_Dispatcher_Should_Be_Wired_Together()
    {
        using var provider = BuildProvider();

        provider.GetRequiredService<IHookService>().Should().BeOfType<LinuxHookService>();
        provider.GetRequiredService<IShortcutDispatcher>().Should().BeOfType<LinuxShortcutDispatcher>();
        provider.GetRequiredService<IPoeActionService>().Should().BeOfType<LinuxPoeActionService>();
        provider.GetRequiredService<IClipboardService>().Should().BeOfType<LinuxClipboardService>();
        provider.GetRequiredService<IInputService>().Should().BeOfType<LinuxInputService>();
    }

    [Fact]
    public void _04_Ui_Framework_Services_Should_Take_Precedence()
    {
        var message = Mock.Of<IMessageAdapterService>();
        // a UI framework registers its own services before the library : TryAdd keeps them
        var framework = new ServiceCollection();
        framework.AddSingleton(message);
        framework.AddLinuxServices();

        framework.Count(d => d.ServiceType == typeof(IMessageAdapterService)).Should().Be(1);
        framework.Single(d => d.ServiceType == typeof(IMessageAdapterService))
            .ImplementationInstance.Should().BeSameAs(message);
    }
}

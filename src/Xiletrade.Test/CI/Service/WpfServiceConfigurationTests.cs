using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Views;
using Xiletrade.Test.Configuration;
using Xiletrade.UI.WPF.Services;

namespace Xiletrade.Test.CI.Service;

public class WpfServiceConfigurationTests : ServiceConfigurationBase
{
    protected override IServiceCollection CreateServices() =>
        new ServiceCollection().AddWpfPlatform(args: null);

    protected override void ReplaceSideEffects(IServiceCollection services)
    {
        services
            // ui
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IViewManager>())) // null object
            .Replace(ServiceDescriptor.Singleton(Mock.Of<ITaskbar>()))
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IMainView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IConfigView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IRegexView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IStartView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IEditorView>())) // null object
            // lib
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IUIService>())) // STA
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IDataUpdaterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IClipboardAdapterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IMessageAdapterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IProtocolRegisterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<ISendInputService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IHookService>()))
            ;
    }

    // WPF-specific tests
    [Fact]
    public void _01_Framework_Required_Services_Should_Be_Registered()
    {
        //SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        foreach (var type in FrameworkRequiredServices)
            scope.ServiceProvider.GetService(type).Should().NotBeNull($"{type.Name} is required");
    }
}
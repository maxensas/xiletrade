using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.UI.WPF.Services;

namespace Xiletrade.Test.Common;

public class WpfServiceConfigurationTests : ServiceConfigurationTestsBase
{
    protected override IServiceCollection CreateServices() =>
        new ServiceCollection().AddWpfPlatform(args: null);

    protected override void ReplaceSideEffects(IServiceCollection services)
    {
        services
            // lib
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IUIService>())) // STA
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IDataUpdaterService>()))
            // platform
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IMainView>()))
            .Replace(ServiceDescriptor.Singleton(Mock.Of<ITaskbar>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IConfigView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IRegexView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IUpdateView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IEditorView>())) // null object
            .Replace(ServiceDescriptor.Singleton(Mock.Of<INavigationService>())) // null object
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IClipboardAdapterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IMessageAdapterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IProtocolRegisterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<ISendInputService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IHookService>()))
            ;
    }

    // WPF-specific tests
    /*
    [Fact]
    public void Message_Adapter_Should_Be_Windows_Implementation() { }
    */
}
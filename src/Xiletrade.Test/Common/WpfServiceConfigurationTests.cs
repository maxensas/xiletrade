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
            // STA
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IUIService>())) // from lib
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IMainView>()))
            .Replace(ServiceDescriptor.Singleton(Mock.Of<ITaskbar>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IConfigView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IRegexView>()))
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IUpdateView>()))
            // null object
            .Replace(ServiceDescriptor.Transient(_ => Mock.Of<IEditorView>()))
            .Replace(ServiceDescriptor.Singleton(Mock.Of<INavigationService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IClipboardAdapterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IMessageAdapterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IProtocolRegisterService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<ISendInputService>()))
            //.Replace(ServiceDescriptor.Singleton(Mock.Of<IHookService>()))
            ;
    }

    // Tests spécifiques WPF
    /*
    [Fact]
    public void Message_Adapter_Should_Be_Windows_Implementation() { }
    */
}
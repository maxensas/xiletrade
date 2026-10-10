using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Views;

namespace Xiletrade.Test.Common;

public class LibServiceConfiguration : ServiceConfigurationBase
{
    protected override IServiceCollection CreateServices() =>
        new ServiceCollection().AddLibraryServices(args: null);
    
    protected override void ReplaceSideEffects(IServiceCollection services)
    {
        services
            // ui
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IViewManager>())) // null object
            .Replace(ServiceDescriptor.Singleton(Mock.Of<ITaskbar>()))
            .Replace(ServiceDescriptor.Singleton(Mock.Of<IMainView>()))
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
}

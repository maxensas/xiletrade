using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xiletrade.Library.Models.Application;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Editor;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Main.Result;
using Xiletrade.Library.ViewModels.Regex;
using Xiletrade.Library.ViewModels.Start;
using Xiletrade.Library.ViewModels.TaskBar;
using Xiletrade.Library.Views;

namespace Xiletrade.Test.Common;

public abstract class ServiceConfigurationBase
{
    /// Actual composition of the tested platform.
    protected abstract IServiceCollection CreateServices();

    /// Replaces components with side effects (windows, threads, network...).
    protected virtual void ReplaceSideEffects(IServiceCollection services) { }

    /// Services that the UI framework MUST provide to the bookstore.
    protected virtual IEnumerable<Type> FrameworkRequiredServices =>
    [
        // framework imp
        typeof(IViewManager),
        //INotifications
        typeof(IMainView),
        typeof(ITaskbar),
        typeof(IConfigView),
        typeof(IEditorView),
        typeof(IRegexView),
        typeof(IStartView),
    ];

    /// Services that the library MUST provide to the bookstore.
    protected virtual IEnumerable<Type> LibRequiredServices =>
    [
        // platform imp
        typeof(IMessageAdapterService),
        typeof(IClipboardService),
        typeof(IInputService),
        typeof(IProtocolHandlerService),
        typeof(IPoeActionService),
        typeof(IShortcutDispatcher),
        typeof(IHookService),
        // lib imp
        typeof(StartupArguments),
        typeof(XiletradeService),
        typeof(DataManagerService),
        typeof(PoeApiService),
        typeof(PoeNinjaService),
        typeof(LocalizationService),
        typeof(FeatureProvider),
        typeof(IUIService),
        typeof(IDataUpdaterService),
        typeof(IAutoUpdaterService),
        typeof(ITokenService),
        typeof(IUpdateDownloader),
        typeof(IFileLoggerService),
        typeof(INetService),
        typeof(IViewModelProvider),
        typeof(IKeyboardAdapterService),
        // registered vm
        typeof(MainViewModel),
        typeof(TaskBarViewModel),
        typeof(ResultViewModel),
        typeof(NinjaViewModel),
        typeof(RegexViewModel),
        typeof(RegexManagerViewModel),
        typeof(EditorViewModel),
        typeof(ConfigViewModel),
        typeof(StartViewModel)
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

    protected static IEnumerable<Type> GetViewModelsFromAssembly(ViewModelCreation mode) =>
        typeof(ViewModelBase).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel"))
        .Where(t => t.GetCustomAttribute<ViewModelCreationAttribute>()?.Mode == mode
            || (mode == ViewModelCreation.Manual && t.GetCustomAttribute<ViewModelCreationAttribute>() is null));

    protected static object CreateRuntimeArgument(ParameterInfo p)
    {
        if (p.HasDefaultValue && p.DefaultValue is not null)
            return p.DefaultValue;

        var t = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;

        if (t.IsValueType)
            return Activator.CreateInstance(t)!; // false, 0, default enum, empty struct...

        if (t == typeof(string))
            return string.Empty;

        // Interface / abstract class: mock Moq
        if (t.IsInterface || t.IsAbstract)
        {
            var mock = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(t))!;
            return mock.Object;
        }

        // Class with a parameterless constructor
        if (t.GetConstructor(Type.EmptyTypes) is not null)
            return Activator.CreateInstance(t)!;

        // Last resort: uninitialized instance (constructor not executed)
        return RuntimeHelpers.GetUninitializedObject(t);
    }
}
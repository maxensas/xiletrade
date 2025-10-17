using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.UI.WPF.Services;
using Xiletrade.Library.Services.Windows;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF;

/// <summary>
/// Interaction logic for Xiletrade WPF UI (Windows implementation) using Xiletrade core library.
/// </summary>
public partial class App : Application, IDisposable
{ 
    private static Mutex _mutex = null;
    private IServiceProvider _serviceProvider;

    public static IServiceProvider Services => ((App)Current)._serviceProvider;

    [STAThread]
    protected override void OnStartup(StartupEventArgs e)
    {
        // Optional: Force rendering mode if needed
        // RenderOptions.ProcessRenderMode = Global.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;

        // Get the protocol URL passed as an argument, if any
        string urlArg = e.Args.Length > 0 ? e.Args[0] : null;

        // Initialize application services
        _serviceProvider = InitServices(urlArg);
        var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Application services initialized");

        if (!TryInitMutex())
        {
            if (string.IsNullOrEmpty(urlArg))
            {
                // App is already running and no argument provided: just show info message
                _serviceProvider.GetRequiredService<IMessageAdapterService>().Show(
                    Library.Resources.Resources.Main188_Alreadystarted,
                    Library.Resources.Resources.Main189_Duplicateexecute,
                    MessageStatus.Information
                );
            }
            else
            {
                // App is already running and a protocol URL was passed: forward it to the running instance
                _serviceProvider.GetRequiredService<IProtocolHandlerService>().SendToRunningInstance(urlArg);
            }

            // Exit the current (secondary) instance
            Environment.Exit(0);
            return;
        }
        _serviceProvider.GetRequiredService<IFileLoggerService>().Reset();

        // Remove all trace listeners from the data binding (no more console display)
        // TO Disable: [TaskBarIcon] non - blocking error 40 BindingExpression on launch
        PresentationTraceSources.DataBindingSource.Listeners.Clear();

        // Subscribe to global exception handler
        Current.DispatcherUnhandledException += AppDispatcherUnhandledException;

        // Starts Xiletrade application.
        logger.LogInformation("Launching Xiletrade service");
        try
        {
            _serviceProvider.GetRequiredService<XiletradeService>();
        }
        catch (Exception ex)
        {
            var message = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            message.Show("Failed to launch Xiletrade :\n" + ex.InnerException.Message, ex.Message, MessageStatus.Exclamation);
            Environment.Exit(1);
        }
        
        logger.LogInformation("Xiletrade launched");

        base.OnStartup(e);
    }

    private static bool TryInitMutex()
    {
        // Create a unique mutex name based on the assembly
        var assembly = Assembly.GetExecutingAssembly();
        var appName = AppDomain.CurrentDomain.FriendlyName;
        var guidAttr = (GuidAttribute)Attribute.GetCustomAttribute(assembly, typeof(GuidAttribute));
        var mutexName = string.Format(CultureInfo.InvariantCulture,
            "Local\\{{{0}}}{{{1}}}", guidAttr.Value, appName);

        // Try to create a mutex to check if another instance is already running
        _mutex = new(true, mutexName, out bool createdNew);

        return createdNew;
    }

    private static ServiceProvider InitServices(string args)
    {
        var sc = new ServiceCollection();
        ConfigureServices(sc, args);
        return sc.BuildServiceProvider();
    }

    // Here we pass all windows platform related implementations
    private static void ConfigureServices(IServiceCollection sc, string args)
    {
        // WPF imp
        sc.AddSingleton<IWindowService, WindowService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<IMessageAdapterService, MessageAdapterService>()
            .AddSingleton<IClipboardAdapterService, ClipboardAdapterService>()
            .AddSingleton<System.ComponentModel.TypeConverter, System.Windows.Forms.KeysConverter>()
            // views
            .AddSingleton(sp => new MainView(sp.GetRequiredService<MainViewModel>()))
            .AddTransient(sp => new ConfigView(sp.CreateScope().ServiceProvider.GetRequiredService<ConfigViewModel>()))
            .AddTransient(sp => new EditorView(sp.GetRequiredService<EditorViewModel>()))
            .AddTransient(sp => new RegexView(sp.GetRequiredService<RegexManagerViewModel>()))
            .AddTransient<UpdateView>()
            // library
            .AddSingleton<IProtocolRegisterService, WindowsProtocolRegisterService>()
            .AddSingleton<ISendInputService, WindowsSendInputService>()
            .AddSingleton<IHookService>(sp => new WindowsHookService(sp.GetRequiredService<WndProcService>().ProcessMessageAsync))
            .AddLibraryServices(args);
    }

    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        RunException(e.Exception);
        e.Handled = true;
    }

    private void RunException(Exception ex)
    {
        _serviceProvider.GetRequiredService<IFileLoggerService>().Log(ex);
        if (ex.InnerException is not null)
        {
            RunException(ex.InnerException);
            return;
        }
        Current.Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable?.Dispose();
        }
        Dispose();
        base.OnExit(e);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _mutex is not null)
        {
            _mutex.ReleaseMutex();
            _mutex.Close();
            _mutex = null;
        }
    }
}
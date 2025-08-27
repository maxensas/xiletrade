using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Config;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.UI.WPF.Services;
using Xiletrade.UI.WPF.Util.Hook;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF;

/// <summary>
/// interaction logic for App.xaml
/// </summary>
/// <remarks>
/// MvvM : Xiletrade use one view/template per WPF window.
/// </remarks>
public partial class App : Application, IDisposable
{ 
    private static string _logFilePath;
    private static Mutex _mutex = null;

    public static IServiceProvider ServiceProvider { get; private set; } // host not needed for now
    //public IConfiguration Configuration { get; private set; }

    // TODO Identify needed services that can be usefull to add : IConfiguration, ILoggerFactory, ...
    // https://marcominerva.wordpress.com/2019/03/06/using-net-core-3-0-dependency-injection-and-service-provider-with-wpf/
    // DI Setup - FULL STACK WPF (.NET CORE) MVVM
    // https://www.youtube.com/watch?v=3EzHn9ir5M8

    [STAThread]
    protected override void OnStartup(StartupEventArgs e)
    {
        // Optional: Force rendering mode if needed
        // RenderOptions.ProcessRenderMode = Global.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;

        // Create a unique mutex name based on the assembly
        Assembly assembly = Assembly.GetExecutingAssembly();
        string mutexName = string.Format(CultureInfo.InvariantCulture,
            "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);

        // Try to create a mutex to check if another instance is already running
        _mutex = new(true, mutexName, out bool createdNew);

        // Initialize application services
        ServiceProvider = InitServices();

        // Get the protocol URL passed as an argument, if any
        string urlArg = e.Args.Length > 0 ? e.Args[0] : null;

        if (!createdNew)
        {
            if (string.IsNullOrEmpty(urlArg))
            {
                // App is already running and no argument provided: just show info message
                ServiceProvider.GetRequiredService<IMessageAdapterService>().Show(
                    Library.Resources.Resources.Main188_Alreadystarted,
                    Library.Resources.Resources.Main189_Duplicateexecute,
                    Library.Models.Enums.MessageStatus.Information
                );
            }
            else
            {
                // App is already running and a protocol URL was passed: forward it to the running instance
                ServiceProvider.GetRequiredService<IProtocolHandlerService>().SendToRunningInstance(urlArg);
            }

            // Exit the current (secondary) instance
            Environment.Exit(0);
            return;
        }

        // Prepare the log file
        _logFilePath = Path.GetFullPath("Xiletrade.log");
        if (File.Exists(_logFilePath)) File.Delete(_logFilePath);

        // Subscribe to global exception handler
        Current.DispatcherUnhandledException += AppDispatcherUnhandledException;

        // Automatically register or update the custom protocol handler in the registry
        ServiceProvider.GetRequiredService<IProtocolHandlerService>().RegisterOrUpdateProtocol();

        // Starts pipe server.
        ServiceProvider.GetRequiredService<IProtocolHandlerService>().StartListening();

        // Starts Xiletrade application.
        ServiceProvider.GetRequiredService<XiletradeService>();

        // If a protocol URL was passed on first launch, handle it now
        if (!string.IsNullOrEmpty(urlArg))
        {
            ServiceProvider.GetRequiredService<IProtocolHandlerService>().HandleUrl(urlArg);
        }

        base.OnStartup(e);
    }
    
    private static IServiceProvider InitServices()
    {
        var sc = new ServiceCollection();
        ConfigureServices(sc);
        return sc.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        //services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

        services.AddSingleton<IWindowService, WindowService>()
            .AddSingleton<IProtocolHandlerService, ProtocolHandlerService>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IAutoUpdaterService, AutoUpdaterService>()
            .AddSingleton<ISendInputService, SendInputService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<IMessageAdapterService, MessageAdapterService>()
            .AddSingleton<IClipboardAdapterService, ClipboardAdapterService>()
            .AddSingleton<System.ComponentModel.TypeConverter, System.Windows.Forms.KeysConverter>()
            .AddSingleton<IHookService>(s => new SpongeWindow(s.GetRequiredService<WndProcService>().ProcessMessageAsync))
            .AddSingleton<MainViewModel>()
            .AddSingleton(s => new MainView(s.GetRequiredService<MainViewModel>()))
            .AddScoped<ConfigViewModel>()
            .AddTransient(s => new ConfigView(s.CreateScope().ServiceProvider.GetRequiredService<ConfigViewModel>()))
            .AddTransient<EditorViewModel>()
            .AddTransient(s => new EditorView(s.GetRequiredService<EditorViewModel>()))
            .AddTransient<RegexManagerViewModel>()
            .AddTransient(s => new RegexView(s.GetRequiredService<RegexManagerViewModel>()))
            .AddTransient<UpdateView>()
            .AddLibraryServices();
    }

    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        RunException(e.Exception);
        e.Handled = true;
    }

    private static void RunException(Exception ex)
    {
        try
        {
            File.AppendAllText(_logFilePath, String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
                , ex.Source, ex.Message, ex.StackTrace));
        }
        catch { }
        if (ex.InnerException is not null)
        {
            RunException(ex.InnerException);
            return;
        }
        Current.Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
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
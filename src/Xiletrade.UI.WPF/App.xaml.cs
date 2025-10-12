using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using Xiletrade.UI.WPF.Util.Hook;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF;

/// <summary>
/// Interaction logic for Xiletrade WPF UI (Windows implementation) using Xiletrade core library.
/// </summary>
public partial class App : Application, IDisposable
{ 
    private static string _logFilePath;
    private static Mutex _mutex = null;

    public static IServiceProvider ServiceProvider { get; private set; } // host not needed for now
    //public IConfiguration Configuration { get; private set; }

    [STAThread]
    protected override void OnStartup(StartupEventArgs e)
    {
        // Optional: Force rendering mode if needed
        // RenderOptions.ProcessRenderMode = Global.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;

        // Get the protocol URL passed as an argument, if any
        string urlArg = e.Args.Length > 0 ? e.Args[0] : null;

        // Initialize application services
        ServiceProvider = InitServices(urlArg);

        if (!TryInitMutex())
        {
            if (string.IsNullOrEmpty(urlArg))
            {
                // App is already running and no argument provided: just show info message
                ServiceProvider.GetRequiredService<IMessageAdapterService>().Show(
                    Library.Resources.Resources.Main188_Alreadystarted,
                    Library.Resources.Resources.Main189_Duplicateexecute,
                    MessageStatus.Information
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

        ResetLogFile();

        // Remove all trace listeners from the data binding (no more console display)
        // TO Disable: [TaskBarIcon] non - blocking error 40 BindingExpression on launch
        PresentationTraceSources.DataBindingSource.Listeners.Clear();

        // Subscribe to global exception handler
        Current.DispatcherUnhandledException += AppDispatcherUnhandledException;

        // Starts Xiletrade application.
        ServiceProvider.GetRequiredService<XiletradeService>();

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

    private static void ResetLogFile()
    {
        _logFilePath = Path.GetFullPath("Xiletrade.log");
        if (File.Exists(_logFilePath)) File.Delete(_logFilePath);
    }
    
    private static IServiceProvider InitServices(string args)
    {
        var sc = new ServiceCollection();
        ConfigureServices(sc, args);
        return sc.BuildServiceProvider();
    }

    // Here we pass all windows platform related implementations
    private static void ConfigureServices(IServiceCollection services, string args)
    {
        //services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

        services.AddSingleton<IWindowService, WindowService>()
            .AddSingleton<IProtocolRegisterService, ProtocolRegisterService>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IAutoUpdaterService, AutoUpdaterService>()
            .AddSingleton<ISendInputService, SendInputService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<IMessageAdapterService, MessageAdapterService>()
            .AddSingleton<IClipboardAdapterService, ClipboardAdapterService>()
            .AddSingleton<System.ComponentModel.TypeConverter, System.Windows.Forms.KeysConverter>()
            .AddSingleton<IHookService>(s => new SpongeWindow(s.GetRequiredService<WndProcService>().ProcessMessageAsync))
            // views
            .AddSingleton(s => new MainView(s.GetRequiredService<MainViewModel>()))
            .AddTransient(s => new ConfigView(s.CreateScope().ServiceProvider.GetRequiredService<ConfigViewModel>()))
            .AddTransient(s => new EditorView(s.GetRequiredService<EditorViewModel>()))
            .AddTransient(s => new RegexView(s.GetRequiredService<RegexManagerViewModel>()))
            .AddTransient<UpdateView>()
            // library
            .AddLibraryServices(args);
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
            File.AppendAllText(_logFilePath, string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
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
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Services;
using Xiletrade.Views;
using Xiletrade.Util.Hook;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.ViewModels.Main;
using Xiletrade.Library.ViewModels.Config;

namespace Xiletrade;

/// <summary>
/// interaction logic for App.xaml
/// </summary>
/// <remarks>
/// MvvM : Xiletrade use one view/template per WPF window.
/// </remarks>
public partial class App : Application, IDisposable
{ 
    private static string logFilePath;
    private static Mutex mutex = null;

    public static IServiceProvider ServiceProvider { get; private set; } // host not needed for now
    //public IConfiguration Configuration { get; private set; }

    // TODO Identify needed services that can be usefull to add : IConfiguration, ILoggerFactory, ...
    // https://marcominerva.wordpress.com/2019/03/06/using-net-core-3-0-dependency-injection-and-service-provider-with-wpf/
    // DI Setup - FULL STACK WPF (.NET CORE) MVVM
    // https://www.youtube.com/watch?v=3EzHn9ir5M8

    [STAThread]
    protected override void OnStartup(StartupEventArgs e)
    {
        //Render mode
        //RenderOptions.ProcessRenderMode = Global.DisableHardwareAcceleration ? RenderMode.SoftwareOnly : RenderMode.Default;
        Assembly assembly = Assembly.GetExecutingAssembly();
        var MutexName = String.Format(CultureInfo.InvariantCulture, 
            "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);
        mutex = new(true, MutexName, out bool createdNew);

        if (!createdNew)
        {
            MessageBox.Show(Library.Resources.Resources.Main188_Alreadystarted, 
                Library.Resources.Resources.Main189_Duplicateexecute, 
                MessageBoxButton.OK, MessageBoxImage.Information);
            Environment.Exit(-1);
            return;
        }
        /*
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls 
            | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        ServicePointManager.Expect100Continue = false;*/
        //ServicePointManager.DefaultConnectionLimit = 5;

        logFilePath = Path.GetFullPath("Xiletrade.log");
        if (File.Exists(logFilePath)) File.Delete(logFilePath);

        Current.DispatcherUnhandledException += AppDispatcherUnhandledException;

        ServiceProvider = StartXiletrade();
        base.OnStartup(e);
    }

    // WIP
    private static IServiceProvider StartXiletrade()
    {
        /*
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        Configuration = builder.Build();
        */
        var sc = new ServiceCollection();
        ConfigureServices(sc);
        IServiceProvider sp = sc.BuildServiceProvider();
        sp.GetRequiredService<XiletradeService>();
        return sp;
    }

    // WIP : moving helpers to services & lifetime/dispose optimization next
    private static void ConfigureServices(IServiceCollection services)
    {
        //services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

        //services.AddScoped<ISampleService, SampleService>();
        // ...
        services.AddSingleton<XiletradeService>()
            .AddSingleton<WndProcService>()
            .AddSingleton<IWindowService, WindowService>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IAutoUpdaterService, AutoUpdaterService>()
            .AddSingleton<ISendInputService, SendInputService>()
            .AddSingleton<MainViewModel>()
            .AddSingleton(s => new MainView(s.GetRequiredService<MainViewModel>()))
            .AddTransient<ConfigViewModel>()
            .AddTransient(s => new ConfigView(s.GetRequiredService<ConfigViewModel>()))
            .AddTransient<EditorViewModel>()
            .AddTransient(s => new EditorView(s.GetRequiredService<EditorViewModel>()))
            .AddTransient<RegexManagerViewModel>()
            .AddTransient(s => new RegexView(s.GetRequiredService<RegexManagerViewModel>()))
            .AddSingleton<System.ComponentModel.TypeConverter, System.Windows.Forms.KeysConverter>()
            .AddSingleton<IHookService>(s => new SpongeWindow(s.GetRequiredService<WndProcService>().ProcessMessageAsync))
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<NetService>()
            .AddSingleton<IMessageAdapterService, MessageAdapterService>()
            .AddSingleton<IClipboardAdapterService, ClipboardAdapterService>();
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
            File.AppendAllText(logFilePath, String.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
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
        if (disposing && mutex is not null)
        {
            mutex.ReleaseMutex();
            mutex.Close();
            mutex = null;
        }
    }
}
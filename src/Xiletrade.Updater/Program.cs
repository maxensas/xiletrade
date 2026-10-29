using Avalonia;
using System;
using System.IO;

namespace Xiletrade.Updater
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // The updater works on its current directory : the folder of the application (3rd argument),
            // whatever the way it was launched (in place or relocated).
            if (args.Length >= 3 && Path.GetDirectoryName(args[2]) is { Length: > 0 } installFolder
                && Directory.Exists(installFolder))
            {
                Directory.SetCurrentDirectory(installFolder);
            }
            // Before any Avalonia / SkiaSharp call : the native libraries are not needed to relocate the updater.
            if (Util.Relocation.TryRelaunch(args))
            {
                return;
            }
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        //
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}

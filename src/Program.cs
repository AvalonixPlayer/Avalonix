using Avalonia;
using System;
using NeoSimpleLogger;
using AvalonixAPI;
using System.Threading;

namespace Avalonix;


public static class Program
{
        public static Logger Logger = new(Logger.TypeLogger.Console);

        [STAThread]
        public static void Main(string[] args)
        {
                Logger.Info(".avalonix path - " + DiskManager.SettingsPath);

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        private static AppBuilder BuildAvaloniaApp()
        {

                Logger.Info("Building App");
#if DEBUG
                Logger.CallStack = true;
#endif
                return AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .WithInterFont()
                    .LogToTrace();

        }
}

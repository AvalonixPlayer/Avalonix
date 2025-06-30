using Avalonia;
using System;
using System.Threading.Tasks;
using Avalonix.AvalonixAPI;
using NeoSimpleLogger;

namespace Avalonix;

public static class Program
{
        public static readonly Logger Logger = new(Logger.TypeLogger.Console);

        [STAThread]
        public static async Task Main(string[] args)
        {
                Logger.Info(".avalonix path - " + DiskManager.SettingsPath);
                await UpdateVersion.CheckUpdates();
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
                                                            .UsePlatformDetect()
                                                            .LogToTrace();
}
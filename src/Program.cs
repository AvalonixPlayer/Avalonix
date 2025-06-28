using Avalonia;
using System;
using NeoSimpleLogger;
using AvalonixAPI;

namespace Avalonix;


public static class Program
{
        public static readonly Logger Logger = new(Logger.TypeLogger.Console);

        [STAThread]
        public static void Main(string[] args)
        {
                Logger.Info(".avalonix path - " + DiskManager.SettingsPath);
                UpdateVersion.CheckUpdates();

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        private static AppBuilder BuildAvaloniaApp()
        {
                Logger.Info("Building App");
                return AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .WithInterFont()
                    .LogToTrace();

        }
}
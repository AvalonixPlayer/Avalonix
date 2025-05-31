using Avalonia;
using System;
using NeoSimpleLogger;
using System.Threading;
using AvalonixAPI;

namespace Avalonix;


public static class Program
{
        public static Logger Logger = new(Logger.TypeLogger.Console);

        [STAThread]
        public static void Main(string[] args)
        {
                Logger.Info(".avalonix path - " + DiskManager.SettingsPath);

                PlaylistsManager.CreateNewPlaylist(new PlaylistData("TEST-1"));

                PlaylistsManager.ChangeSettingsToPlaylist("TEST-1", new PlaylistData("TEST-2"));

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
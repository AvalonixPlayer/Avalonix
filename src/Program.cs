using Avalonia;
using System;
using NeoSimpleLogger;
using AvalonixAPI;
using System.Collections.Generic;
using Avalonix.AvalonixAPI;

namespace Avalonix;


public static class Program
{
        public static Logger Logger = new(Logger.TypeLogger.Console);

        [STAThread]
        public static void Main(string[] args)
        {
                Logger.Info(".avalonix path - " + DiskManager.SettingsPath);

                var s = new Dictionary<string, SongData>
                {
                        {"ExamplePath",new SongData("Name",2024) }
                };

                PlaylistsManager.CreateNewPlaylist(new PlaylistData("TEST-1", s, 2024, "Gorillaz", "Example album"));
                PlaylistsManager.AddSongToPlaylist(PlaylistsManager.PlaylistsNames[0], "Example path", new SongData("Test"));

                //MediaPlayer.Play(PlaylistsManager.JsonToPlaylist(PlaylistsManager.PlaylistsPaths[0]).Songs[0]);

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
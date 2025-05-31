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

                List<string> sas = new List<string> { };

                PlaylistsManager.CreateNewPlaylist(new PlaylistData("TEST-1", sas, 2024, "Gorillaz", "Example album"));
                PlaylistsManager.AddSongToPlaylist(PlaylistsManager.PlaylistsNames[0], "Example path");

                MediaPlayer.Play(PlaylistsManager.JsonToPlaylist(PlaylistsManager.PlaylistsPaths[0]).SongsPaths[0]);

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
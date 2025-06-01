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

                var intiSong = new List<SongData>();
                intiSong.Add(new SongData("InitSong", "InitSongPath", 2025));

                string playlistName = PlaylistsManager.PlaylistsNames[0];

                PlaylistsManager.CreateNewPlaylist(new PlaylistData(playlistName, intiSong, 2024, "Gorillaz", "Example album"));
                PlaylistsManager.AddSongToPlaylist(playlistName, new SongData("CreatedSong", "CreatedSongPath"));

                PlaylistsManager.RemoveSongFromPlaylist(playlistName, "CreatedSong");
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
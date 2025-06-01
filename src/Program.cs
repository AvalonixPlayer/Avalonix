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

                List<SongData> songs = new List<SongData>();

                string playlistName = "TestPlaylist";

                PlaylistsManager.CreateNewPlaylist(new PlaylistData(playlistName, songs, 2024, "Gorillaz", "Example album"));
                PlaylistsManager.AddSongToPlaylist(playlistName, new SongData("CreatedSong", "CreatedSongPath"));

                foreach (var i in PlaylistsManager.SongsNamesInPlaylist(playlistName))
                {
                        Console.WriteLine(i);
                }

                PlaylistsManager.RemoveSongFromPlaylist(playlistName, "CreatedSong");

                PlaylistsManager.RemovePlaylist(playlistName);

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
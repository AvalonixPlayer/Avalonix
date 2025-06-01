using Avalonia;
using System;
using NeoSimpleLogger;
using AvalonixAPI;
using System.Collections.Generic;
using Avalonix.AvalonixAPI;
using System.Threading;

namespace Avalonix;


public static class Program
{
        public static Logger Logger = new(Logger.TypeLogger.Console);

        [STAThread]
        public static void Main(string[] args)
        {
                Logger.Info(".avalonix path - " + DiskManager.SettingsPath);

                Thread testPlaylistThread = new Thread(() =>
                {
                        List<SongData> songs = new List<SongData>() { new SongData("blood // water", @"F:\Плейлисты\Простой\Blood water.mp3") };

                        string playlistName = "TestPlaylist";

                        PlaylistsManager.CreateNewPlaylist(new PlaylistData(playlistName, songs));
                        //PlaylistsManager.AddSongToPlaylist(playlistName, new SongData("CreatedSong", "CreatedSongPath"));

                        PlaylistsManager.PlayPlaylist(playlistName);

                        Thread.Sleep(1000);

                        PlaylistsManager.PausePlaylist();

                        Thread.Sleep(1000);

                        PlaylistsManager.ContinuePlaylist();

                        Thread.Sleep(1000);

                        PlaylistsManager.StopPlaylist();

                        PlaylistsManager.PlayPlaylist(playlistName);

                        Thread.Sleep(1000);

                        PlaylistsManager.PausePlaylist();

                        Thread.Sleep(1000);

                        PlaylistsManager.ContinuePlaylist();

                        foreach (var i in PlaylistsManager.SongsNamesInPlaylist(playlistName))
                        {
                                Console.WriteLine(i);
                        }

                        //PlaylistsManager.RemoveSongFromPlaylist(playlistName, "CreatedSong");

                        //PlaylistsManager.RemovePlaylist(playlistName);

                        //MediaPlayer.Play(PlaylistsManager.JsonToPlaylist(PlaylistsManager.PlaylistsPaths[0]).Songs[0]);
                });

                testPlaylistThread.Start();
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
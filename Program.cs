using Avalonia;
using System;
using System.Collections.Generic;
using Avalonix.API;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        /* test Playlist creating and song adding
        var playlist = new PlaylistData("test_playlist");

        DiskManager.CreatePlaylist(playlist);
        playlist.AddTrack(new Track(@"F:\Плейлисты\Accept\Accept - Fast As A Shark.mp3").TrackData);
        */

        /* test Playlist loading
        Console.WriteLine(DiskManager.GetPlaylistData("test_playlist").Tracks[0].Lyric);
        */
        
        /* test Settings create
        var settings = new Settings();
        DiskManager.SaveSettings(settings);
        */

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
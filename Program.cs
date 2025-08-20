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
        playlist.AddTrack(new Track(@"C:\Accept\Accept - Teutonic Terror.mp3").TrackData);
        */
        
        /* test Playlist loading
        DiskManager.GetPlaylistData("test_playlists");
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
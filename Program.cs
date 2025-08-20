using Avalonia;
using System;
using System.Collections.Generic;
using Avalonix.API;

namespace Avalonix;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
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

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
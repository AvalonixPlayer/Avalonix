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
        //DiskManager.SavePlaylist(new Playlist("test"));
        //Console.WriteLine(DiskManager.GetPlaylist("test").Name);
        /*
        var playlist = DiskManager.GetPlaylist("test");
        
        playlist.AddTrack(new Track("F:\\Плейлисты\\Accept\\Accept - Fast As A Shark.mp3"));
        DiskManager.SavePlaylist(playlist);
        */
        
        Console.WriteLine(DiskManager.GetPlaylist("test").Tracks[0].TrackData.Path);
        Console.WriteLine(DiskManager.GetPlaylist("test").Tracks[0].Metadata.Album);
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
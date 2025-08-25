using Avalonia;
using System;
using Avalonix.API;
using Microsoft.Extensions.Logging;
using NeoSimpleLogger;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        
        var playlist = DiskManager.GetPlaylist("test");
        /*
        //playlist.AddTrack(new Track("F:\\Плейлисты\\Accept\\Accept - Fast As A Shark.mp3"));
        
        playlist.PlaylistData.Tracks[0].IncreaseRarity(1);
        playlist.PlaylistData.Tracks[0].UpdateLastListenDate();
        playlist.UpdateLastListenDate();
        
        
        playlist.Save();
        
        //Console.WriteLine(playlist.Tracks[0].TrackData.LastListen.Date);
        
        //DiskManager.SavePlaylist(playlist);
        */
        
        var player = new MediaPlayer(new Logger());
        player.Play(playlist.PlaylistData.Tracks[0]);
        player.Pause();
        player.Resume();
        player.Stop();
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
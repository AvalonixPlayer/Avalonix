#define DEBUG
using Avalonia;
using System;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.Models.Media.TrackFiles;
using NeoSimpleLogger;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        Logger logger = new();
        var playlist = await new DiskManager(logger, new MediaPlayer(logger)).GetPlaylist("test");
        //await playlist?.AddTrack(new Track("F:\\Плейлисты\\Metallica\\Metallica - Enter Sandman (Remastered 2021).mp3"))!;

        SortPlaylistTrackFlags flags = SortPlaylistTrackFlags.ArtistInverted;
        
        await playlist?.SortTracks(flags)!;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().LogToTrace().UsePlatformDetect();
}
using Avalonia;
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.Media.TrackFiles;
using NeoSimpleLogger;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var logger = new Logger();
        var playlist = new DiskManager(logger).GetPlaylist("test",new MediaPlayer(logger),new DiskManager(logger)).Result;
        Console.Write(playlist.ToString());
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().LogToTrace().UsePlatformDetect();
}
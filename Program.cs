#define DEBUG
using Avalonia;
using System;

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
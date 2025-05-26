using Avalonia;
using AvalonixAPI;
using System;
using System.Threading;

namespace Avalonix;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Playlist pl = new Playlist("gorkiy park");
        Thread thread = new Thread(() => pl.Play());
        thread.Start();

        /*
        while (true)
        {
            Console.WriteLine(MediaPlayer.TotalMusicTime());
            Thread.Sleep(100);
        }
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

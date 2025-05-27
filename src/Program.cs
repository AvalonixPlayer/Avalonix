using Avalonia;
using System;
using NeoSimpleLogger;
using AvalonixAPI;
using System.Threading;
using System.IO;

namespace Avalonix;

internal abstract class Program
{
    public static Logger? Logger { get; private set; }
    [STAThread]
    public static void Main(string[] args)
    {
        Logger = new Logger();
        Logger.Info("App started");
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

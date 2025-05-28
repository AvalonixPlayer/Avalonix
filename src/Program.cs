using Avalonia;
using System;
using NeoSimpleLogger;

namespace Avalonix;

public class Program
{
    public static Logger Logger = new(Logger.TypeLogger.Console);
    [STAThread]
    public static void Main(string[] args)
    {
        Logger.Info("App started");
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }


    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

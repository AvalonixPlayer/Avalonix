using Avalonia;
using System;
using NeoSimpleLogger;
using static NeoSimpleLogger.Logger;

namespace Avalonix;

public class Program
{
    public static Logger? Logger { get; private set; }
    [STAThread]
    public static void Main(string[] args)
    {
        Logger = new Logger(TypeLogger.Console);
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

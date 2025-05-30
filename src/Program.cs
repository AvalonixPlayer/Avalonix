using Avalonia;
using System;
using NeoSimpleLogger;

namespace Avalonix;

public class Program
{
    public static Logger Logger = new(Logger.TypeLogger.Console);
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);


    private static AppBuilder BuildAvaloniaApp()
    {
        Logger.Info("Building App");
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}

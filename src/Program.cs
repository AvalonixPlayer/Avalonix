using Avalonia;
using System;
using NeoSimpleLogger;
    
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

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

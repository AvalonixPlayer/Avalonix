#define DEBUG
using System;
using Avalonia;

namespace Avalonix;

internal static class Program
{
    private static readonly string _avalonixPipeName = "Avalonix_FilePipe";

    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>().LogToTrace().UsePlatformDetect();
    }
}
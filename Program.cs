using Avalonia;
using System;
using Avalonix.Models.Disk;
using Avalonix.Models.Media;
using NeoSimpleLogger;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().LogToTrace().UsePlatformDetect();
}
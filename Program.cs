#define DEBUG
using Avalonia;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonix.Services.AlbumManager;
using Avalonix.Services.VersionManager;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().LogToTrace().UsePlatformDetect();
}
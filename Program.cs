#define DEBUG
using Avalonia;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (!Directory.GetFiles(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!).Contains("bass.dll"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: bass.dll not found!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
            Environment.Exit(1);
        }
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().LogToTrace().UsePlatformDetect();
}
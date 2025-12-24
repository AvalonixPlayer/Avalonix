#define DEBUG
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;

namespace Avalonix;

internal static class Program
{
    private readonly static string _avalonixPipeName = "Avalonix_FilePipe";
    
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
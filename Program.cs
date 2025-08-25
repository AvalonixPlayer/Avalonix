using Avalonia;
using System;
using Avalonix.API;
using Microsoft.Extensions.Logging;
using NeoSimpleLogger;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
}
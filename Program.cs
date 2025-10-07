﻿#define DEBUG
using Avalonia;
using System;

namespace Avalonix;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().LogToTrace().UsePlatformDetect();
}
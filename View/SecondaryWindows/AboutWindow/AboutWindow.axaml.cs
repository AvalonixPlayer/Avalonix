using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Services.VersionManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SecondaryWindows.AboutWindow;

public partial class AboutWindow : Window
{
    private const string Url = "https://github.com/AvalonixPlayer/Avalonix";
    private readonly ILogger _logger;

    public AboutWindow(ILogger logger, IVersionManager versionManager)
    {
        _logger = logger;
        InitializeComponent();
        var currentRelease = Task.Run(versionManager.GetCurrentRelease).Result;
        var lastRelease = Task.Run(versionManager.GetLastRelease).Result;
        _logger.LogInformation("About window loaded");
        VersionLabel.Content = $"Version: {currentRelease.Version}";
        LastVersionLabel.Content = $"Last Version: {lastRelease.Version}";
    }

    private void OpenUrlButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _logger.LogInformation("About window opened");
        OpenUrlInBrowser(Url);
    }

    private void OpenUrlInBrowser(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", url);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start("xdg-open", url);
            else
                throw new PlatformNotSupportedException("Unsupported platform");
        }
        catch (Win32Exception noBrowser)
        {
            _logger.LogError("Error: No browser found to open {Url}. {NoBrowserMessage}", url, noBrowser.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {ExMessage}", ex.Message);
        }
    }
}
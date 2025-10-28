using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.AboutWindow;

public partial class AboutWindow : Window
{
    private readonly ILogger _logger;
    private const string Url = "https://github.com/AvalonixPlayer/Avalonix";
    public AboutWindow(ILogger logger, string version)
    {
        _logger = logger;
        InitializeComponent();
        _logger.LogInformation("About window loaded");
        VersionLabel.Content = $"Version: {version}";
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
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported platform");
            }
        }
        catch (System.ComponentModel.Win32Exception noBrowser)
        {
            _logger.LogError("Error: No browser found to open {Url}. {NoBrowserMessage}", url, noBrowser.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {ExMessage}", ex.Message);
        }
    }
}
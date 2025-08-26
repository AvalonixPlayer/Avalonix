using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;

namespace Avalonix.Services;

public class WindowManager(ILogger<WindowManager> logger, PlaylistCreateWindowViewModel _playlistCreateWindowViewModel) : IWindowManager
{
    private static void CloseMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        } 
    }

    public async Task CloseMainWindowAsync()
    {
        try
        {
            // Saving Data | NOT IMPLEMENTED
            await Task.Delay(TimeSpan.FromSeconds(0));
            CloseMainWindow();
        }
        catch (Exception ex)
        {
            logger.LogCritical("Error during closing: {ex}", ex);
            CloseMainWindow();
        }
    }

    public async Task<PlaylistCreateWindow> PlaylistCreateWindow_Open() => new(logger, _playlistCreateWindowViewModel);
}
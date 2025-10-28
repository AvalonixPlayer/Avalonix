using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.Services.SettingsManager;
using Avalonix.ViewModels;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.WindowManager;

public class WindowManager(ILogger<WindowManager> logger,
    IPlaylistEditOrCreateWindowViewModel playlistEditOrCreateWindowViewModel, 
    IPlaylistSelectWindowViewModel playlistSelectWindowViewModel,
    ISettingsManager settingsManager) 
    : IWindowManager
{
    private static void CloseMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) desktop.Shutdown();
    }

    public async Task CloseMainWindowAsync()
    {
        try
        {
            var settings = await settingsManager.GetSettings();
            await settingsManager.SaveSettings(settings);
            CloseMainWindow();
        }
        catch (Exception ex)
        {
            logger.LogCritical("Error during closing and saving: {ex}", ex);
            CloseMainWindow();
        }
    }

    public PlaylistCreateWindow PlaylistCreateWindow_Open() => new(logger, playlistEditOrCreateWindowViewModel);
    public PlaylistSelectWindow PlaylistSelectWindow_Open() => new(logger, playlistSelectWindowViewModel);
}

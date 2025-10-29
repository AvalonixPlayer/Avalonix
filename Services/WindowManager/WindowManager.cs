using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.Models.Media.Track;
using Avalonix.Services.SettingsManager;
using Avalonix.ViewModels.PlaylistEditOrCreate;
using Avalonix.ViewModels.PlaylistSelect;
using Avalonix.Views.SecondaryWindows.AboutWindow;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;
using Avalonix.Views.SecondaryWindows.ShowTrackWindow;
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
    public AboutWindow AboutWindow_Open() => new(logger, "v1.0.0");

    public ShowTrackWindow ShowTrackWindow_Open(Track track) => new(logger, track);
}

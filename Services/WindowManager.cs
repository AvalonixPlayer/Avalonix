using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

namespace Avalonix.Services;

public class WindowManager(ILogger logger,
    IPlaylistCreateWindowViewModel playlistCreateWindowViewModel, 
    IPlaylistSelectWindowViewModel playlistSelectWindowViewModel) 
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
            // Saving Data | NOT IMPLEMENTED
            CloseMainWindow();
        }
        catch (Exception ex)
        {
            logger.LogCritical("Error during closing and saving: {ex}", ex);
            CloseMainWindow();
        }
    }

    public Task<PlaylistCreateWindow> PlaylistCreateWindow_Open() => Task.FromResult(new PlaylistCreateWindow(logger, playlistCreateWindowViewModel));
    public Task<PlaylistSelectWindow> PlaylistSelectWindow_Open() => Task.FromResult(new PlaylistSelectWindow(logger, playlistSelectWindowViewModel));
}

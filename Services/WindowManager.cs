using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.ViewModels;
using Avalonix.ViewModels.Strategy;
using Microsoft.Extensions.Logging;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

namespace Avalonix.Services;

public class WindowManager(ILogger<WindowManager> logger, IPlaylistManager playlistManager) : IWindowManager
{
    private IPlaylistCreateWindowViewModel? _playlistCreateWindowViewModel;
    private IPlaylistSelectWindowViewModel? _playlistSelectWindowViewModel;
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

    public Task<PlaylistCreateWindow> PlaylistCreateWindow_Open()
    {
        var strategy = new CreatePlaylistStrategy(playlistManager);
        _playlistCreateWindowViewModel = new PlaylistCreateWindowViewModel(logger, playlistManager, strategy);
        return Task.FromResult(new PlaylistCreateWindow(logger, _playlistCreateWindowViewModel));   
    }
    
    public Task<PlaylistCreateWindow> PlaylistEditWindow_Open()
    {
        var strategy = new EditPlaylistStrategy(playlistManager);
        _playlistCreateWindowViewModel = new PlaylistCreateWindowViewModel(logger, playlistManager, strategy);
        return Task.FromResult(new PlaylistCreateWindow(logger, _playlistCreateWindowViewModel));   
    }

    public Task<PlaylistSelectWindow> PlaylistSelectToEditWindow_Open()
    {
        var strategy = new SelectToEditPlaylistStrategy(playlistManager);
        _playlistCreateWindowViewModel = new PlaylistCreateWindowViewModel(logger, playlistManager, strategy);
        return Task.FromResult(new PlaylistSelectWindow(logger, _playlistSelectWindowViewModel!));   
    }
    
    public Task<PlaylistSelectWindow> PlaylistSelectToDeleteWindow_Open()
    {
        var strategy = new SelectToDeletePlaylistStrategy(playlistManager);
        _playlistCreateWindowViewModel = new PlaylistCreateWindowViewModel(logger, playlistManager, strategy);
        return Task.FromResult(new PlaylistSelectWindow(logger, _playlistSelectWindowViewModel!));   
    }

    public Task<PlaylistSelectWindow> PlaylistSelectToPlayWindow_Open()
    {
        var strategy = new SelectToPlayPlaylistStrategy(playlistManager);
        _playlistCreateWindowViewModel = new PlaylistCreateWindowViewModel(logger, playlistManager, strategy);
        return Task.FromResult(new PlaylistSelectWindow(logger, _playlistSelectWindowViewModel!));   
    }
}

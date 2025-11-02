using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.Models.Media.Track;
using Avalonix.Services.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.ViewModels.EditMetadata;
using Avalonix.ViewModels.PlaylistEditOrCreate;
using Avalonix.ViewModels.PlaylistSelect;
using Avalonix.ViewModels.Strategy;
using Avalonix.Views.SecondaryWindows.AboutWindow;
using Avalonix.Views.SecondaryWindows.EditMetadataWindow;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;
using Avalonix.Views.SecondaryWindows.ShowTrackWindow;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.WindowManager;

public class WindowManager(ILogger<WindowManager> logger, ISettingsManager settingsManager, IPlaylistManager playlistManager) 
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

    private PlaylistCreateWindow PlaylistCreateWindow_Open(ISecondWindowStrategy strategy)
    {
        var vm = new PlaylistEditOrCreateWindowViewModel(logger, playlistManager, strategy);
        return new PlaylistCreateWindow(logger, vm);
    }

    private PlaylistSelectWindow PlaylistSelectWindow_Open(ISecondWindowStrategy strategy)
    {
        var vm = new PlaylistSelectWindowViewModel(playlistManager, strategy);
        return new PlaylistSelectWindow(logger, vm);
    }

    public PlaylistSelectWindow PlaylistSelectToPlayWindow_Open() =>
        PlaylistSelectWindow_Open(new SelectAndPlayPlaylistWindowStrategy(playlistManager)); 
    public PlaylistSelectWindow PlaylistSelectToEditWindow_Open() =>
        PlaylistSelectWindow_Open(new SelectAndEditPlaylistWindowStrategy(playlistManager)); 
    public PlaylistCreateWindow PlaylistCreateWindow_Open() =>
        PlaylistCreateWindow_Open(new CreatePlaylistWindowStrategy(playlistManager));

    public PlaylistSelectWindow PlaylistSelectWindow_Open() => 
        PlaylistSelectWindow_Open(new SelectAndPlayPlaylistWindowStrategy(playlistManager));

    public PlaylistCreateWindow PlaylistEditWindow_Open() =>
        PlaylistCreateWindow_Open(new CreatePlaylistWindowStrategy(playlistManager));

    public AboutWindow AboutWindow_Open() => new(logger, "v1.0.0");

    public ShowTrackWindow ShowTrackWindow_Open(Track track) => new(logger, track);
    
    public EditMetadataWindow EditMetadataWindow_Open(Track track) => new(logger, new EditMetadataWindowViewModel(logger, null!),track, playlistManager);
    
    public PlaylistSelectWindow PlaylistDeleteWindow_Open() => 
        PlaylistSelectWindow_Open(new SelectAndDeletePlaylistWindowStrategy(playlistManager)); 
}

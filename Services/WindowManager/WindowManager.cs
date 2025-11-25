using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.Model.Media.Track;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.PlayableManager.AlbumManager;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.VersionManager;
using Avalonix.View.SecondaryWindows.AboutWindow;
using Avalonix.View.SecondaryWindows.EditMetadataWindow;
using Avalonix.View.SecondaryWindows.PlayableSelectWindow;
using Avalonix.View.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.View.SecondaryWindows.ShowTrackWindow;
using Avalonix.ViewModel.EditMetadata;
using Avalonix.ViewModel.PlayableSelectViewModel;
using Avalonix.ViewModel.PlaylistEditOrCreate;
using Avalonix.ViewModel.Strategy;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.WindowManager;

public class WindowManager(
    ILogger<WindowManager> logger,
    ISettingsManager settingsManager,
    IPlayablesManager playablesManager,
    IPlaylistManager playlistManager,
    IVersionManager versionManager,
    IAlbumManager albumManager)
    : IWindowManager
{
    public async Task CloseMainWindowAsync()
    {
        try
        {
            await settingsManager.SaveSettings();
            CloseMainWindow();
        }
        catch (Exception ex)
        {
            logger.LogCritical("Error during closing and saving: {ex}", ex);
            CloseMainWindow();
        }
    }

    public PlaylistCreateWindow PlaylistCreateWindow_Open()
    {
        return PlaylistCreateWindow_Open(new CreatePlaylistWindowStrategy(playlistManager));
    }

    public PlayableSelectWindow PlaylistSelectWindow_Open()
    {
        return PlaylistSelectWindow_Open(new SelectAndPlayPlaylistWindowStrategy(playablesManager));
    }

    public AboutWindow AboutWindow_Open()
    {
        return new AboutWindow(logger, versionManager);
    }

    public ShowTrackWindow ShowTrackWindow_Open(Track track)
    {
        return new ShowTrackWindow(logger, track);
    }

    public EditMetadataWindow EditMetadataWindow_Open(Track track)
    {
        return new EditMetadataWindow(logger, new EditMetadataWindowViewModel(logger, null!), track, playlistManager);
    }

    public PlayableSelectWindow PlaylistDeleteWindow_Open()
    {
        return PlaylistSelectWindow_Open(new SelectAndDeletePlaylistWindowStrategy(playlistManager));
    }

    public PlayableSelectWindow AlbumSelectAndPlayWindow_Open()
    {
        return AlbumSelectWindow_Open(new SelectAndPlayAlbumWindowStrategy(playablesManager));
    }

    private static void CloseMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private PlaylistCreateWindow PlaylistCreateWindow_Open(IPlayableWindowStrategy strategy)
    {
        var vm = new PlaylistEditOrCreateWindowViewModel(logger, playlistManager, strategy);
        return new PlaylistCreateWindow(logger, vm);
    }

    private PlayableSelectWindow PlaylistSelectWindow_Open(IPlayableWindowStrategy strategy)
    {
        var vm = new PlayableSelectViewModel(playlistManager, strategy);
        return new PlayableSelectWindow(logger, vm);
    }

    private PlayableSelectWindow AlbumSelectWindow_Open(IPlayableWindowStrategy strategy)
    {
        var vm = new PlayableSelectViewModel(albumManager, strategy);
        return new PlayableSelectWindow(logger, vm);
    }

    public PlayableSelectWindow PlaylistSelectToPlayWindow_Open()
    {
        return PlaylistSelectWindow_Open(new SelectAndPlayPlaylistWindowStrategy(playablesManager));
    }

    public PlayableSelectWindow PlaylistSelectToEditWindow_Open()
    {
        return PlaylistSelectWindow_Open(new SelectAndEditPlaylistWindowStrategy(playlistManager));
    }

    public PlaylistCreateWindow PlaylistEditWindow_Open()
    {
        return PlaylistCreateWindow_Open(new CreatePlaylistWindowStrategy(playlistManager));
    }

    public PlayableSelectWindow AlbumSelectAndDeleteWindow_Open()
    {
        return AlbumSelectWindow_Open(new SelectAndDeleteAlbumWindowStrategy(albumManager));
    }
}
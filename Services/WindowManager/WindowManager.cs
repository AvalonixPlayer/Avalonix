using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonix.Models.Media.Track;
using Avalonix.Models.UserSettings;
using Avalonix.Services.AlbumManager;
using Avalonix.Services.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.VersionManager;
using Avalonix.ViewModels.EditMetadata;
using Avalonix.ViewModels.ItemSelect.Album;
using Avalonix.ViewModels.PlayableSelectViewModel;
using Avalonix.ViewModels.PlaylistEditOrCreate;
using Avalonix.ViewModels.Strategy;
using Avalonix.Views.SecondaryWindows.AboutWindow;
using Avalonix.Views.SecondaryWindows.EditMetadataWindow;
using Avalonix.Views.SecondaryWindows.PlayableSelectWindow;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.ShowTrackWindow;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.WindowManager;

public class WindowManager(ILogger<WindowManager> logger, ISettingsManager settingsManager, IPlaylistManager playlistManager, IVersionManager versionManager, IAlbumManager albumManager)
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
            await settingsManager.SaveSettings();
            CloseMainWindow();
        }
        catch (Exception ex)
        {
            logger.LogCritical("Error during closing and saving: {ex}", ex);
            CloseMainWindow();
        }
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

    public PlayableSelectWindow PlaylistSelectToPlayWindow_Open() =>
        PlaylistSelectWindow_Open(new SelectAndPlayPlaylistWindowStrategy(playlistManager));
    public PlayableSelectWindow PlaylistSelectToEditWindow_Open() =>
        PlaylistSelectWindow_Open(new SelectAndEditPlaylistWindowStrategy(playlistManager));
    public PlaylistCreateWindow PlaylistCreateWindow_Open() =>
        PlaylistCreateWindow_Open(new CreatePlaylistWindowStrategy(playlistManager));

    public PlayableSelectWindow PlaylistSelectWindow_Open() =>
        PlaylistSelectWindow_Open(new SelectAndPlayPlaylistWindowStrategy(playlistManager));

    public PlaylistCreateWindow PlaylistEditWindow_Open() =>
        PlaylistCreateWindow_Open(new CreatePlaylistWindowStrategy(playlistManager));

    public AboutWindow AboutWindow_Open() => new(logger, versionManager);

    public ShowTrackWindow ShowTrackWindow_Open(Track track) => new(logger, track);

    public EditMetadataWindow EditMetadataWindow_Open(Track track) => new(logger, new EditMetadataWindowViewModel(logger, null!),track, playlistManager);

    public PlayableSelectWindow PlaylistDeleteWindow_Open() =>
        PlaylistSelectWindow_Open(new SelectAndDeletePlaylistWindowStrategy(playlistManager));

    public PlayableSelectWindow AlbumSelectAndPlayWindow_Open() =>
        AlbumSelectWindow_Open(new SelectAndPlayAlbumWindowStrategy(albumManager));

    public PlayableSelectWindow AlbumSelectAndDeleteWindow_Open() =>
        AlbumSelectWindow_Open(new SelectAndDeleteAlbumWindowStrategy(albumManager));
}

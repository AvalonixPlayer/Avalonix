using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonix.Model.Media.Track;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModel.Strategy;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModel.PlaylistEditOrCreate;

public class PlaylistEditOrCreateWindowViewModel(
    ILogger<WindowManager> logger,
    IPlaylistManager playlistManager,
    IPlayableWindowStrategy strategy)
    : ViewModelBase, IPlaylistEditOrCreateWindowViewModel
{
    private readonly FilePickerOpenOptions _filePickerOptions = new()
    {
        Title = "Select Audio Files",
        AllowMultiple = true,
        FileTypeFilter =
        [
            new FilePickerFileType("Audio Files")
            {
                Patterns = ["*.mp3", "*.flac", "*.wav", "*.waw"] // Only tested formats
            },
            FilePickerFileTypes.All
        ]
    };

    private readonly FolderPickerOpenOptions _folderPickerOptions = new()
    {
        Title = "Select observing directory",
        AllowMultiple = false
    };

    public IPlayableWindowStrategy Strategy => strategy;

    public async Task<List<string>?> OpenTrackFileDialogAsync(Window parent)
    {
        try
        {
            var storageProvider = parent.StorageProvider;

            logger.LogInformation("Opening track file dialog");

            var files = await storageProvider.OpenFilePickerAsync(_filePickerOptions);

            if (files.Count.Equals(0))
            {
                logger.LogInformation("No files selected");
                return null;
            }

            var filePaths = new string[files.Count];
            for (var i = 0; i < files.Count; i++)
                filePaths[i] = files[i].Path.LocalPath;

            logger.LogInformation("Selected {Count} files: {filepaths}", files.Count, filePaths);
            return filePaths.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError("Error opening file dialog: {ex}", ex.Message);
            return null;
        }
    }

    public async Task<string?> OpenObservingDirectoryDialogAsync(Window parent)
    {
        try
        {
            var storageProvider = parent.StorageProvider;

            logger.LogInformation("Opening observing directory dialog");

            var directory = (await storageProvider.OpenFolderPickerAsync(_folderPickerOptions))[0].Path.LocalPath;

            if (string.IsNullOrEmpty(directory))
            {
                logger.LogInformation("No directory selected");
                return null;
            }

            logger.LogInformation("Selected directory: {directory}", directory);
            return directory;
        }
        catch (Exception ex)
        {
            logger.LogError("Error opening folder dialog: {ex}", ex.Message);
            return null;
        }
    }

    public async Task ExecuteAsync(string playlistName, List<Track> tracksPaths, string? observingDirectory)
    {
        var playlist = playlistManager.ConstructPlaylist(playlistName, tracksPaths, observingDirectory);
        await Strategy.ExecuteAsync(playlist);
    }
}

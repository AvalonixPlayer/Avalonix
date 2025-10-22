using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayer;
using Microsoft.Extensions.Logging;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.Models.Media.Track;
using Avalonix.Services.PlaylistManager;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels;

public class PlaylistEditOrCreateWindowViewModel(
    ILogger<PlaylistEditOrCreateWindowViewModel> logger, 
    IPlaylistManager playlistManager,  
    ISecondWindowStrategy strategy)
    : ViewModelBase, IPlaylistEditOrCreateWindowViewModel
{
    public async Task<string[]?> OpenTrackFileDialogAsync(Window parent)
    {
        try
        {
            var storageProvider = parent.StorageProvider;

            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Select Audio Files",
                AllowMultiple = true,
                FileTypeFilter =
                [
                    new FilePickerFileType("Audio Files")
                    {
                        Patterns = ["*.mp3", "*.flac", "*.m4a", "*.wav", "*.waw"]
                    },
                    FilePickerFileTypes.All
                ]
            };


            logger.LogInformation("Opening track file dialog");

            var files = await storageProvider.OpenFilePickerAsync(filePickerOptions);

            if (files.Count.Equals(0))
            {
                logger.LogInformation("No files selected");
                return null;
            }

            var filePaths = new string[files.Count];
            for (var i = 0; i < files.Count; i++)
                filePaths[i] = files[i].Path.LocalPath;

            logger.LogInformation("Selected {Count} files: " + filePaths, files.Count);
            return filePaths;
        }
        catch (Exception ex)
        {
            logger.LogError("Error opening file dialog: {ex}", ex.Message);
            return null;
        }
    }

    public async Task ExecuteAsync(string playlistName, List<Track> tracksPaths)
    {
        var playlist = playlistManager.ConstructPlaylist(playlistName, tracksPaths);
        await strategy.ExecuteAsync(playlist);
    }

}
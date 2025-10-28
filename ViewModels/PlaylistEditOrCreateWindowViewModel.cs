using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
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

            logger.LogInformation("Selected {Count} files: " + filePaths, files.Count);
            return filePaths.ToList();
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
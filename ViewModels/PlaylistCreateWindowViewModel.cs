using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class PlaylistCreateWindowViewModel(ILogger<PlaylistCreateWindowViewModel> logger) : ViewModelBase
{
    public async Task<string[]?> OpenTrackFileDialog(Window parent)
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
            {
                filePaths[i] = files[i].Path.LocalPath;
            }

            logger.LogInformation("Selected {Count} files: " + filePaths, files.Count);
            return filePaths;
        }
        catch (System.Exception ex)
        {
            logger.LogError("Error opening file dialog: {ex}", ex.Message);
            return null;
        }
    }
}
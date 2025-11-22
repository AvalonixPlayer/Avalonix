using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModels.Strategy;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels.EditMetadata;

public class EditMetadataWindowViewModel(ILogger<WindowManager> logger,
    ISecondWindowStrategy strategy) : ViewModelBase, IEditMetadataWindowViewModel
{
    private readonly FilePickerOpenOptions _filePickerOptions = new()
    {
        Title = "Select Picture Files",
        AllowMultiple = false,
        FileTypeFilter =
        [
            new FilePickerFileType("Picture Files")
            {
                Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"] // Only tested formats
            },
            FilePickerFileTypes.All
        ]
    };
    
    public async Task<string?> OpenTrackFileDialogAsync(Window parent)
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

            var result = files.FirstOrDefault()?.Path.LocalPath;

            logger.LogInformation("Selected file: {filepaths}", result);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("Error opening file dialog: {ex}", ex.Message);
            return null;
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModel.Settings;

public class SettingsWindowViewModel(ISettingsManager manager, ILogger logger) : ViewModelBase, ISettingsWindowViewModel
{
    private readonly FilePickerOpenOptions _filePickerOptions = new()
    {
        Title = "Select Picture Files",
        AllowMultiple = false,
        FileTypeFilter =
        [
            new FilePickerFileType("Picture Files")
            {
                Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"]
            },
            FilePickerFileTypes.All
        ]
    };

    private readonly FolderPickerOpenOptions _folderPickerOptions = new()
    {
        Title = "Select Folder",
        AllowMultiple = false
    };

    public async Task<string?> OpenFolderDialogAsync(Window parent)
    {
        try
        {
            var storageProvider = parent.StorageProvider;
            logger.LogInformation("Opening folder dialog");
            var folder = await storageProvider.OpenFolderPickerAsync(_folderPickerOptions);
            if (!folder.Count.Equals(0)) return folder.FirstOrDefault()?.Path.LocalPath;
            logger.LogInformation("No folders selected");
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to open folder");
            return null;
        }
    }

    public async Task<string?> OpenCoverFileDialogAsync(Window parent)
    {
        try
        {
            var storageProvider = parent.StorageProvider;

            logger.LogInformation("Opening cover file dialog");

            var files = await storageProvider.OpenFilePickerAsync(_filePickerOptions);

            if (files.Count.Equals(0))
            {
                logger.LogInformation("No files selected");
                return null;
            }

            var result = files.FirstOrDefault()?.Path.LocalPath;

            logger.LogInformation("Selected file: {file paths}", result);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("Error opening file dialog: {ex}", ex.Message);
            return null;
        }
    }

    public async Task SaveSettingsAsync(Model.UserSettings.Settings settings)
    {
        manager.Settings = settings;
        await manager.SaveSettingsAsync();
    }

    public Task<Model.UserSettings.Settings?> GetSettingsAsync()
    {
        return Task.FromResult(manager.Settings);
    }
}
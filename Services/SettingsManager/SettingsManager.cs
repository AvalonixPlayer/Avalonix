using System;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Disk.DiskLoader;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.DiskWriter;
using Avalonix.Models.UserSettings;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.SettingsManager;

public class SettingsManager(IDiskWriter writer,IDiskLoader loader,ILogger logger) : ISettingsManager
{
    private static string SettingsPath { get; } =
        Path.Combine(DiskManager.AvalonixFolderPath, "settings" + DiskManager.Extension);
    
    public async Task SaveSettings(Settings settings)
    {
        await writer.WriteAsync(settings, SettingsPath);
        logger.LogInformation("Settings saved");
    }
    
    public async Task<Settings> GetSettings()
    {
        try
        {
            var result = await loader.LoadAsync<Settings>(SettingsPath);
            if (result != null) return result;
            await SaveSettings(new Settings());
            result = await loader.LoadAsync<Settings>(SettingsPath);
            return result!;
        }
        catch (Exception e)
        {
            logger.LogWarning("Catch warning while getting settings: {Ex}", e.Message);
            throw;
        }
    }
}
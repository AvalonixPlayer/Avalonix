using System;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Services.DiskLoader;
using Avalonix.Services.DiskWriter;
using Avalonix.Services.UserSettings;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.SettingsManager;

public class SettingsManager : ISettingsManager
{
    private static string SettingsPath { get; } =
        Path.Combine(DiskManager.DiskManager.AvalonixFolderPath, "settings" + DiskManager.DiskManager.Extension);
    private readonly IDiskWriter _diskWriter;
    private readonly IDiskLoader _diskLoader;
    private readonly ILogger _logger;
    
    public Settings? Settings { get; }

    public SettingsManager(IDiskWriter writer,IDiskLoader loader,ILogger logger)
    {
        _diskWriter = writer;
        _diskLoader = loader;
        _logger = logger;
        Settings = Task.Run(async () => await GetSettings()).Result;
    }
    
    public async Task SaveSettings()
    {
        await _diskWriter.WriteJsonAsync(Settings, SettingsPath);
        _logger.LogInformation("Settings saved");
    }

    private async Task<Settings> GetSettings()
    {
        try
        {
            if(!Directory.Exists(DiskManager.DiskManager.AvalonixFolderPath))
                Directory.CreateDirectory(DiskManager.DiskManager.AvalonixFolderPath);
            if (!Path.Exists(SettingsPath))
            {
                await File.Create(SettingsPath).DisposeAsync();
                await _diskWriter.WriteJsonAsync(new Settings(), SettingsPath);
            }
            var result = await _diskLoader.LoadAsyncFromJson<Settings>(SettingsPath);
            return result ?? default!;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Catch warning while getting settings: {Ex}", e.Message);
            throw;
        }
    }
}
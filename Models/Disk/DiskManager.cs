using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.UserSettings;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.Models.UserSettings.Theme;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk;

public class DiskManager(ILogger logger, IMediaPlayer player) : IDiskManager
{
    private IDiskManager IDM => this;

    public async Task SavePlaylist(Playlist playlist)
    {
        await IDM.WriteAsync(playlist, Path.Combine(IDM.PlaylistsPath, playlist.Name + IDM.Extension));
        logger.LogDebug("Playlist saved");
    }

    public async Task<Playlist> GetPlaylist(string name)
    {
        try
        {
            var result = await IDM.LoadAsync<Playlist>(Path.Combine(IDM.PlaylistsPath, name + IDM.Extension));
            await result?.InitializeAsync(name, player, IDM, logger)!;
            if(result == null!) logger.LogError("Playlist get error: {name}", name);
            else logger.LogDebug("Playlist get: {name}", name);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("Playlist error while get: {ex}", ex);
            return null!;
        }
    }

    public async Task<List<Playlist>> GetAllPlaylists()
    {
        var files = Directory.EnumerateFiles(IDM.PlaylistsPath, $"*{IDM.Extension}");
        var playlists = new List<Playlist>();
        foreach (var file in files)
        {
            var playlist = await GetPlaylist(Path.GetFileNameWithoutExtension(file));
            if (playlist == null!) continue;
            playlists.Add(playlist);
        }

        return playlists;
    }

    public async Task SaveSettings(Settings settings)
    {
        await IDM.WriteAsync(settings, IDM.SettingsPath);
        logger.LogInformation("Settings saved");
    }

    public async Task CreateNewTheme(string name)
    {
        var theme = new Theme { Name = name };
        await IDM.WriteAsync(theme, Path.Combine(IDM.ThemesPath, name + IDM.Extension));
    }

    public async Task SaveTheme(Theme theme) =>
        await IDM.WriteAsync(theme, Path.Combine(IDM.ThemesPath, theme.Name + IDM.Extension));

    public async Task<Theme> GetTheme(string name)
    {
        var result = await IDM.LoadAsync<Theme>(Path.Combine(IDM.ThemesPath, name + IDM.Extension));
        return result ?? new Theme();
    }
        

    public async Task<Settings> GetSettings()
    {
        var result = await IDM.LoadAsync<Settings>(IDM.SettingsPath);
        if (result != null) return result;
        await SaveSettings(new Settings());
        result = await IDM.LoadAsync<Settings>(IDM.SettingsPath);
        return result!;
    }
}
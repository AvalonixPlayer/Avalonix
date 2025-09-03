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

public class DiskManager(ILogger logger, IMediaPlayer player)
{

    private const string Extension = ".avalonix";

    private static string AvalonixFolderPath
    {
        get
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    private static string PlaylistsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, "playlists");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    private static string SettingsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, "settings" + Extension);
            if (!File.Exists(path))
                File.Create(path).Close();
            return path;
        }
    }

    private static string ThemesPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, "themes");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public async Task SavePlaylist(Playlist playlist)
    {
        await DiskWriter.WriteAsync(playlist, Path.Combine(PlaylistsPath, playlist.Name + Extension));
        logger.LogDebug("Playlist saved");
    }

    public async Task<Playlist?> GetPlaylist(string name)
    {
        try
        {
            var result = await DiskLoader.LoadAsync<Playlist>(Path.Combine(PlaylistsPath, name + Extension));
            await result?.InitializeAsync(name, player, this, logger)!;
            if (result == null!) logger.LogError("Playlist get error: {name}", name);
            else logger.LogDebug("Playlist get: {name}", name);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("Playlist error while get: {ex}", ex);
            return null!;
        }
    }

    public void RemovePlaylist(string name)
    {
        logger.LogInformation("Removing playlist {name}",name);
        File.Delete(Path.Combine(PlaylistsPath, name + Extension));
        logger.LogInformation("Playlist {name} was been removed",name);
    }

    public async Task<List<Playlist>> GetAllPlaylists()
    {
        var files = Directory.EnumerateFiles(PlaylistsPath, $"*{Extension}");
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
        await DiskWriter.WriteAsync(settings, SettingsPath);
        logger.LogInformation("Settings saved");
    }

    public async Task CreateNewTheme(string name)
    {
        var theme = new Theme { Name = name };
        await DiskWriter.WriteAsync(theme, Path.Combine(ThemesPath, name + Extension));
    }

    public async Task SaveTheme(Theme theme) =>
        await DiskWriter.WriteAsync(theme, Path.Combine(ThemesPath, theme.Name + Extension));

    public async Task<Theme> GetTheme(string name)
    {
        var result = await DiskLoader.LoadAsync<Theme>(Path.Combine(ThemesPath, name + Extension));
        return result ?? new Theme();
    }


    public async Task<Settings> GetSettings()
    {
        var result = await DiskLoader.LoadAsync<Settings>(SettingsPath);
        if (result != null) return result;
        await SaveSettings(new Settings());
        result = await DiskLoader.LoadAsync<Settings>(SettingsPath);
        return result!;
    }
}
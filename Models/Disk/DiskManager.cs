using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.UserSettings;
using Avalonix.Models.UserSettings.Theme;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk;

public class DiskManager : IDiskManager
{
    private const string Extension = ".avalonix";
    private readonly IDiskWriter _diskWriter;
    private readonly IDiskLoader _diskLoader;

    private static string AvalonixFolderPath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");

    private static string PlaylistsPath { get; } =
        Path.Combine(AvalonixFolderPath, "playlists");

    private static string SettingsPath { get; } =
        Path.Combine(AvalonixFolderPath, "settings" + Extension);

    private static string ThemesPath { get; } =
        Path.Combine(AvalonixFolderPath, "themes");

    private readonly ILogger _logger;
    private readonly IMediaPlayer _player;

    public DiskManager(ILogger logger, IMediaPlayer player, IDiskWriter diskWriter, IDiskLoader diskLoader)
    {
        _logger = logger;
        _diskWriter = diskWriter;
        _diskLoader = diskLoader;
        _player = player;

        CheckDirectory(AvalonixFolderPath);
        CheckDirectory(PlaylistsPath);
        CheckDirectory(ThemesPath);
        CheckFile(SettingsPath);
        return;

        void CheckFile(string path)
        {
            if (!File.Exists(path))
                File.Create(path).Close();
        }

        void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }

    public async Task SavePlaylist(Playlist playlist)
    {
        await _diskWriter.WriteAsync(playlist.PlaylistData, Path.Combine(PlaylistsPath, playlist.Name + Extension));
        _logger.LogDebug("Playlist({playlistName}) saved", playlist.Name);
    }

    public async Task<Playlist?> GetPlaylist(string name)
    {
        try
        {
            var playlistData =
                await _diskLoader.LoadAsync<PlaylistData>(Path.Combine(PlaylistsPath, name + Extension));
            if (playlistData == null!) _logger.LogError("Playlist get error: {name}", name);
            else _logger.LogDebug("Playlist get: {name}", name);
            return new Playlist(name, playlistData!, _player, this, _logger, await GetSettings());
        }
        catch (Exception ex)
        {
            _logger.LogError("Playlist error while get: {ex}", ex);
            return null!;
        }
    }

    public void RemovePlaylist(string name)
    {
        _logger.LogInformation("Removing playlist {name}", name);
        File.Delete(Path.Combine(PlaylistsPath, name + Extension));
        _logger.LogInformation("Playlist {name} was been removed", name);
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
        await _diskWriter.WriteAsync(settings, SettingsPath);
        _logger.LogInformation("Settings saved");
    }

    public async Task CreateNewTheme(string name)
    {
        var theme = new Theme { Name = name };
        await _diskWriter.WriteAsync(theme, Path.Combine(ThemesPath, name + Extension));
    }

    public async Task SaveTheme(Theme theme) =>
        await _diskWriter.WriteAsync(theme, Path.Combine(ThemesPath, theme.Name + Extension));

    public async Task<Theme?> GetTheme(string name)
    {
        var result = await _diskLoader.LoadAsync<Theme>(Path.Combine(ThemesPath, name + Extension));
        return result;
    }


    public async Task<Settings> GetSettings()
    {
        var result = await _diskLoader.LoadAsync<Settings>(SettingsPath);
        if (result != null) return result;
        await SaveSettings(new Settings());
        result = await _diskLoader.LoadAsync<Settings>(SettingsPath);
        return result!;
    }
}
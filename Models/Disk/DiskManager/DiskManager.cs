using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Models.Disk.DiskLoader;
using Avalonix.Models.Disk.DiskWriter;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.UserSettings.Theme;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk.DiskManager;

public class DiskManager : IDiskManager
{
    public const string Extension = ".avalonix";
    public static readonly string AvalonixFolderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");
    
    private readonly IDiskWriter _diskWriter;
    private readonly IDiskLoader _diskLoader;
    
    private static string PlaylistsPath { get; } =
        Path.Combine(AvalonixFolderPath, "playlists");

    private static string ThemesPath { get; } =
        Path.Combine(AvalonixFolderPath, "themes");

    private readonly ILogger _logger;
    private readonly IMediaPlayer _player;
    private readonly ISettingsManager _settingsManager;

    public DiskManager(ILogger logger, IMediaPlayer player, IDiskWriter diskWriter, IDiskLoader diskLoader, ISettingsManager settingsManager)
    {
        _logger = logger;
        _diskWriter = diskWriter;
        _diskLoader = diskLoader;
        _player = player;
        _settingsManager = settingsManager;

        CheckDirectory(AvalonixFolderPath);
        CheckDirectory(PlaylistsPath);
        CheckDirectory(ThemesPath);
        return;

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
            return new Playlist(name, playlistData!, _player, this, _logger, (await _settingsManager.GetSettings()).Avalonix.PlaySettings);
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
}
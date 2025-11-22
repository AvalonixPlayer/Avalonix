using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Services.Disk.DiskLoader;
using Avalonix.Services.Disk.DiskWriter;
using Avalonix.Services.Media.MediaPlayer;
using Avalonix.Services.Media.Playlist;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.UserSettings.Theme;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.Disk.DiskManager;

public class DiskManager : IDiskManager
{
    public const string Extension = ".avalonix";
    public static readonly string[] MusicFilesExtensions = ["*.mp3", "*.flac", "*.m4a", "*.wav", "*.waw"];

    public static readonly string AvalonixFolderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");

    private readonly IDiskWriter _diskWriter;
    private readonly IDiskLoader _diskLoader;

    private static string PlaylistsPath { get; } =
        Path.Combine(AvalonixFolderPath, "playlists");

    private static string ThemesPath { get; } =
        Path.Combine(AvalonixFolderPath, "themes");

    private static string MusicPath { get; } =
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

    private readonly ILogger _logger;
    private readonly IMediaPlayer _player;
    private readonly ISettingsManager _settingsManager;

    public DiskManager(ILogger logger, IMediaPlayer player, IDiskWriter diskWriter, IDiskLoader diskLoader,
        ISettingsManager settingsManager)
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
        await _diskWriter.WriteJsonAsync(playlist.PlaylistData, Path.Combine(PlaylistsPath, playlist.Name + Extension));
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
            return new Playlist(name, playlistData!, _player, this, _logger,
                _settingsManager.Settings!.Avalonix.PlaySettings);
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
        await _diskWriter.WriteJsonAsync(theme, Path.Combine(ThemesPath, name + Extension));
    }

    public async Task SaveTheme(Theme theme) =>
        await _diskWriter.WriteJsonAsync(theme, Path.Combine(ThemesPath, theme.Name + Extension));

    public async Task<Theme?> GetTheme(string name)
    {
        var result = await _diskLoader.LoadAsync<Theme>(Path.Combine(ThemesPath, name + Extension));
        return result;
    }

    public List<string> GetMusicFilesForAlbums()
    {
        return FindFiles();

        List<string> FindFiles()
        {
            var files = new List<string>();
            foreach (var ext in MusicFilesExtensions)
            {
                var foundFiles = Directory.EnumerateFiles(MusicPath, $"*{ext}", SearchOption.AllDirectories);
                files.AddRange(foundFiles);
            }
            return files;
        }
    }
}

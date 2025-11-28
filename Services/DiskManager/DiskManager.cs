using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Playlist;
using Avalonix.Services.DiskLoader;
using Avalonix.Services.DiskWriter;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.UserSettings.Theme;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.DiskManager;

public class DiskManager : IDiskManager
{
    public const string Extension = ".avalonix";
    private static readonly string[] MusicFilesExtensions = ["*.mp3", "*.flac", "*.m4a", "*.wav", "*.waw"];

    public static readonly string AvalonixFolderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");
    
    private readonly IDiskLoader _diskLoader;

    private readonly IDiskWriter _diskWriter;

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

    private static string PlaylistsPath { get; } =
        Path.Combine(AvalonixFolderPath, "playlists");

    private static string ThemesPath { get; } =
        Path.Combine(AvalonixFolderPath, "themes");

    private static string MusicPath { get; } =
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

    public async Task SavePlaylist(Playlist playlist)
    {
        await _diskWriter.WriteJsonAsync(playlist.PlaylistData, Path.Combine(PlaylistsPath, playlist.PlaylistData.Name + Extension));
        _logger.LogDebug("Playlist({playlistName}) saved", playlist.PlaylistData.Name);
    }

    public Task RemovePlaylist(string name)
    {
        _logger.LogInformation("Removing playlist {name}", name);
        var path = Path.Combine(PlaylistsPath, name);
        File.Delete(path);
        _logger.LogInformation("Playlist {name} was been removed", name);
        return Task.CompletedTask;
    }

    public async Task<List<PlaylistData>> GetAllPlaylists()
    {
        var files = Directory.EnumerateFiles(PlaylistsPath, $"*{Extension}");
        var playlists = new List<PlaylistData>();
        foreach (var file in files)
        {
            var playlist = await _diskLoader.LoadAsyncFromJson<PlaylistData>(Path.GetFileNameWithoutExtension(file));
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

    public async Task SaveTheme(Theme theme)
    {
        await _diskWriter.WriteJsonAsync(theme, Path.Combine(ThemesPath, theme.Name + Extension));
    }

    public async Task<Theme?> GetTheme(string name)
    {
        var result = await _diskLoader.LoadAsyncFromJson<Theme>(Path.Combine(ThemesPath, name + Extension));
        return result;
    }

    public List<string> GetMusicFiles()
    {
        return FindFiles();

        List<string> FindFiles()
        {
            var files = new List<string>();
            foreach (var ext in MusicFilesExtensions)
            {
                var foundFiles = Directory.EnumerateFiles(MusicPath, $"*{ext}", SearchOption.AllDirectories);
                files.AddRange(foundFiles);
                var f = Directory.EnumerateFiles("D:\\плейлисты", $"*{ext}", SearchOption.AllDirectories);
                files.AddRange(f);
            }

            return files;
        }
    }
}

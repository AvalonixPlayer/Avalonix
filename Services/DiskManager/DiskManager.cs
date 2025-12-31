using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;
using Avalonix.Model.UserSettings.Theme;
using Avalonix.Services.DiskLoader;
using Avalonix.Services.DiskWriter;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.DiskManager;

public class DiskManager : IDiskManager
{
    public const string Extension = ".avalonix";
    private static readonly string[] MusicFilesExtensions = ["*.mp3", "*.flac", "*.m4a", "*.wav", "*.waw"];

    public static readonly string AvalonixFolderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Extension);

    public static readonly string TracksMetadataCachePath =
        Path.Combine(AvalonixFolderPath, "tracksMetadataCache" + Extension);

    private readonly IDiskLoader _diskLoader;
    private readonly IDiskWriter _diskWriter;
    private readonly ILogger _logger;

    private readonly ISettingsManager _settingsManager;

    public DiskManager(ILogger logger, IDiskWriter diskWriter, IDiskLoader diskLoader, ISettingsManager settingsManager)
    {
        _logger = logger;
        _diskWriter = diskWriter;
        _diskLoader = diskLoader;
        _settingsManager = settingsManager;

        CheckDirectory(AvalonixFolderPath);
        CheckDirectory(PlaylistsPath);
        CheckDirectory(ThemesPath);
        CheckFile(TracksMetadataCachePath);
        return;

        void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        void CheckFile(string path)
        {
            if (!File.Exists(path))
                File.Create(path).Close();
        }
    }

    private static string PlaylistsPath { get; } =
        Path.Combine(AvalonixFolderPath, "playlists");

    private static string ThemesPath { get; } =
        Path.Combine(AvalonixFolderPath, "themes");

    public async Task SavePlaylist(Playlist playlist)
    {
        await _diskWriter.WriteJsonAsync(playlist.Data, Path.Combine(PlaylistsPath, playlist.Name + Extension));
        _logger.LogDebug("Playlist({playlistName}) saved", playlist.Name);
    }

    public Task RemovePlaylist(string name)
    {
        _logger.LogInformation("Removing playlist {name}", name);
        var path = Path.Combine(PlaylistsPath, name + Extension);
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
            var playlist = await _diskLoader.LoadAsyncFromJson<PlaylistData>(file);
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
        var paths = _settingsManager.Settings!.Avalonix.MusicFilesPaths.Where(Directory.Exists);
        var result = FindFiles();
        return result;

        List<string> FindFiles()
        {
            var files = new List<string>();

            foreach (var path in paths)
            {
                foreach (var ext in MusicFilesExtensions)
                {
                    try
                    {
                        var foundFiles = Directory.EnumerateFiles(path, $"*{ext}", SearchOption.TopDirectoryOnly);
                        files.AddRange(foundFiles);

                        if (SearchOption.AllDirectories == SearchOption.AllDirectories)
                        {
                            TraverseSubdirectories(path, ext, files);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }
                }
            }

            return files;
        }
    }

    private static void TraverseSubdirectories(string rootPath, string extension, List<string> files)
    {
        try
        {
            var directories = Directory.EnumerateDirectories(rootPath);

            foreach (var directory in directories)
            {
                try
                {
                    var foundFiles =
                        Directory.EnumerateFiles(directory, $"*{extension}", SearchOption.TopDirectoryOnly);
                    files.AddRange(foundFiles);
                    TraverseSubdirectories(directory, extension, files);
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
        }
        catch (UnauthorizedAccessException)
        {

        }
    }

    public List<string> GetMusicFiles(string path)
    {
        var result = new List<string>();
        foreach (var ext in MusicFilesExtensions)
            result.AddRange(Directory.EnumerateFiles(path, ext, SearchOption.AllDirectories));
        return result;
    }
}
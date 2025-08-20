using System;
using System.IO;
using System.Text.Json;

namespace Avalonix.API;

public static class DiskManager
{
    public static readonly string Extension = ".avalonix";

    public static string AvalonixFolderPath
    {
        get
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string PlaylistsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, ".playlists");
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string CoversPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, ".covers");
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string SettingsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, ".settings" + Extension);
            if (!Path.Exists(path))
                File.Create(path).Close();
            return path;
        }
    }

    public static string[] PlaylistsPaths => Directory.GetFiles(PlaylistsPath);

    #region Playlists

    public static PlaylistData GetPlaylistData(string name)
    {
        string path = Path.Combine(PlaylistsPath, name + Extension);
        string json = File.ReadAllText(path);

        var playlistData = JsonSerializer.Deserialize<PlaylistData>(json);
        return playlistData;
    }

    public static void SavePlaylistData(PlaylistData playlistData)
    {
        string path = Path.Combine(PlaylistsPath, playlistData.Name + Extension);

        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };
        
        string json = JsonSerializer.Serialize(playlistData,opt);

        File.WriteAllText(path, json);
    }

    public static void CreatePlaylist(PlaylistData playlistData)
    {
        string path = Path.Combine(PlaylistsPath, playlistData.Name + Extension);
        File.Create(path).Close();
        SavePlaylistData(playlistData);
    }

    public static void RemovePlaylist(PlaylistData playlistData)
    {
        string path = Path.Combine(PlaylistsPath, playlistData.Name + Extension);
        File.Delete(path);
    }

    #endregion

    #region Settings

    public static Settings GetSettings()
    {
        string path = Path.Combine(PlaylistsPath, SettingsPath);
        string json = File.ReadAllText(path);

        var settingsData = JsonSerializer.Deserialize<Settings>(json);
        return settingsData;
    }

    public static void SaveSettings(Settings settings)
    {
        string path = Path.Combine(PlaylistsPath, SettingsPath);
        
        string json = JsonSerializer.Serialize(settings);

        File.WriteAllText(path, json);
    }

    #endregion
}
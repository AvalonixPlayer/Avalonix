using System;
using System.IO;
using System.Text.Json;
using Avalonia.Logging;

namespace Avalonix.API;

public static class DiskManager
{
    private static readonly string Extension = ".avalonix";

    private static string AvalonixFolderPath
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

    public static void SavePlaylist(Playlist playlist)
    {
        var path = Path.Combine(PlaylistsPath, playlist.Name + Extension);
        if (!Path.Exists(path))
        {
            Console.WriteLine($"No playlist found with path {path}");
            File.Create(path).Close();
        }

        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };

        var json = JsonSerializer.Serialize(playlist, opt);

        File.WriteAllText(path, json);
    }

    public static Playlist GetPlaylist(string name)
    {
        var path = Path.Combine(PlaylistsPath, name + Extension);

        if (!Path.Exists(path))
        {
            Console.WriteLine($"No playlist found with path {path}");
            SavePlaylist(new Playlist(name));
        }
            

        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Playlist>(json, opt);
    }

    public static void CreatePlaylist(string name) =>
        File.Create(Path.Combine(PlaylistsPath, name + Extension)).Close();
}
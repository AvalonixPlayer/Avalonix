using System;
using System.IO;
using Newtonsoft.Json;

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

    public static string[] PlaylistsPaths => Directory.GetFiles(PlaylistsPath);

    public static PlaylistData GetPlaylistData(string name)
    {
        string path = Path.Combine(PlaylistsPath, name + Extension);
        string json = File.ReadAllText(path);
        
        var playlistData = JsonConvert.DeserializeObject<PlaylistData>(json);
        return playlistData;
    }

    public static void SavePlaylistData(PlaylistData playlistData)
    {
        string path = Path.Combine(PlaylistsPath, playlistData.Name + Extension);

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        };

        string json = JsonConvert.SerializeObject(playlistData, settings);

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
}
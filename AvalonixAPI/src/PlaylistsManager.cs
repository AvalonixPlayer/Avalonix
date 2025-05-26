using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AvalonixAPI;

public static class PlaylistsManager
{
    public static string[] GetAudios(string playlistName)
    {
        string pathToPlaylist = DiskManager.EnvPath() + "\\playlists\\" + playlistName + ".json";
        string json = File.Exists(pathToPlaylist) ? File.ReadAllText(pathToPlaylist) : null!;
        string[] jsonObj = JsonConvert.DeserializeObject<string[]>(json)!;
        return jsonObj;
    }

    public static string[] Playlists()
    {
        return Directory.GetFiles(DiskManager.EnvPath() + "\\playlists\\", "*.json");
    }

    public static void CreatePlaylist(string playlistName)
    {
        Directory.CreateDirectory(DiskManager.EnvPath() + "\\playlists\\" + playlistName + ".json");
    }

    public static void AddToPlaylist(string playlistName, string musicPath)
    {
        List<string> list = GetAudios(playlistName).ToList();
        list.Add(musicPath);
        string json = JsonConvert.SerializeObject(list);
        File.WriteAllText(DiskManager.EnvPath() + "\\playlists\\" + playlistName + ".json", json);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AvalonixAPI;

public static class PlaylistsManager
{
    public static void CreateNewPlaylist(PlaylistData data)
    {
        CreatePlaylistFile(data);

        ChangeSettingsToPlaylist(data.Name, data);
    }

    public static void ChangeSettingsToPlaylist(string name, PlaylistData data)
    {
        PlaylistData playlistData = new PlaylistData();
        playlistData.Name = data.Name;

        string oldPath = Path.Combine(DiskManager.SettingsPath, $"{name}.json");
        string path = Path.Combine(DiskManager.SettingsPath, $"{playlistData.Name}.json");

        File.Delete(oldPath);

        CreatePlaylistFile(data);

        PlaylistToJson(path, playlistData);
    }

    public static void PlaylistToJson(string path, PlaylistData playlist)
    {
        JsonSerializer serializer = new JsonSerializer();

        using (StreamWriter sw = new StreamWriter(path))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            serializer.Serialize(writer, playlist);
        }
    }

    private static void CreatePlaylistFile(PlaylistData data)
    {
        string path = Path.Combine(DiskManager.SettingsPath, $"{data.Name}.json");

        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        {
            fs.Write(null!);
            fs.Dispose();
        }
    }
}
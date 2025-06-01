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
    public static string[] PlaylistsPaths => Directory.GetFiles(DiskManager.SettingsPath);

    public static string[] PlaylistsNames
    {
        get
        {
            List<string> result = new List<string>();

            foreach (var i in PlaylistsPaths) result.Add(Path.GetFileNameWithoutExtension(i));

            return result.ToArray();
        }
    }


    public static void AddSongToPlaylist(string playlistName, SongData songData)
    {
        string path = Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json");
        PlaylistData data = JsonToPlaylist(path);
        data.Songs.Add(songData);
        PlaylistToJson(path, data);
    }

    public static void RemoveSongFromPlaylist(string playlistName, string songName)
    {
        string path = Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json");
        PlaylistData data = JsonToPlaylist(path);

        SongData songToRemove = new SongData();
        foreach (var song in data.Songs)
        {
            if (song.Name == songName) songToRemove = song;
        }
        data.Songs.Remove(songToRemove);


        PlaylistToJson(path, data);
    }

    public static void CreateNewPlaylist(PlaylistData data)
    {
        CreatePlaylistFile(data);

        ChangeSettingsToPlaylist(data.Name, data);
    }

    public static void ChangeSettingsToPlaylist(string name, PlaylistData data)
    {
        string oldPath = Path.Combine(DiskManager.SettingsPath, $"{name}.json");
        string path = Path.Combine(DiskManager.SettingsPath, $"{data.Name}.json");

        File.Delete(oldPath);

        CreatePlaylistFile(data);

        PlaylistToJson(path, data);
    }

    public static void PlaylistToJson(string path, PlaylistData playlist)
    {
        JsonSerializer serializer = new JsonSerializer();

        using (StreamWriter sw = new StreamWriter(path))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, playlist);
        }
    }

    public static PlaylistData JsonToPlaylist(string path) => JsonConvert.DeserializeObject<PlaylistData>(File.ReadAllText(path));

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
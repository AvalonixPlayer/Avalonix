using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.AvalonixAPI;
using Newtonsoft.Json;

namespace AvalonixAPI;

public static class PlaylistsManager
{
    public static string[] PlaylistsPaths => Directory.GetFiles(DiskManager.SettingsPath);

    private static CancellationTokenSource _playlistCts = new CancellationTokenSource();
    private static CancellationToken _playlistCtsToken = _playlistCts.Token;

    public static string[] PlaylistsNames
    {
        get
        {
            List<string> result = new List<string>();

            foreach (var i in PlaylistsPaths) result.Add(Path.GetFileNameWithoutExtension(i));

            return result.ToArray();
        }
    }

    public static string[] SongsNamesInPlaylist(string playlistName)
    {
        SongData[] allSongs = JsonToPlaylist(Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json")).Songs.ToArray();
        string[] result = new string[allSongs.Length];
        for (int i = 0; i < allSongs.Length; i++)
        {
            result[i] = allSongs[i].Title;
        }
        return result;
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

        SongData songToRemove = null!;
        foreach (var song in data.Songs.Where(song => song.Title == songName))
        {
            songToRemove = song;
        }
        data.Songs.Remove(songToRemove);


        PlaylistToJson(path, data);
    }

    public static void CreateNewPlaylist(PlaylistData data)
    {
        CreatePlaylistFile(data);

        ChangeSettingsToPlaylist(data.Name, data);
    }

    public static void RemovePlaylist(string playlistName) => File.Delete(Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json"));

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

    public static void PlayPlaylist(string playlistName)
    {
        PlaylistData data = JsonToPlaylist(Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json"));

        Thread thread = new Thread(() =>
        {
            if (Settings.Loop)
            {
                while (_playlistCtsToken.IsCancellationRequested == false)
                {
                    Play();
                }
            }
            else
            {
                Play();
            }
            void Play()
            {
                if (Settings.Shuffle == true)
                {
                    data.Songs = data.Songs.OrderBy(x => Random.Shared.Next()).ToList();
                }
                foreach (var song in data.Songs)
                {
                    MediaPlayer.Play(song.FilePath);
                }
            }
        });
        thread.Start();
    }

    public static void PausePlaylist()
    {
        MediaPlayer.Pause();
    }

    public static void ContinuePlaylist()
    {
        MediaPlayer.Continue();
    }

    public static void StopPlaylist()
    {
        _playlistCts.Cancel();
        _playlistCts.Dispose();
        _playlistCts = new CancellationTokenSource();
        _playlistCtsToken = _playlistCts.Token;

        MediaPlayer.Stop();
    }

    private static void CreatePlaylistFile(PlaylistData data)
    {
        string path = Path.Combine(DiskManager.SettingsPath, $"{data.Name}.json");

        using FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        fs.Write(null!);
    }

}
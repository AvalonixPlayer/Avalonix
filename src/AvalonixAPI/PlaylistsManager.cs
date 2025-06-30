using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonix.AvalonixAPI;
using Newtonsoft.Json;

namespace AvalonixAPI;

public static class PlaylistsManager
{
    private static string[] PlaylistsPaths => Directory.GetFiles(DiskManager.SettingsPath);

    private static CancellationTokenSource _playlistCts = new CancellationTokenSource();
    private static CancellationToken _playlistCtsToken = _playlistCts.Token;

    public static string?[] PlaylistsNames => PlaylistsPaths.Select(Path.GetFileNameWithoutExtension).ToArray();

    public static string[] SongsNamesInPlaylist(string playlistName)
    {
        var allSongs = JsonToPlaylist(Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json")).Songs.ToArray();
        var result = new string[allSongs.Length];
        for (var i = 0; i < allSongs.Length; i++)
        {
            result[i] = allSongs[i].Title;
        }
        return result;
    }

    public static void AddSongToPlaylist(string playlistName, SongData songData)
    {
        var path = Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json");
        var data = JsonToPlaylist(path);
        data.Songs.Add(songData);
        PlaylistToJson(path, data);
    }

    public static void RemoveSongFromPlaylist(string playlistName, string songNameToRemove)
    {
        var path = Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json");
        var data = JsonToPlaylist(path);

        SongData songToRemove = null!;
        foreach (var song in data.Songs.Where(song => song.Title == songNameToRemove))
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

    public static void PlaylistToJson(string path, PlaylistData playlist) => 
        new JsonSerializer { Formatting = Formatting.Indented }.Serialize(new JsonTextWriter(new StreamWriter(path)), playlist);

    public static PlaylistData JsonToPlaylist(string path) => JsonConvert.DeserializeObject<PlaylistData>(File.ReadAllText(path));

    public static void PlayPlaylist(string playlistName)
    {
        var data = JsonToPlaylist(Path.Combine(DiskManager.SettingsPath, $"{playlistName}.json"));

        new Thread(() =>
        {
            if (Settings.Loop)
            {
                while (_playlistCtsToken.IsCancellationRequested == false)
                {
                    PlaySongs();
                    if (_playlistCtsToken.IsCancellationRequested) break; // Exit loop if cancellation requested mid-loop
                }
            }
            else
            {
                PlaySongs();
            }

            return;

            void PlaySongs()
            {
                if (Settings.Shuffle)
                {
                    data.Songs = data.Songs.OrderBy(_ => Random.Shared.Next()).ToList();
                }
                foreach (var song in data.Songs)
                {
                    // Check for cancellation before playing each song
                    if (_playlistCtsToken.IsCancellationRequested) return; 
                    MediaPlayer.Play(song.FilePath);
                }
            }
        }).Start();
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
        var path = Path.Combine(DiskManager.SettingsPath, $"{data.Name}.json");
        new FileStream(path, FileMode.OpenOrCreate).Write(null!);
    }
}
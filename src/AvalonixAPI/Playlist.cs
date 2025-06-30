using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace Avalonix.AvalonixAPI;

public class Playlist
{
    public string Name { get; set; }
    public List<SongData> Songs { get; set; }
    public int? Year { get; set; }
    public string? Performer { get; set; }
    public string? Album { get; set; }

    private static string PlaylistsDirectory => DiskManager.SettingsPath;

    private static CancellationTokenSource _playlistCts = new CancellationTokenSource();
    private static CancellationToken _playlistCtsToken = _playlistCts.Token;

    public Playlist(string name, List<SongData> songs, int? year = -1, string? performer = null!, string? album = null!)
    {
        Name = name;
        Songs = songs;
        Year = year;
        Performer = performer;
        Album = album;
    }

    public Playlist()
    {
    }

    public string[] GetSongsNames()
    {
        return Songs.Select(song => song.Title).ToArray();
    }

    public void AddSong(SongData songData)
    {
        Songs.Add(songData);
        SaveToJsonFile();
    }

    public void RemoveSong(string songNameToRemove)
    {
        var songToRemove = Songs.FirstOrDefault(song => song.Title == songNameToRemove);
        if (songToRemove == null) return;
        Songs.Remove(songToRemove);
        SaveToJsonFile();
    }

    public void Create()
    {
        CreatePlaylistFile(Name);
        SaveToJsonFile();
    }

    public void Delete()
    {
        File.Delete(Path.Combine(PlaylistsDirectory, $"{Name}.json"));
    }

    public void UpdateName(string newName)
    {
        var oldPath = Path.Combine(PlaylistsDirectory, $"{Name}.json");
        File.Delete(oldPath);
        CreatePlaylistFile(newName);
        SaveToJsonFile();
    }

    private void SaveToJsonFile()
    {
        var path = Path.Combine(PlaylistsDirectory, $"{Name}.json");
        new JsonSerializer { Formatting = Formatting.Indented }.Serialize(new JsonTextWriter(new StreamWriter(path)), this);
    }

    public static Playlist Load(string playlistName)
    {
        var path = Path.Combine(PlaylistsDirectory, $"{playlistName}.json");
        return JsonConvert.DeserializeObject<Playlist>(File.ReadAllText(path))!;
    }

    public void Play()
    {
        _playlistCts = new CancellationTokenSource();
        _playlistCtsToken = _playlistCts.Token;

        new Thread(() =>
        {
            if (Settings.Loop)
            {
                while (!_playlistCtsToken.IsCancellationRequested)
                {
                    PlaySongsInternal();
                    if (_playlistCtsToken.IsCancellationRequested) break;
                }
            }
            else
            {
                PlaySongsInternal();
            }
        }).Start();
    }

    private void PlaySongsInternal()
    {
        var songsToPlay = Songs.ToList();

        if (Settings.Shuffle)
        {
            songsToPlay = songsToPlay.OrderBy(_ => Random.Shared.Next()).ToList();
        }
        
        foreach (var song in songsToPlay)
        {
            if (_playlistCtsToken.IsCancellationRequested) return;
            MediaPlayer.Play(song.FilePath);
        }
    }

    public void Pause() => MediaPlayer.Pause();

    public void Continue() => MediaPlayer.Continue();

    public void Stop()
    {
        _playlistCts.Cancel();
        _playlistCts.Dispose();
        _playlistCts = new CancellationTokenSource();
        _playlistCtsToken = _playlistCts.Token;

        MediaPlayer.Stop();
    }

    private static void CreatePlaylistFile(string playlistName)
    {
        var path = Path.Combine(PlaylistsDirectory, $"{playlistName}.json");
        Directory.CreateDirectory(PlaylistsDirectory);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "{}");
        }
    }

    public static string?[] GetAllPlaylistNames() => Directory.GetFiles(PlaylistsDirectory)
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();
}
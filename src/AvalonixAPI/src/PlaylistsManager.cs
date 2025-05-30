using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvalonixAPI;

public static class PlaylistsManager
{
    public static PlaylistData LoadPlaylist(string playlistName)
    {
        DiskManager.EnsurePlaylistsDirectoryExists();
        string path = GetPlaylistPath(playlistName);
        
        if (!File.Exists(path))
            throw new FileNotFoundException($"Playlist {playlistName} not found at {path}");

        string json = File.ReadAllText(path);
        return PlaylistData.FromJson(json);
    }

    public static void SavePlaylist(PlaylistData playlistData)
    {
        DiskManager.EnsurePlaylistsDirectoryExists();
        string path = GetPlaylistPath(playlistData.Name);
        File.WriteAllText(path, playlistData.ToJson());
    }

    public static PlaylistData CreateNewPlaylist(string name, int year = -1)
    {
        if (year == -1) year = DateTime.Now.Year;
        
        var playlist = new PlaylistData
        {
            Name = name,
            Year = year,
            PlayCount = 0,
            LastPlayed = DateTime.MinValue,
            Songs = new List<PlaylistData.SongInfo>()
        };
        
        SavePlaylist(playlist);
        return playlist;
    }

    public static void AddSongToPlaylist(string playlistName, string songPath, 
        string title = null, string artist = null, string album = null, TimeSpan? duration = null)
    {
        var playlist = LoadPlaylist(playlistName);
        
        title ??= Path.GetFileNameWithoutExtension(songPath);
        duration ??= TimeSpan.Zero;
        
        playlist.Songs.Add(new PlaylistData.SongInfo
        {
            Title = title,
            Path = DiskManager.GetFullPath(songPath),
            Duration = duration.Value,
            Artist = artist,
            Album = album
        });
        
        SavePlaylist(playlist);
    }

    public static string[] GetAvailablePlaylists()
    {
        DiskManager.EnsurePlaylistsDirectoryExists();
        return Directory.GetFiles(Settings.PlaylistsDirectory, "*.json")
                       .Select(Path.GetFileNameWithoutExtension)
                       .ToArray();
    }

    public static void DeletePlaylist(string playlistName)
    {
        string path = GetPlaylistPath(playlistName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string GetPlaylistPath(string playlistName) => 
        Path.Combine(Settings.PlaylistsDirectory, $"{playlistName}.json");
}
using System;
using System.Collections.Generic;

namespace Avalonix.API;

public class Playlist(string name)
{
    public string Name => name;

    public PlaylistData PlaylistData = new();

    public void AddTrack(Track track) => PlaylistData.Tracks.Add(track);
    public void RemoveTrack(Track track) => PlaylistData.Tracks.Remove(track);

    public void Save() => DiskManager.SavePlaylist(this);
    public void UpdateLastListenDate() => PlaylistData.LastListen = DateTime.Now.TimeOfDay;
}

public struct PlaylistData()
{
    public List<Track> Tracks { get; set; } = [];
    public TimeSpan? LastListen { get; set; } = null!;
}
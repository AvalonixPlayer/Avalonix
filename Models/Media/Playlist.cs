using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Disk;

namespace Avalonix.Models.Media;

public class Playlist(string name, IMediaPlayer player, IDiskManager disk)
{
    public string Name => name;

    public PlaylistData PlaylistData = new();

    public void AddTrack(Track track) => PlaylistData.Tracks.Add(track);

    public void RemoveTrack(Track track)
    {
        for(var i = 0; i < PlaylistData.Tracks.Count; i++)
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
    }

    public async Task Save() => await disk.SavePlaylist(this);
    public void UpdateLastListenDate() => PlaylistData.LastListen = DateTime.Now.TimeOfDay;

    public async Task Play()
    {
        foreach (var track in PlaylistData.Tracks)
        {
            player.Play(track);
            while (!player.IsFree)
                Task.Delay(1000).Wait();
        }
    }

    public void Pause() =>
        player.Pause();

    public void Resume() =>
        player.Resume();
}

public struct PlaylistData()
{
    public List<Track> Tracks { get; init; } = [];
    public TimeSpan? LastListen { get; set; } = null!;
}
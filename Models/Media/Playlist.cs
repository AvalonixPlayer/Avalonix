using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using NeoSimpleLogger;

namespace Avalonix.Models.Media;

public class Playlist(string name)
{
    public string Name => name;

    public PlaylistData PlaylistData = new();
    private readonly MediaPlayer _player = new(new Logger());

    public void AddTrack(Track track) => PlaylistData.Tracks.Add(track);

    public void RemoveTrack(Track track)
    {
        for(int i = 0; i < PlaylistData.Tracks.Count; i++)
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
    }

    public void Save() => new DiskManager().SavePlaylist(this);
    public void UpdateLastListenDate() => PlaylistData.LastListen = DateTime.Now.TimeOfDay;

    public void Play()
    {
        Task.Run(() =>
        {
            foreach (var track in PlaylistData.Tracks)
            {
                _player.Play(track);
                while (!_player.IsFree)
                    Task.Delay(1000).Wait();
            }
        });
    }

    public void Pause() =>
        _player.Pause();

    public void Resume() =>
        _player.Resume();
}

public struct PlaylistData()
{
    public List<Track> Tracks { get; set; } = [];
    public TimeSpan? LastListen { get; set; } = null!;
}
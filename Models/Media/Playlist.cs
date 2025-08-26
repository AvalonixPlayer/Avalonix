using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoSimpleLogger;

namespace Avalonix.API;

public class Playlist(string name)
{
    public string Name => name;

    public PlaylistData PlaylistData = new();
    private readonly MediaPlayer _player = new(new Logger());

    public void AddTrack(Track track) => PlaylistData.Tracks.Add(track);
    public void RemoveTrack(Track track) => PlaylistData.Tracks.Remove(track);

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
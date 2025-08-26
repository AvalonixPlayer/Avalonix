using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Disk;

namespace Avalonix.Models.Media;

//хрень с конструктором опять
public class Playlist
{
    public Playlist() { }

    public Playlist(string name, IMediaPlayer player, IDiskManager disk)
    {
        Name = name;
        Player = player;
        Disk = disk;
    }

    private IMediaPlayer Player;
    private IDiskManager Disk;
    public string Name {get; set;}

    public PlaylistData PlaylistData = new();

    public async Task AddTrack(Track track)
    {
        PlaylistData.Tracks.Add(track);
        await Save();
    }

    public async Task RemoveTrack(Track track)
    {
        for (var i = 0; i < PlaylistData.Tracks.Count; i++)
        {
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
            await Save();
        }
    }

    public async Task Save() => await Disk.SavePlaylist(this);

    public async Task Play()
    {
        foreach (var track in PlaylistData.Tracks)
        {
            PlaylistData.LastListen = DateTime.Now.Date;

            await Save();

            Player.Play(track);

            while (!Player.IsFree)
                Task.Delay(1000).Wait();
        }
    }

    public void Pause() =>
        Player.Pause();

    public void Resume() =>
        Player.Resume();
}

public struct PlaylistData()
{
    public List<Track> Tracks { get; init; } = [];
    public DateTime? LastListen { get; set; } = null!;

    public TimeSpan? PlaylistDuration
    {
        get
        {
            TimeSpan totalTime = TimeSpan.Zero;
            foreach (var track in Tracks)
                totalTime += track.Metadata.Duration;
            return totalTime;
        }
    }
}
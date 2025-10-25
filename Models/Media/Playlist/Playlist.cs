using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.Models.UserSettings;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.Playlist;

public record Playlist
{
    public string Name { get; }
    public PlaylistData PlaylistData { get; }
    private IMediaPlayer Player { get; }
    private IDiskManager Disk { get; }
    private ILogger Logger { get; }
    private Settings Settings { get; }
    private PlayQueue PlayQueue { get; }

    private readonly Random _random = new();

    public Playlist(string name, PlaylistData playlistData, IMediaPlayer player, IDiskManager disk, ILogger logger, Settings settings)
    {
        Name = name;
        PlaylistData = playlistData;
        Player = player;
        Disk = disk;
        Logger = logger;
        Settings = settings;
        PlayQueue = new PlayQueue(Settings, _random);

        PlayQueue.FillQueue(PlaylistData);
    }

    public async Task AddTrack(Track.Track track)
    {
        Logger.LogDebug("{TrackName} added into {playlist}", track.Metadata.TrackName, Name);
        PlaylistData.Tracks.Add(track);
        await Save();
    }

    public Playlist MergePlaylist(Playlist[] otherPlaylists)
    {
        foreach (var otherPlaylist in otherPlaylists)
            Logger.LogDebug("{Playlist1} merged with {playlist2}", Name, otherPlaylist.Name);

        var result = this;

        for (var i = 0; i < otherPlaylists.Length; i++)
            result.PlaylistData.Tracks.ForEach(track => PlaylistData.Tracks.Add(track));
        return result;
    }

    public async Task RemoveTrack(Track.Track track)
    {
        for (var i = 0; i < PlaylistData.Tracks.Count; i++)
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
        await Save();
    }

    public async Task RemoveDuplicativeTracks()
    {
        var uniquePaths = new HashSet<string>();
        var tracksToKeep = PlaylistData.Tracks.Where(track => uniquePaths.Add(track.TrackData.Path)).ToList();

        PlaylistData.Tracks = tracksToKeep;
        await Save();
    }

    public void SortTracks(SortPlaylistTrackFlags flags)
    {
        if (flags == SortPlaylistTrackFlags.Artist)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Artist).ToList();
        if (flags == SortPlaylistTrackFlags.ArtistInverted)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Artist).Reverse().ToList();
        if (flags == SortPlaylistTrackFlags.Year)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Year).ToList();
        if (flags == SortPlaylistTrackFlags.YearInverted)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Year).Reverse().ToList();
        if (flags == SortPlaylistTrackFlags.Durration)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Duration).ToList();
        if (flags == SortPlaylistTrackFlags.DurrationInverted)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Duration).Reverse().ToList();
    }


    public async Task Save()
    {
        Logger.LogDebug("Playlist saved {playlistName}", Name);
        await Disk.SavePlaylist(this);
    }

    private void UpdateLastListen()
    {
        Logger.LogDebug("Updated last listen info of playlist {playlistName}", Name);
        PlaylistData.LastListen = DateTime.Now.Date;
    }

    private void UpdateRarity(ref Track.Track track)
    {
        Logger.LogDebug("Updated rarity of playlist {playlistName} and track {trackName} in it", Name,
            track.Metadata.TrackName);
        PlaylistData.Rarity++;
        track.IncreaseRarity(1);
    }

    public async Task Play(int startSong = 0)
    {
        Logger.LogDebug("Playlist {Name} has started", Name);

        for (var i = startSong; i < PlayQueue.Tracks.Count; i++)
        {
            PlayQueue.PlayingIndex = i;
            var track = PlayQueue.Tracks[PlayQueue.PlayingIndex];

            UpdateLastListen();
            UpdateRarity(ref track);

            await Save();

            Player.Play(track);

            while (!Player.IsFree)
                await Task.Delay(1000);
        }

        PlayQueue.FillQueue(PlaylistData);
        if (Settings.Avalonix.Playlists.Loop) await Play();

        Logger.LogDebug("Playlist {Name} completed", Name);
    }

    public void NextTrack()
    {
        if (PlayQueue.PlayingIndex>= PlayQueue.Tracks.Count)
        {
            _ = Play(PlayQueue.PlayingIndex);
            PlayQueue.FillQueue(PlaylistData);
        }
        else
            _ = Play(PlayQueue.PlayingIndex + 1);
    }

    public void BackTrack()
    {
        if (PlayQueue.PlayingIndex - 1 <= 0)
            _ = Play(0);
        else
            _ = Play(PlayQueue.PlayingIndex - 1);
    }

    public void Stop()
    {
        Player.Stop();
        Logger.LogDebug("Playlist stopped");
        Player.Stop();
    }

    public void Pause()
    {
        Player.Pause();
        Logger.LogDebug("Playlist paused");
        Player.Pause();
    }

    public void Resume()
    {
        Player.Resume();
        Logger.LogDebug("Playlist resumed");
        Player.Resume();
    }

    public bool Paused() =>
        Player.IsPaused;
}
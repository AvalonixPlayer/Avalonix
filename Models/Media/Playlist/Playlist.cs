using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.UserSettings;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.Playlist;

public record Playlist
{
    public string Name { get; }
    public PlaylistData PlaylistData { get; }
    private IMediaPlayer Player { get; }
    private IDiskManager Disk { get; }
    private ILogger Logger { get; }
    private PlaySettings Settings { get; }
    public PlayQueue PlayQueue { get; }

    private readonly Random _random = new();
    private CancellationTokenSource? _cancellationTokenSource;
    public bool Paused => Player.IsPaused;

    public Playlist(string name, PlaylistData playlistData, IMediaPlayer player, IDiskManager disk, ILogger logger, PlaySettings settings)
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
        PlaylistData.Tracks = flags switch
        {
            SortPlaylistTrackFlags.Artist => PlaylistData.Tracks.OrderBy(track => track.Metadata.Artist).ToList(),
            SortPlaylistTrackFlags.ArtistInverted => PlaylistData.Tracks.OrderBy(track => track.Metadata.Artist)
                .Reverse()
                .ToList(),
            SortPlaylistTrackFlags.Year => PlaylistData.Tracks.OrderBy(track => track.Metadata.Year).ToList(),
            SortPlaylistTrackFlags.YearInverted => PlaylistData.Tracks.OrderBy(track => track.Metadata.Year)
                .Reverse()
                .ToList(),
            SortPlaylistTrackFlags.Duration => PlaylistData.Tracks.OrderBy(track => track.Metadata.Duration).ToList(),
            SortPlaylistTrackFlags.DurationInverted => PlaylistData.Tracks.OrderBy(track => track.Metadata.Duration)
                .Reverse()
                .ToList(),
            _ => PlaylistData.Tracks
        };
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
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        Logger.LogDebug("Playlist {Name} has started", Name);

        var playing = true;
        
        Console.WriteLine(Settings.Loop);
        
        while (playing)
        {
            for (var i = startSong; i < PlayQueue.Tracks.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                PlayQueue.PlayingIndex = i;
                var track = PlayQueue.Tracks[PlayQueue.PlayingIndex];

                UpdateLastListen();
                UpdateRarity(ref track);

                Player.Play(track);
            
                while (!Player.IsFree && !cancellationToken.IsCancellationRequested)
                    await Task.Delay(1000, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            if (!Settings.Loop)
            {
                playing = false;
                Logger.LogDebug("Playlist {Name} completed", Name);
            }
        }
        if (!cancellationToken.IsCancellationRequested)
        {
            PlayQueue.FillQueue(PlaylistData);
            if (Settings.Loop) 
                await Play();
        }
    }
    
    public async Task Stop()
    {
        await Save();
        _cancellationTokenSource?.Cancel();
        Player.Stop();
        Logger.LogDebug("Playlist stopped");
    }

    public void NextTrack()
    {
        if (PlayQueue.PlayingIndex >= PlayQueue.Tracks.Count - 1)
        {
            PlayQueue.FillQueue(PlaylistData);
            _ = Play(PlayQueue.PlayingIndex);
        }
        else
            _ = Play(PlayQueue.PlayingIndex + 1);
    }

    public void BackTrack() =>
        _ = PlayQueue.PlayingIndex - 1 <= 0 ? Play(0) : Play(PlayQueue.PlayingIndex - 1);

    public void Pause()
    {
        Player.Pause();
        Logger.LogDebug("Playlist paused");
    }

    public void Resume()
    {
        Player.Resume();
        Logger.LogDebug("Playlist resumed");
    }
}
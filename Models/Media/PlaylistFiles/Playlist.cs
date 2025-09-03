using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.Media.TrackFiles;
using Avalonix.Models.UserSettings;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.PlaylistFiles;

public class Playlist
{
    private IMediaPlayer _player = null!;
    private DiskManager _disk = null!;
    private ILogger _logger = null!;
    private Settings _settings = null!;
    private readonly Random _random = new();
    public string Name { get; private set; } = null!;

    public PlaylistData PlaylistData = new();

    [JsonConstructor]
    public Playlist()
    {
    }

    public Playlist(string name, IMediaPlayer player, DiskManager disk)
    {
        Name = name;
        _player = player;
        _disk = disk;
    }

    public async Task InitializeAsync(string name, IMediaPlayer player, DiskManager disk, ILogger logger)
    {
        Name = name;
        _player = player;
        _disk = disk;
        _logger = logger;
        _settings = await _disk.GetSettings();
    }

    public async Task AddTrack(Track track)
    {
        PlaylistData.Tracks.Add(track);
        await Save();
    }

    public async Task AddOtherPlaylistsContaints(Playlist[] otherPlaylists)
    {
        foreach (var playlist in otherPlaylists)
            playlist.PlaylistData.Tracks.ForEach(track => PlaylistData.Tracks.Add(track));
        await Save();
    }

    public async Task RemoveTrack(Track track)
    {
        for (var i = 0; i < PlaylistData.Tracks.Count; i++)
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
        await Save();
    }

    public async Task RemoveDuplicativeTracks()
    {
        var uniquePaths = new HashSet<string>();
        var tracksToKeep = new List<Track>();

        foreach (var track in PlaylistData.Tracks)
        {
            if (uniquePaths.Add(track.TrackData.Path)) // Add возвращает true если путь уникальный
            {
                tracksToKeep.Add(track);
            }
        }

        PlaylistData.Tracks = tracksToKeep;
        await Save();
    }

    public async Task SortTracks(SortPlaylistTrackFlags flags)
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
        
        await Save();
    }


    public async Task Save() => await _disk.SavePlaylist(this);

    private void UpdateLastListen() => PlaylistData.LastListen = DateTime.Now.Date;

    private void UpdateRarity(ref Track track)
    {
        PlaylistData.Rarity++;
        track.IncreaseRarity(1);  
    } 

    public async Task Play()
    {
        var tracks = PlaylistData.Tracks;

        if (_settings.Avalonix.Playlists.Shuffle)
            tracks = tracks.OrderBy(_ => _random.Next()).ToList();

        _logger.LogInformation("Playlist {Name} has started", Name);

        for (int i = 0;i < tracks.Count;i++)
        {
            var track = tracks[i];
            
            UpdateLastListen();
            UpdateRarity(ref track);

            await Save();

            _player.Play(track);

            while (!_player.IsFree)
                await Task.Delay(1000);
        }

        if (_settings.Avalonix.Playlists.Loop) await Play();

        _logger.LogInformation("Playlist {Name} completed", Name);
    }

    public void Stop() =>
        _player.Stop();

    public void Pause() =>
        _player.Pause();

    public void Resume() =>
        _player.Resume();
}
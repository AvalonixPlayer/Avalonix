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
    private IDiskManager _disk = null!;
    private ILogger _logger = null!;
    private Settings _settings = null!;
    private readonly Random _random = new();
    public string Name { get; private set; } = null!;

    public PlaylistData PlaylistData = new();

    [JsonConstructor]
    public Playlist()
    {
    }

    public async Task InitializeAsync(string name, IMediaPlayer player, IDiskManager disk, ILogger logger)
    {
        Name = name;
        _player = player;
        _disk = disk;
        _logger = logger;
    }

    public async Task AddTrack(Track track)
    {
        _logger.LogDebug("{TrackName} added into {playlist}", track.Metadata.TrackName, Name);
        PlaylistData.Tracks.Add(track);
        await Save();
    }

    public Playlist MergePlaylist(Playlist[] otherPlaylists)
    {
        foreach (var otherPlaylist in otherPlaylists)
            _logger.LogDebug("{Playlist1} merged with {playlist2}", Name, otherPlaylist.Name);

        var result = this;

        for (var i = 0; i < otherPlaylists.Length; i++)
            result.PlaylistData.Tracks.ForEach(track => PlaylistData.Tracks.Add(track));
        return result;
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
        var tracksToKeep = PlaylistData.Tracks.Where(track => uniquePaths.Add(track.TrackData.Path)).ToList();

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


    public async Task Save()
    {
        _logger.LogDebug("Playlist saved {playlistName}", Name);
        await _disk.SavePlaylist(this);
    }

    private void UpdateLastListen()
    {
        _logger.LogDebug("Updated last listen info of playlist {playlistName}", Name);
        PlaylistData.LastListen = DateTime.Now.Date;
    }

    private void UpdateRarity(ref Track track)
    {
        _logger.LogDebug("Updated rarity of playlist {playlistName} and track {trackName} in it", Name,
            track.Metadata.TrackName);
        PlaylistData.Rarity++;
        track.IncreaseRarity(1);
    }

    public async Task Play(int startSong = 0)
    {
        var tracks = PlaylistData.Tracks;

        if (_settings.Avalonix.Playlists.Shuffle)
            tracks = tracks.OrderBy(_ => _random.Next()).ToList();

        _logger.LogDebug("Playlist {Name} has started", Name);

        for (var i = _settings.Avalonix.Playlists.Shuffle ? startSong : 0; i < tracks.Count; i++)
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

        _logger.LogDebug("Playlist {Name} completed", Name);
    }

    public void Stop()
    {
        _logger.LogDebug("Playlist stopped");
        _player.Stop();
    }

    public void Pause()
    {
        _logger.LogDebug("Playlist paused");
        _player.Pause();
    }

    public void Resume()
    {
        _logger.LogDebug("Playlist resumed");
        _player.Resume();
    }
}
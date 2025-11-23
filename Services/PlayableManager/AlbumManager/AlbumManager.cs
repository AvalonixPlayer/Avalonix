using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.Album;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Track;
using Avalonix.Services.DiskManager;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.UserSettings;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlayableManager.AlbumManager;

public class AlbumManager(
    ILogger logger,
    IMediaPlayer player,
    ISettingsManager settingsManager,
    IDiskManager diskManager,
    IMediaPlayer mediaPlayer) : IAlbumManager
{
    public Action? TrackLoaded { get; set; }

    private readonly List<Track> _tracks = [];

    private CancellationTokenSource? _globalCancellationTokenSource;

    public IMediaPlayer MediaPlayer { get; } = mediaPlayer;
    public IPlayable? PlayingPlayable { get; set; }

    public Track? CurrentTrack { get; }
    private readonly Settings _settings = settingsManager.Settings!;
    public event Action<bool> PlaybackStateChanged
    {
        add => player.PlaybackStateChanged += value;
        remove => player.PlaybackStateChanged -= value;
    }

    public event Action TrackChanged
    {
        add => player.TrackChanged += value;
        remove => player.TrackChanged -= value;
    }

    public event Action<bool> ShuffleChanged
    {
        add => _settings.Avalonix.SuffleChanged += value;
        remove => _settings.Avalonix.SuffleChanged -= value;
    }

    public event Action<bool> LoopChanged
    {
        add => _settings.Avalonix.LoopChanged += value;
        remove => _settings.Avalonix.LoopChanged -= value;
    }
    public event Action? PlayableChanged;

    public void LoadTracks()
    {
        var paths = diskManager.GetMusicFilesForAlbums();

        _tracks.Clear();
        foreach (var path in paths)
        {
            var track = new Track(path);
            track.Metadata.Init(path);
            track.Metadata.FillTrackMetaData();
            _tracks.Add(track);
            TrackLoaded?.Invoke();
        }
    }

    public List<Album> GetAlbums()
    {
        var albums = new List<Album>();

        foreach (var track in _tracks)
        {
            if (track.Metadata.Artist == null || track.Metadata.Album == null) continue;

            var album = ContainAlbum(track.Metadata.Artist, track.Metadata.Album);

            if (album != null) album.PlayQueue.Tracks.Add(track);
            else
            {
                album = new Album([track.TrackData.Path], player, logger,
                    settingsManager.Settings!.Avalonix.PlaySettings);
                albums.Add(album);
            }
        }

        return albums;

        Album? ContainAlbum(string artist, string albumName)
        {
            if (albums.Count == 0) return null;

            return albums.FirstOrDefault(album =>
                album.Metadata!.ArtistName == artist &&
                album.Metadata.AlbumName == albumName);
        }
    }

    public Task StartAlbum(IPlayable album)
    {
        try
        {
            _globalCancellationTokenSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            /* ignore */
        }
        finally
        {
            _globalCancellationTokenSource?.Dispose();
        }

        PlayingPlayable = album;

        PlayableChanged?.Invoke();

        _ = Task.Run(async () =>
        {
            try
            {
                await PlayingPlayable.Play().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                /* expected on cancel */
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Playlist play failed");
            }
        });
        return Task.CompletedTask;
    }

    public void RemoveAlbum(Album album)
    {
        // TODO: remove tracks
    }

    public Task<List<IPlayable>> GetPlaylists()
    {
        LoadTracks();
        return Task.FromResult(GetAlbums().Cast<IPlayable>().ToList());
    }
}

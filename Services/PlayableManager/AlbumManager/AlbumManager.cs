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
    private readonly Settings _settings = settingsManager.Settings!;
    private readonly List<Track> _tracks = [];
    private bool _tracksLoaded;
    public Action? TrackLoaded { get; set; }

    public CancellationTokenSource? GlobalCancellationTokenSource { get; private set; }
    public IMediaPlayer MediaPlayer { get; } = mediaPlayer;
    public IPlayable? PlayingPlayable { get; set; }

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

    public void StartPlayable(IPlayable album)
    {
        try
        {
            GlobalCancellationTokenSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            GlobalCancellationTokenSource?.Dispose();
        }

        GlobalCancellationTokenSource = new CancellationTokenSource();

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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Album play failed");
            }
        });
    }

    public void RemoveAlbum(Album album)
    {
    }

    public async Task<List<IPlayable>> GetPlayables()
    {
        var result = await Task.Run(GetAlbums);
        return result.Cast<IPlayable>().ToList();
    }

    private async Task<List<Album>> GetAlbums()
    {
        await Task.Run(LoadTracks);

        if (!_tracksLoaded)
        {
            logger.LogWarning("Tracks metadata not loaded yet. Call LoadTracks() first.");
            return [];
        }

        var albumGroups = _tracks
            .Where(track => track.Metadata.Artist != null && track.Metadata.Album != null)
            .GroupBy(track => new { Artist = track.Metadata.Artist!, Album = track.Metadata.Album! });

        return albumGroups.Select(group => group.Select(track => track.TrackData.Path).ToList()).Select(tracksPaths =>
            new Album(tracksPaths, player, logger, settingsManager.Settings!.Avalonix.PlaySettings)).ToList();
    }

    private async Task LoadTracks()
    {
        var paths = diskManager.GetMusicFiles();

        _tracks.Clear();
        _tracksLoaded = false;

        var loadTasks = new List<Task>();

        foreach (var path in paths)
        {
            var track = new Track(path);
            track.Metadata.Init(path);
            _tracks.Add(track);

            var loadTask = Task.Run(async () =>
            {
                await track.Metadata.FillTrackMetaData();
                TrackLoaded?.Invoke();
            });
            loadTasks.Add(loadTask);
        }

        await Task.WhenAll(loadTasks);
        _tracksLoaded = true;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Playlist;
using Avalonix.Model.Media.Track;
using Avalonix.Services.DiskManager;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.UserSettings;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlayableManager.PlaylistManager;

public class PlaylistManager(
    IMediaPlayer player,
    IDiskManager diskManager,
    ILogger logger,
    ISettingsManager settingsManager)
    : IPlaylistManager
{
    public IMediaPlayer MediaPlayer => player;
    public IPlayable? PlayingPlayable { get; set; }
    private CancellationTokenSource? _globalCancellationTokenSource;

    private readonly Settings _settings = settingsManager.Settings!;

    public event Action? PlayableChanged;


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

    public async Task<List<PlaylistData>> GetAllPlaylistData() => await diskManager.GetAllPlaylists();

    public Playlist ConstructPlaylist(string title, List<Track> tracks, string? observingDirectory)
    {
        var playlistData = new PlaylistData
        {
            Tracks = tracks,
            LastListen = null,
            ObserveDirectory = observingDirectory is not null,
            ObservingDirectory = observingDirectory
        };

        var settings = settingsManager.Settings!;
        return new Playlist(playlistData, player, diskManager, logger, settings.Avalonix.PlaySettings);
    }

    public async Task EditPlaylist(Playlist playlist) => await playlist.Save();

    public async Task CreatePlaylist(Playlist playlist) => await playlist.Save();

    public void DeletePlaylist(Playlist playlist) => diskManager.RemovePlaylist(playlist.PlaylistData.Name);

    public void StartPlayable(IPlayable playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        try
        {
            _globalCancellationTokenSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            _globalCancellationTokenSource?.Dispose();
        }

        _globalCancellationTokenSource = new CancellationTokenSource();

        if (PlayingPlayable != null)
        {
            try
            {
                PlayingPlayable.Stop();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error stopping previous playlist");
            }

            PlayingPlayable = null;
        }

        PlayingPlayable = playlist;

        PlayableChanged?.Invoke();

        Task.Run(PlayingPlayable.LoadTracksMetadata);

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
                logger.LogError(ex, "Playlist play failed");
            }
        });
    }

    public async Task<List<IPlayable>> GetPlayables()
    {
        var allPlaylistData = await GetAllPlaylistData();
        return allPlaylistData.Select(data =>
            new Playlist(data, player, diskManager, logger, _settings.Avalonix.PlaySettings)
        ).Cast<IPlayable>().ToList();
    }
}
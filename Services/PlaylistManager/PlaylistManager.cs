using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.Track;
using Avalonix.Models.UserSettings;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlaylistManager;

public class PlaylistManager(
    IMediaPlayer player,
    IDiskManager diskManager,
    ILogger logger,
    ISettingsManager settingsManager)
    : IPlaylistManager
{
    public IMediaPlayer MediaPlayer => player;
    public IPlayable? PlayingPlaylist { get; set; }
    private CancellationTokenSource? _globalCancellationTokenSource;
    public Track? CurrentTrack => player.CurrentTrack;

    private readonly Settings _settings = settingsManager.Settings!;

    public event Action? PlaylistChanged;

    public void ResetSnuffle()
    {
        logger.LogDebug("Changing shuffle mode");
        _settings.Avalonix.SuffleChanged?.Invoke(!_settings.Avalonix.PlaySettings.Shuffle);
        _settings.Avalonix.PlaySettings.Shuffle = !_settings.Avalonix.PlaySettings.Shuffle;
    }

    public void ResetLoop()
    {
        logger.LogDebug("Changing loop mode");
        _settings.Avalonix.LoopChanged?.Invoke(!_settings.Avalonix.PlaySettings.Loop);
        _settings.Avalonix.PlaySettings.Loop = !_settings.Avalonix.PlaySettings.Loop;
    }

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

    public async Task<List<Playlist>> GetAllPlaylists() => await diskManager.GetAllPlaylists();

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
        return new Playlist(title, playlistData, player, diskManager, logger, settings.Avalonix.PlaySettings);
    }

    public async Task EditPlaylist(Playlist playlist) => await playlist.Save();

    public async Task CreatePlaylist(Playlist playlist) => await playlist.Save();
    public void DeletePlaylist(Playlist playlist) => diskManager.RemovePlaylist(playlist.Name);

    public Task StartPlaylist(Playlist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

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

        _globalCancellationTokenSource = new CancellationTokenSource();

        if (PlayingPlaylist != null)
        {
            try
            {
                PlayingPlaylist.Stop();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error stopping previous playlist");
            }

            PlayingPlaylist = null;
        }

        PlayingPlaylist = playlist;

        PlaylistChanged?.Invoke();

        Task.Run(PlayingPlaylist.LoadTracksMetadata);

        _ = Task.Run(async () =>
        {
            try
            {
                await PlayingPlaylist.Play().ConfigureAwait(false);
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

    public async Task ChangeVolume(uint volume) => await player.ChangeVolume(volume);

    public void PausePlaylist() => PlayingPlaylist?.Pause();

    public void ResumePlaylist() => PlayingPlaylist?.Resume();

    public void NextTrack() => PlayingPlaylist?.NextTrack();

    public void TrackBefore() => PlayingPlaylist?.BackTrack();

    public void ForceStartTrackByIndex(int index) => PlayingPlaylist?.ForceStartTrackByIndex(index);
}
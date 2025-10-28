using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.Track;
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
    public Playlist? PlayingPlaylist { get; set; }
    private CancellationTokenSource? _globalCancellationTokenSource;
    public bool IsPaused { get; } = player.IsPaused;
    public Track? CurrentTrack { get; } = player.CurrentTrack;
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

    public Playlist ConstructPlaylist(string title, List<Track> tracks)
    {
        var playlistData = new PlaylistData
        {
            Tracks = tracks,
            LastListen = null,
            Rarity = 0 
        };
        var settings = Task.Run(async () => await settingsManager.GetSettings()).Result;
        return new Playlist(title, playlistData, player, diskManager, logger, settings);
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
        catch (ObjectDisposedException) { /* ignore */ }
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

        _ = Task.Run(async () =>
        {
            try
            {
                await PlayingPlaylist.Play().ConfigureAwait(false);
            }
            catch (OperationCanceledException) { /* expected on cancel */ }
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
}
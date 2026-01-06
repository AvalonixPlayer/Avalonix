using System;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.Album;
using Avalonix.Model.Media.Artist;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.PlayBox;
using Avalonix.Model.Media.Playlist;
using Avalonix.Model.UserSettings;
using Avalonix.Services.PlayableManager.AlbumManager;
using Avalonix.Services.PlayableManager.ArtistManager;
using Avalonix.Services.PlayableManager.PlayboxManager;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Track = Avalonix.Model.Media.Track.Track;

namespace Avalonix.Services.PlayableManager;

public class PlayablesManager(
    ILogger logger,
    IPlaylistManager playlistManager,
    IAlbumManager albumManager,
    IArtistManager artistManager,
    IPlayboxManager playboxManager,
    IMediaPlayer mediaPlayer,
    ISettingsManager settingsManager) : IPlayablesManager
{
    private readonly Settings _settings = settingsManager.Settings;
    public IMediaPlayer MediaPlayer => mediaPlayer;
    public IPlayable? PlayingPlayable { get; private set; }
    public Track? CurrentTrack => MediaPlayer.CurrentTrack;

    public Task StartPlayable(IPlayable playable)
    {
        playlistManager.PlayingPlayable?.Stop();
        albumManager.PlayingPlayable?.Stop();
        artistManager.PlayingPlayable?.Stop();
        playboxManager.PlayingPlayable?.Stop();
        PlayingPlayable = playable;

        switch (playable)
        {
            case Playlist:
                playlistManager.StartPlayable(playable);
                break;
            case Album:
                albumManager.StartPlayable(playable);
                break;
            case Artist:
                artistManager.StartPlayable(playable);
                break;
            case Playbox:
                playboxManager.StartPlayable(playable);
                break;
        }

        PlayableChanged?.Invoke();
        return Task.CompletedTask;
    }

    public async Task ChangeVolume(uint volume)
    {
        await MediaPlayer.ChangeVolume(volume);
    }

    public void PausePlayable()
    {
        PlayingPlayable?.Pause();
    }

    public void ResumePlayable()
    {
        PlayingPlayable?.Resume();
    }

    public void NextTrack()
    {
        PlayingPlayable?.NextTrack();
    }

    public void TrackBefore()
    {
        PlayingPlayable?.BackTrack();
    }

    public void ResetSnuffle()
    {
        logger.LogDebug("Changing shuffle mode");
        _settings.Avalonix.ShuffleChanged?.Invoke(!_settings.Avalonix.PlaySettings.Shuffle);
        _settings.Avalonix.PlaySettings.Shuffle = !_settings.Avalonix.PlaySettings.Shuffle;
    }

    public void ResetLoop()
    {
        logger.LogDebug("Changing loop mode");
        _settings.Avalonix.LoopChanged?.Invoke(!_settings.Avalonix.PlaySettings.Loop);
        _settings.Avalonix.PlaySettings.Loop = !_settings.Avalonix.PlaySettings.Loop;
    }

    public void ForceStartTrack(Track track)
    {
        PlayingPlayable?.ForceStartTrack(track);
    }

    public event Action<bool> PlaybackStateChanged
    {
        add => MediaPlayer.PlaybackStateChanged += value;
        remove => MediaPlayer.PlaybackStateChanged -= value;
    }

    public event Action TrackChanged
    {
        add => MediaPlayer.TrackChanged += value;
        remove => MediaPlayer.TrackChanged -= value;
    }

    public event Action<bool> ShuffleChanged
    {
        add => _settings.Avalonix.ShuffleChanged += value;
        remove => _settings.Avalonix.ShuffleChanged -= value;
    }

    public event Action<bool> LoopChanged
    {
        add => _settings.Avalonix.LoopChanged += value;
        remove => _settings.Avalonix.LoopChanged -= value;
    }

    public event Action? PlayableChanged;
}
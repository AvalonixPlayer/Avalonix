using System;
using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.UserSettings;
using Avalonix.Services.PlayableManager.AlbumManager;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Track = Avalonix.Models.Media.Track.Track;

namespace Avalonix.Services.PlayableManager;

public class PlayablesManager(ILogger logger, IPlaylistManager playlistManager, IAlbumManager albumManager, IMediaPlayer mediaPlayer, ISettingsManager settingsManager) : IPlayablesManager
{
    public IMediaPlayer MediaPlayer => mediaPlayer;
    public IPlayable? PlayingPlayable { get; private set; }
    public Track? CurrentTrack => MediaPlayer.CurrentTrack;
    private readonly Settings _settings = settingsManager.Settings!;
    public Task StartPlayable(IPlayable playable)
    {
        playlistManager.PlayingPlayable?.Stop();
        albumManager.PlayingPlayable?.Stop();
        PlayingPlayable = playable;
        
        switch (playable)
        {
            case Playlist:
                playlistManager.StartPlaylist(playable);
                break;
            case Album:
                albumManager.StartAlbum(playable);
                break;
        }
        PlayableChanged?.Invoke();
        return Task.CompletedTask;
    }

    public async Task ChangeVolume(uint volume) => await MediaPlayer.ChangeVolume(volume);

    public void PausePlayable() => PlayingPlayable?.Pause();

    public void ResumePlayable() => PlayingPlayable?.Resume();

    public void NextTrack() => PlayingPlayable?.NextTrack();

    public void TrackBefore() => PlayingPlayable?.BackTrack();
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

    public void ForceStartTrackByIndex(int index) => PlayingPlayable?.ForceStartTrackByIndex(index);

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
        add => _settings.Avalonix.SuffleChanged += value;
        remove => _settings.Avalonix.SuffleChanged -= value;
    }

    public event Action<bool> LoopChanged
    {
        add => _settings.Avalonix.LoopChanged += value;
        remove => _settings.Avalonix.LoopChanged -= value;
    }
    
    public event Action? PlayableChanged;
}
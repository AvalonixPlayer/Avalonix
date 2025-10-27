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
    private CancellationTokenSource _globalCancellationTokenSource;
    public bool IsPaused { get; } = player.IsPaused;
    public Track? CurrentTrack { get; } = player.CurrentTrack;

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

    public async Task StartPlaylist(Playlist playlist)
    {
        _globalCancellationTokenSource?.Cancel();
        _globalCancellationTokenSource = new CancellationTokenSource();
        
        if (PlayingPlaylist != null)
        {
            PlayingPlaylist.Stop();
            PlayingPlaylist = null;
        }

        PlayingPlaylist = playlist;

        _ = Task.Run(async () => await PlayingPlaylist.Play());
    }

    public async Task ChangeVolume(uint volume) => await player.ChangeVolume(volume);

    public void PausePlaylist() =>
        PlayingPlaylist?.Pause();
    
    public void ResumePlaylist() =>
        PlayingPlaylist?.Resume();
    
    public void NextTrack() =>
        PlayingPlaylist?.NextTrack();

    public void TrackBefore() =>
        PlayingPlaylist?.BackTrack();
}
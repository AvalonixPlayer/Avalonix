using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.Track;

namespace Avalonix.Services.PlaylistManager;

public interface IPlaylistManager
{
    IMediaPlayer MediaPlayer { get; }
    Playlist? PlayingPlaylist { get; set; }
    Track? CurrentTrack { get; }
    bool IsPaused { get; }
    Playlist ConstructPlaylist(string title, List<Track> tracks);
    Task EditPlaylist(Playlist playlist);
    Task CreatePlaylist(Playlist playlist);
    void DeletePlaylist(Playlist playlist);
    Task StartPlaylist(Playlist playlist);
    void PausePlaylist();
    void ResumePlaylist();
    void NextTrack();
    void TrackBefore();
    Task ChangeVolume(uint volume);
    void ResetSnuffle();
    event Action<bool> PlaybackStateChanged;
    event Action TrackChanged;
}